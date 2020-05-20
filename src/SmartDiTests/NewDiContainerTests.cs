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
        public void StaticRegister()
        {
            var mock = new Mock<ConcurrentDictionary<Tuple<Type, string>, ObjectFactory>>();
            NewDiContainer.SetContainer(mock.Object);

            NewDiContainer.Register<MyService>();

            mock.Verify(c => c.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));

            NewDiContainer.ResetContainer();
        }

    }
}
