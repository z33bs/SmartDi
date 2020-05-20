namespace SmartDi
{
    public interface INewDiContainer
    {
        void Register<ConcreteType>(LifeCycle lifeCycle = LifeCycle.Transient)
            where ConcreteType : notnull;

        void Register<ConcreteType, ResolvedType>(LifeCycle lifeCycle = LifeCycle.Singleton)
            where ConcreteType : ResolvedType;
    }
}