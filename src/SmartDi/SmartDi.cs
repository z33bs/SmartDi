using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq.Dynamic.Core;

//todo Swap TResolve around
//todo pass container around - might help child
namespace SmartDi
{
    /// <summary>
    /// Customise behaviour. All settings default to true.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Settings
    {
        public Settings()
        {
            TryResolveUnregistered = true;
        }
        public bool TryResolveUnregistered { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public delegate object ObjectActivator(params object[] args);

    //todo documentation
    //todo list registrations
    //todo autoregister (with flags like bindingflags) and exclusion like Tiny
    public interface IDiContainer : IDisposable
    {
        IDiContainer Parent { get; set; }

        IDiContainer NewChildContainer();

        //RegisterType
        IRegisterOptions RegisterType(Type concreteType, Type resolvedType = null, string key = null, params Type[] constructorParameters);

        //Register
        RegisterOptions Register<TConcrete>()
            where TConcrete : notnull;

        RegisterOptions Register<TResolved, TConcrete>()
            where TConcrete : notnull, TResolved;

        RegisterOptions Register<TConcrete>(string key)
            where TConcrete : notnull;

        RegisterOptions Register<TResolved, TConcrete>(string key)
            where TConcrete : notnull, TResolved;

        //... with Ctor
        RegisterOptions Register<TConcrete>(params Type[] constructorParameters)
            where TConcrete : notnull;

        RegisterOptions Register<TResolved, TConcrete>(params Type[] constructorParameters)
            where TConcrete : notnull, TResolved;

        RegisterOptions Register<TConcrete>(string key, params Type[] constructorParameters)
            where TConcrete : notnull;

        RegisterOptions Register<TResolved, TConcrete>(string key, params Type[] constructorParameters)
            where TConcrete : notnull, TResolved;

        // ... Expression
        RegisterOptions RegisterExplicit<TConcrete>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate)
            where TConcrete : notnull;

        RegisterOptions RegisterExplicit<TResolved, TConcrete>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate)
            where TConcrete : notnull, TResolved;

        RegisterOptions RegisterExplicit<TConcrete>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate, string key)
            where TConcrete : notnull;

