using System;
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

        RegisterOptions INewDiContainer.Register<ConcreteType>()
            => (this as INewDiContainer).Register<ConcreteType>(Type.EmptyTypes);

        public static RegisterOptions Register<ConcreteType>(params Type[] constructorParameters)
            where ConcreteType : notnull
            => new RegisterOptions(
                staticContainer,
                InternalRegister(staticContainer, null, null, new MetaObject(typeof(ConcreteType), LifeCycle.Transient, constructorParameters)));

        RegisterOptions INewDiContainer.Register<ConcreteType>(params Type[] constructorParameters)
            => new RegisterOptions(
                container,
                InternalRegister(container, null, null, new MetaObject(typeof(ConcreteType), LifeCycle.Transient, constructorParameters)));



        public static RegisterOptions Register<ConcreteType, ResolvedType>()
            where ConcreteType : notnull, ResolvedType
            => Register<ConcreteType, ResolvedType>(Type.EmptyTypes);

        RegisterOptions INewDiContainer.Register<ConcreteType, ResolvedType>()
            => (this as INewDiContainer).Register<ConcreteType, ResolvedType>(Type.EmptyTypes);

        public static RegisterOptions Register<ConcreteType, ResolvedType>(params Type[] constructorParameters)
            where ConcreteType : notnull, ResolvedType
            => new RegisterOptions(
                staticContainer,
                InternalRegister(staticContainer, typeof(ResolvedType), null, new MetaObject(typeof(ConcreteType), LifeCycle.Singleton, constructorParameters)));

        RegisterOptions INewDiContainer.Register<ConcreteType, ResolvedType>(params Type[] constructorParameters)
            => new RegisterOptions(
                container,
                InternalRegister(container, typeof(ResolvedType), null, new MetaObject(typeof(ConcreteType), LifeCycle.Singleton, constructorParameters)));



        #endregion
        #region Register with Key

        public static RegisterOptions Register<ConcreteType>(string key)
            where ConcreteType : notnull
            => Register<ConcreteType>(key, Type.EmptyTypes);

        RegisterOptions INewDiContainer.Register<ConcreteType>(string key)
            => (this as INewDiContainer).Register<ConcreteType>(key, Type.EmptyTypes);

        public static RegisterOptions Register<ConcreteType>(string key, params Type[] constructorParameters)
            where ConcreteType : notnull
            => new RegisterOptions(
                staticContainer,
                InternalRegister(staticContainer, null, key, new MetaObject(typeof(ConcreteType), LifeCycle.Transient, constructorParameters)));

        RegisterOptions INewDiContainer.Register<ConcreteType>(string key, params Type[] constructorParameters)
            => new RegisterOptions(
                container,
                InternalRegister(container, null, key, new MetaObject(typeof(ConcreteType), LifeCycle.Transient, constructorParameters)));



        public static RegisterOptions Register<ConcreteType, ResolvedType>(string key)
            where ConcreteType : notnull, ResolvedType
            => Register<ConcreteType, ResolvedType>(key, Type.EmptyTypes);

        RegisterOptions INewDiContainer.Register<ConcreteType, ResolvedType>(string key)
            => (this as INewDiContainer).Register<ConcreteType, ResolvedType>(key, Type.EmptyTypes);

        public static RegisterOptions Register<ConcreteType, ResolvedType>(string key, params Type[] constructorParameters)
            where ConcreteType : notnull, ResolvedType
            => new RegisterOptions(
                staticContainer,
                InternalRegister(staticContainer, typeof(ResolvedType), key, new MetaObject(typeof(ConcreteType), LifeCycle.Singleton, constructorParameters)));

        RegisterOptions INewDiContainer.Register<ConcreteType, ResolvedType>(string key, params Type[] constructorParameters)
            => new RegisterOptions(
                container,
                InternalRegister(container, typeof(ResolvedType), key, new MetaObject(typeof(ConcreteType), LifeCycle.Singleton, constructorParameters)));



        #endregion

        #region RegisterExpression

        public static RegisterOptions RegisterExpression<ResolvedType>(Expression<Func<ResolvedType>> instanceDelegate)
            where ResolvedType : notnull
            => new RegisterOptions(
                staticContainer,
                InternalRegister(staticContainer, null, null, new MetaObject(typeof(ResolvedType), LifeCycle.Transient, () => instanceDelegate.Compile().Invoke())));

        RegisterOptions INewDiContainer.RegisterExpression<ResolvedType>(Expression<Func<INewDiContainer, ResolvedType>> instanceDelegate)
            => new RegisterOptions(
                container,
                InternalRegister(container, null, null, new MetaObject(typeof(ResolvedType), LifeCycle.Transient, () => instanceDelegate.Compile().Invoke(this))));



        public static RegisterOptions RegisterExpression<ResolvedType>(Expression<Func<ResolvedType>> instanceDelegate, string key)
            where ResolvedType : notnull
            => new RegisterOptions(
                staticContainer,
                InternalRegister(staticContainer, null, key, new MetaObject(typeof(ResolvedType), LifeCycle.Transient, () => instanceDelegate.Compile().Invoke())));

        RegisterOptions INewDiContainer.RegisterExpression<ResolvedType>(Expression<Func<INewDiContainer, ResolvedType>> instanceDelegate, string key)
            => new RegisterOptions(
                container,
                InternalRegister(container, null, key, new MetaObject(typeof(ResolvedType), LifeCycle.Transient, () => instanceDelegate.Compile().Invoke(this))));

        #endregion

        #region Register Instance

        public static void RegisterInstance(object instance)
            => new RegisterOptions(
                staticContainer
                , InternalRegister(staticContainer, null, null, new MetaObject(instance)));

        void INewDiContainer.RegisterInstance(object instance)
            => new RegisterOptions(
                container
                , InternalRegister(container, null, null, new MetaObject(instance)));



        public static void RegisterInstance<ResolvedType>(object instance)
            where ResolvedType : notnull
            => new RegisterOptions(
                staticContainer
                , InternalRegister(staticContainer, typeof(ResolvedType), null, new MetaObject(instance)));

        void INewDiContainer.RegisterInstance<ResolvedType>(object instance)
            => new RegisterOptions(
                container
                , InternalRegister(container, typeof(ResolvedType), null, new MetaObject(instance)));



        public static void RegisterInstance(object instance, string key)
            => new RegisterOptions(
                staticContainer
                , InternalRegister(staticContainer, null, key, new MetaObject(instance)));

        void INewDiContainer.RegisterInstance(object instance, string key)
            => new RegisterOptions(
                container
                , InternalRegister(container, null, key, new MetaObject(instance)));



        public static void RegisterInstance<ResolvedType>(object instance, string key)
            where ResolvedType : notnull
            => new RegisterOptions(
                staticContainer
                , InternalRegister(staticContainer, typeof(ResolvedType), key, new MetaObject(instance)));

        void INewDiContainer.RegisterInstance<ResolvedType>(object instance, string key)
            => new RegisterOptions(
                container
                , InternalRegister(container, typeof(ResolvedType), key, new MetaObject(instance)));



        #endregion

        #region Register Type
        public static RegisterOptions RegisterType(Type concreteType, Type resolvedType = null, string key = null, params Type[] constructorParameters)
            => new RegisterOptions(
                staticContainer,
                InternalRegister(staticContainer, resolvedType, key,
                    new MetaObject(
                        concreteType,
                        resolvedType == null ? LifeCycle.Transient : LifeCycle.Singleton,
                        constructorParameters)));

        RegisterOptions INewDiContainer.RegisterType(Type concreteType, Type resolvedType, string key, params Type[] constructorParameters)
            => new RegisterOptions(
                container,
                InternalRegister(container, resolvedType, key,
                    new MetaObject(
                        concreteType,
                        resolvedType == null ? LifeCycle.Transient : LifeCycle.Singleton,
                        constructorParameters)));


        #endregion

        internal static Tuple<Type, string> InternalRegister(
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
            => (T)InternalResolve(staticContainer, typeof(T), null);

        T INewDiContainer.Resolve<T>()
            => (T)InternalResolve(container, typeof(T), null);


        public static T Resolve<T>(string key) where T : notnull
            => (T)InternalResolve(staticContainer, typeof(T), key);

        T INewDiContainer.Resolve<T>(string key)
            => (T)InternalResolve(container, typeof(T), key);

        public static object Resolve(Type type)
            => InternalResolve(staticContainer, type, null);

        object INewDiContainer.Resolve(Type type)
            => InternalResolve(container, type, null);

        public static object Resolve(Type type, string key)
            => InternalResolve(staticContainer, type, key);

        object INewDiContainer.Resolve(Type type, string key)
            => InternalResolve(container, type, key);

        internal static object InternalResolve(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Type resolvedType, string key)
        {
            //if registered
            if (container.TryGetValue(new Tuple<Type, string>(resolvedType, key), out MetaObject metaObject))
            {
                if (metaObject.Instance != null) //Will only be the case if Singleton
                    return metaObject.Instance;

                var instance = metaObject.ObjectActivator(ResolveDependencies(container, metaObject).ToArray());

                if (metaObject.LifeCycle == LifeCycle.Singleton)
                    metaObject.Instance = instance; //Cache if singleton

                return instance;
            }

            if (MySettings.ResolveBubblesToStaticContainer
                && container != staticContainer
                && staticContainer.Any())
                return InternalResolve(staticContainer, resolvedType, key);

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
                    $". It is not registered in {nameof(NewDiContainer)}. Furthermore, " +
                    $"smart resolve couldn't create an instance.", ex);
            }
        }

        internal static IEnumerable<object> ResolveDependencies(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, MetaObject metaObject)
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

        public void MultiInstance()
        {
            container[key].LifeCycle = LifeCycle.Transient;
        }
    }

}
