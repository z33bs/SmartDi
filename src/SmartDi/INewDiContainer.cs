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

        void RegisterInstance<ConcreteType>(ConcreteType instance)
            where ConcreteType : notnull;

        void RegisterInstance<ConcreteType, ResolvedType>(ConcreteType instance)
            where ConcreteType : notnull, ResolvedType;

        void RegisterInstance<ConcreteType>(ConcreteType instance, string key)
            where ConcreteType : notnull;

        void RegisterInstance<ConcreteType, ResolvedType>(ConcreteType instance, string key)
            where ConcreteType : notnull, ResolvedType;

        //todo add Register(Func<TConcrete>)
        //todo isRegistered & list resolutions

        T Resolve<T>() where T : notnull;
        T Resolve<T>(string key) where T : notnull;

        void Unregister<T>()
            where T : notnull;

        void Unregister<T>(string key)
            where T : notnull;

        void UnregisterAll();
    }
}