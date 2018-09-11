using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using TracerLib;

namespace TracerLibTests
{
    [TestClass]
    public class TracerTests
    {
        [TestMethod]
        public void MethodTime10000mks()
        {
            Tracer tracer = new Tracer();
            float expected = 20000;
            float eps = 2000;

            tracer.StartTrace();
            Thread.Sleep(20);
            tracer.StopTrace();

            float actual = tracer.GetTraceResult().threads[Thread.CurrentThread.ManagedThreadId].methods[0].time;
            Assert.AreEqual(expected, actual, eps);
        }
    }
}
