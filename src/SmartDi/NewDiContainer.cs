using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

// Simple
// No key changes
// Static / instace duplication
[assembly: InternalsVisibleTo("SmartDiTests")]
namespace SmartDi
{
    public class NewDiContainer : INewDiContainer
    {
        public static bool IsStrictMode { get; set; }

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

        #region Register
        public static void Register<ConcreteType>()
            where ConcreteType : new()
            => InternalRegister(staticContainer, null, typeof(ConcreteType), null, LifeCycle.Transient);

        void INewDiContainer.Register<ConcreteType>()
            => InternalRegister(container, null, typeof(ConcreteType), null, LifeCycle.Transient);


        public static void Register<ConcreteType,ResolvedType>()
            where ConcreteType : ResolvedType, new()
            => InternalRegister(staticContainer, typeof(ResolvedType), typeof(ConcreteType), null, LifeCycle.Singleton);

        void INewDiContainer.Register<ConcreteType, ResolvedType>()
            => InternalRegister(container, typeof(ResolvedType), typeof(ConcreteType), null, LifeCycle.Singleton);

        #region with Key

        public static void Register<ConcreteType>(string key)
            where ConcreteType : new()
            => InternalRegister(staticContainer, null, typeof(ConcreteType), key, LifeCycle.Transient);

        void INewDiContainer.Register<ConcreteType>(string key)
            => InternalRegister(container, null, typeof(ConcreteType), key, LifeCycle.Transient);


        public static void Register<ConcreteType, ResolvedType>(string key)
            where ConcreteType : ResolvedType, new()
            => InternalRegister(staticContainer, typeof(ResolvedType), typeof(ConcreteType), key, LifeCycle.Singleton);

        void INewDiContainer.Register<ConcreteType, ResolvedType>(string key)
            => InternalRegister(container, typeof(ResolvedType), typeof(ConcreteType), key, LifeCycle.Singleton);

        #endregion

        internal static void InternalRegister(
            ConcurrentDictionary<Tuple<Type, string>, MetaObject> container,
            Type resolvedType,
            Type concreteType,
            string key,
            LifeCycle lifeCycle
            )
        {
            if (concreteType is null)
                throw new ArgumentNullException(nameof(concreteType));

            var containerKey = new Tuple<Type, string>(
                resolvedType ?? concreteType, key);

            if (!container.TryAdd(
                containerKey,
                new MetaObject(concreteType, lifeCycle)))
            {
                var builder = new StringBuilder();
                    builder.Append($"{nameof(containerKey.Item1)} is already registered");
                if(containerKey.Item2 != null)
                    builder.Append($" with key '{nameof(containerKey.Item2)}'");
                builder.Append(".");
                throw new RegistrationException(builder.ToString());
            }
        }
        #endregion

        #region Resolve

        public static T Resolve<T>() where T : notnull
            => (T)InnerResolve(typeof(T), null);

        T INewDiContainer.Resolve<T>()
            => (T)InnerResolve(typeof(T), null);

        internal static object InnerResolve(Type resolvedType, string key)
        {
            //if registered
            if (staticContainer.TryGetValue(new Tuple<Type, string>(resolvedType, key), out MetaObject metaObject))
            {
                if (metaObject.Instance != null)
                    return metaObject.Instance;
                else
                {
                    var instance =
                        Activator.CreateInstance(
                            metaObject.ConcreteType,
                            ResolveDependencies(metaObject.ConcreteType).ToArray());

                    if (metaObject.LifeCycle == LifeCycle.Singleton)
                        metaObject.Instance = instance;

                    return instance;
                }
            }

            if (IsStrictMode)
                throw new TypeNotRegisteredException(
                    $"The type {resolvedType.Name} has not been registered. Either " +
                    $"register the class, or configure {nameof(IsStrictMode)}.");

            if(resolvedType.IsInterface || resolvedType.IsAbstract)
                throw new TypeNotRegisteredException(
                    $"Could not Resolve or Create {resolvedType.Name}" +
                    $". It is not registered in {nameof(DiContainer)}. Furthermore, " +
                    $"smart resolve couldn't create an instance.");

            //else try resolve concreteType anyway
            try
            {
                var args = ResolveDependencies(resolvedType).ToArray();

                if(args.Any())
                    return Activator.CreateInstance(
                        resolvedType,
                        args);

                return Activator.CreateInstance(resolvedType);
            }
            catch (Exception ex)
            {
                if (ex is TypeNotRegisteredException)
                    throw ex;

                throw new TypeNotRegisteredException(
                    $"Could not Resolve or Create {resolvedType.Name}" +
                    $". It is not registered in {nameof(DiContainer)}. Furthermore, " +
                    $"smart resolve couldn't create an instance.", ex);
            }
        }

        internal static IEnumerable<object> ResolveDependencies(Type resolvedType)
        {
            //todo MetaObject specified?

            ParameterInfo[] parameters = GetConstructorParams(resolvedType);

            foreach (var parameter in parameters)
            {
                var namedDependencyAttribute = parameter.GetCustomAttribute<ResolveNamedAttribute>();
                if (namedDependencyAttribute != null)
                    yield return InnerResolve(parameter.ParameterType, namedDependencyAttribute.Key);
                else
                    yield return InnerResolve(parameter.ParameterType, null);
            }
        }

        internal static ParameterInfo[] GetConstructorParams(Type resolvedType)
        {
            var constructors = resolvedType.
                    GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                    .ToList();

            if (constructors.Count > 1)
            {
                //if flagged, shorten to only flagged constructors
                var flaggedConstructors = constructors
                    .Where(c => c.GetCustomAttribute<ResolveUsingAttribute>() != null)
                    .ToList();

                //todo if strict mode throw if more than one
                if (flaggedConstructors.Any())
                    constructors = flaggedConstructors;

                return constructors
                    .Aggregate((i, j)
                        => i.GetParameters().Count() > j.GetParameters().Count()
                        ? i
                        : j)
                    .GetParameters();
            }

            return constructors[0].GetParameters();
        }

        internal static MetaObject GetMetaObject(Type resolvedType,string key)
        {
            if (staticContainer.TryGetValue(new Tuple<Type, string>(resolvedType, key), out MetaObject value))
                return value;

            return null;
        }

        internal static object GetConstructorParameters()
        {
            return new object();
        }

        internal static object[] GetDependencies()
        {
            object v = new object();
            return v as object[];
        }

        internal static object TryCreateObject()
        {
            return new object();
        }
        #endregion
    }
}
