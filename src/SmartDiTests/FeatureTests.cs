using System;
using Xunit;
using SmartDi;
using IocPerformance.Classes.Generics;
using System.Diagnostics;
using Xunit.Abstractions;
using IocPerformance.Classes.Multiple;
using System.Collections.Generic;
using IocPerformance.Classes.Dummy;
using IocPerformance.Classes.Standard;
using IocPerformance.Classes.Complex;
using IocPerformance.Classes.Properties;
using IocPerformance.Classes.Conditions;

namespace SmartDiTests
{
    [Collection("SmartDi")]
    public class FeatureTests
    {
        void Prepare()
        {
            DiContainer.ResetContainer();

            PrepareBasic();

            RegisterPropertyInjection();
            RegisterOpenGeneric();
            RegisterMultiple();
            RegisterConditional();
        }

        void PrepareBasic()
        {
            DiContainer.ResetContainer();
            RegisterDummies();
            RegisterStandard();
            RegisterComplexObject();
        }


        private void RegisterDummies()
        {
            DiContainer.Register<DummyOne, IDummyOne>();
            DiContainer.Register<DummyTwo, IDummyTwo>();
            DiContainer.Register<DummyThree, IDummyThree>();
            DiContainer.Register<DummyFour, IDummyFour>();
            DiContainer.Register<DummyFive, IDummyFive>();
            DiContainer.Register<DummySix, IDummySix>();
            DiContainer.Register<DummySeven, IDummySeven>();
            DiContainer.Register<DummyEight, IDummyEight>();
            DiContainer.Register<DummyNine, IDummyNine>();
            DiContainer.Register<DummyTen, IDummyTen>();
        }

        private void RegisterStandard()
        {
            DiContainer.Register<Singleton1, ISingleton1>();
            DiContainer.Register<Singleton2, ISingleton2>();
            DiContainer.Register<Singleton3, ISingleton3>();

            DiContainer.Register<Transient1, ITransient1>();
            DiContainer.Register<Transient2, ITransient2>();
            DiContainer.Register<Transient3, ITransient3>();

            DiContainer.RegisterExplicit<ICombined1>(c => new Combined1(DiContainer.Resolve<ISingleton1>(), DiContainer.Resolve<ITransient1>()));
            DiContainer.RegisterExplicit<ICombined2>(c => new Combined2(DiContainer.Resolve<ISingleton2>(), DiContainer.Resolve<ITransient2>()));
            DiContainer.RegisterExplicit<ICombined3>(c => new Combined3(DiContainer.Resolve<ISingleton3>(), DiContainer.Resolve<ITransient3>()));
        }

        private void RegisterComplexObject()
        {
            DiContainer.Register<FirstService, IFirstService>();
            DiContainer.Register<SecondService, ISecondService>();
            DiContainer.Register<ThirdService, IThirdService>();

            DiContainer.RegisterExplicit<ISubObjectOne>(c => new SubObjectOne(DiContainer.Resolve<IFirstService>()));
            DiContainer.RegisterExplicit<ISubObjectTwo>(c => new SubObjectTwo(DiContainer.Resolve<ISecondService>()));
            DiContainer.RegisterExplicit<ISubObjectThree>(c => new SubObjectThree(DiContainer.Resolve<IThirdService>()));

            DiContainer.RegisterExplicit<IComplex1>(c => new Complex1(DiContainer.Resolve<IFirstService>(), DiContainer.Resolve<ISecondService>(), DiContainer.Resolve<IThirdService>(), DiContainer.Resolve<ISubObjectOne>(), DiContainer.Resolve<ISubObjectTwo>(), DiContainer.Resolve<ISubObjectThree>()));
            DiContainer.RegisterExplicit<IComplex2>(c => new Complex2(DiContainer.Resolve<IFirstService>(), DiContainer.Resolve<ISecondService>(), DiContainer.Resolve<IThirdService>(), DiContainer.Resolve<ISubObjectOne>(), DiContainer.Resolve<ISubObjectTwo>(), DiContainer.Resolve<ISubObjectThree>()));
            DiContainer.RegisterExplicit<IComplex3>(c => new Complex3(DiContainer.Resolve<IFirstService>(), DiContainer.Resolve<ISecondService>(), DiContainer.Resolve<IThirdService>(), DiContainer.Resolve<ISubObjectOne>(), DiContainer.Resolve<ISubObjectTwo>(), DiContainer.Resolve<ISubObjectThree>()));
        }

