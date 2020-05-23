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
            public string ConstructorUsed { get; }
            public IService Service { get; }
            public ConcreteOnly Concrete { get; }

            public ClassWith3Ctors(IService service)
            { this.Service = service; ConstructorUsed = "(IService service)"; }
            public ClassWith3Ctors() { ConstructorUsed = "()"; }
            internal ClassWith3Ctors(IService service, ConcreteOnly concrete)
            { this.Service = service; this.Concrete = concrete; ConstructorUsed = "(IService service, ConcreteOnly concrete)"; }
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

        class ClassWithKeyedDependency
        {
            public IService Service { get; }
            public ClassWithKeyedDependency()
            {

            }
            public ClassWithKeyedDependency([ResolveNamed("test")]IService service)
            {
                Service = service;
            }
        }

        class ClassThatsUnresolvable
        {
            public ClassThatsUnresolvable()
            {
                throw new Exception("Its not possible to instantiate this class");
            }
        }

        class ClassWithInternalConstructor
        {
            public string ConstructorUsed { get; }

            public ClassWithInternalConstructor()
            {
                ConstructorUsed = "public";
            }

            internal ClassWithInternalConstructor(ConcreteOnly concreteOnly)
            {
                ConstructorUsed = "internal";
            }
        }

        class ClassThatHasToBeRegistered
        {
            public ClassThatHasToBeRegistered(int number)
            {

            }
        }

        class ClassWithoutParamaterlessCtor
        {
            public ClassWithoutParamaterlessCtor(IService service)
            {

            }
        }

        public class ClassThatThrowsOnDisposed : IDisposable
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        class ClassThatsDisposable : DisposableBase
        {
            private Action _onExplicitDispose;
            private Action _onImplicitDispose;

            public ClassThatsDisposable(Action onExplicitDispose, Action onImplicitDispose)
            {
                _onExplicitDispose = onExplicitDispose;
                _onImplicitDispose = onImplicitDispose;
            }

            protected override void DisposeExplicit()
                => _onExplicitDispose?.DynamicInvoke();
            protected override void DisposeImplicit()
                => _onImplicitDispose?.DynamicInvoke();
        }

        [Fact]
        public void Constructor()
        {
            Assert.IsAssignableFrom<INewDiContainer>(new NewDiContainer());
        }

        #region Registration
        #region internal

        [Fact]
        public void InnerRegister_ConcreteTypeIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(()=>
            NewDiContainer.InternalRegister(
                new ConcurrentDictionary<Tuple<Type, string>, MetaObject>(),
                typeof(IService),
                concreteType: null, //We need this at a minimum to justify registration 
                null,
                LifeCycle.Transient));
        }

        #endregion
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
        public void StaticRegisterResolvedType_RegistersAsSingleInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer.Register<MyService,IService>();

            Assert.Equal(LifeCycle.Singleton, mock[new Tuple<Type, string>(typeof(IService), null)].LifeCycle);

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterResolvedType_RegistersAsSingleInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            INewDiContainer container = new NewDiContainer(mock);

            container.Register<MyService,IService>();

            Assert.Equal(LifeCycle.Singleton, mock[new Tuple<Type, string>(typeof(IService), null)].LifeCycle);
        }
        #endregion
        #region Register<ConcreteType>(string key)
        [Fact]
        public void StaticRegisterConcreteTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer.Register<MyService>("test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), "test")));

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterConcreteTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            INewDiContainer container = new NewDiContainer(mock);

            container.Register<MyService>("test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), "test")));
        }

        #endregion
        #region Register<ConcreteType,ResolvedType>(string key)
        [Fact]
        public void StaticRegisterResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer.Register<MyService, IService>("test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            INewDiContainer container = new NewDiContainer(mock);

            container.Register<MyService, IService>("test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));
        }

        #endregion

        #region RegisterInstance
        [Fact]
        public void StaticRegisterInstance_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer.RegisterInstance(new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterInstance_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            INewDiContainer container = new NewDiContainer(mock);

            container.RegisterInstance(new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));
        }

        [Fact]
        public void StaticRegisterInstanceWithResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer.RegisterInstance<MyService,IService>(new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterInstanceWithResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            INewDiContainer container = new NewDiContainer(mock);

            container.RegisterInstance<MyService,IService>(new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));
        }

        [Fact]
        public void StaticRegisterInstanceWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer.RegisterInstance(new MyService(),"test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), "test")));

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterInstanceWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            INewDiContainer container = new NewDiContainer(mock);

            container.RegisterInstance(new MyService(),"test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), "test")));
        }

        [Fact]
        public void StaticRegisterInstanceWithResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer.RegisterInstance<MyService, IService>(new MyService(),"test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterInstanceWithResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            INewDiContainer container = new NewDiContainer(mock);

            container.RegisterInstance<MyService, IService>(new MyService(),"test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));
        }


        #endregion
        #region RegisterOptions
        [Fact]
        public void StaticRegisterConcreteType_OptionsSingleInstance_RegistersAsSingleInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer
                .Register<MyService>()
                .SingleInstance();

            Assert.Equal(LifeCycle.Singleton, mock[new Tuple<Type, string>(typeof(MyService), null)].LifeCycle);

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterConcreteType_OptionsSingleInstance_RegistersAsSingleInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            INewDiContainer container = new NewDiContainer(mock);

            container
                .Register<MyService>()
                .SingleInstance();

            Assert.Equal(LifeCycle.Singleton, mock[new Tuple<Type, string>(typeof(MyService), null)].LifeCycle);
        }

        [Fact]
        public void StaticRegisterResolvedType_OptionsMultiInstance_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer
                .Register<MyService, IService>()
                .MultiInstance();

            Assert.Equal(LifeCycle.Transient, mock[new Tuple<Type, string>(typeof(IService), null)].LifeCycle);

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterResolvedType_OptionsMultiInstance_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            INewDiContainer container = new NewDiContainer(mock);

            container
                .Register<MyService, IService>()
                .MultiInstance();

            Assert.Equal(LifeCycle.Transient, mock[new Tuple<Type, string>(typeof(IService), null)].LifeCycle);
        }

        [Fact]
        public void StaticRegisterConcreteType_UsingConstructorOption_ResolvesSpecifiedConstructor()
        {
            NewDiContainer.Register<MyService, IService>();

            NewDiContainer
                .Register<ClassWith3Ctors>()
                .UsingConstructor(new Type[] { typeof(IService) });

            var resolved = NewDiContainer.Resolve<ClassWith3Ctors>();
            Assert.Equal("(IService service)", resolved.ConstructorUsed);

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterConcreteType__UsingConstructorOption_ResolvesSpecifiedConstructor()
        {
            INewDiContainer container = new NewDiContainer();
            container.Register<MyService, IService>();

            container
                .Register<ClassWith3Ctors>()
                .UsingConstructor(new Type[] { typeof(IService) });

            var resolved = container.Resolve<ClassWith3Ctors>();
            Assert.Equal("(IService service)", resolved.ConstructorUsed);
        }

        [Fact]
        public void StaticRegisterConcreteType_UsingConstructorOption_WrongCtor_ThrowsRegistrationException()
        {
            Assert.Throws<RegistrationException>(()=>
                NewDiContainer
                    .Register<ClassWith3Ctors>()
                    .UsingConstructor(new Type[] { typeof(ConcreteOnly) }));

            NewDiContainer.ResetContainer();
        }


        [Fact]
        public void StaticRegisterConcreteType_UsingConstructorOption_ResolvesInternalConstructor()
        {
            NewDiContainer
                .Register<ClassWithInternalConstructor>()
                .UsingConstructor(new Type[] { typeof(ConcreteOnly) });

            var resolved = NewDiContainer.Resolve<ClassWithInternalConstructor>();
            Assert.Equal("internal", resolved.ConstructorUsed);

            NewDiContainer.ResetContainer();
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

        [Fact]
        public void Resolve_Unregistered_Works()
        {
            INewDiContainer container = new NewDiContainer();
            var obj = container.Resolve<ClassThatsResolvableWithoutRegistering>();

            Assert.IsType<ClassThatsResolvableWithoutRegistering>(obj);

        }

        [Fact]
        public void StaticResolve_IsStrictModeTrue_Unregistered_Throws()
        {
            NewDiContainer.MySettings.TryResolveUnregistered = false;
            Assert.Throws<TypeNotRegisteredException>(
                ()=>NewDiContainer.Resolve<ClassThatsResolvableWithoutRegistering>());

            NewDiContainer.MySettings.TryResolveUnregistered = true;

        }

        [Fact]
        public void StaticResolve_KeyedDependency_Works()
        {
            NewDiContainer.Register<MyService,IService>("test");
            NewDiContainer.Register<ClassWithKeyedDependency>();
            var obj = NewDiContainer.Resolve<ClassWithKeyedDependency>();

            Assert.IsType<ClassWithKeyedDependency>(obj);

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void StaticResolve_InstanceRegistered_ReturnsInstance()
        {
            var instance = new MyService();
            NewDiContainer.RegisterInstance(instance);
            var resolved = NewDiContainer.Resolve<MyService>();
            Assert.Equal(instance, resolved); //exactly same object returned

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void StaticResolve_InterfaceThatsNotRegistered_Throws()
        {
            Assert.Throws<TypeNotRegisteredException>(()=>NewDiContainer.Resolve<IService>());

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void StaticResolve_UnregisteredUnresolvable_Throws()
        {
            Assert.Throws<TypeNotRegisteredException>(
                () => NewDiContainer.Resolve<ClassWithFlaggedCtor>());
        }



        [Fact]
        public void StaticResolve_UnregisteredUnresolvable2_Throws()
        {
            Assert.Throws<TypeNotRegisteredException>(
                () => NewDiContainer.Resolve<ClassThatsUnresolvable>());
        }

        [Fact]
        public void ResolveFromContainerInstance_RegisteredInStaticContainer_ResolvesFromStaticContainer()
        {
            var registeredObject = new ClassThatHasToBeRegistered(3);
            NewDiContainer.RegisterInstance(registeredObject);

            INewDiContainer container = new NewDiContainer();
            var resolved = container.Resolve<ClassThatHasToBeRegistered>();

            Assert.Equal(registeredObject, resolved);

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void Resolve_SettingsNotBubble_RegisteredInStaticContainer_NotResolved()
        {
            NewDiContainer.MySettings.ResolveBubblesToStaticContainer = false;

            var registeredObject = new ClassThatHasToBeRegistered(3);
            NewDiContainer.RegisterInstance(registeredObject);

            INewDiContainer container = new NewDiContainer();
            Assert.Throws<TypeNotRegisteredException>(()=>container.Resolve<ClassThatHasToBeRegistered>());

            NewDiContainer.ResetContainer();
            NewDiContainer.MySettings.ResolveBubblesToStaticContainer = true;
        }

        [Fact]
        public void StaticResolve_Interface_ThrowsResolutionException()
        {
            NewDiContainer.Register<IService>();
            Assert.Throws<TypeNotRegisteredException>(() => NewDiContainer.Resolve<IService>());
            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void StaticResolveWithKey_InstanceRegistered_ReturnsInstance()
        {
            var instance = new MyService();
            NewDiContainer.RegisterInstance(instance,"test");
            var resolved = NewDiContainer.Resolve<MyService>("test");
            Assert.Equal(instance, resolved); //exactly same object returned

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void ResolveWithKey_InstanceRegistered_ReturnsInstance()
        {
            var instance = new MyService();
            INewDiContainer container = new NewDiContainer();
            container.RegisterInstance(instance, "test");
            var resolved = container.Resolve<MyService>("test");
            Assert.Equal(instance, resolved); //exactly same object returned
        }

        #endregion

        #region Unregister
        [Fact]
        public void StaticUnregister_NotRegistered_ThrowsResolutionExceptoin()
        {
            Assert.Throws<TypeNotRegisteredException>(() => NewDiContainer.Unregister<ConcreteOnly>());
        }

        [Fact]
        public void StaticUnregister_ExceptionWhileDisposing_ThrowsExceptoin()
        {
            //Ensure MetaObject.Instance is set
            NewDiContainer.RegisterInstance(new ClassThatThrowsOnDisposed());

            Assert.Throws<Exception>(() => NewDiContainer.Unregister<ClassThatThrowsOnDisposed>());

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void StaticUnregister_Registered_IsRemoved()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer.Register<ConcreteOnly>();

            Assert.NotEmpty(mock);

            NewDiContainer.Unregister<ConcreteOnly>();

            Assert.Empty(mock);

            NewDiContainer.ResetContainer();
        }
        [Fact]
        public void Unregister_Registered_IsRemoved()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            INewDiContainer container = new NewDiContainer(mock);

            container.Register<ConcreteOnly>();

            Assert.NotEmpty(mock);

            container.Unregister<ConcreteOnly>();

            Assert.Empty(mock);
        }
        [Fact]
        public void StaticUnregisterWithKey_Registered_IsRemoved()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer.Register<ConcreteOnly>("test");

            Assert.NotEmpty(mock);

            NewDiContainer.Unregister<ConcreteOnly>("test");

            Assert.Empty(mock);

            NewDiContainer.ResetContainer();
        }
        [Fact]
        public void UnregisterWithKey_Registered_IsRemoved()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            INewDiContainer container = new NewDiContainer(mock);

            container.Register<ConcreteOnly>("test");

            Assert.NotEmpty(mock);

            container.Unregister<ConcreteOnly>("test");

            Assert.Empty(mock);
        }
        [Fact]
        public void StaticUnregisterWithKey_NotRegistered_ThrowsResolutionException()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer.Register<ConcreteOnly>("test");

            Assert.NotEmpty(mock);

               Assert.Throws<TypeNotRegisteredException>(
                   ()=>NewDiContainer.Unregister<ConcreteOnly>("wrongKey"));

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void StaticUnregister_Registered_DisposeCalled()
        {
            bool wasDisposedExplicitly = false;
            var disposable = new ClassThatsDisposable(() => wasDisposedExplicitly = true, () => { });
            NewDiContainer.RegisterInstance(disposable);

            NewDiContainer.Unregister<ClassThatsDisposable>();

            Assert.True(wasDisposedExplicitly);

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void StaticUnregisterAll()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            NewDiContainer.SetContainer(mock);

            NewDiContainer.Register<MyService>();
            NewDiContainer.Register<ConcreteOnly>();
            NewDiContainer.Register<ClassWith3Ctors>();

            Assert.Equal(3, mock.Count);

            NewDiContainer.UnregisterAll();

            Assert.Empty(mock);

            NewDiContainer.ResetContainer();
        }

        [Fact]
        public void UnregisterAll()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            INewDiContainer container = new NewDiContainer(mock);

            container.Register<MyService>();
            container.Register<ConcreteOnly>();
            container.Register<ClassWith3Ctors>();

            Assert.Equal(3, mock.Count);

            container.UnregisterAll();

            Assert.Empty(mock);
        }


        #endregion

        #region Exceptions
        [Fact]
        public void StaticRegisterConcreteType_Duplicate_ThrowsRegistrationException()
        {
            NewDiContainer.Register<MyService>();
            Assert.Throws<RegistrationException>(() => NewDiContainer.Register<MyService>());
        }

        [Fact]
        public void StaticRegisterConcreteTypeWithKey_Duplicate_ThrowsRegistrationException()
        {
            NewDiContainer.Register<MyService>("test");
            Assert.Throws<RegistrationException>(() => NewDiContainer.Register<MyService>("test"));
        }

        [Fact]
        public void StaticResolveClassWithInternalConstructor_ResolvesPublicEvenThoughInternalIsGreediest()
        {
            NewDiContainer
                .Register<ClassWithInternalConstructor>();

            var resolved = NewDiContainer.Resolve<ClassWithInternalConstructor>();
            Assert.Equal("public", resolved.ConstructorUsed);

            NewDiContainer.ResetContainer();
        }



        #endregion

    }
}
