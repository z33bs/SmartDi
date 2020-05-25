using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace SmartDi
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MetaObject : IDisposable
    {
        public MetaObject(Type concreteType, LifeCycle lifeCycle, Lazy<object> instance)
        {
            ConcreteType = concreteType;
            LifeCycle = lifeCycle;
            Instance = instance;
        }

        public Type ConcreteType { get; }

        public Lazy<object> Instance { get; set; }

        public LifeCycle LifeCycle { get; set; }

        public ParameterInfo[] ConstructorParameterCache { get; set; }

        public void Dispose()
        {
            if (Instance != null
                && Instance.Value is IDisposable disposable)
                disposable.Dispose();
        }
    }
}

