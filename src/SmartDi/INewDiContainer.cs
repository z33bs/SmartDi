using System;
using System.Linq.Expressions;

namespace SmartDi
{
    public interface INewDiContainer
    {
        RegisterOptions Register<ConcreteType>()
            where ConcreteType : notnull;

        RegisterOptions Register<ConcreteType, ResolvedType>()
            where ConcreteType : notnull, ResolvedType;

        RegisterOptions Register<ConcreteType>(string key)
            where ConcreteType : notnull;

        RegisterOptions Register<ConcreteType, ResolvedType>(string key)
            where ConcreteType : notnull, ResolvedType;
        // Delegate

        RegisterOptions RegisterInstance<ConcreteType>(Expression<Func<INewDiContainer, ConcreteType>> instanceDelegate)
            where ConcreteType : notnull;

        RegisterOptions RegisterInstance<ConcreteType, ResolvedType>(Expression<Func<INewDiContainer, ConcreteType>> instanceDelegate)
            where ConcreteType : notnull, ResolvedType;

        //RegisterOptions RegisterInstance<ConcreteType>(Expression<Func<INewDiContainer, ConcreteType>> instanceDelegate, string key)
        //    where ConcreteType : notnull;

        //RegisterOptions RegisterInstance<ConcreteType, ResolvedType>(Expression<Func<INewDiContainer, ConcreteType>> instanceDelegate, string key)
        //    where ConcreteType : notnull, ResolvedType;



        void RegisterInstance<ConcreteType>(ConcreteType instance)
            where ConcreteType : notnull;

        void RegisterInstance<ConcreteType, ResolvedType>(ConcreteType instance)
            where ConcreteType : notnull, ResolvedType;

        void RegisterInstance<ConcreteType>(ConcreteType instance, string key)
            where ConcreteType : notnull;

        void RegisterInstance<ConcreteType, ResolvedType>(ConcreteType instance, string key)
            where ConcreteType : notnull, ResolvedType;

        //todo documentation
        //todo list registrations
        //todo autoregister (with flags like bindingflags) and exclusion like Tiny
        //todo .Static() / Global() overload to move to Static container from local container?
        //todo consolidate to one file
        //todo pass settings to instance version



        T Resolve<T>() where T : notnull;
        T Resolve<T>(string key) where T : notnull;

        void Unregister<T>()
            where T : notnull;

        void Unregister<T>(string key)
            where T : notnull;

        void UnregisterAll();
    }
}