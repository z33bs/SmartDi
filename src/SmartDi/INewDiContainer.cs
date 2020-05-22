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

        //todo add keyed overloads for RegisterInstance
        //todo add Register(Func<TConcrete>)
        //todo isRegistered & list resolutions
        //todo destructive stuff running dispose on Instances

        T Resolve<T>() where T : notnull;
    }
}