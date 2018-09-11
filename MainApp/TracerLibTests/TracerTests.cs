using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using TracerLib;

namespace TracerLibTests
{
    class MyClass
    {
        private Tracer tracer;

        public MyClass(Tracer _tracer)
        {
            tracer = _tracer;
        }

        public void MyMethod()
        {
            tracer.StartTrace();
            Thread.Sleep(10);
            tracer.StopTrace();
        }

        public void OtherMethod()
        {
            tracer.StartTrace();
            Thread.Sleep(10);
            MyMethod();
            tracer.StopTrace();
        }
    }

    [TestClass]
    public class TracerTests
    {
        [TestMethod]
        public void MethodTime_10000mks()
        {
            Tracer tracer = new Tracer();
            MyClass myObject = new MyClass(tracer);
            float expected = 10000;

            tracer.StartTrace();
            myObject.MyMethod();
            tracer.StopTrace();

            float actual = tracer.GetTraceResult().threads[Thread.CurrentThread.ManagedThreadId].methods[0].time;
            Assert.IsTrue(actual >= expected);
        }

        [TestMethod]
        public void RecursiveMethodTime_10000mks()
        {
            Tracer tracer = new Tracer();
            MyClass myObject = new MyClass(tracer);
            float expected = 10000;

            tracer.StartTrace();
            myObject.OtherMethod();
            tracer.StopTrace();

            float actual = tracer.GetTraceResult().threads[Thread.CurrentThread.ManagedThreadId].methods[0].methods[0].time;
            Assert.IsTrue(actual >= expected);
        }

        [TestMethod]
        public void MethodName_MyMethod()
        {
            Tracer tracer = new Tracer();
            MyClass myObject = new MyClass(tracer);
            string expected = "Void MyMethod()";

            myObject.MyMethod();

            string actual = tracer.GetTraceResult().threads[Thread.CurrentThread.ManagedThreadId].methods[0].name;
            Assert.AreEqual(expected, actual);            
        }

        [TestMethod]
        public void ClassName_MyClass()
        {
            Tracer tracer = new Tracer();
            MyClass myObject = new MyClass(tracer);
            string expected = "TracerLibTests.MyClass";

            myObject.MyMethod();

            string actual = tracer.GetTraceResult().threads[Thread.CurrentThread.ManagedThreadId].methods[0].className;
            Assert.AreEqual(expected, actual);
        }
    }
}
