using System;
using System.ComponentModel;
using System.Reflection;

namespace SmartDi
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MetaObject : IDisposable
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

        public ParameterInfo[] ConstructorParameterCache { get; set; }

        public void Dispose()
        {
            if (Instance != null && Instance is IDisposable)
                (Instance as IDisposable).Dispose();
        }
    }
}

