using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Testing
{
    internal class Program
    {
        private static void Main(string[] args)
        {
         }
    }

    public interface ILoggerDependency
    {
        string GetCurrentDirectory();
        string GetDirectoryByLoggerName(string loggerName);
        string DefaultLogger { get; }
    }

    [TestFixture]
    public class GeneralTest
    {
        [Test]
        public void Test1()
        {
            var loggerDependency =
                Mock.Of<ILoggerDependency>(d => d.GetCurrentDirectory() == "D:\\Temp");
            var currentDirectory = loggerDependency.GetCurrentDirectory();
            Assert.That(currentDirectory, Is.EqualTo("D:\\Temp"));
        }

        [Test]
        public void Test2()
        {
            // Для любого аргумента метода GetDirectoryByLoggerName вернуть "C:\\Foo".
            ILoggerDependency loggerDependency = Mock.Of<ILoggerDependency>(
                ld => ld.GetDirectoryByLoggerName(It.IsAny<string>()) == "C:\\Foo");

            string directory = loggerDependency.GetDirectoryByLoggerName("хрен");

            Assert.That(directory, Is.EqualTo("C:\\Foo"));
        }

        [Test]
        public void Test3()
        {
            // Для любого аргумента метода GetDirectoryByLoggerName вернуть "C:\\Foo".
            Mock<ILoggerDependency> stub = new Mock<ILoggerDependency>();
            stub.Setup(x => x.GetDirectoryByLoggerName(It.IsAny<string>())).Returns<string>(x => "C:\\" + x);

            string loggerName = "SomeLogger";
            var logger = stub.Object;

            Assert.That(logger.GetDirectoryByLoggerName("хрен"), Is.EqualTo("C:\\" + "хрен"));
        }

        [Test]
        public void Test4()
        {
            
            var logger = Mock.Of<ILoggerDependency>(
                d => d.DefaultLogger == "DefaultLogger");

            var defaultLogger = logger.DefaultLogger;

            Assert.That(defaultLogger, Is.EqualTo("DefaultLogger"));
        }

        [Test]
        public void Test5()
        {
            ILoggerDependency logger =
                Mock.Of<ILoggerDependency>(
                    d => d.GetCurrentDirectory() == "D:\\Temp" &&
                         d.DefaultLogger == "DefaultLogger" &&
                         d.GetDirectoryByLoggerName(It.IsAny<string>()) == "C:\\Temp");

            Assert.That(logger.GetCurrentDirectory(), Is.EqualTo("D:\\Temp"));
            Assert.That(logger.DefaultLogger, Is.EqualTo("DefaultLogger"));
            Assert.That(logger.GetDirectoryByLoggerName("CustomLogger"), Is.EqualTo("C:\\Temp"));
        }

        [Test]
        public void Test6()
        {
            var mock = new Mock<ILogWriter>();
            var logger = new Logger(mock.Object);

            logger.WriteLine("Hello, logger!");

            // Проверяем, что вызвался метод Write нашего мока с любым аргументом
            mock.Verify(lw => lw.Write(It.IsAny<string>()),Times.AtLeast(2));
        }

        [Test]
        public void Test7()
        {
            var mock = new Mock<ILogWriter>();
            //mock.Setup(lw => lw.Write(It.IsAny<string>()));
            mock.Setup(lw => lw.SetLogger(It.IsAny<string>()));

            var logger = new Logger(mock.Object);
            //logger.WriteLine("Hello, logger!");

            mock.Verify(x=>x.SetLogger("dsfsd"));
        }

        [Test]
        public void Test8()
        {
            var logger = Mock.Of<ILoggerDependency>(
                ld => ld.GetCurrentDirectory() == "D:\\Temp"
                      && ld.DefaultLogger == "DefaultLogger");

            // Задаем более сложное поведение метода GetDirectoryByLoggerName
            // для возвращения разных результатов, в зависимости от аргумента
            
            Mock.Get(logger)
                .Setup(ld => ld.GetDirectoryByLoggerName(It.IsAny<string>()))
                .Returns<string>(loggerName => "C:\\" + loggerName);

            Assert.That(logger.GetCurrentDirectory(), Is.EqualTo("D:\\Temp"));
            Assert.That(logger.DefaultLogger, Is.EqualTo("DefaultLogger"));
            Assert.That(logger.GetDirectoryByLoggerName("Foo"), Is.EqualTo("C:\\Foo"));
            Assert.That(logger.GetDirectoryByLoggerName("Boo"), Is.EqualTo("C:\\Boo"));
        }

    }
    public interface ILogWriter
    {
        string GetLogger();
        void SetLogger(string logger);
        void Write(string message);
    }

    public class Logger
    {
        private readonly ILogWriter _logWriter;

        public Logger(ILogWriter logWriter)
        {
            _logWriter = logWriter;
        }

        public void  WriteLine(string message)
        {
            _logWriter.Write(message);
        }
    }
}
