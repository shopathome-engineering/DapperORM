using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentAssertions;

namespace ShopAtHome.DapperORMCore.Tests
{
    /// <summary>
    /// We downgraded from Automapper 4, which contained this instance API, to 3, which only exposes statics.
    /// These tests try to ensure that our shims work as expected
    /// </summary>
    [TestClass]
    public class AutomapperShimTests
    {
        [TestMethod]
        public void BasicMappingStillWorks()
        {
            var mapper = GetConfiguredMapper(new List<IObjectMappingProvider> {new ABMapper()});
            var myA = new A {Foo = 42};
            var result = mapper.Map<B>(myA);
            result.Foo.Should().Be(42);
            result.Bar.Should().BeNullOrEmpty();
        }

        [TestMethod, ExpectedException(typeof(AutoMapperMappingException))]
        public void RegisteringMappingsStillWorks()
        {
            var mapper = GetConfiguredMapper(new List<IObjectMappingProvider> { new ExceptionalMapper() });
            var dbProvider = new DapperDBProvider(mapper);
            using (var conn = dbProvider.Connect("TransactionProcessing"))
            {
                var results = dbProvider.Query<object>(conn, "SELECT * From TransactionProcessing.dbo.QueueConfiguration with (nolock)").ToList();
            }
        }

        [TestMethod]
        public void MappingTwoObjectsTogetherStillWorks()
        {
            var mapper = GetConfiguredMapper(new List<IObjectMappingProvider> { new ABMapper() });
            var A = new A {Foo = 24};
            var B = new B {Bar = "I exist!"};
            mapper.Map(A, B);
            B.Foo.Should().Be(24);
            B.Bar.Should().Be("I exist!");
        }
        
        private static IMapper GetConfiguredMapper(IEnumerable<IObjectMappingProvider> configurations)
        {
            var config = new MapperConfiguration(x => { });
            foreach (var objectMappingProvider in configurations)
            {
                objectMappingProvider.ApplyMappings(config);
            }
            return config.CreateMapper();
        }

        public class A
        {
            public int Foo { get; set; }
        }

        public class B
        {
            public int Foo { get; set; }
            public string Bar { get; set; }
        }

        public class ABMapper : IObjectMappingProvider
        {
            public void ApplyMappings(IMapperConfiguration configuration)
            {
                configuration.CreateMap<A, B>();
            }
        }

        public class ExceptionalMapper : IObjectMappingProvider
        {
            public void ApplyMappings(IMapperConfiguration configuration)
            {
                configuration.CreateMap<dynamic, object>().ConvertUsing(x => { throw new Exception(); });
            }
        }
    }
}
