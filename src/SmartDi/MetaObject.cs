using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SmartDi
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MetaObject : IDisposable
    {
        #region Constructors
        public MetaObject(object instance) : this(instance?.GetType(),LifeCycle.Singleton)
        {
            //this( ctor will throw if instance is null
            Instance = instance;
        }

        public MetaObject(Type concreteType, LifeCycle lifeCycle, Func<object> instanceDelegate) : this(concreteType, lifeCycle)
        {
            if (instanceDelegate is null)
                throw new ArgumentNullException(nameof(instanceDelegate));

            //todo Increase performance by passing compiled expression
            ObjectActivator = (args) => instanceDelegate();
        }

        public MetaObject(Type concreteType, LifeCycle lifeCycle, params Type[] args) : this(concreteType, lifeCycle)
        {
            ConstructorCache = args != Type.EmptyTypes
                    ? GetSpecificConstructor(concreteType, args)
                    : GetBestConstructor(concreteType);

            ObjectActivator = GetActivator(ConstructorCache);
        }

        private MetaObject(Type concreteType, LifeCycle lifeCycle)
        {
            ConcreteType = concreteType ?? throw new ArgumentNullException(nameof(concreteType));
            LifeCycle = lifeCycle;
        }
        #endregion

        #region Properties
        public Type ConcreteType { get; }

        object instance;
        public object Instance
        {
            get => instance;
            set
            {
#if DEBUG
                if (LifeCycle is LifeCycle.Singleton)
                    instance = value;
                else
                    throw new Exception("Should only set Instance if LifeCycle is Singleton");
#else
                instance = value;
#endif
            }
        }

        LifeCycle lifeCycle;
        public LifeCycle LifeCycle
        {
            get => lifeCycle;
            set
            {
                if (value is LifeCycle.Transient)
                    instance = null; //Can only have instance if Singleton

                lifeCycle = value;
            }
        }

        public ConstructorInfo ConstructorCache { get; }

        public ObjectActivator ObjectActivator { get; }

        #endregion

        #region Internal methods
        internal ConstructorInfo GetSpecificConstructor(Type concreteType, params Type[] args)
        {
            try
            {
                var constructor = concreteType
                    .GetConstructor(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        args,
                        null);

                //Rather throw error on registration
                if (constructor is null)
                    throw new Exception($"No matching constructor found.");

                return constructor;
            }
            catch (Exception ex)
            {
                throw new RegisterException($"Could not register {ConcreteType.Name} with specified constructor.", ex);
            }
        }

        internal ConstructorInfo GetBestConstructor(Type concreteType)
        {
            var constructors = concreteType.
                    GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                    .ToList();

            if (constructors.Count == 0)
                throw new RegisterException($"{concreteType.Name} won't be resolved as it has no constructors.");

            if (constructors.Count > 1)
            {
                //if flagged, shorten to only flagged constructors
                var flaggedConstructors = constructors
                    .Where(c => c.GetCustomAttribute<ResolveUsingAttribute>() != null)
                    .ToList();

                if (flaggedConstructors.Any())
                {
                    if (flaggedConstructors.Count > 1)
                        throw new ResolveException($"{concreteType.Name} may only have one [ResolveUsing] attribute");
                    constructors = flaggedConstructors;
                }

                return constructors
                    .Aggregate((i, j)
                        => i.GetParameters().Count() > j.GetParameters().Count()
                        ? i
                        : j);
            }

            return constructors[0];
        }

        internal ObjectActivator GetActivator
            (ConstructorInfo ctor)
        {
            var paramsInfo = ctor.GetParameters();

            //create a single param of type object[]
            var param =
                Expression.Parameter(typeof(object[]), "args");

            var argsExp =
                new Expression[paramsInfo.Length];

            //pick each arg from the params array 
            //and create a typed expression of them
            for (int i = 0; i < paramsInfo.Length; i++)
            {
                var index = Expression.Constant(i);
                var paramType = paramsInfo[i].ParameterType;

                var paramAccessorExp =
                    Expression.ArrayIndex(param, index);

                var paramCastExp =
                    Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the
            //ctor with the args we just created
            var newExp = Expression.New(ctor, argsExp);

            //create a lambda with the New
            //Expression as body and our param object[] as arg
            var lambda =
                Expression.Lambda(typeof(ObjectActivator), newExp, param);

            //compile it
            ObjectActivator compiled = (ObjectActivator)lambda.Compile();
            return compiled;
        }

        #endregion

        public void Dispose()
        {
            if (Instance != null
                && Instance is IDisposable disposable)
                disposable.Dispose();
        }
    }
}