        RegisterOptions RegisterExplicit<TResolved, TConcrete>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate, string key)
            where TConcrete : notnull, TResolved;

        // ... Instance
        void RegisterInstance(object instance);

        void RegisterInstance<TResolved>(object instance)
            where TResolved : notnull;

        void RegisterInstance(object instance, string key);

        void RegisterInstance<TResolved>(object instance, string key)
            where TResolved : notnull;

        void Compile();

        // Resolve

        T Resolve<T>() where T : notnull;
        T Resolve<T>(string key) where T : notnull;

        object Resolve(Type type);
        object Resolve(Type type, string key);

        // Unregister

        void Unregister<T>()
            where T : notnull;

        void Unregister<T>(string key)
            where T : notnull;

        void UnregisterAll();

    }



    public class DiContainer : IDiContainer
    {
        public class EnumerableBinding
        {
            public List<string> Implementations { get; } = new List<string>();
            public LifeCycle LifeCycle { get; set; } = LifeCycle.Transient;
        }

        public static Settings Settings { get; } = new Settings();


        public DiContainer()
        {
            container = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            //enumerableLookup = new ConcurrentDictionary<Type, EnumerableBinding>();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public DiContainer(
            ConcurrentDictionary<Tuple<Type, string>, MetaObject> container,
            ConcurrentDictionary<Type, EnumerableBinding> enumerableLookup = null)
        {
            this.container = container;
            //this.enumerableLookup = enumerableLookup ?? new ConcurrentDictionary<Type, EnumerableBinding>();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetContainer(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container)
            => Instance.container = container;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void ResetContainer()
        {
            Instance = new DiContainer();
        }

        internal static DiContainer Instance { get; private set; } = new DiContainer();

        public IDiContainer Parent { get; set; }

        //todo if key is string, string - don't need separate generic container
        internal ConcurrentDictionary<Tuple<Type, string>, MetaObject> container;
        internal ConcurrentDictionary<Tuple<Type, string>, MetaObject> parentContainer;
        //internal readonly ConcurrentDictionary<Type, EnumerableBinding> enumerableLookup;

        public IDiContainer NewChildContainer()
        {
            return new DiContainer() { Parent = this, parentContainer = this.container };
        }

        #region Registration
        #region Register

        public static RegisterOptions Register<TConcrete>()
            where TConcrete : notnull
            => (Instance as IDiContainer).Register<TConcrete>(Type.EmptyTypes);

        RegisterOptions IDiContainer.Register<TConcrete>()
            => (this as IDiContainer).Register<TConcrete>(Type.EmptyTypes);

        public static RegisterOptions Register<TConcrete>(params Type[] constructorParameters)
            where TConcrete : notnull
            => (Instance as IDiContainer).Register<TConcrete>(constructorParameters);

        RegisterOptions IDiContainer.Register<TConcrete>(params Type[] constructorParameters)
            => new RegisterOptions(
                container,
                InternalRegister(container, null, null, new MetaObject(typeof(TConcrete), LifeCycle.Transient, constructorParameters)));



        public static RegisterOptions Register<TResolved, TConcrete>()
            where TConcrete : notnull, TResolved
            => (Instance as IDiContainer).Register<TResolved, TConcrete>(Type.EmptyTypes);

        RegisterOptions IDiContainer.Register<TResolved, TConcrete>()
            => (this as IDiContainer).Register<TResolved, TConcrete>(Type.EmptyTypes);

        public static RegisterOptions Register<TResolved, TConcrete>(params Type[] constructorParameters)
            where TConcrete : notnull, TResolved
            => (Instance as IDiContainer).Register<TResolved, TConcrete>(constructorParameters);

        RegisterOptions IDiContainer.Register<TResolved, TConcrete>(params Type[] constructorParameters)
            => new RegisterOptions(
                container,
                InternalRegister(container, typeof(TResolved), null, new MetaObject(typeof(TConcrete), LifeCycle.Transient, constructorParameters)));



        #endregion
        #region Register with Key

        public static RegisterOptions Register<TConcrete>(string key)
            where TConcrete : notnull
            => (Instance as IDiContainer).Register<TConcrete>(key, Type.EmptyTypes);

        RegisterOptions IDiContainer.Register<TConcrete>(string key)
            => (this as IDiContainer).Register<TConcrete>(key, Type.EmptyTypes);

        public static RegisterOptions Register<TConcrete>(string key, params Type[] constructorParameters)
            where TConcrete : notnull
            => (Instance as IDiContainer).Register<TConcrete>(key, constructorParameters);

        RegisterOptions IDiContainer.Register<TConcrete>(string key, params Type[] constructorParameters)
            => new RegisterOptions(
                container,
                InternalRegister(container, null, key, new MetaObject(typeof(TConcrete), LifeCycle.Transient, constructorParameters)));



        public static RegisterOptions Register<TResolved, TConcrete>(string key)
            where TConcrete : notnull, TResolved
            => (Instance as IDiContainer).Register<TResolved, TConcrete>(key, Type.EmptyTypes);

        RegisterOptions IDiContainer.Register<TResolved, TConcrete>(string key)
            => (this as IDiContainer).Register<TResolved, TConcrete>(key, Type.EmptyTypes);

        public static RegisterOptions Register<TResolved, TConcrete>(string key, params Type[] constructorParameters)
            where TConcrete : notnull, TResolved
            => (Instance as IDiContainer).Register<TResolved, TConcrete>(key, constructorParameters);

        RegisterOptions IDiContainer.Register<TResolved, TConcrete>(string key, params Type[] constructorParameters)
            => new RegisterOptions(
                container,
                InternalRegister(container, typeof(TResolved), key, new MetaObject(typeof(TConcrete), LifeCycle.Transient, constructorParameters)));



        #endregion

        #region RegisterExplicit
        Expression<Func<TInput, object>> CastToUntypedOutput<TInput, TOutput>
                (Expression<Func<TInput, TOutput>> expression)
        {
            // Add the boxing operation, but get a weakly typed expression
            Expression converted = Expression.Convert
                 (expression.Body, typeof(object));
            // Use Expression.Lambda to get back to strong typing
            return Expression.Lambda<Func<TInput, object>>
                 (converted, expression.Parameters);
        }



        public static RegisterOptions RegisterExplicit<TConcrete>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate)
            where TConcrete : notnull
            => (Instance as IDiContainer).RegisterExplicit<TConcrete>(instanceDelegate);

        RegisterOptions IDiContainer.RegisterExplicit<TConcrete>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate)
            => new RegisterOptions(
                container,
                InternalRegister(container, null, null, new MetaObject(typeof(TConcrete), LifeCycle.Transient, CastToUntypedOutput(instanceDelegate), new Tuple<Type, string>(typeof(TConcrete), null))));



        public static RegisterOptions RegisterExplicit<TResolved, TConcrete>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate)
            where TConcrete : notnull, TResolved
            => (Instance as IDiContainer).RegisterExplicit<TResolved, TConcrete>(instanceDelegate);

        RegisterOptions IDiContainer.RegisterExplicit<TResolved, TConcrete>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate)
            => new RegisterOptions(
                container,
                InternalRegister(container, typeof(TResolved), null, new MetaObject(typeof(TConcrete), LifeCycle.Transient, CastToUntypedOutput(instanceDelegate), new Tuple<Type, string>(typeof(TResolved), null))));



        public static RegisterOptions RegisterExplicit<TConcrete>(Expression<Func<IDiContainer, TConcrete>>
 instanceDelegate, string key)
            where TConcrete : notnull
                => (Instance as IDiContainer).RegisterExplicit<TConcrete>(instanceDelegate, key);

        RegisterOptions IDiContainer.RegisterExplicit<TConcrete>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate, string key)
            => new RegisterOptions(
                container,
                InternalRegister(container, null, key, new MetaObject(typeof(TConcrete), LifeCycle.Transient, CastToUntypedOutput(instanceDelegate), new Tuple<Type, string>(typeof(TConcrete), key))));

        public static RegisterOptions RegisterExplicit<TResolved, TConcrete>(Expression<Func<IDiContainer, TConcrete>>
 instanceDelegate, string key)
            where TConcrete : notnull, TResolved
                => (Instance as IDiContainer).RegisterExplicit<TResolved, TConcrete>(instanceDelegate, key);

        RegisterOptions IDiContainer.RegisterExplicit<TResolved, TConcrete>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate, string key)
            => new RegisterOptions(
                container,
                InternalRegister(container, typeof(TResolved), key, new MetaObject(typeof(TConcrete), LifeCycle.Transient, CastToUntypedOutput(instanceDelegate), new Tuple<Type, string>(typeof(TResolved), key))));

        #endregion

        #region Register Instance

        public static void RegisterInstance(object instance)
            => (Instance as IDiContainer).RegisterInstance(instance);

        void IDiContainer.RegisterInstance(object instance)
            => new RegisterOptions(
                container
                , InternalRegister(container, null, null, new MetaObject(instance)));



        public static void RegisterInstance<TResolved>(object instance)
            where TResolved : notnull
            => (Instance as IDiContainer).RegisterInstance<TResolved>(instance);
        //todo Validate TResolved : TConcrete - perhaps bring type conversion forward to here : Can return typeof(TResolved to fit into one method call)
        void IDiContainer.RegisterInstance<TResolved>(object instance)
            => new RegisterOptions(
                container
                , InternalRegister(container, typeof(TResolved), null, new MetaObject(instance)));



        public static void RegisterInstance(object instance, string key)
            => (Instance as IDiContainer).RegisterInstance(instance, key);

        void IDiContainer.RegisterInstance(object instance, string key)
            => new RegisterOptions(
                container
                , InternalRegister(container, null, key, new MetaObject(instance)));



        public static void RegisterInstance<TResolved>(object instance, string key)
            where TResolved : notnull
            => (Instance as IDiContainer).RegisterInstance<TResolved>(instance, key);

        void IDiContainer.RegisterInstance<TResolved>(object instance, string key)
            => new RegisterOptions(
                container
                , InternalRegister(container, typeof(TResolved), key, new MetaObject(instance)));



        #endregion

        #region Register Type
        public static IRegisterOptions RegisterType(Type concreteType, Type resolvedType = null, string key = null, params Type[] constructorParameters)
            => (Instance as IDiContainer).RegisterType(concreteType, resolvedType, key, constructorParameters);

        IRegisterOptions IDiContainer.RegisterType(Type concreteType, Type resolvedType, string key, params Type[] constructorParameters)
        {
            //if (concreteType.IsGenericTypeDefinition)
            //    return new GenericRegisterOptions(
            //        openGenericContainer,
            //        InternalRegisterOpenGeneric(openGenericContainer, resolvedType, key,
            //            new GenericMetaObject(
            //                concreteType,
            //                LifeCycle.Transient)));
            //else
            return new RegisterOptions(
                container,
                InternalRegister(container, resolvedType, key,
                    new MetaObject(
                        concreteType,
                        LifeCycle.Transient,
                        constructorParameters)));
        }
        //todo validatee resolvedType:TConcrete
        //todo investigate possibility of ctor params

        //public void EnumerableBindingLifeCycle<T>(LifeCycle lifeCycle) where T : notnull
        //{
        //    if (enumerableLookup.TryGetValue(typeof(T), out EnumerableBinding binding))
        //        binding.LifeCycle = lifeCycle;
        //    else
        //        enumerableLookup.TryAdd(typeof(T), new EnumerableBinding());
        //    //staticEnumerableLookup[typeof(T)].Item2 = lifeCycle;
        //}

        #endregion


        public static void Compile()
            => (Instance as IDiContainer).Compile();

        void IDiContainer.Compile()
        {
            foreach (var keyValuePair in container)
            {
                if (keyValuePair.Value.ActivationExpression == null)
                    MakeNewExpression(container, keyValuePair.Value);
            }
        }


        internal Tuple<Type, string> InternalRegister(
            ConcurrentDictionary<Tuple<Type, string>, MetaObject> container,
            Type resolvedType,
            string key,
            MetaObject metaObject
            )
        {
            var containerKey = new Tuple<Type, string>(
                resolvedType ?? metaObject.TConcrete, key);

            if (!container.TryAdd(containerKey, metaObject))
            {
                var builder = new StringBuilder();
                builder.Append($"{containerKey.Item1} is already registered");
                if (containerKey.Item2 != null)
                    builder.Append($" with key '{nameof(containerKey.Item2)}'");
                builder.Append(".");
                throw new RegisterException(builder.ToString());
            }

            ////the way its setup resolved type should only be interface / abstract
            //if (resolvedType != null && (resolvedType.IsInterface || resolvedType.IsAbstract))
            //{
            //    //staticEnumerableLookup.AddOrUpdate(resolvedType, new List<string>() { key }, (k, oldvalue) => { oldvalue.Add(key); return oldvalue; });
            //    enumerableLookup.TryAdd(resolvedType, new EnumerableBinding());
            //    enumerableLookup[resolvedType].Implementations.Add(key);
            //}

            return containerKey;
        }

        //internal Tuple<Type, string> InternalRegisterOpenGeneric(
        //    ConcurrentDictionary<Tuple<Type, string>, GenericMetaObject> container,
        //    Type resolvedType,
        //    string key,
        //    GenericMetaObject metaObject
        //    )
        //{
        //    var containerKey = new Tuple<Type, string>(
        //        resolvedType?.Name ?? metaObject.TConcrete.Name, key);

        //    if (!container.TryAdd(containerKey, metaObject))
        //    {
        //        var builder = new StringBuilder();
        //        builder.Append($"{nameof(containerKey.Item1)} is already registered");
        //        if (containerKey.Item2 != null)
        //            builder.Append($" with key '{nameof(containerKey.Item2)}'");
        //        builder.Append(".");
        //        throw new RegisterException(builder.ToString());
        //    }

        //    return containerKey;
        //}


        #endregion

        #region Resolve

        public static T Resolve<T>() where T : notnull
            => (Instance as IDiContainer).Resolve<T>();

        T IDiContainer.Resolve<T>()
            => (T)InternalResolve(container, typeof(T), null);


        public static T Resolve<T>(string key) where T : notnull
            => (Instance as IDiContainer).Resolve<T>(key);

        T IDiContainer.Resolve<T>(string key)
            => (T)InternalResolve(container, typeof(T), key);

        public static object Resolve(Type type)
            => (Instance as IDiContainer).Resolve(type);

        object IDiContainer.Resolve(Type type)
            => InternalResolve(container, type, null);

        public static object Resolve(Type type, string key)
            => (Instance as IDiContainer).Resolve(type, key);

        object IDiContainer.Resolve(Type type, string key)
            => InternalResolve(container, type, key);



        internal Expression GetExpression(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Type resolvedType, string key)
        {
            var metaObject = GetMetaObject(container, resolvedType, key);

            if (metaObject.LifeCycle is LifeCycle.Singleton)
                return Expression.Call(
                    MetaObject.IDiContainerParameter,
                    typeof(IDiContainer)
                        .GetMethod(
                            nameof(IDiContainer.Resolve),
                            new Type[] { typeof(string) })
                        .MakeGenericMethod(resolvedType),
                    Expression.Constant(key, typeof(string)));

            if (metaObject.NewExpression != null)
                return metaObject.NewExpression;

            MakeNewExpression(container, metaObject);

            return metaObject.NewExpression;
        }

        private void MakeNewExpression(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, MetaObject metaObject)
        {
            var paramsInfo = metaObject.ConstructorCache?.GetParameters();

            if (paramsInfo != null)
            {

                var argsExp = new Expression[paramsInfo.Length];

                for (int i = 0; i < paramsInfo.Length; i++)
                {
                    var param = paramsInfo[i];
                    var namedAttribute = param.GetCustomAttribute<ResolveNamedAttribute>();

                    argsExp[i] = GetExpression(container, param.ParameterType, namedAttribute?.Key);
                }

                metaObject.NewExpression = Expression.New(metaObject.ConstructorCache, argsExp);
            }

            else if (metaObject.TConcrete.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                metaObject.NewExpression = GetEnumerableExpression(container, metaObject.TConcrete);

            else
                throw new Exception($"{nameof(metaObject.ConstructorCache)} should not be null");
        }

        ///<param name="container">Needed so we can call with ParentContainer</param>
        internal MetaObject GetMetaObject(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Type resolvedType, string key)
        {
            if (container.TryGetValue(new Tuple<Type, string>(resolvedType, key), out MetaObject metaObject))
                return metaObject;

            if (resolvedType.IsGenericType)
            {
                if (resolvedType.IsConstructedGenericType) //if Generic
                {
                    var genericTypeDefinition = resolvedType.GetGenericTypeDefinition();

                    if (container.TryGetValue(new Tuple<Type, string>(genericTypeDefinition, key), out MetaObject genericMetaObject))
                    {
                        Type[] closedTypeArgs = resolvedType.GetGenericArguments();
                        Type resolvableType = genericTypeDefinition.MakeGenericType(closedTypeArgs);
                        Type makeableType = genericMetaObject.TConcrete.MakeGenericType(closedTypeArgs);

                        //todo - investigate if specify constructor with generic type and pass params here or constructorcache
                        var specificMetaObject = new MetaObject(makeableType, genericMetaObject.LifeCycle);
                        InternalRegister(container, resolvableType, key, specificMetaObject);

                        return specificMetaObject;
                    }
                }

                throw new ResolveException(
                    $"Could not Resolve or Create {resolvedType.Name}" +
                    $". It is not registered in {nameof(DiContainer)}. Furthermore, " +
                    $"smart resolve couldn't create an instance.");
            }

            if (Parent != null)
                return GetMetaObject(parentContainer, resolvedType, key);

            if (resolvedType.IsInterface || resolvedType.IsAbstract)
                throw new ResolveException(
                    $"Could not Resolve or Create {resolvedType.Name}" +
                    $". It is not registered in {nameof(DiContainer)}. Furthermore, " +
                    $"smart resolve couldn't create an instance.");

            if (!Settings.TryResolveUnregistered)
                throw new ResolveException(
                    $"The type {resolvedType.Name} has not been registered. Either " +
                    $"register the class, or configure {nameof(Settings)}.");

            metaObject = new MetaObject(resolvedType, LifeCycle.Transient);
            if (container.TryAdd(new Tuple<Type, string>(resolvedType, null), metaObject))
                return metaObject;

            throw new ResolveException(
                $"The type {resolvedType.Name} has not been registered and SmartResolve didn't work.");
        }

        internal object InternalResolve(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Type resolvedType, string key)
        {
            var metaObject = GetMetaObject(container, resolvedType, key);

            if (metaObject.ActivationExpression is null)
                MakeNewExpression(container, metaObject);

            return metaObject.GetObject(this);
        }

        internal Expression GetEnumerableExpression(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Type resolvedType)
        {
            var resolvableType = resolvedType.GetGenericArguments()[0];
            var addMethod = typeof(List<>).MakeGenericType(resolvableType).GetMethod("Add");

            List<ElementInit> listElements = new List<ElementInit>();
            foreach (var key in container.Keys)
            {
                if (key.Item1 == resolvableType)
                {
                    var expression = GetExpression(container,resolvableType, key.Item2);
                    listElements.Add(Expression.ElementInit(addMethod, expression));
                }
            }

            // Create a NewExpression that represents constructing
            // a new instance of Dictionary<int, string>.
            NewExpression newDictionaryExpression =
                Expression.New(typeof(List<>).MakeGenericType(resolvableType));

            // Create a ListInitExpression that represents initializing
            // a new Dictionary<> instance with two key-value pairs.
            if(listElements.Any())
            return
                Expression.ListInit(
                    newDictionaryExpression,
                    listElements);

            throw new ResolveException($"Could not resolve {resolvedType.Name}");
        }

        //private IEnumerable<ElementInit> GetListElementExpressionsForEnumerable(Type resolvedType, List<string> implementations)
        //{
        //    var addMethod = typeof(List<>).MakeGenericType(resolvedType).GetMethod("Add");
        //    foreach (var implementation in implementations)
        //    {
        //        var expression = GetExpression(resolvedType, implementation);
        //        var elementInit = Expression.ElementInit(addMethod, expression);
        //        yield return elementInit; //InternalResolve(container, resolvedType, implementation);
        //    }
        //}

        internal IEnumerable<object> ResolveDependencies(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, MetaObject metaObject)
        {
            if (metaObject.ConstructorCache != null) //null if instanceDelegate was passed
            {
                foreach (var parameter in metaObject.ConstructorCache.GetParameters())
                {
                    var namedDependencyAttribute = parameter.GetCustomAttribute<ResolveNamedAttribute>();
                    if (namedDependencyAttribute != null)
                        yield return InternalResolve(container, parameter.ParameterType, namedDependencyAttribute.Key);
                    else
                        yield return InternalResolve(container, parameter.ParameterType, null);
                }
            }
        }

        #endregion
        #region Unregister
        public static void Unregister<T>()
            where T : notnull
                => (Instance as IDiContainer).Unregister<T>();

        void IDiContainer.Unregister<T>()
            => InternalUnregister(container, typeof(T), null);


        public static void Unregister<T>(string key)
            where T : notnull
                => (Instance as IDiContainer).Unregister<T>(key);

        void IDiContainer.Unregister<T>(string key)
            => InternalUnregister(container, typeof(T), key);


        static void InternalUnregister(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Type resolvedType, string key)
        {
            if (container.TryRemove(new Tuple<Type, string>(resolvedType, key), out MetaObject metaObject))
                TryDispose(metaObject);
            else
            {
                var builder = new StringBuilder();
                builder.Append($"Can't find {resolvedType.Name}");
                if (!string.IsNullOrEmpty(key))
                    builder.Append($" with key '{key}'");
                builder.Append(".");
                throw new ResolveException(builder.ToString());
            }
        }

        public static void UnregisterAll()
            => (Instance as IDiContainer).UnregisterAll();

        void IDiContainer.UnregisterAll()
            => InternalUnregisterAll(container);


        static void InternalUnregisterAll(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container)
        {
            foreach (var registration in container.ToArray())
            {
                TryDispose(registration.Value);
                container.TryRemove(registration.Key, out _);
            }
        }

        private static void TryDispose(MetaObject metaObject)
        {
            try
            {
                metaObject.Dispose();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while disposing registered object of type {metaObject.TConcrete.Name}", ex);
            }
        }
        #endregion

        void IDisposable.Dispose()
        {
            InternalUnregisterAll(container);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IRegisterOptions : ILifeCycleOptions
    {
    }


    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ILifeCycleOptions
    {
        void SingleInstance();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class RegisterOptions : IRegisterOptions
    {
        readonly ConcurrentDictionary<Tuple<Type, string>, MetaObject> container;
        readonly Tuple<Type, string> key;

        public RegisterOptions(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Tuple<Type, string> key)
        {
            this.container = container;
            this.key = key;
        }

        public void SingleInstance()
        {
            container[key].LifeCycle = LifeCycle.Singleton;
        }
    }


    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MetaObject : IDisposable
    {
        #region Constructors
        public MetaObject(object instance) : this(instance?.GetType(), LifeCycle.Singleton)
        {
            //this( ctor will throw if instance is null
            //lazy = new Lazy<object>(() => instance);
            Instance = instance;
            ActivationExpression = c => instance; //todo I dont think we need this
        }

        //public MetaObject(Type concreteType, LifeCycle lifeCycle, Func<object> staticInstanceDelegate) : this(concreteType, lifeCycle)
        //{
        //    if (staticInstanceDelegate is null)
        //        throw new ArgumentNullException(nameof(staticInstanceDelegate));

        //    StaticActivationExpression = staticInstanceDelegate;
        //}

        ///<param name="key">Need to know its full dictionary key to make its uncompiled expression</param>
        public MetaObject(Type concreteType, LifeCycle lifeCycle, Expression<Func<IDiContainer, object>> instanceDelegate, Tuple<Type, string> key) : this(concreteType, lifeCycle)
        {
            if (instanceDelegate is null)
                throw new ArgumentNullException(nameof(instanceDelegate));

            var resolveMethod = typeof(IDiContainer).GetMethod(nameof(IDiContainer.Resolve), new Type[] { typeof(string) }).MakeGenericMethod(key.Item1);

            //Cant set to instanceDelegate.Body because the parameter c is out of scope
            //...figure this out and you'll have a more elegant solution
            NewExpression = Expression.Call(
                IDiContainerParameter,
                resolveMethod,
                Expression.Constant(key.Item2, typeof(string)));

            ActivationExpression = instanceDelegate.Compile();
        }

        public MetaObject(Type concreteType, LifeCycle lifeCycle, params Type[] args) : this(concreteType, lifeCycle)
        {
            ConstructorCache = args != Type.EmptyTypes
                    ? GetSpecificConstructor(concreteType, args)
                    : GetBestConstructor(concreteType);
        }

        private MetaObject(Type concreteType, LifeCycle lifeCycle)
        {
            TConcrete = concreteType ?? throw new ArgumentNullException(nameof(concreteType));
            LifeCycle = lifeCycle;
        }
        #endregion

        #region Properties

        //todo test whether static causes problem for child containers. Me thinks shouldn't
        public static ParameterExpression IDiContainerParameter { get; } = Expression.Parameter(typeof(IDiContainer), "c");

        Expression newExpression;
        public Expression NewExpression
        {
            get => newExpression;
            set
            {
                newExpression = value;

                //lazy = new Lazy<object>(Expression.Lambda<Func<object>>(newExpression, null).Compile());


                ActivationExpression = Expression.Lambda(
                    newExpression,
                    IDiContainerParameter
                    ).Compile() as Func<IDiContainer, object>;
            }
        }


        public Type TConcrete { get; }

        //Lazy<object> lazy;
        //object instance;
        public object Instance
        {
            get; set;
            //get => lazy?.Value;
            //            set
            //            {
            //#if DEBUG
            //                if (LifeCycle is LifeCycle.Singleton)
            //                    instance = value;
            //                else
            //                    throw new Exception("Should only set Instance if LifeCycle is Singleton");
            //#else
            //                instance = value;
            //#endif
            //            }
        }

        LifeCycle lifeCycle;
        public LifeCycle LifeCycle
        {
            get => lifeCycle;
            set
            {
                //todo check this logic when clean up

                //if (value is LifeCycle.Transient)
                //    instance = null; //Can only have instance if Singleton

                lifeCycle = value;
            }
        }

        public ConstructorInfo ConstructorCache { get; }

        //todo can we translate this into an ActivationExpression?
        public ObjectActivator ObjectActivator { get; }

        //public Func<object> StaticActivationExpression { get; }

        public Func<IDiContainer, object> ActivationExpression { get; private set; }

        #endregion

        #region Internal methods

        public object GetObject(IDiContainer container)
        {
            if (LifeCycle is LifeCycle.Singleton)
            {
                if (Instance is null)
                    Instance = ActivationExpression(container);

                return Instance;
            }

            return ActivationExpression(container);
        }

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
                throw new RegisterException($"Could not register {TConcrete.Name} with specified constructor.", ex);
            }
        }

        internal ConstructorInfo GetBestConstructor(Type concreteType)
        {
            var constructors = concreteType.
                    GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                    .ToList();

            if (constructors.Count == 0)
                if (concreteType.IsGenericType
                    && concreteType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return null; //ok because we won't need the constructor
                else
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


        //todo - don't compile on register. On first resolve compile by substituting each expression.
        internal string TestExpressionCreator(ConstructorInfo ctor)
        {
            var builder = new StringBuilder();
            builder.Append($"new {ctor.DeclaringType.Name}(");
            var paramsInfo = ctor.GetParameters();
            var argsExp =
    new Expression[paramsInfo.Length];



            //var p = Expression.Parameter(typeof(IDiContainer), "c");

            int i = 0;
            foreach (var item in paramsInfo)
            {
                var namedAttribute = item.GetCustomAttribute<ResolveNamedAttribute>();
                if (namedAttribute != null)
                {
                    //todo: only need to do once
                    var get = typeof(IDiContainer).GetMethods().Where(m => m.Name == "Resolve" && m.GetParameters().Count() == 1 && m.IsGenericMethod).ToArray();
                    MethodInfo method = get[0].MakeGenericMethod(item.ParameterType);
                    argsExp[i] = Expression.Call(IDiContainerParameter, method, Expression.Constant(namedAttribute.Key));
                }
                else
                {
                    var get = typeof(IDiContainer).GetMethods().Where(m => m.Name == "Resolve" && m.GetParameters().Count() == 0 && m.IsGenericMethod).ToArray();
                    MethodInfo method = get[0].MakeGenericMethod(item.ParameterType);
                    argsExp[i] = Expression.Call(IDiContainerParameter, method);
                }
                builder.Append($"c.Resolve<{item.ParameterType.Name}>(),");
                i++;
            }
            builder.Remove(builder.Length - 1, 1);
            builder.Append(")");

            //const string exp = @"(Person.Age > 3 AND Person.Weight > 50) OR Person.Age < 3";

            //var e = DynamicExpression.ParseLambda(new[] { p }, null, exp);
            //DynamicExpression.
            //var lambda = DynamicExpressionParser.ParseLambda(new[] { p }, null, builder.ToString());
            var test = Expression.Lambda(Expression.New(ctor, argsExp), IDiContainerParameter);
            //todo somehow need the parameter in here
            ActivationExpression = test.Compile() as Func<IDiContainer, object>;

            return builder.ToString();
        }


        internal ObjectActivator GetActivator
            (ConstructorInfo ctor)
        {
            Debug.WriteLine(TestExpressionCreator(ctor));
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

    /// <summary>
    /// Lifecycle of the registered object
    /// </summary>
    public enum LifeCycle
    {
        /// <summary>
        /// Single instance only
        /// </summary>
        Singleton,
        /// <summary>
        /// Multiple instances - a new one instantiated
        /// each time <c>Resolve</c> is called
        /// </summary>
        Transient
    }
    public class RegisterException : Exception
    {
        //public RegistrationException() : base()
        //{ }
        public RegisterException(string message) : base(message)
        { }
        public RegisterException(string message, Exception innerException) : base(message, innerException)
        { }

    }
    /// <summary>
    /// Exception thrown when <c>Resolve</c> is called for a type that has not been registered
    /// </summary>
    public class ResolveException : Exception
    {
        /// <summary>
        /// Exception thrown when <c>Resolve</c> is called for a type that has not been registered
        /// </summary>
        public ResolveException(string message)
            : base(message)
        {
        }
        /// <summary>
        /// Exception thrown when <c>Resolve</c> is called for a type that has not been registered
        /// </summary>
        public ResolveException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

    }
    /// <summary>
    /// Will resolved the dependency associated with the named <see cref="Key"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ResolveNamedAttribute : Attribute
    {
        /// <summary>
        /// The Key
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// Will resolved the dependency associated with the named <paramref name="key"/>
        /// </summary>
        public ResolveNamedAttribute(string key)
        {
            Key = key;
        }
    }
    /// <summary>
    /// Attribute to explicitly mark which constructor should be used by the <see cref="DiContainer"/>
    /// to instantiate the class
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
    public class ResolveUsingAttribute : Attribute
    {
        /// <summary>
        /// Attribute to explicitly mark which constructor should be used by the <see cref="DiContainer"/>
        /// to instantiate the class
        /// </summary>
        public ResolveUsingAttribute()
        {
        }
    }

}
