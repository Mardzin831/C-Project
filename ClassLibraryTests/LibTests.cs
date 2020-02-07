using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;

namespace ClassLibrary.Tests
{
    [TestClass()]
    public class LibTests
    {
        [TestMethod()]
        public void GetVideoDurationTest()
        {
            Lib t = new Lib();
            Assert.AreEqual(new TimeSpan(0, 10, 49), t.GetVideoDuration(@"C:\Users\Marcin Nowak\Videos\film.mkv"));
            Assert.AreEqual(new TimeSpan(0), t.GetVideoDuration(@"C:\Users\Marcin Nowak\Videos\test.txt"));
            
        }

        [TestMethod()]
        public void StopServiceTest()
        {
            Lib t = new Lib();
            Assert.ThrowsException<Exception>(() => t.StopService());
        }

        [TestMethod()]
        public void StartServiceTest()
        {
            Lib t = new Lib();
            Assert.ThrowsException<ArgumentException>(() => t.StartService());
        }
    }
}