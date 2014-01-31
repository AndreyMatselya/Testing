using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using System.Web.Mvc;

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

        public class BloggingContext : DbContext
        {
            public virtual DbSet<Blog> Blogs { get; set; }
            public virtual DbSet<Post> Posts { get; set; }
        }

        public class Blog
        {
            public int BlogId { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }

            public virtual List<Post> Posts { get; set; }
        }

        public class Post
        {
            public int PostId { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }

            public int BlogId { get; set; }
            public virtual Blog Blog { get; set; }
        }

        public class BlogService
        {
            private BloggingContext _context;

            public BlogService(BloggingContext context)
            {
                _context = context;
            }

            public Blog AddBlog(string name, string url)
            {
                var blog = _context.Blogs.Add(new Blog { Name = name, Url = url });
                _context.SaveChanges();

                return blog;
            }

            public List<Blog> GetAllBlogs()
            {
                var query = from b in _context.Blogs
                            orderby b.Name
                            select b;

                return query.ToList();
            }

            public async Task<List<Blog>> GetAllBlogsAsync()
            {
                var query = from b in _context.Blogs
                            orderby b.Name
                            select b;

                return await query.ToListAsync();
            }
        } 

        [Test]
        public void GetAllBlogs_orders_by_name()
        {
            var data = new List<Blog> 
            { 
                new Blog { Name = "BBB" }, 
                new Blog { Name = "ZZZ" }, 
                new Blog { Name = "AAA" }, 
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Blog>>();
            mockSet.As<IQueryable<Blog>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Blog>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Blog>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Blog>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<BloggingContext>();
            mockContext.Setup(c => c.Blogs).Returns(mockSet.Object);

            var service = new BlogService(mockContext.Object);
            var blogs = service.GetAllBlogs();

            Assert.AreEqual(3, blogs.Count);
            Assert.AreEqual("AAA", blogs[0].Name);
            Assert.AreEqual("BBB", blogs[1].Name);
            Assert.AreEqual("ZZZ", blogs[2].Name);
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
