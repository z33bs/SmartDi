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

        //todo add keyed overloads for RegisterInstance
        //todo add Register(Func<TConcrete>)
        //todo isRegistered & list resolutions
        //todo destructive stuff running dispose on Instances

        T Resolve<T>() where T : notnull;
    }
}