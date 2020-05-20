using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text;

// Simple
// No key changes
// Static / instace duplication
namespace SmartDi
{
    public class NewDiContainer : INewDiContainer
    {
        public NewDiContainer()
        {
            container = new ConcurrentDictionary<Tuple<Type, string>, ObjectFactory>();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public NewDiContainer(ConcurrentDictionary<Tuple<Type, string>, ObjectFactory> container)
        {
            this.container = container;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetContainer(ConcurrentDictionary<Tuple<Type, string>, ObjectFactory> container)
            => staticContainer = container;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void ResetContainer()
        {
            staticContainer = new ConcurrentDictionary<Tuple<Type, string>, ObjectFactory>();
        }

        static ConcurrentDictionary<Tuple<Type, string>, ObjectFactory> staticContainer
            = new ConcurrentDictionary<Tuple<Type, string>, ObjectFactory>();

        readonly ConcurrentDictionary<Tuple<Type, string>, ObjectFactory> container;
            

        public static void Register<ConcreteType>(LifeCycle lifeCycle = LifeCycle.Transient)
            where ConcreteType : notnull
            => InternalRegister(staticContainer, null, typeof(ConcreteType), null, lifeCycle);
        void INewDiContainer.Register<ConcreteType>(LifeCycle lifeCycle)
            => InternalRegister(container, null, typeof(ConcreteType), null, lifeCycle);

        public static void Register<ConcreteType,ResolvedType>(LifeCycle lifeCycle = LifeCycle.Singleton)
            where ConcreteType : ResolvedType
            => InternalRegister(staticContainer, typeof(ResolvedType), typeof(ConcreteType), null, lifeCycle);
        void INewDiContainer.Register<ConcreteType, ResolvedType>(LifeCycle lifeCycle)
            => InternalRegister(container, typeof(ResolvedType), typeof(ConcreteType), null, lifeCycle);

        static void InternalRegister(
            ConcurrentDictionary<Tuple<Type, string>, ObjectFactory> container,
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
                new ObjectFactory(concreteType, lifeCycle)))
            {
                var builder = new StringBuilder();
                    builder.Append($"{nameof(containerKey.Item1)} is already registered");
                if(containerKey.Item2 != null)
                    builder.Append($" with key '{nameof(containerKey.Item2)}'");
                builder.Append(".");
                throw new RegistrationException(builder.ToString());
            }
        }
    }
}