        private void RegisterPropertyInjection()
        {
            DiContainer.RegisterExplicit<IServiceA>(c => new ServiceA()).SingleInstance();
            DiContainer.RegisterExplicit<IServiceB>(c => new ServiceB()).SingleInstance();
            DiContainer.RegisterExplicit<IServiceC>(c => new ServiceC()).SingleInstance();

            DiContainer.RegisterExplicit<ISubObjectA>(c => new SubObjectA { ServiceA = DiContainer.Resolve<IServiceA>() });
            DiContainer.RegisterExplicit<ISubObjectB>(c => new SubObjectB { ServiceB = DiContainer.Resolve<IServiceB>() });
            DiContainer.RegisterExplicit<ISubObjectC>(c => new SubObjectC { ServiceC = DiContainer.Resolve<IServiceC>() });

            DiContainer.RegisterExplicit<IComplexPropertyObject1>(c => new ComplexPropertyObject1
            {
                ServiceA = DiContainer.Resolve<IServiceA>(),
                ServiceB = DiContainer.Resolve<IServiceB>(),
                ServiceC = DiContainer.Resolve<IServiceC>(),
                SubObjectA = DiContainer.Resolve<ISubObjectA>(),
                SubObjectB = DiContainer.Resolve<ISubObjectB>(),
                SubObjectC = DiContainer.Resolve<ISubObjectC>()
            });


            DiContainer.RegisterExplicit<IComplexPropertyObject2>(c => new ComplexPropertyObject2
            {
                ServiceA = DiContainer.Resolve<IServiceA>(),
                ServiceB = DiContainer.Resolve<IServiceB>(),
                ServiceC = DiContainer.Resolve<IServiceC>(),
                SubObjectA = DiContainer.Resolve<ISubObjectA>(),
                SubObjectB = DiContainer.Resolve<ISubObjectB>(),
                SubObjectC = DiContainer.Resolve<ISubObjectC>()
            });

            DiContainer.RegisterExplicit<IComplexPropertyObject3>(c => new ComplexPropertyObject3
            {
                ServiceA = DiContainer.Resolve<IServiceA>(),
                ServiceB = DiContainer.Resolve<IServiceB>(),
                ServiceC = DiContainer.Resolve<IServiceC>(),
                SubObjectA = DiContainer.Resolve<ISubObjectA>(),
                SubObjectB = DiContainer.Resolve<ISubObjectB>(),
                SubObjectC = DiContainer.Resolve<ISubObjectC>()
            });
        }

        private void RegisterOpenGeneric()
        {
            DiContainer.RegisterType(typeof(GenericExport<>), typeof(IGenericInterface<>)); //todo should be multi
            DiContainer.RegisterType(typeof(ImportGeneric<>));
        }

        private void RegisterMultiple()
        {
            DiContainer.Register<SimpleAdapterOne, ISimpleAdapter>();
            DiContainer.Register<SimpleAdapterTwo, ISimpleAdapter>("2");
            DiContainer.Register<SimpleAdapterThree, ISimpleAdapter>("3");
            DiContainer.Register<SimpleAdapterFour, ISimpleAdapter>("4");
            DiContainer.Register<SimpleAdapterFive, ISimpleAdapter>("5");

            //todo make alternative
            //DiContainer.EnumerableBindingLifeCycle<ISimpleAdapter>(LifeCycle.Singleton);

            DiContainer.RegisterExplicit<ImportMultiple1>(c => new ImportMultiple1(DiContainer.Resolve<IEnumerable<ISimpleAdapter>>()));
            DiContainer.RegisterExplicit<ImportMultiple2>(c => new ImportMultiple2(DiContainer.Resolve<IEnumerable<ISimpleAdapter>>()));
            DiContainer.RegisterExplicit<ImportMultiple3>(c => new ImportMultiple3(DiContainer.Resolve<IEnumerable<ISimpleAdapter>>()));
        }

        private void RegisterConditional()
        {
            DiContainer.Register<ExportConditionalObject1, IExportConditionInterface>("ExportConditionalObject1");
            DiContainer.Register<ExportConditionalObject2, IExportConditionInterface>("ExportConditionalObject2");
            DiContainer.Register<ExportConditionalObject3, IExportConditionInterface>("ExportConditionalObject3");

            DiContainer.RegisterExplicit<ImportConditionObject1>(c => new ImportConditionObject1(DiContainer.Resolve<IExportConditionInterface>("ExportConditionalObject1")));
            DiContainer.RegisterExplicit<ImportConditionObject2>(c => new ImportConditionObject2(DiContainer.Resolve<IExportConditionInterface>("ExportConditionalObject2")));
            DiContainer.RegisterExplicit<ImportConditionObject3>(c => new ImportConditionObject3(DiContainer.Resolve<IExportConditionInterface>("ExportConditionalObject3")));
        }

        private readonly ITestOutputHelper output;

