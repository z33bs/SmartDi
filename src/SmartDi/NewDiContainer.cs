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
            

        public static void Register<ConcreteType>()
            where ConcreteType : notnull
            => InternalRegister(staticContainer, null, typeof(ConcreteType), null, LifeCycle.Transient);

        void INewDiContainer.Register<ConcreteType>()
            => InternalRegister(container, null, typeof(ConcreteType), null, LifeCycle.Transient);


        public static void Register<ConcreteType,ResolvedType>()
            where ConcreteType : ResolvedType
            => InternalRegister(staticContainer, typeof(ResolvedType), typeof(ConcreteType), null, LifeCycle.Singleton);

        void INewDiContainer.Register<ConcreteType, ResolvedType>()
            => InternalRegister(container, typeof(ResolvedType), typeof(ConcreteType), null, LifeCycle.Singleton);


        static void InternalRegister(
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
    }
}
