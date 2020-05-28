using System;
using Xunit;
using SmartDi;
using IocPerformance.Classes.Generics;
using System.Diagnostics;
using Xunit.Abstractions;
using IocPerformance.Classes.Multiple;
using System.Collections.Generic;

namespace SmartDiTests
{
    public class FeatureTests
    {
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
        public void IEn()
        {
            //todo test for abstract too

            DiContainer.Register<SimpleAdapterOne, ISimpleAdapter>();
            DiContainer.Register<SimpleAdapterTwo, ISimpleAdapter>("2");
            DiContainer.Register<SimpleAdapterThree, ISimpleAdapter>("3");
            DiContainer.Register<SimpleAdapterFour, ISimpleAdapter>("4");
            DiContainer.Register<SimpleAdapterFive, ISimpleAdapter>("5");
            //DiContainer.Register<IEnumerable<ISimpleAdapter>>();

            DiContainer.RegisterExpression<ImportMultiple1>(() => new ImportMultiple1(DiContainer.Resolve<IEnumerable<ISimpleAdapter>>()));
            DiContainer.RegisterExpression<ImportMultiple2>(() => new ImportMultiple2(DiContainer.Resolve<IEnumerable<ISimpleAdapter>>()));
            DiContainer.RegisterExpression<ImportMultiple3>(() => new ImportMultiple3(DiContainer.Resolve<IEnumerable<ISimpleAdapter>>()));


            var importMultiple1 = (ImportMultiple1)DiContainer.Resolve(typeof(ImportMultiple1));
            var importMultiple2 = (ImportMultiple2)DiContainer.Resolve(typeof(ImportMultiple2));
            var importMultiple3 = (ImportMultiple3)DiContainer.Resolve(typeof(ImportMultiple3));

        }

        [Fact]
        public void RegisterGenericForReal()
        {
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
        }
    }
}
