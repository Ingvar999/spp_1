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
        private Tracer tracer;
        private MyClass myObject;

        private void setup()
        {
            tracer = new Tracer();
            myObject = new MyClass(tracer);
        }

        [TestMethod]
        public void MethodTime_10000mks()
        {
            setup();
            float expected = 10000;

            tracer.StartTrace();
            myObject.MyMethod();
            tracer.StopTrace();

            float actual = tracer.GetTraceResult().threads[0].Methods[0].Time;
            Assert.IsTrue(actual >= expected);
        }

        [TestMethod]
        public void RecursiveMethodTime_10000mks()
        {
            setup();
            float expected = 10000;

            tracer.StartTrace();
            myObject.OtherMethod();
            tracer.StopTrace();

            float actual = tracer.GetTraceResult().threads[0].Methods[0].Methods[0].Time;
            Assert.IsTrue(actual >= expected);
        }

        [TestMethod]
        public void MethodName_MyMethod()
        {
            setup();
            string expected = "Void MyMethod()";

            myObject.MyMethod();

            string actual = tracer.GetTraceResult().threads[0].Methods[0].Name;
            Assert.AreEqual(expected, actual);            
        }

        [TestMethod]
        public void ClassName_MyClass()
        {
            setup();
            string expected = "TracerLibTests.MyClass";

            myObject.MyMethod();

            string actual = tracer.GetTraceResult().threads[0].Methods[0].ClassName;
            Assert.AreEqual(expected, actual);
        }
    }
}
