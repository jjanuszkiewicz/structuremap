using System;
using NUnit.Framework;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;

namespace StructureMap.Testing.Graph
{
    [TestFixture]
    public class GenericConnectionScannerTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            container = new Container(x =>
            {
                x.Scan(o =>
                {
                    o.TheCallingAssembly();
                    o.ConnectImplementationsToTypesClosing(typeof (IFinder<>));
                });
            });
        }

        #endregion

        private Container container;

        [Test]
        public void can_find_the_closed_finders()
        {
            container.GetInstance<IFinder<string>>().ShouldBeOfType<StringFinder>();
            container.GetInstance<IFinder<int>>().ShouldBeOfType<IntFinder>();
            container.GetInstance<IFinder<double>>().ShouldBeOfType<DoubleFinder>();
        }

        [Test, ExpectedException(typeof (ApplicationException))]
        public void fails_on_closed_type()
        {
            new GenericConnectionScanner(typeof (double));
        }
    }

    public interface IFinder<T>
    {
    }

    public class StringFinder : IFinder<string>
    {
    }

    public class IntFinder : IFinder<int>
    {
    }

    public class DoubleFinder : IFinder<double>
    {
    }


    [TestFixture]
    public class DefaultConventionScanningTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            PluginCache.ResetAll();
        }

        #endregion

        [Test]
        public void FindPluginType()
        {
            Assert.AreEqual(typeof (IConvention), new DefaultConventionScanner().FindPluginType(typeof (Convention)));
            Assert.IsNull(new DefaultConventionScanner().FindPluginType(GetType()));
        }


        [Test]
        public void Process_to_Container()
        {
            var container = new Container(registry =>
            {
                registry.Scan(x =>
                {
                    x.TheCallingAssembly();
                    x.Convention<DefaultConventionScanner>();
                });
            });

            container.GetInstance<IConvention>().ShouldBeOfType<Convention>();
        }


        [Test]
        public void Process_to_Container_2()
        {
            var container = new Container(registry =>
            {
                registry.Scan(x =>
                {
                    x.TheCallingAssembly();
                    x.With(new DefaultConventionScanner());
                });
            });

            Assert.IsInstanceOfType(typeof (Convention), container.GetInstance<IConvention>());
        }

        [Test]
        public void Process_to_PluginGraph()
        {
            var graph = new PluginGraph();
            var scanner = new DefaultConventionScanner();

            var registry = new Registry();

            scanner.Process(typeof (Convention), registry);

            registry.ConfigurePluginGraph(graph);

            Assert.IsFalse(graph.PluginFamilies.Contains(typeof (IServer)));
            Assert.IsTrue(graph.PluginFamilies.Contains(typeof (IConvention)));

            PluginFamily family = graph.FindFamily(typeof (IConvention));
            family.Seal();
            Assert.AreEqual(1, family.InstanceCount);
        }
    }

    public interface IConvention
    {
    }

    public interface IServer
    {
    }

    public class Convention : IConvention, IServer
    {
        public void Go()
        {
            throw new NotImplementedException();
        }
    }

    public class Something : IConvention
    {
    }
}