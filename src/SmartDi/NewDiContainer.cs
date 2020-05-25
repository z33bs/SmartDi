using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
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

    public class NewDiContainer : INewDiContainer
    {
        public static Settings MySettings { get; } = new Settings();


        public NewDiContainer()
        {
            container = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public NewDiContainer(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container)
        {
            this.container = container;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetContainer(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container)
            => staticContainer = container;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void ResetContainer()
        {
            staticContainer = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
        }

        static ConcurrentDictionary<Tuple<Type, string>, MetaObject> staticContainer
            = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();

        readonly ConcurrentDictionary<Tuple<Type, string>, MetaObject> container;

        #region Registration
        #region Register
        public static RegisterOptions Register<ConcreteType>()
            where ConcreteType : notnull
            => Register<ConcreteType>(Type.EmptyTypes);

        public static RegisterOptions Register<ConcreteType>(params Type[] args)
            where ConcreteType : notnull
            => new RegisterOptions(
                staticContainer,
                InternalRegister(staticContainer, null, null, new MetaObject(typeof(ConcreteType), LifeCycle.Transient, args)));

        RegisterOptions INewDiContainer.Register<ConcreteType>()
            => new RegisterOptions(
                container,
                InternalRegister(container, null, null, new MetaObject(typeof(ConcreteType), LifeCycle.Transient)));


        public static RegisterOptions Register<ConcreteType, ResolvedType>()
            where ConcreteType : notnull, ResolvedType
            => new RegisterOptions(
                staticContainer,
                InternalRegister(staticContainer, typeof(ResolvedType), null, new MetaObject(typeof(ConcreteType), LifeCycle.Singleton)));

        RegisterOptions INewDiContainer.Register<ConcreteType, ResolvedType>()
            => new RegisterOptions(
                container,
                InternalRegister(container, typeof(ResolvedType), null, new MetaObject(typeof(ConcreteType), LifeCycle.Singleton)));
        #endregion
        #region Register with Key

        public static RegisterOptions Register<ConcreteType>(string key, params Type[] args)
            where ConcreteType : notnull
            => new RegisterOptions(
                staticContainer,
                InternalRegister(staticContainer, null, key, new MetaObject(typeof(ConcreteType), LifeCycle.Transient, args)));

        RegisterOptions INewDiContainer.Register<ConcreteType>(string key)
            => new RegisterOptions(
                container,
                InternalRegister(container, null, key, new MetaObject(typeof(ConcreteType), LifeCycle.Transient)));


        public static RegisterOptions Register<ConcreteType, ResolvedType>(string key)
            where ConcreteType : notnull, ResolvedType
            => new RegisterOptions(
                staticContainer,
                InternalRegister(staticContainer, typeof(ResolvedType), key, new MetaObject(typeof(ConcreteType), LifeCycle.Singleton)));

        RegisterOptions INewDiContainer.Register<ConcreteType, ResolvedType>(string key)
            => new RegisterOptions(
                container,
                InternalRegister(container, typeof(ResolvedType), key, new MetaObject(typeof(ConcreteType), LifeCycle.Singleton)));

        #endregion
        #region RegisterInstance
        public static void RegisterInstance<ConcreteType>(ConcreteType instance)
            where ConcreteType : notnull
            => new RegisterOptions(
                staticContainer,
                InternalRegister(staticContainer, null, null, new MetaObject(typeof(ConcreteType), LifeCycle.Transient, () => instance)));

        void INewDiContainer.RegisterInstance<ConcreteType>(ConcreteType instance)
            => new RegisterOptions(
                container,
                InternalRegister(container, null, null, new MetaObject(typeof(ConcreteType), LifeCycle.Transient, () => instance)));


        public static void RegisterInstance<ConcreteType, ResolvedType>(ConcreteType instance)
            where ConcreteType : notnull, ResolvedType
            => new RegisterOptions(
                staticContainer,
                InternalRegister(staticContainer, typeof(ResolvedType), null, new MetaObject(typeof(ConcreteType), LifeCycle.Singleton, () => instance)));

        void INewDiContainer.RegisterInstance<ConcreteType, ResolvedType>(ConcreteType instance)
            => new RegisterOptions(
                container,
                InternalRegister(container, typeof(ResolvedType), null, new MetaObject(typeof(ConcreteType), LifeCycle.Singleton, () => instance)));


        public static void RegisterInstance<ConcreteType>(ConcreteType instance, string key)
            where ConcreteType : notnull
            => new RegisterOptions(
                staticContainer,
                InternalRegister(staticContainer, null, key, new MetaObject(typeof(ConcreteType), LifeCycle.Transient, () => instance)));

        void INewDiContainer.RegisterInstance<ConcreteType>(ConcreteType instance, string key)
            => new RegisterOptions(
                container,
                InternalRegister(container, null, key, new MetaObject(typeof(ConcreteType), LifeCycle.Transient, () => instance)));


        public static void RegisterInstance<ConcreteType, ResolvedType>(ConcreteType instance, string key)
            where ConcreteType : notnull, ResolvedType
            => new RegisterOptions(
                staticContainer,
                InternalRegister(staticContainer, typeof(ResolvedType), key, new MetaObject(typeof(ConcreteType), LifeCycle.Singleton, () => instance)));

        void INewDiContainer.RegisterInstance<ConcreteType, ResolvedType>(ConcreteType instance, string key)
            => new RegisterOptions(
                container,
                InternalRegister(container, typeof(ResolvedType), key, new MetaObject(typeof(ConcreteType), LifeCycle.Singleton, () => instance)));

        #endregion

        #region Register with Delegate
        public static RegisterOptions RegisterInstance<ConcreteType>(Expression<Func<ConcreteType>> instanceDelegate)
            where ConcreteType : notnull
            => new RegisterOptions(
                staticContainer,
                InternalRegister(staticContainer, null, null, new MetaObject(typeof(ConcreteType), LifeCycle.Transient, () => instanceDelegate.Compile().Invoke())));

        RegisterOptions INewDiContainer.RegisterInstance<ConcreteType>(Expression<Func<INewDiContainer, ConcreteType>> instanceDelegate)
            => new RegisterOptions(
                container,
                InternalRegister(container, null, null, new MetaObject(typeof(ConcreteType), LifeCycle.Transient, () => instanceDelegate.Compile().Invoke(this))));


        public static RegisterOptions RegisterInstance<ConcreteType, ResolvedType>(Expression<Func<ConcreteType>> instanceDelegate)
            where ConcreteType : notnull, ResolvedType
            => new RegisterOptions(
                staticContainer,
                InternalRegister(staticContainer, typeof(ResolvedType), null, new MetaObject(typeof(ConcreteType), LifeCycle.Singleton, () => instanceDelegate.Compile().Invoke())));

        RegisterOptions INewDiContainer.RegisterInstance<ConcreteType, ResolvedType>(Expression<Func<INewDiContainer, ConcreteType>> instanceDelegate)
            => new RegisterOptions(
                container,
                InternalRegister(container, typeof(ResolvedType), null, new MetaObject(typeof(ConcreteType), LifeCycle.Singleton, () => instanceDelegate.Compile().Invoke(this))));


        #endregion

        internal static Tuple<Type, string> InternalRegister(
            ConcurrentDictionary<Tuple<Type, string>, MetaObject> container,
            Type resolvedType,
            string key,
            MetaObject metaObject
            )
        {
            if (metaObject.ConcreteType is null)
                throw new ArgumentNullException(nameof(metaObject.ConcreteType));

            var containerKey = new Tuple<Type, string>(
                resolvedType ?? metaObject.ConcreteType, key);

            if (!container.TryAdd(containerKey,metaObject))
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
            => (T)InnerResolve(staticContainer, typeof(T), null);

        T INewDiContainer.Resolve<T>()
            => (T)InnerResolve(container, typeof(T), null);


        public static T Resolve<T>(string key) where T : notnull
            => (T)InnerResolve(staticContainer, typeof(T), key);

        T INewDiContainer.Resolve<T>(string key)
            => (T)InnerResolve(container, typeof(T), key);

        internal static object InnerResolve(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Type resolvedType, string key)
        {
            //todo try https://rogerjohansson.blog/2008/02/28/linq-expressions-creating-objects/
            //if registered
            if (container.TryGetValue(new Tuple<Type, string>(resolvedType, key), out MetaObject metaObject))
            {
                if (metaObject.Instance != null) //Will only be the case if Singleton
                    return metaObject.Instance;

                //object instance;

                //if (metaObject.InstanceDelegate != null)
                //    instance = (object)metaObject.InstanceDelegate.Invoke();
                //else
                    var instance = metaObject.ObjectActivator(ResolveDependencies(container, metaObject).ToArray());

                    //instance = //NonPublic Ctor allowed if specify constructor
                    //    Activator.CreateInstance(
                    //        metaObject.ConcreteType,
                    //        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    //        null,
                    //        ResolveDependencies(container, metaObject).ToArray(),
                    //        CultureInfo.InvariantCulture);

                if (metaObject.LifeCycle == LifeCycle.Singleton)
                    metaObject.Instance = instance; //Cache if singleton

                return instance;
            }

            if (MySettings.ResolveBubblesToStaticContainer
                && container != staticContainer
                && staticContainer.Any())
                return InnerResolve(staticContainer, resolvedType, key);

            if (!MySettings.TryResolveUnregistered)
                throw new ResolveException(
                    $"The type {resolvedType.Name} has not been registered. Either " +
                    $"register the class, or configure {nameof(MySettings)}.");

            if (resolvedType.IsInterface || resolvedType.IsAbstract)
                throw new ResolveException(
                    $"Could not Resolve or Create {resolvedType.Name}" +
                    $". It is not registered in {nameof(NewDiContainer)}. Furthermore, " +
                    $"smart resolve couldn't create an instance.");

            //else try resolve concreteType anyway
            try
            {
                //todo Via MetaObject -> if success then store in dictionary
                //var args = ResolveDependencies(container,
                //        metaObject
                //        ?? new MetaObject(
                //            resolvedType,
                //            LifeCycle.Transient,
                //            null)
                //        ).ToArray();

                //if (args.Any())
                //    return Activator.CreateInstance(
                //        resolvedType,
                //        args);

                //return Activator.CreateInstance(resolvedType);
                var tryMetaObject = new MetaObject(resolvedType, LifeCycle.Transient);
                var instance = tryMetaObject.ObjectActivator(ResolveDependencies(container, tryMetaObject).ToArray());
                //if success then add registration
                container.AddOrUpdate(new Tuple<Type, string>(resolvedType, key), tryMetaObject, (key,oldvalue) =>  tryMetaObject);
                return instance;
            }
            catch (Exception ex)
            {
                if (ex is ResolveException)
                    throw ex;

                throw new ResolveException(
                    $"Could not Resolve or Create {resolvedType.Name}" +
                    $". It is not registered in {nameof(NewDiContainer)}. Furthermore, " +
                    $"smart resolve couldn't create an instance.", ex);
            }
        }

        internal static IEnumerable<object> ResolveDependencies(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, MetaObject metaObject)
        {
            //if (metaObject.ConstructorParameterCache is null)
            //    metaObject.ConstructorParameterCache //cycle through all ctors to choose
            //        = GetConstructorParams(metaObject.ConcreteType);
            if (metaObject.ConstructorParameterCache != null) //null if instanceDelegate was passed
            {
                foreach (var parameter in metaObject.ConstructorParameterCache?.GetParameters())
                {
                    var namedDependencyAttribute = parameter.GetCustomAttribute<ResolveNamedAttribute>();
                    if (namedDependencyAttribute != null)
                        yield return InnerResolve(container, parameter.ParameterType, namedDependencyAttribute.Key);
                    else
                        yield return InnerResolve(container, parameter.ParameterType, null);
                }
            }
        }




        #endregion
        #region Unregister
        public static void Unregister<T>()
            where T : notnull
            => InternalUnregister(staticContainer, typeof(T), null);

        void INewDiContainer.Unregister<T>()
            => InternalUnregister(container, typeof(T), null);


        public static void Unregister<T>(string key)
            where T : notnull
            => InternalUnregister(staticContainer, typeof(T), key);

        void INewDiContainer.Unregister<T>(string key)
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
            => InternalUnregisterAll(staticContainer);

        void INewDiContainer.UnregisterAll()
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

    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ILifeCycleOptions
    {
        void MultiInstance();
        void SingleInstance();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IConstructorOptions
    {
        ILifeCycleOptions UsingConstructor(Type[] args);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class RegisterOptions : ILifeCycleOptions, IConstructorOptions
    {
        readonly ConcurrentDictionary<Tuple<Type, string>, MetaObject> container;
        readonly Tuple<Type, string> key;

        public RegisterOptions(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Tuple<Type, string> key)
        {
            this.container = container;
            this.key = key;
        }

        public ILifeCycleOptions UsingConstructor(params Type[] args)
        {
            var metaObject = container[key];

            try
            {
                var constructor = metaObject.ConcreteType
                    .GetConstructor(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        args,
                        null);

                //Rather throw error on registration
                if (constructor is null)
                    throw new Exception($"No matching constructor found.");

                ////We've done the work, so cache it here
                //metaObject.ConstructorParameterCache = constructor;//.GetParameters();
            }
            catch (Exception ex)
            {
                throw new RegisterException($"Could not register {metaObject.ConcreteType.Name} with specified constructor.", ex);
            }

            return this;
        }

        public void SingleInstance()
        {
            container[key].LifeCycle = LifeCycle.Singleton;
        }

        public void MultiInstance()
        {
            container[key].LifeCycle = LifeCycle.Transient;
        }
    }

}
