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
            DiContainer.Register<DummyOne, IDummyOne>().MultiInstance();
            DiContainer.Register<DummyTwo, IDummyTwo>().MultiInstance();
            DiContainer.Register<DummyThree, IDummyThree>().MultiInstance();
            DiContainer.Register<DummyFour, IDummyFour>().MultiInstance();
            DiContainer.Register<DummyFive, IDummyFive>().MultiInstance();
            DiContainer.Register<DummySix, IDummySix>().MultiInstance();
            DiContainer.Register<DummySeven, IDummySeven>().MultiInstance();
            DiContainer.Register<DummyEight, IDummyEight>().MultiInstance();
            DiContainer.Register<DummyNine, IDummyNine>().MultiInstance();
            DiContainer.Register<DummyTen, IDummyTen>().MultiInstance();
        }

        private void RegisterStandard()
        {
            DiContainer.Register<Singleton1, ISingleton1>();
            DiContainer.Register<Singleton2, ISingleton2>();
            DiContainer.Register<Singleton3, ISingleton3>();

            DiContainer.Register<Transient1, ITransient1>().MultiInstance();
            DiContainer.Register<Transient2, ITransient2>().MultiInstance();
            DiContainer.Register<Transient3, ITransient3>().MultiInstance();

            DiContainer.RegisterExpression<ICombined1>(c => new Combined1(DiContainer.Resolve<ISingleton1>(), DiContainer.Resolve<ITransient1>())).MultiInstance();
            DiContainer.RegisterExpression<ICombined2>(c => new Combined2(DiContainer.Resolve<ISingleton2>(), DiContainer.Resolve<ITransient2>())).MultiInstance();
            DiContainer.RegisterExpression<ICombined3>(c => new Combined3(DiContainer.Resolve<ISingleton3>(), DiContainer.Resolve<ITransient3>())).MultiInstance();
        }

        private void RegisterComplexObject()
        {
            DiContainer.Register<FirstService, IFirstService>();
            DiContainer.Register<SecondService, ISecondService>();
            DiContainer.Register<ThirdService, IThirdService>();

            DiContainer.RegisterExpression<ISubObjectOne>(c => new SubObjectOne(DiContainer.Resolve<IFirstService>())).MultiInstance();
            DiContainer.RegisterExpression<ISubObjectTwo>(c => new SubObjectTwo(DiContainer.Resolve<ISecondService>())).MultiInstance();
            DiContainer.RegisterExpression<ISubObjectThree>(c => new SubObjectThree(DiContainer.Resolve<IThirdService>())).MultiInstance();

            DiContainer.RegisterExpression<IComplex1>(c => new Complex1(DiContainer.Resolve<IFirstService>(), DiContainer.Resolve<ISecondService>(), DiContainer.Resolve<IThirdService>(), DiContainer.Resolve<ISubObjectOne>(), DiContainer.Resolve<ISubObjectTwo>(), DiContainer.Resolve<ISubObjectThree>())).MultiInstance();
            DiContainer.RegisterExpression<IComplex2>(c => new Complex2(DiContainer.Resolve<IFirstService>(), DiContainer.Resolve<ISecondService>(), DiContainer.Resolve<IThirdService>(), DiContainer.Resolve<ISubObjectOne>(), DiContainer.Resolve<ISubObjectTwo>(), DiContainer.Resolve<ISubObjectThree>())).MultiInstance();
            DiContainer.RegisterExpression<IComplex3>(c => new Complex3(DiContainer.Resolve<IFirstService>(), DiContainer.Resolve<ISecondService>(), DiContainer.Resolve<IThirdService>(), DiContainer.Resolve<ISubObjectOne>(), DiContainer.Resolve<ISubObjectTwo>(), DiContainer.Resolve<ISubObjectThree>())).MultiInstance();
        }

        private void RegisterPropertyInjection()
        {
            DiContainer.RegisterExpression<IServiceA>(c => new ServiceA()).SingleInstance();
            DiContainer.RegisterExpression<IServiceB>(c => new ServiceB()).SingleInstance();
            DiContainer.RegisterExpression<IServiceC>(c => new ServiceC()).SingleInstance();

            DiContainer.RegisterExpression<ISubObjectA>(c => new SubObjectA { ServiceA = DiContainer.Resolve<IServiceA>() });
            DiContainer.RegisterExpression<ISubObjectB>(c => new SubObjectB { ServiceB = DiContainer.Resolve<IServiceB>() });
            DiContainer.RegisterExpression<ISubObjectC>(c => new SubObjectC { ServiceC = DiContainer.Resolve<IServiceC>() });

            DiContainer.RegisterExpression<IComplexPropertyObject1>(c => new ComplexPropertyObject1
            {
                ServiceA = DiContainer.Resolve<IServiceA>(),
                ServiceB = DiContainer.Resolve<IServiceB>(),
                ServiceC = DiContainer.Resolve<IServiceC>(),
                SubObjectA = DiContainer.Resolve<ISubObjectA>(),
                SubObjectB = DiContainer.Resolve<ISubObjectB>(),
                SubObjectC = DiContainer.Resolve<ISubObjectC>()
            });


            DiContainer.RegisterExpression<IComplexPropertyObject2>(c => new ComplexPropertyObject2
            {
                ServiceA = DiContainer.Resolve<IServiceA>(),
                ServiceB = DiContainer.Resolve<IServiceB>(),
                ServiceC = DiContainer.Resolve<IServiceC>(),
                SubObjectA = DiContainer.Resolve<ISubObjectA>(),
                SubObjectB = DiContainer.Resolve<ISubObjectB>(),
                SubObjectC = DiContainer.Resolve<ISubObjectC>()
            });

            DiContainer.RegisterExpression<IComplexPropertyObject3>(c => new ComplexPropertyObject3
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
            DiContainer.RegisterOpenGeneric(typeof(GenericExport<>), typeof(IGenericInterface<>)); //todo should be multi
            DiContainer.RegisterOpenGeneric(typeof(ImportGeneric<>));
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

            DiContainer.RegisterExpression<ImportMultiple1>(c => new ImportMultiple1(DiContainer.Resolve<IEnumerable<ISimpleAdapter>>()));
            DiContainer.RegisterExpression<ImportMultiple2>(c => new ImportMultiple2(DiContainer.Resolve<IEnumerable<ISimpleAdapter>>()));
            DiContainer.RegisterExpression<ImportMultiple3>(c => new ImportMultiple3(DiContainer.Resolve<IEnumerable<ISimpleAdapter>>()));
        }

        private void RegisterConditional()
        {
            DiContainer.Register<ExportConditionalObject1, IExportConditionInterface>("ExportConditionalObject1");
            DiContainer.Register<ExportConditionalObject2, IExportConditionInterface>("ExportConditionalObject2");
            DiContainer.Register<ExportConditionalObject3, IExportConditionInterface>("ExportConditionalObject3");

            DiContainer.RegisterExpression<ImportConditionObject1>(c => new ImportConditionObject1(DiContainer.Resolve<IExportConditionInterface>("ExportConditionalObject1")));
            DiContainer.RegisterExpression<ImportConditionObject2>(c => new ImportConditionObject2(DiContainer.Resolve<IExportConditionInterface>("ExportConditionalObject2")));
            DiContainer.RegisterExpression<ImportConditionObject3>(c => new ImportConditionObject3(DiContainer.Resolve<IExportConditionInterface>("ExportConditionalObject3")));
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

            DiContainer.RegisterExpression<ImportMultiple1>(c => new ImportMultiple1(DiContainer.Resolve<IEnumerable<ISimpleAdapter>>()));
            DiContainer.RegisterExpression<ImportMultiple2>(c => new ImportMultiple2(DiContainer.Resolve<IEnumerable<ISimpleAdapter>>()));
            DiContainer.RegisterExpression<ImportMultiple3>(c => new ImportMultiple3(DiContainer.Resolve<IEnumerable<ISimpleAdapter>>()));


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

            container.RegisterExpression<ImportMultiple1>(c => new ImportMultiple1(c.Resolve<IEnumerable<ISimpleAdapter>>()));
            container.RegisterExpression<ImportMultiple2>(c => new ImportMultiple2(c.Resolve<IEnumerable<ISimpleAdapter>>()));
            container.RegisterExpression<ImportMultiple3>(c => new ImportMultiple3(c.Resolve<IEnumerable<ISimpleAdapter>>()));


            var importMultiple1 = (ImportMultiple1)container.Resolve(typeof(ImportMultiple1));
            var importMultiple2 = (ImportMultiple2)container.Resolve(typeof(ImportMultiple2));
            var importMultiple3 = (ImportMultiple3)container.Resolve(typeof(ImportMultiple3));

        }

        [Fact]
        public void RegisterGenericForReal()
        {
            DiContainer.ResetContainer();
            DiContainer.RegisterOpenGeneric(typeof(GenericExport<>), typeof(IGenericInterface<>));
            DiContainer.RegisterOpenGeneric(typeof(ImportGeneric<>));

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
