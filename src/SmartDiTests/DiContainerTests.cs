using System;
using Xunit;
using Moq;
using SmartDi;


namespace SmartDiTests
{
    public class DiContainerTests
    {
        interface IRandom { }
        interface ITestClass { int Instances { get; } }
        class TestClass : ITestClass
        {
            public TestClass()
            {
                _instances++;
            }
            static int _instances;
            public int Instances => _instances;
            public static void ResetInstanceCount() => _instances = 0;
        }

        #region Usage, NOT Unit Tests
        [Fact]
        public void RegisterT_IsRegistered()
        {
            var container = new DiContainer();
            container.Register<TestClass>();
            Assert.True(container.IsRegistered<TestClass>());
        }

        [Fact]
        public void RegisterT_IsLocalScope()
        {
            var container = new DiContainer();
            container.Register<TestClass>();
            Assert.Equal(Scope.Local, container.GetScope<TestClass>());
        }

        [Fact]
        public void RegisterT_IsMultiInstance()
        {
            TestClass.ResetInstanceCount();

            var container = new DiContainer();
            container.Register<TestClass>();            

            var instance = container.Resolve<TestClass>();
            container.Resolve<TestClass>();

            Assert.Equal(2, instance.Instances);
        }


        [Fact]
        public void RegisterT_LocalScope_IsLocal()
        {
            var container = new DiContainer();
            //todo thow error if Register(Scope.Local)
            container.Register<TestClass>(Scope.Local);
            Assert.True(container.IsRegistered<TestClass>());
            Assert.Equal(Scope.Local, container.GetScope<TestClass>());
        }

        [Fact]
        public void RegisterT_GlobalScope_IsGlobal()
        {
            var container = new DiContainer();
            //todo thow error if Register(Scope.Local)
            container.Register<TestClass>(Scope.Global);
            Assert.True(container.IsRegistered<TestClass>());
            Assert.Equal(Scope.Global, container.GetScope<TestClass>());
        }

        [Fact]
        public void RegisterTestClass_AsIRandom_ThrowsArgumentOutOfRange()
        {
            //Testclass doesn't implement IRandom
            var container = new DiContainer();
            Assert.Throws<ArgumentOutOfRangeException>(()=>container.Register<TestClass>().As<IRandom>());            
        }


        [Fact]
        public void RegisterTestClass_AsITestClass_IsRegistered()
        {
            var container = new DiContainer();
            container.Register<TestClass>().As<ITestClass>();
            Assert.True(container.IsRegistered<ITestClass>());
        }

        [Fact]
        public void RegisterT_AsIT_IsSingleInstance()
        {
            TestClass.ResetInstanceCount();

            var container = new DiContainer();
            container.Register<Mock<TestClass>>();
            container.Register<TestClass>().As<ITestClass>();

            var instance = container.Resolve<ITestClass>();
            container.Resolve<ITestClass>();

            Assert.Equal(1, instance.Instances);
        }

        [Fact]
        public void RegisterTestClass_WithKey_IsRegistered()
        {
            var container = new DiContainer();
            //todo ensure unique key
            container.Register<TestClass>().WithKey("test");
            Assert.True(container.IsRegistered<TestClass>());
            Assert.IsType<TestClass>(container.Resolve("test"));
            //todo does registering with a key hide TypeToResolve?
            Assert.IsType<TestClass>(container.Resolve<TestClass>());
        }

        [Fact]
        public void RegisterTestClass_AsITestClass_WithKey_IsRegistered()
        {
            var container = new DiContainer();
            //todo ensure unique key
            container.Register<TestClass>().As<ITestClass>().WithKey("test");
            Assert.True(container.IsRegistered<ITestClass>());
            Assert.IsType<TestClass>(container.Resolve("test"));
            //todo does registering with a key hide TypeToResolve?
            Assert.IsType<TestClass>(container.Resolve<ITestClass>());
        }

        [Fact]
        public void Register_WithKey_TwoOfTheSame_IsLegal()
        {
            var container = new DiContainer();
            //todo ensure unique key
            container.Register<TestClass>().WithKey("test");
            container.Register<TestClass>().WithKey("duplicate");
            Assert.IsType<TestClass>(container.Resolve("test"));
            Assert.IsType<TestClass>(container.Resolve("duplicate"));
        }

        [Fact]
        public void RegisterT__TwoOfTheSame_IsLegal()
        {
            var container = new DiContainer();            
            container.Register<TestClass>();
            container.Register<TestClass>();
            Assert.IsType<TestClass>(container.Resolve<TestClass>());
        }

        [Fact]
        public void RegisterT_AsIT__TwoOfTheSame_IsLegal()
        {
            //todo Can make NOT legal. Dangerous to have two different implementations for one interface as there will be ambiguity. Could do the same for other switches, and only allow WithKey as first option to allow legal
            var container = new DiContainer();
            container.Register<TestClass>().As<ITestClass>();
            container.Register<TestClass>().As<ITestClass>();
            Assert.IsType<TestClass>(container.Resolve<ITestClass>());
        }

        #endregion

    }
}
