using System;
using Xunit;
using Moq;
using SmartDi;
using System.Collections.Concurrent;
    
namespace SmartDiTests
{
    public class NewDiContainerTests
    {
        interface IService { }
        class MyService : IService { }
        class ConcreteOnly { }

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
            var mock = new ConcurrentDictionary<Tuple<Type, string>, ObjectFactory>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer.Register<MyService>();

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));
                
            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterConcreteType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, ObjectFactory>();
            INewDiContainer container = new NewDiContainer(mock);

            container.Register<MyService>();

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));
        }

        [Fact]
        public void StaticRegisterConcreteType_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, ObjectFactory>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer.Register<MyService>();

            Assert.Equal(LifeCycle.Transient,mock[new Tuple<Type, string>(typeof(MyService), null)].LifeCycle);

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterConcreteType_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, ObjectFactory>();
            INewDiContainer container = new NewDiContainer(mock);

            container.Register<MyService>();

            Assert.Equal(LifeCycle.Transient, mock[new Tuple<Type, string>(typeof(MyService), null)].LifeCycle);
        }
        #endregion
        #region Register<ConcreteType,ResolvedType>()
        [Fact]
        public void StaticRegisterResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, ObjectFactory>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer.Register<MyService,IService>();

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, ObjectFactory>();
            INewDiContainer container = new NewDiContainer(mock);

            container.Register<MyService,IService>();

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));
        }

        [Fact]
        public void StaticRegisterResolvedType_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, ObjectFactory>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer.Register<MyService,IService>();

            Assert.Equal(LifeCycle.Singleton, mock[new Tuple<Type, string>(typeof(IService), null)].LifeCycle);

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterResolvedType_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, ObjectFactory>();
            INewDiContainer container = new NewDiContainer(mock);

            container.Register<MyService,IService>();

            Assert.Equal(LifeCycle.Singleton, mock[new Tuple<Type, string>(typeof(IService), null)].LifeCycle);
        }
        #endregion
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
