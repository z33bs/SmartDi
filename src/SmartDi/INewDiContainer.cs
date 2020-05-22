namespace SmartDi
{
    public interface INewDiContainer
    {
        void Register<ConcreteType>()
            where ConcreteType : new();

        void Register<ConcreteType, ResolvedType>()
            where ConcreteType : ResolvedType, new();

        void Register<ConcreteType>(string key)
            where ConcreteType : new();

        void Register<ConcreteType, ResolvedType>(string key)
            where ConcreteType : ResolvedType, new();

        T Resolve<T>() where T : notnull;
    }
}