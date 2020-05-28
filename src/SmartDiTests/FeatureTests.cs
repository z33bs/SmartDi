using System;
using Xunit;
using SmartDi;
using IocPerformance.Classes.Generics;
using System.Diagnostics;
using Xunit.Abstractions;

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
