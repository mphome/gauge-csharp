﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using NuGet;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    [TestFixture]
    internal class SetupPhaseExecutorTests
    {
        private const string Version = "0.5.2";
        private Mock<IPackageRepositoryFactory> _packageRepositoryFactory;

        [SetUp]
        public void Setup()
        {
            _packageRepositoryFactory = new Mock<IPackageRepositoryFactory>();
            var packageRepository = new Mock<IPackageRepository>();
            var package = new Mock<IPackage>();
            package.Setup(p => p.Id).Returns("Gauge.CSharp.Lib");
            var list = new List<IPackage> {package.Object};
            package.Setup(p => p.Version).Returns(new SemanticVersion(Version));
            packageRepository.Setup(repository => repository.GetPackages()).Returns(list.AsQueryable());
            _packageRepositoryFactory.Setup(factory => factory.CreateRepository(SetupPhaseExecutor.NugetEndpoint))
                .Returns(packageRepository.Object);
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", Directory.GetCurrentDirectory());

        }

        [Test]
        public void ShouldFetchMaxLibVersionOnlyOnce()
        {
            var setupPhaseExecutor = new SetupPhaseExecutor(_packageRepositoryFactory.Object);
            var maxLibVersion = setupPhaseExecutor.MaxLibVersion;

            maxLibVersion = setupPhaseExecutor.MaxLibVersion; // call again, just for fun!

            Assert.AreEqual(Version, maxLibVersion.ToString());
            _packageRepositoryFactory.Verify(factory => factory.CreateRepository(SetupPhaseExecutor.NugetEndpoint), Times.Once);
        }
    }
}
