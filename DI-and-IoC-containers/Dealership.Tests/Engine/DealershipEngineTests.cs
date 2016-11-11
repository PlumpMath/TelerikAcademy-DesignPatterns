using Dealership.Contracts;
using Dealership.Engine;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Dealership.Tests.Engine
{
    [TestFixture]
    public class DealershipEngineTests
    {
        [Test]
        public void Constructor_WhenCommandReaderIsNull_ShouldThrow()
        {
            var mockedUserService = new Mock<IUserService>();
            var mockedCommandProcessor = new Mock<ICommandProcessor>();
            var mockedReportPrinter = new Mock<IReportPrinter>();

            Assert.Throws<ArgumentNullException>(() => new DealershipEngine(null, mockedCommandProcessor.Object, mockedReportPrinter.Object, mockedUserService.Object));
        }

        [Test]
        public void Constructor_WhenCommandProcessorIsNull_ShouldThrow()
        {
            var mockedCommandReader = new Mock<ICommandReader>();
            var mockedUserService = new Mock<IUserService>();
            var mockedReportPrinter = new Mock<IReportPrinter>();

            Assert.Throws<ArgumentNullException>(() => new DealershipEngine(mockedCommandReader.Object, null, mockedReportPrinter.Object, mockedUserService.Object));
        }

        [Test]
        public void Constructor_WhenReportPrinterIsNull_ShouldThrow()
        {
            var mockedCommandReader = new Mock<ICommandReader>();
            var mockedCommandProcessor = new Mock<ICommandProcessor>();
            var mockedUserService = new Mock<IUserService>();

            Assert.Throws<ArgumentNullException>(() => new DealershipEngine(mockedCommandReader.Object, mockedCommandProcessor.Object, null, mockedUserService.Object));
        }

        [Test]
        public void Constructor_WhenUserServicerIsNull_ShouldThrow()
        {
            var mockedCommandReader = new Mock<ICommandReader>();
            var mockedReportPrinter = new Mock<IReportPrinter>();
            var mockedCommandProcessor = new Mock<ICommandProcessor>();

            Assert.Throws<ArgumentNullException>(() => new DealershipEngine(mockedCommandReader.Object, mockedCommandProcessor.Object, mockedReportPrinter.Object, null));
        }

        [Test]
        public void Start_ShouldReadCommands()
        {
            var mockedCommandReader = new Mock<ICommandReader>();
            mockedCommandReader.Setup(x => x.ReadCommands()).Returns(new List<ICommand>());
            var mockedCommandProcessor = new Mock<ICommandProcessor>();
            var mockedReportPrinter = new Mock<IReportPrinter>();
            var mockedUserService = new Mock<IUserService>();
            var engine = new DealershipEngine(mockedCommandReader.Object, mockedCommandProcessor.Object, mockedReportPrinter.Object, mockedUserService.Object);

            engine.Start();

            mockedCommandReader.Verify(x => x.ReadCommands(), Times.Once);
        }

        [Test]
        public void Start_ShouldProcessCommands()
        {
            var mockedCommandReader = new Mock<ICommandReader>();
            var mockedCommand = new Mock<ICommand>();
            mockedCommandReader.Setup(x => x.ReadCommands()).Returns(new List<ICommand>() { mockedCommand.Object });
            var mockedCommandProcessor = new Mock<ICommandProcessor>();
            mockedCommandProcessor.Setup(x => x.Process(mockedCommand.Object)).Returns(It.IsAny<string>());
            var mockedReportPrinter = new Mock<IReportPrinter>();
            var mockedUserService = new Mock<IUserService>();
            var engine = new DealershipEngine(mockedCommandReader.Object, mockedCommandProcessor.Object, mockedReportPrinter.Object, mockedUserService.Object);

            engine.Start();

            mockedCommandProcessor.Verify(x => x.Process(mockedCommand.Object), Times.Once);
        }

        [Test]
        public void Start_ShouldPrintReports()
        {
            var mockedCommandReader = new Mock<ICommandReader>();
            var mockedCommand = new Mock<ICommand>();
            mockedCommandReader.Setup(x => x.ReadCommands()).Returns(new List<ICommand>() { mockedCommand.Object });
            var mockedCommandProcessor = new Mock<ICommandProcessor>();
            mockedCommandProcessor.Setup(x => x.Process(mockedCommand.Object)).Returns("report");
            var mockedReportPrinter = new Mock<IReportPrinter>();
            mockedReportPrinter.Setup(x => x.PrintReports(It.IsAny<IEnumerable<string>>()));
            var mockedUserService = new Mock<IUserService>();
            var engine = new DealershipEngine(mockedCommandReader.Object, mockedCommandProcessor.Object, mockedReportPrinter.Object, mockedUserService.Object);

            engine.Start();

            mockedReportPrinter.Verify(x => x.PrintReports(It.Is<ICollection<string>>(t => t.Contains("report"))), Times.Once);
        }

        [Test]
        public void Start_WhenProcessComandThrows_ShouldPrintExceptionMessage()
        {
            var mockedCommandReader = new Mock<ICommandReader>();
            var mockedCommand = new Mock<ICommand>();
            mockedCommandReader.Setup(x => x.ReadCommands()).Returns(new List<ICommand>() { mockedCommand.Object });
            var mockedCommandProcessor = new Mock<ICommandProcessor>();
            mockedCommandProcessor.Setup(x => x.Process(mockedCommand.Object)).Throws(new Exception("exception message"));
            var mockedReportPrinter = new Mock<IReportPrinter>();
            mockedReportPrinter.Setup(x => x.PrintReports(It.IsAny<IEnumerable<string>>()));
            var mockedUserService = new Mock<IUserService>();
            var engine = new DealershipEngine(mockedCommandReader.Object, mockedCommandProcessor.Object, mockedReportPrinter.Object, mockedUserService.Object);

            engine.Start();

            mockedReportPrinter.Verify(x => x.PrintReports(It.Is<ICollection<string>>(t => t.Contains("exception message"))), Times.Once);
        }

        [Test]
        public void Reset_ShouldDeleteAllUsers()
        {
            var mockedCommandReader = new Mock<ICommandReader>();
            var mockedCommandProcessor = new Mock<ICommandProcessor>();
            var mockedReportPrinter = new Mock<IReportPrinter>();
            mockedReportPrinter.Setup(x => x.PrintReports(It.IsAny<IEnumerable<string>>()));
            var mockedUserService = new Mock<IUserService>();
            mockedUserService.Setup(x => x.DeleteAllUsers());
            var engine = new DealershipEngine(mockedCommandReader.Object, mockedCommandProcessor.Object, mockedReportPrinter.Object, mockedUserService.Object);

            engine.Reset();

            mockedUserService.Verify(x => x.DeleteAllUsers(), Times.Once);
        }
    }
}
