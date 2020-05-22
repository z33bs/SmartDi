using System;
using System.ComponentModel;

namespace SmartDi
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MetaObject
    {
        public MetaObject(Type concreteType, LifeCycle lifeCycle, object instance)
        {
            ConcreteType = concreteType;
            LifeCycle = lifeCycle;
            Instance = instance;
        }

        public Type ConcreteType { get; }

        public object Instance { get; set; }

        public LifeCycle LifeCycle { get; set; }

        public Type[] ConstructorSignature { get; set; }
    }
}

