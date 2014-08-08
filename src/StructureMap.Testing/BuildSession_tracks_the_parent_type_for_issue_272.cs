﻿using System;
using System.Linq;
using NUnit.Framework;
using StructureMap.Pipeline;
using StructureMap.Testing.Acceptance;

namespace StructureMap.Testing
{
    [TestFixture]
    public class BuildSession_tracks_the_parent_type_for_issue_272
    {
        private Type parentType;

        [SetUp]
        public void SetUp()
        {
            parentType = null;
        }

        [Test]
        public void for_basic_get_instance()
        {
            var container = new Container(x => {
                x.For<ILoggerHolder>().Use<LoggerHolder>();

                x.For<FakeLogger>().Use(c => new FakeLogger(c.RootType));
            });

            container.GetInstance<ILoggerHolder>()
                .Logger.RootType.ShouldEqual(typeof (LoggerHolder));
        }

        [Test]
        public void in_get_instance_by_name()
        {
            var container = new Container(x => {
                x.For<ILoggerHolder>().Use<BuildSessionTarget>()
                    .Named("Red");

                x.For<ILoggerHolder>().Add<LoggerHolder>()
                    .Named("Blue");

                x.For<FakeLogger>().Use(c => new FakeLogger(c.RootType));
            });

            container.GetInstance<ILoggerHolder>("Red")
                .Logger.RootType.ShouldEqual(typeof (BuildSessionTarget));

            container.GetInstance<ILoggerHolder>("Blue")
                .Logger.RootType.ShouldEqual(typeof(LoggerHolder));

            container.GetInstance(typeof(ILoggerHolder),"Red")
                .As<ILoggerHolder>()
                .Logger.RootType.ShouldEqual(typeof(BuildSessionTarget));

            container.GetInstance(typeof(ILoggerHolder), "Blue")
                .As<ILoggerHolder>()
                .Logger.RootType.ShouldEqual(typeof(LoggerHolder));



        }

        [Test]
        public void from_within_get_all_instances()
        {
            var container = new Container(x =>
            {
                x.For<ILoggerHolder>().Use<BuildSessionTarget>()
                    .Named("Red");

                x.For<ILoggerHolder>().Add<LoggerHolder>()
                    .Named("Blue");

                x.For<FakeLogger>().Use(c => new FakeLogger(c.RootType)).AlwaysUnique();
            });

            var holders = container.GetAllInstances<ILoggerHolder>().ToArray();
            holders[0].Logger.RootType.ShouldEqual(typeof (BuildSessionTarget));
            holders[1].Logger.RootType.ShouldEqual(typeof (LoggerHolder));

        }

        [Test]
        public void get_instance_for_a_supplied_instance()
        {
            var container = new Container(x => {
                x.For<FakeLogger>().Use(c => new FakeLogger(c.RootType));
            });

            container.GetInstance<ILoggerHolder>(new SmartInstance<LoggerHolder>())
                .Logger.RootType
                .ShouldEqual(typeof (LoggerHolder));
        }

        [Test]
        public void get_instance_with_args()
        {
            var container = new Container(x => {
                x.For<FakeLogger>().Use(c => new FakeLogger(c.RootType));

                x.For<ILoggerHolder>().Use<WidgetLoggerHolder>();
            });

            var explicitArguments = new ExplicitArguments();
            explicitArguments.Set<IWidget>(new AWidget());
            container.GetInstance<ILoggerHolder>(explicitArguments)
                .Logger.RootType.ShouldEqual(typeof(WidgetLoggerHolder));
        }


        [Test]
        public void get_instance_with_args_by_name()
        {
            var container = new Container(x =>
            {
                x.For<FakeLogger>().Use(c => new FakeLogger(c.RootType));

                x.For<ILoggerHolder>().Use<WidgetLoggerHolder>().Named("Foo");
            });

            var explicitArguments = new ExplicitArguments();
            explicitArguments.Set<IWidget>(new AWidget());
            container.GetInstance<ILoggerHolder>(explicitArguments, "Foo")
                .Logger.RootType.ShouldEqual(typeof(WidgetLoggerHolder));
        }

        [Test]
        public void inside_a_func()
        {
            var container = new Container(x => {
                x.For<FakeLogger>().Use(c => new FakeLogger(c.RootType));
                x.For<ILoggerHolder>().Use<LoggerHolder>();
            });

            container.GetInstance<LazyLoggerHolderHolder>()
                .Logger.RootType.ShouldEqual(typeof (LoggerHolder));
        }
    }

    public interface ILoggerHolder
    {
        FakeLogger Logger { get; }
    }

    public class LazyLoggerHolderHolder
    {
        public LazyLoggerHolderHolder(Func<ILoggerHolder> func)
        {
            Logger = func().Logger;
        }

        public FakeLogger Logger { get; set; }
    }

    public class WidgetLoggerHolder : ILoggerHolder
    {
        private readonly IWidget _widget;
        private readonly FakeLogger _logger;

        public WidgetLoggerHolder(IWidget widget, FakeLogger logger)
        {
            _widget = widget;
            _logger = logger;
        }

        public FakeLogger Logger
        {
            get { return _logger; }
        }
    }

    public class BuildSessionTarget : ILoggerHolder
    {
        private readonly FakeLogger _logger;

        public BuildSessionTarget(FakeLogger logger)
        {
            _logger = logger;
        }

        public FakeLogger Logger
        {
            get { return _logger; }
        }
    }

    public class LoggerHolder : ILoggerHolder
    {
        private readonly FakeLogger _logger;

        public LoggerHolder(FakeLogger logger)
        {
            _logger = logger;
        }

        public FakeLogger Logger
        {
            get { return _logger; }
        }
    }

    public class FakeLogger
    {
        private readonly Type _rootType;

        public FakeLogger(Type rootType)
        {
            _rootType = rootType;
        }

        public Type RootType
        {
            get { return _rootType; }
        }
    }
    
}