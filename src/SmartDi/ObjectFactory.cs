using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SmartDi
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ObjectFactory
    {
        public ObjectFactory(Type concreteType, LifeCycle lifeCycle)
        {
            _concreteType = concreteType;
            LifeCycle = lifeCycle;
        }

        readonly Type _concreteType;

        object _instance;

        internal LifeCycle LifeCycle { get; set; }

        internal object GetConcreteType(params object[] args)
        {
            if (LifeCycle == LifeCycle.Transient || _instance is null)
                _instance = Activator.CreateInstance(_concreteType, args);

            return _instance;
        }
    }
}

