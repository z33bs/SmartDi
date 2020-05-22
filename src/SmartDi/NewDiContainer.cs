﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

// todo BubbleToStatic
// todo UsingConstructor returns restricted API
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

        #region Registration
        #region Register
        public static RegisterOptions Register<ConcreteType>()
            where ConcreteType : new()
            => new RegisterOptions(staticContainer, InternalRegister(staticContainer, null, typeof(ConcreteType), null, LifeCycle.Transient));

        RegisterOptions INewDiContainer.Register<ConcreteType>()
            => new RegisterOptions(container, InternalRegister(container, null, typeof(ConcreteType), null, LifeCycle.Transient));


        public static RegisterOptions Register<ConcreteType, ResolvedType>()
            where ConcreteType : ResolvedType, new()
            => new RegisterOptions(staticContainer, InternalRegister(staticContainer, typeof(ResolvedType), typeof(ConcreteType), null, LifeCycle.Singleton));

        RegisterOptions INewDiContainer.Register<ConcreteType, ResolvedType>()
            => new RegisterOptions(container, InternalRegister(container, typeof(ResolvedType), typeof(ConcreteType), null, LifeCycle.Singleton));
        #endregion
        #region Register with Key

        public static RegisterOptions Register<ConcreteType>(string key)
            where ConcreteType : new()
            => new RegisterOptions(staticContainer, InternalRegister(staticContainer, null, typeof(ConcreteType), key, LifeCycle.Transient));

        RegisterOptions INewDiContainer.Register<ConcreteType>(string key)
            => new RegisterOptions(container, InternalRegister(container, null, typeof(ConcreteType), key, LifeCycle.Transient));


        public static RegisterOptions Register<ConcreteType, ResolvedType>(string key)
            where ConcreteType : ResolvedType, new()
            => new RegisterOptions(staticContainer, InternalRegister(staticContainer, typeof(ResolvedType), typeof(ConcreteType), key, LifeCycle.Singleton));

        RegisterOptions INewDiContainer.Register<ConcreteType, ResolvedType>(string key)
            => new RegisterOptions(container, InternalRegister(container, typeof(ResolvedType), typeof(ConcreteType), key, LifeCycle.Singleton));

        #endregion
        #region RegisterInstance
        public static void RegisterInstance<ConcreteType>(ConcreteType instance)
            where ConcreteType : new()
            => InternalRegister(staticContainer, typeof(ConcreteType), typeof(ConcreteType), null, LifeCycle.Singleton, instance);

        void INewDiContainer.RegisterInstance<ConcreteType>(ConcreteType instance)
            => InternalRegister(container, typeof(ConcreteType), typeof(ConcreteType), null, LifeCycle.Singleton, instance);


        public static void RegisterInstance<ConcreteType, ResolvedType>(ConcreteType instance)
            where ConcreteType : ResolvedType, new()
            => InternalRegister(staticContainer, typeof(ResolvedType), typeof(ConcreteType), null, LifeCycle.Singleton, instance);

        void INewDiContainer.RegisterInstance<ConcreteType, ResolvedType>(ConcreteType instance)
            => InternalRegister(container, typeof(ResolvedType), typeof(ConcreteType), null, LifeCycle.Singleton, instance);

        #endregion

        internal static Tuple<Type, string> InternalRegister(
            ConcurrentDictionary<Tuple<Type, string>, MetaObject> container,
            Type resolvedType,
            Type concreteType,
            string key,
            LifeCycle lifeCycle,
            object instance = null
            )
        {
            if (concreteType is null)
                throw new ArgumentNullException(nameof(concreteType));

            var containerKey = new Tuple<Type, string>(
                resolvedType ?? concreteType, key);

            if (!container.TryAdd(
                containerKey,
                new MetaObject(concreteType, lifeCycle, instance)))
            {
                var builder = new StringBuilder();
                builder.Append($"{nameof(containerKey.Item1)} is already registered");
                if (containerKey.Item2 != null)
                    builder.Append($" with key '{nameof(containerKey.Item2)}'");
                builder.Append(".");
                throw new RegistrationException(builder.ToString());
            }

            return containerKey;
        }
        #endregion

        #region Resolve

        public static T Resolve<T>() where T : notnull
            => (T)InnerResolve(staticContainer, typeof(T), null);

        T INewDiContainer.Resolve<T>()
            => (T)InnerResolve(container, typeof(T), null);

        internal static object InnerResolve(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Type resolvedType, string key)
        {
            //if registered
            if (container.TryGetValue(new Tuple<Type, string>(resolvedType, key), out MetaObject metaObject))
            {
                if (metaObject.Instance != null)
                    return metaObject.Instance;
                else
                {
                    //NonPublic allowed if specify constructor
                    var instance =
                        Activator.CreateInstance(
                            metaObject.ConcreteType,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                            null,
                            ResolveDependencies(container, metaObject).ToArray(),
                            CultureInfo.InvariantCulture);

                    if (metaObject.LifeCycle == LifeCycle.Singleton)
                        metaObject.Instance = instance;

                    return instance;
                }
            }

            if (IsStrictMode)
                throw new TypeNotRegisteredException(
                    $"The type {resolvedType.Name} has not been registered. Either " +
                    $"register the class, or configure {nameof(IsStrictMode)}.");

            if (resolvedType.IsInterface || resolvedType.IsAbstract)
                throw new TypeNotRegisteredException(
                    $"Could not Resolve or Create {resolvedType.Name}" +
                    $". It is not registered in {nameof(NewDiContainer)}. Furthermore, " +
                    $"smart resolve couldn't create an instance.");

            //else try resolve concreteType anyway
            try
            {
                var args = ResolveDependencies(container,
                        metaObject
                        ?? new MetaObject(
                            resolvedType,
                            LifeCycle.Transient,
                            null)
                        ).ToArray();

                if (args.Any())
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
                    $". It is not registered in {nameof(NewDiContainer)}. Furthermore, " +
                    $"smart resolve couldn't create an instance.", ex);
            }
        }

        internal static IEnumerable<object> ResolveDependencies(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, MetaObject metaObject)
        {
            //If specify constructor then allow NonPublic
            ParameterInfo[] parameters
                = metaObject.ConstructorSignature != null
                ? metaObject
                    .ConcreteType
                    .GetConstructor(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        metaObject.ConstructorSignature,
                        null)
                    .GetParameters()
                : GetConstructorParams(metaObject.ConcreteType);

            foreach (var parameter in parameters)
            {
                var namedDependencyAttribute = parameter.GetCustomAttribute<ResolveNamedAttribute>();
                if (namedDependencyAttribute != null)
                    yield return InnerResolve(container, parameter.ParameterType, namedDependencyAttribute.Key);
                else
                    yield return InnerResolve(container, parameter.ParameterType, null);
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

        #endregion

    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class RegisterOptions
    {
        readonly ConcurrentDictionary<Tuple<Type, string>, MetaObject> container;
        readonly Tuple<Type, string> key;

        public RegisterOptions(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Tuple<Type, string> key)
        {
            this.container = container;
            this.key = key;
        }

        public RegisterOptions UsingConstructor(Type[] args)
        {
            var metaObject = container[key];

            try
            {
                //Rather throw error on registration
                if(metaObject.ConcreteType.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    args,
                    null) is null)
                        throw new RegistrationException("Could not register with specified constructor.");
            }
            catch(Exception ex)
            {
                throw new RegistrationException("Could not register with specified constructor.", ex);
            }

            metaObject.ConstructorSignature = args;
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
