using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Reflection;

namespace ClassLibrary.Tests
{
    [TestClass()]
    public class LibTests
    {
        [TestMethod()]
        public void GetVideoDurationTest()
        {
            Lib t = new Lib();
            Assert.AreEqual(new TimeSpan(0, 0, 3), t.GetVideoDuration(AppDomain.CurrentDomain.BaseDirectory + "\\film.mkv"));
            Assert.AreEqual(new TimeSpan(0), t.GetVideoDuration(AppDomain.CurrentDomain.BaseDirectory + "\\Test.txt"));
            
        }

        [TestMethod()]
        public void StopServiceTest()
        {
            Lib t = new Lib();
            Assert.ThrowsException<Exception>(() => t.StopService());
        }
    }
}