        public FeatureTests(ITestOutputHelper output)
        {
            this.output = output;
        }
        private static void DisplayGenericParameter(Type tp)
        {
            Console.WriteLine("      Type parameter: {0} position {1}",
                tp.Name, tp.GenericParameterPosition);
        }

        [Fact]
        public void IEnumerable()
        {
            //todo test for abstract too
            DiContainer.ResetContainer();

            DiContainer.Register<SimpleAdapterOne, ISimpleAdapter>();
            DiContainer.Register<SimpleAdapterTwo, ISimpleAdapter>("2");
            DiContainer.Register<SimpleAdapterThree, ISimpleAdapter>("3");
            DiContainer.Register<SimpleAdapterFour, ISimpleAdapter>("4");
            DiContainer.Register<SimpleAdapterFive, ISimpleAdapter>("5");
            //DiContainer.Register<IEnumerable<ISimpleAdapter>>();

            DiContainer.RegisterExplicit<ImportMultiple1>(c => new ImportMultiple1(DiContainer.Resolve<IEnumerable<ISimpleAdapter>>()));
            DiContainer.RegisterExplicit<ImportMultiple2>(c => new ImportMultiple2(DiContainer.Resolve<IEnumerable<ISimpleAdapter>>()));
            DiContainer.RegisterExplicit<ImportMultiple3>(c => new ImportMultiple3(DiContainer.Resolve<IEnumerable<ISimpleAdapter>>()));


            var importMultiple1 = (ImportMultiple1)DiContainer.Resolve(typeof(ImportMultiple1));
            var importMultiple2 = (ImportMultiple2)DiContainer.Resolve(typeof(ImportMultiple2));
            var importMultiple3 = (ImportMultiple3)DiContainer.Resolve(typeof(ImportMultiple3));

            DiContainer.ResetContainer();
        }

        [Fact]
        public void InstanceIEnumerable()
        {
            //todo test for abstract too
            IDiContainer container = new DiContainer();
            container.Register<SimpleAdapterOne, ISimpleAdapter>();
            container.Register<SimpleAdapterTwo, ISimpleAdapter>("2");
            container.Register<SimpleAdapterThree, ISimpleAdapter>("3");
            container.Register<SimpleAdapterFour, ISimpleAdapter>("4");
            container.Register<SimpleAdapterFive, ISimpleAdapter>("5");
            //DiContainer.Register<IEnumerable<ISimpleAdapter>>();

            container.RegisterExplicit<ImportMultiple1>(c => new ImportMultiple1(c.Resolve<IEnumerable<ISimpleAdapter>>()));
            container.RegisterExplicit<ImportMultiple2>(c => new ImportMultiple2(c.Resolve<IEnumerable<ISimpleAdapter>>()));
            container.RegisterExplicit<ImportMultiple3>(c => new ImportMultiple3(c.Resolve<IEnumerable<ISimpleAdapter>>()));


            var importMultiple1 = (ImportMultiple1)container.Resolve(typeof(ImportMultiple1));
            var importMultiple2 = (ImportMultiple2)container.Resolve(typeof(ImportMultiple2));
            var importMultiple3 = (ImportMultiple3)container.Resolve(typeof(ImportMultiple3));

        }

        [Fact]
        public void RegisterGenericForReal()
        {
            DiContainer.ResetContainer();
            DiContainer.RegisterType(typeof(GenericExport<>), typeof(IGenericInterface<>));
            DiContainer.RegisterType(typeof(ImportGeneric<>));

            DiContainer.Resolve<ImportGeneric<int>>();
            Assert.IsType<ImportGeneric<int>>(DiContainer.Resolve(typeof(ImportGeneric<int>)));
            Assert.IsType<ImportGeneric<float>>(DiContainer.Resolve(typeof(ImportGeneric<float>)));
            Assert.IsType<ImportGeneric<object>>(DiContainer.Resolve(typeof(ImportGeneric<object>)));
            //try 2nd resolve
            Assert.IsType<ImportGeneric<int>>(DiContainer.Resolve(typeof(ImportGeneric<int>)));
            Assert.IsType<ImportGeneric<float>>(DiContainer.Resolve(typeof(ImportGeneric<float>)));
            Assert.IsType<ImportGeneric<object>>(DiContainer.Resolve(typeof(ImportGeneric<object>)));
            DiContainer.ResetContainer();
        }

        [Fact] public void PrepareAndRegister()
        {
            PrepareBasic();
            DiContainer.ResetContainer(); //was dipose

        }
        [Fact]
        public void PrepareAndRegisterAndSimpleResolve()
        {
            PrepareBasic();
            DiContainer.Resolve(typeof(IDummyOne));
            DiContainer.Resolve(typeof(ISingleton1));
            DiContainer.ResetContainer();

        }
    }
}
