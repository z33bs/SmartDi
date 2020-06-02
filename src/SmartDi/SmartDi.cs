using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("SmartDiTests")]
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
            ResolveBubblesToStaticContainer = true;
        }
        public bool TryResolveUnregistered { get; set; }
        public bool ResolveBubblesToStaticContainer { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public delegate object ObjectActivator(params object[] args);

    //todo documentation
    //todo list registrations
    //todo autoregister (with flags like bindingflags) and exclusion like Tiny
    //todo refactor to TConcrete etc
    public interface IDiContainer : IDisposable
    {
        //RegisterType
        RegisterOptions RegisterType(Type concreteType, Type resolvedType = null, string key = null, params Type[] constructorParameters);

        void RegisterOpenGeneric(Type concreteType, Type resolvedType = null, string key = null);

        //Register
        RegisterOptions Register<ConcreteType>()
            where ConcreteType : notnull;

        RegisterOptions Register<ConcreteType, ResolvedType>()
            where ConcreteType : notnull, ResolvedType;

        RegisterOptions Register<ConcreteType>(string key)
            where ConcreteType : notnull;

        RegisterOptions Register<ConcreteType, ResolvedType>(string key)
            where ConcreteType : notnull, ResolvedType;

        //... with Ctor
        RegisterOptions Register<ConcreteType>(params Type[] constructorParameters)
            where ConcreteType : notnull;

        RegisterOptions Register<ConcreteType, ResolvedType>(params Type[] constructorParameters)
            where ConcreteType : notnull, ResolvedType;

        RegisterOptions Register<ConcreteType>(string key, params Type[] constructorParameters)
            where ConcreteType : notnull;

        RegisterOptions Register<ConcreteType, ResolvedType>(string key, params Type[] constructorParameters)
            where ConcreteType : notnull, ResolvedType;

        // ... Expression
        //todo Try refactor to just Register?
        RegisterOptions RegisterExpression<TConcrete>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate)
            where TConcrete : notnull;

        RegisterOptions RegisterExpression<TConcrete, TResolved>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate)
            where TConcrete : notnull, TResolved;

        RegisterOptions RegisterExpression<TConcrete>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate, string key)
            where TConcrete : notnull;

        RegisterOptions RegisterExpression<TConcrete, TResolved>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate, string key)
            where TConcrete : notnull, TResolved;

        // ... Instance
        //todo This must be typed! See tests - are there any? Did they work?
        void RegisterInstance(object instance);

        void RegisterInstance<ResolvedType>(object instance)
            where ResolvedType : notnull;

        void RegisterInstance(object instance, string key);

        void RegisterInstance<ResolvedType>(object instance, string key)
            where ResolvedType : notnull;

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

        public static Settings MySettings { get; } = new Settings();


        public DiContainer()
        {
            container = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            openGenericContainer = new ConcurrentDictionary<Tuple<string, string>, GenericMetaObject>();
            enumerableLookup = new ConcurrentDictionary<Type, EnumerableBinding>();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public DiContainer(
            ConcurrentDictionary<Tuple<Type, string>, MetaObject> container,
            ConcurrentDictionary<Tuple<string, string>, GenericMetaObject> openGenericContainer = null,
            ConcurrentDictionary<Type, EnumerableBinding> enumerableLookup = null)
        {
            this.container = container;
            this.openGenericContainer = openGenericContainer ?? new ConcurrentDictionary<Tuple<string, string>, GenericMetaObject>();
            this.enumerableLookup = enumerableLookup ?? new ConcurrentDictionary<Type, EnumerableBinding>();
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

        public ConcurrentDictionary<Tuple<Type, string>, MetaObject> ParentContainer { get; set; }

        internal ConcurrentDictionary<Tuple<Type, string>, MetaObject> container;
        internal readonly ConcurrentDictionary<Tuple<string, string>, GenericMetaObject> openGenericContainer;
        internal readonly ConcurrentDictionary<Type, EnumerableBinding> enumerableLookup;

        public IDiContainer NewChildContainer()
        {
            return new DiContainer() { ParentContainer = this.container };
        }

        #region Registration
        #region Register

        public static RegisterOptions Register<ConcreteType>()
            where ConcreteType : notnull
            => (Instance as IDiContainer).Register<ConcreteType>(Type.EmptyTypes);

        RegisterOptions IDiContainer.Register<ConcreteType>()
            => (this as IDiContainer).Register<ConcreteType>(Type.EmptyTypes);

        public static RegisterOptions Register<ConcreteType>(params Type[] constructorParameters)
            where ConcreteType : notnull
            => (Instance as IDiContainer).Register<ConcreteType>(constructorParameters);

        RegisterOptions IDiContainer.Register<ConcreteType>(params Type[] constructorParameters)
            => new RegisterOptions(
                container,
                InternalRegister(container, null, null, new MetaObject(typeof(ConcreteType), LifeCycle.Transient, constructorParameters)));



        public static RegisterOptions Register<ConcreteType, ResolvedType>()
            where ConcreteType : notnull, ResolvedType
            => (Instance as IDiContainer).Register<ConcreteType, ResolvedType>(Type.EmptyTypes);

        RegisterOptions IDiContainer.Register<ConcreteType, ResolvedType>()
            => (this as IDiContainer).Register<ConcreteType, ResolvedType>(Type.EmptyTypes);

        public static RegisterOptions Register<ConcreteType, ResolvedType>(params Type[] constructorParameters)
            where ConcreteType : notnull, ResolvedType
            => (Instance as IDiContainer).Register<ConcreteType, ResolvedType>(constructorParameters);

        RegisterOptions IDiContainer.Register<ConcreteType, ResolvedType>(params Type[] constructorParameters)
            => new RegisterOptions(
                container,
                InternalRegister(container, typeof(ResolvedType), null, new MetaObject(typeof(ConcreteType), LifeCycle.Transient, constructorParameters)));



        #endregion
        #region Register with Key

        public static RegisterOptions Register<ConcreteType>(string key)
            where ConcreteType : notnull
            => (Instance as IDiContainer).Register<ConcreteType>(key, Type.EmptyTypes);

        RegisterOptions IDiContainer.Register<ConcreteType>(string key)
            => (this as IDiContainer).Register<ConcreteType>(key, Type.EmptyTypes);

        public static RegisterOptions Register<ConcreteType>(string key, params Type[] constructorParameters)
            where ConcreteType : notnull
            => (Instance as IDiContainer).Register<ConcreteType>(key, constructorParameters);

        RegisterOptions IDiContainer.Register<ConcreteType>(string key, params Type[] constructorParameters)
            => new RegisterOptions(
                container,
                InternalRegister(container, null, key, new MetaObject(typeof(ConcreteType), LifeCycle.Transient, constructorParameters)));



        public static RegisterOptions Register<ConcreteType, ResolvedType>(string key)
            where ConcreteType : notnull, ResolvedType
            => (Instance as IDiContainer).Register<ConcreteType, ResolvedType>(key, Type.EmptyTypes);

        RegisterOptions IDiContainer.Register<ConcreteType, ResolvedType>(string key)
            => (this as IDiContainer).Register<ConcreteType, ResolvedType>(key, Type.EmptyTypes);

        public static RegisterOptions Register<ConcreteType, ResolvedType>(string key, params Type[] constructorParameters)
            where ConcreteType : notnull, ResolvedType
            => (Instance as IDiContainer).Register<ConcreteType, ResolvedType>(key, constructorParameters);

        RegisterOptions IDiContainer.Register<ConcreteType, ResolvedType>(string key, params Type[] constructorParameters)
            => new RegisterOptions(
                container,
                InternalRegister(container, typeof(ResolvedType), key, new MetaObject(typeof(ConcreteType), LifeCycle.Transient, constructorParameters)));



        #endregion

        #region RegisterExpression
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



        public static RegisterOptions RegisterExpression<TConcrete>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate)
            where TConcrete : notnull
            => (Instance as IDiContainer).RegisterExpression<TConcrete>(instanceDelegate);

        RegisterOptions IDiContainer.RegisterExpression<TConcrete>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate)
            => new RegisterOptions(
                container,
                InternalRegister(container, null, null, new MetaObject(typeof(TConcrete), LifeCycle.Transient, CastToUntypedOutput(instanceDelegate).Compile())));



        public static RegisterOptions RegisterExpression<TConcrete, TResolved>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate)
            where TConcrete : notnull, TResolved
            => (Instance as IDiContainer).RegisterExpression<TConcrete, TResolved>(instanceDelegate);

        RegisterOptions IDiContainer.RegisterExpression<TConcrete, TResolved>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate)
            => new RegisterOptions(
                container,
                InternalRegister(container, typeof(TResolved), null, new MetaObject(typeof(TConcrete), LifeCycle.Transient, CastToUntypedOutput(instanceDelegate).Compile())));



        public static RegisterOptions RegisterExpression<TConcrete>(Expression<Func<IDiContainer, TConcrete>>
 instanceDelegate, string key)
            where TConcrete : notnull
                => (Instance as IDiContainer).RegisterExpression<TConcrete>(instanceDelegate, key);

        RegisterOptions IDiContainer.RegisterExpression<TConcrete>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate, string key)
            => new RegisterOptions(
                container,
                InternalRegister(container, null, key, new MetaObject(typeof(TConcrete), LifeCycle.Transient, CastToUntypedOutput(instanceDelegate).Compile())));

        public static RegisterOptions RegisterExpression<TConcrete, TResolved>(Expression<Func<IDiContainer, TConcrete>>
 instanceDelegate, string key)
            where TConcrete : notnull, TResolved
                => (Instance as IDiContainer).RegisterExpression<TConcrete, TResolved>(instanceDelegate, key);

        RegisterOptions IDiContainer.RegisterExpression<TConcrete, TResolved>(Expression<Func<IDiContainer, TConcrete>> instanceDelegate, string key)
            => new RegisterOptions(
                container,
                InternalRegister(container, typeof(TResolved), key, new MetaObject(typeof(TConcrete), LifeCycle.Transient, CastToUntypedOutput(instanceDelegate).Compile())));

        #endregion

        #region Register Instance

        public static void RegisterInstance(object instance)
            => (Instance as IDiContainer).RegisterInstance(instance);

        void IDiContainer.RegisterInstance(object instance)
            => new RegisterOptions(
                container
                , InternalRegister(container, null, null, new MetaObject(instance)));



        public static void RegisterInstance<ResolvedType>(object instance)
            where ResolvedType : notnull
            => (Instance as IDiContainer).RegisterInstance<ResolvedType>(instance);

        void IDiContainer.RegisterInstance<ResolvedType>(object instance)
            => new RegisterOptions(
                container
                , InternalRegister(container, typeof(ResolvedType), null, new MetaObject(instance)));



        public static void RegisterInstance(object instance, string key)
            => (Instance as IDiContainer).RegisterInstance(instance, key);

        void IDiContainer.RegisterInstance(object instance, string key)
            => new RegisterOptions(
                container
                , InternalRegister(container, null, key, new MetaObject(instance)));



        public static void RegisterInstance<ResolvedType>(object instance, string key)
            where ResolvedType : notnull
            => (Instance as IDiContainer).RegisterInstance<ResolvedType>(instance, key);

        void IDiContainer.RegisterInstance<ResolvedType>(object instance, string key)
            => new RegisterOptions(
                container
                , InternalRegister(container, typeof(ResolvedType), key, new MetaObject(instance)));



        #endregion

        #region Register Type
        public static RegisterOptions RegisterType(Type concreteType, Type resolvedType = null, string key = null, params Type[] constructorParameters)
            => (Instance as IDiContainer).RegisterType(concreteType, resolvedType, key, constructorParameters);

        RegisterOptions IDiContainer.RegisterType(Type concreteType, Type resolvedType, string key, params Type[] constructorParameters)
            => new RegisterOptions(
                container,
                InternalRegister(container, resolvedType, key,
                    new MetaObject(
                        concreteType,
                        LifeCycle.Transient,
                        constructorParameters)));

        //todo validatee resolvedType:ConcreteType
        //todo investigate possibility of ctor params
        public static void RegisterOpenGeneric(Type concreteType, Type resolvedType = null, string key = null)
            => (Instance as IDiContainer).RegisterOpenGeneric(concreteType, resolvedType, key);

        void IDiContainer.RegisterOpenGeneric(Type concreteType, Type resolvedType, string key)
            => InternalRegisterOpenGeneric(openGenericContainer, resolvedType, key,
                    new GenericMetaObject(
                        concreteType,
                        LifeCycle.Transient));

        //todo ?should be nonstatic now?
        public void EnumerableBindingLifeCycle<T>(LifeCycle lifeCycle) where T : notnull
        {
            if (enumerableLookup.TryGetValue(typeof(T), out EnumerableBinding binding))
                binding.LifeCycle = lifeCycle;
            else
                enumerableLookup.TryAdd(typeof(T), new EnumerableBinding());
            //staticEnumerableLookup[typeof(T)].Item2 = lifeCycle;
        }

        #endregion

        internal Tuple<Type, string> InternalRegister(
            ConcurrentDictionary<Tuple<Type, string>, MetaObject> container,
            Type resolvedType,
            string key,
            MetaObject metaObject
            )
        {
            var containerKey = new Tuple<Type, string>(
                resolvedType ?? metaObject.ConcreteType, key);

            if (!container.TryAdd(containerKey, metaObject))
            {
                var builder = new StringBuilder();
                builder.Append($"{containerKey.Item1.Name} is already registered");
                if (containerKey.Item2 != null)
                    builder.Append($" with key '{nameof(containerKey.Item2)}'");
                builder.Append(".");
                throw new RegisterException(builder.ToString());
            }

            //the way its setup resolved type should only be interface / abstract
            if (resolvedType != null && (resolvedType.IsInterface || resolvedType.IsAbstract))
            {
                //staticEnumerableLookup.AddOrUpdate(resolvedType, new List<string>() { key }, (k, oldvalue) => { oldvalue.Add(key); return oldvalue; });
                enumerableLookup.TryAdd(resolvedType, new EnumerableBinding());
                enumerableLookup[resolvedType].Implementations.Add(key);
            }

            return containerKey;
        }

        //todo only diff is the Tuple - any such thing as a generic Tuple?
        internal static Tuple<string, string> InternalRegisterOpenGeneric(
            ConcurrentDictionary<Tuple<string, string>, GenericMetaObject> container,
            Type resolvedType,
            string key,
            GenericMetaObject metaObject
            )
        {
            var containerKey = new Tuple<string, string>(
                resolvedType?.Name ?? metaObject.ConcreteType.Name, key);

            if (!container.TryAdd(containerKey, metaObject))
            {
                var builder = new StringBuilder();
                builder.Append($"{nameof(containerKey.Item1)} is already registered");
                if (containerKey.Item2 != null)
                    builder.Append($" with key '{nameof(containerKey.Item2)}'");
                builder.Append(".");
                throw new RegisterException(builder.ToString());
            }

            return containerKey;
        }


        #endregion

        #region Resolve

        public static T Resolve<T>() where T : notnull
            => (Instance as IDiContainer).Resolve<T>();

        T IDiContainer.Resolve<T>()
            => (T)InternalResolve(container, typeof(T), null, this);


        public static T Resolve<T>(string key) where T : notnull
            => (Instance as IDiContainer).Resolve<T>(key);

        T IDiContainer.Resolve<T>(string key)
            => (T)InternalResolve(container, typeof(T), key, this);

        public static object Resolve(Type type)
            => (Instance as IDiContainer).Resolve(type);

        object IDiContainer.Resolve(Type type)
            => InternalResolve(container, type, null, this);

        public static object Resolve(Type type, string key)
            => (Instance as IDiContainer).Resolve(type, key);

        object IDiContainer.Resolve(Type type, string key)
            => InternalResolve(container, type, key, this);

        internal object InternalResolve(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Type resolvedType, string key, IDiContainer smartDiInstance = null)
        {
            //if registered
            if (container.TryGetValue(new Tuple<Type, string>(resolvedType, key), out MetaObject metaObject))
            {
                if (metaObject.Instance != null) //Will only be the case if Singleton
                    return metaObject.Instance;

                object instance;
                if (metaObject.ActivationExpression != null)
                    instance = metaObject.ActivationExpression(smartDiInstance);
                //else if (metaObject.StaticActivationExpression != null)
                //    instance = metaObject.StaticActivationExpression();
                else
                    instance = metaObject.ObjectActivator(ResolveDependencies(container, metaObject).ToArray());

                if (metaObject.LifeCycle == LifeCycle.Singleton)
                    metaObject.Instance = instance; //Cache if singleton

                return instance;
            }

            if (resolvedType.IsGenericType)
            {
                //todo move to register with ActivationExpression
                if (resolvedType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    var resolvableType = resolvedType.GetGenericArguments()[0];

                    if (enumerableLookup.TryGetValue(resolvableType, out EnumerableBinding enumerableBinding))
                    {
                        var metainstance = EnumerateFromRegistrations(container, resolvableType, enumerableBinding.Implementations);

                        var instance = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(resolvableType));
                        foreach (var item in metainstance)
                        {
                            instance.Add(item);
                        }

                        //todo if success, add to container
                        if (enumerableBinding.LifeCycle == LifeCycle.Singleton && !container.TryAdd(new Tuple<Type, string>(resolvedType, null), new MetaObject(instance)))
                        {
#if DEBUG
                            throw new ResolveException("Debugging exception: Unextpected behaviour. Should have found listing");
#endif
                        }
                        return instance;
                    }

                    throw new ResolveException(
                        $"Could not Resolve or Create {resolvedType.Name}" +
                        $". It is not registered in {nameof(DiContainer)}. Furthermore, " +
                        $"smart resolve couldn't create an instance.");


                }
                if (resolvedType.IsConstructedGenericType)
                {
                    //todo need to replicate for instance
                    if (openGenericContainer.TryGetValue(new Tuple<string, string>(resolvedType.Name, key), out GenericMetaObject genericMetaObject))
                    {
                        Type[] closedTypeArgs = resolvedType.GetGenericArguments();
                        Type resolvableType = genericMetaObject.ConcreteType.MakeGenericType(closedTypeArgs);

                        //todo code repetition
                        var tryMetaObject = new MetaObject(resolvableType, genericMetaObject.LifeCycle);
                        var instance = tryMetaObject.ObjectActivator(ResolveDependencies(container, tryMetaObject).ToArray());
                        //if success then add registration
                        container.TryAdd(new Tuple<Type, string>(resolvedType, key), tryMetaObject);
                        return instance;
                    }
                }
            }

            if (ParentContainer != null)
                return InternalResolve(ParentContainer, resolvedType, key);

            if (!MySettings.TryResolveUnregistered)
                throw new ResolveException(
                    $"The type {resolvedType.Name} has not been registered. Either " +
                    $"register the class, or configure {nameof(MySettings)}.");

            if (resolvedType.IsInterface || resolvedType.IsAbstract)
                throw new ResolveException(
                    $"Could not Resolve or Create {resolvedType.Name}" +
                    $". It is not registered in {nameof(DiContainer)}. Furthermore, " +
                    $"smart resolve couldn't create an instance.");

            //else try resolve concreteType anyway
            try
            {
                var tryMetaObject = new MetaObject(resolvedType, LifeCycle.Transient);
                var instance = tryMetaObject.ObjectActivator(ResolveDependencies(container, tryMetaObject).ToArray());
                //if success then add registration
                container.TryAdd(new Tuple<Type, string>(resolvedType, key), tryMetaObject);
                return instance;
            }
            catch (Exception ex)
            {
                if (ex is ResolveException)
                    throw ex;

                throw new ResolveException(
                    $"Could not Resolve or Create {resolvedType.Name}" +
                    $". It is not registered in {nameof(DiContainer)}. Furthermore, " +
                    $"smart resolve couldn't create an instance.", ex);
            }
        }

        private IEnumerable<object> EnumerateFromRegistrations(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Type resolvedType, List<string> implementations)
        {
            foreach (var implementation in implementations)
            {
                yield return InternalResolve(container, resolvedType, implementation);
            }
        }

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
            foreach (var registration in container)
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
                throw new Exception($"Error while disposing registered object of type {metaObject.ConcreteType.Name}", ex);
            }
        }
        #endregion

        void IDisposable.Dispose()
        {
            InternalUnregisterAll(container);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ILifeCycleOptions
    {
        void SingleInstance();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class RegisterOptions : ILifeCycleOptions
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
            Instance = instance;
        }

        //public MetaObject(Type concreteType, LifeCycle lifeCycle, Func<object> staticInstanceDelegate) : this(concreteType, lifeCycle)
        //{
        //    if (staticInstanceDelegate is null)
        //        throw new ArgumentNullException(nameof(staticInstanceDelegate));

        //    StaticActivationExpression = staticInstanceDelegate;
        //}

        public MetaObject(Type concreteType, LifeCycle lifeCycle, Func<IDiContainer, object> instanceDelegate) : this(concreteType, lifeCycle)
        {
            if (instanceDelegate is null)
                throw new ArgumentNullException(nameof(instanceDelegate));

            ActivationExpression = instanceDelegate;
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

        //todo can we translate this into an ActivationExpression?
        public ObjectActivator ObjectActivator { get; }

        //public Func<object> StaticActivationExpression { get; }

        public Func<IDiContainer, object> ActivationExpression { get; }

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

    //todo can derive from a common abstractType
    public class GenericMetaObject
    {
        public GenericMetaObject(Type concreteType, LifeCycle lifeCycle)
        {
            if (!concreteType.IsGenericTypeDefinition)
            {
                var builder = new StringBuilder();
                builder.Append($"{concreteType.Name} is not an open (unbound) generic type.");
                if (concreteType.IsConstructedGenericType)
                    builder.Append("Register using standard Register options.");

                throw new RegisterException(builder.ToString());
            }

            this.ConcreteType = concreteType;
            this.LifeCycle = lifeCycle;
        }

        public LifeCycle LifeCycle { get; set; }
        public Type ConcreteType { get; }

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
