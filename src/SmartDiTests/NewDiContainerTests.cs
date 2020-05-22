using System;
using Xunit;
using Moq;
using SmartDi;
using System.Collections.Concurrent;
using System.Linq;

namespace SmartDiTests
{
    public class NewDiContainerTests
    {
        interface IService { }
        class MyService : IService { }
        class ConcreteOnly { }
        class ClassWith3Ctors
        {
            public IService Service { get; }
            public ConcreteOnly Concrete { get; }

            public ClassWith3Ctors() { }
            public ClassWith3Ctors(IService service)
            { this.Service = service; }
            internal ClassWith3Ctors(IService service, ConcreteOnly concrete)
            { this.Service = service; this.Concrete = concrete; }
        }

        class ClassWithFlaggedCtor
        {
            public IService Service { get; }
            public ConcreteOnly Concrete { get; }

            [ResolveUsing]
            public ClassWithFlaggedCtor(IService service)
            { this.Service = service; }

            public ClassWithFlaggedCtor(IService service, ConcreteOnly concrete)
            { this.Service = service; this.Concrete = concrete; }
        }

        class ClassThatsResolvableWithoutRegistering
        {
            public ConcreteOnly Concrete { get; }
            public ClassThatsResolvableWithoutRegistering(ConcreteOnly concrete)
            {
                this.Concrete = concrete;
            }
        }

        [Fact]
        public void Constructor()
        {
            Assert.IsAssignableFrom<INewDiContainer>(new NewDiContainer());
        }

        #region Registration
        #region Register<ConcreteType>()
        [Fact]
        public void StaticRegisterConcreteType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer.Register<MyService>();

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));
                
            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterConcreteType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            INewDiContainer container = new NewDiContainer(mock);

            container.Register<MyService>();

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));
        }

        [Fact]
        public void StaticRegisterConcreteType_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer.Register<MyService>();

            Assert.Equal(LifeCycle.Transient,mock[new Tuple<Type, string>(typeof(MyService), null)].LifeCycle);

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterConcreteType_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            INewDiContainer container = new NewDiContainer(mock);

            container.Register<MyService>();

            Assert.Equal(LifeCycle.Transient, mock[new Tuple<Type, string>(typeof(MyService), null)].LifeCycle);
        }
        #endregion
        #region Register<ConcreteType,ResolvedType>()
        [Fact]
        public void StaticRegisterResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer.Register<MyService,IService>();

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            INewDiContainer container = new NewDiContainer(mock);

            container.Register<MyService,IService>();

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));
        }

        [Fact]
        public void StaticRegisterResolvedType_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer.Register<MyService,IService>();

            Assert.Equal(LifeCycle.Singleton, mock[new Tuple<Type, string>(typeof(IService), null)].LifeCycle);

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterResolvedType_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            INewDiContainer container = new NewDiContainer(mock);

            container.Register<MyService,IService>();

            Assert.Equal(LifeCycle.Singleton, mock[new Tuple<Type, string>(typeof(IService), null)].LifeCycle);
        }
        #endregion
        #endregion

        #region Resolve
        #region Internal

        [Fact]
        public void GetConstructorParams_gt1Constructor_ReturnsFromGreediestPublic()
        {
            //ClassWith3Ctors has 3 constructors, two public () & (IService)
            //and one internal (IService, ConcreteOnly)

            //Expect to ignore internal, but take greediest public ctor
            var exepectedParamters
                = typeof(ClassWith3Ctors)
                    .GetConstructor(new Type[] { typeof(IService) }).GetParameters();

            var parameters
                = NewDiContainer.
                    GetConstructorParams(typeof(ClassWith3Ctors));

            Assert.Equal(exepectedParamters, parameters);
        }

        [Fact]
        public void GetConstructorParams_gt1Ctor_FlaggedCtor_ReturnsFlaggedNotGreediest()
        {
            //ClassWithFlaggedCtor has 2 public constructors
            //(IService) - Flagged
            //(IService, ConcreteOnly)

            //Expect to pick flagged ctor
            var exepectedParamters
                = typeof(ClassWithFlaggedCtor)
                    .GetConstructor(new Type[] { typeof(IService) }).GetParameters();

            var parameters
                = NewDiContainer.
                    GetConstructorParams(typeof(ClassWithFlaggedCtor));

            Assert.Equal(exepectedParamters, parameters);
        }

        #endregion

        [Fact]
        public void StaticResolve_Unregistered_Works()
        {
            var obj = NewDiContainer.Resolve<ClassThatsResolvableWithoutRegistering>();

            Assert.IsType<ClassThatsResolvableWithoutRegistering>(obj);

        }

        #endregion

        #region Exceptions
        [Fact]
        public void StaticRegisterConcreteType_Duplicate_ThrowsRegistrationException()
        {
            NewDiContainer.Register<MyService>();
            Assert.Throws<RegistrationException>(() => NewDiContainer.Register<MyService>());
        }

        #endregion

    }
}
