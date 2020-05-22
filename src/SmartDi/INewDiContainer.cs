namespace SmartDi
{
    public interface INewDiContainer
    {
        RegisterOptions Register<ConcreteType>()
            where ConcreteType : new();

        RegisterOptions Register<ConcreteType, ResolvedType>()
            where ConcreteType : ResolvedType, new();

        RegisterOptions Register<ConcreteType>(string key)
            where ConcreteType : new();

        RegisterOptions Register<ConcreteType, ResolvedType>(string key)
            where ConcreteType : ResolvedType, new();

        void RegisterInstance<ConcreteType>(ConcreteType instance)
            where ConcreteType : new();

        void RegisterInstance<ConcreteType, ResolvedType>(ConcreteType instance)
            where ConcreteType : ResolvedType, new();

        T Resolve<T>() where T : notnull;
    }
}