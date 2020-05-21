using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SmartDi
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MetaObject
    {
        public MetaObject(Type concreteType, LifeCycle lifeCycle)
        {
            ConcreteType = concreteType;
            LifeCycle = lifeCycle;
        }

        public Type ConcreteType { get; }

        public object Instance { get; set; }

        public LifeCycle LifeCycle { get; set; }

        public object GetConcreteType(params object[] args)
        {
            if (LifeCycle == LifeCycle.Transient || Instance is null)
                Instance = Activator.CreateInstance(ConcreteType, args);

            return Instance;
        }
    }
}

