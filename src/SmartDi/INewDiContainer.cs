namespace SmartDi
{
    public interface INewDiContainer
    {
        void Register<ConcreteType>()
            where ConcreteType : notnull;

        void Register<ConcreteType, ResolvedType>()
            where ConcreteType : ResolvedType;

        T Resolve<T>() where T : notnull;
    }
}