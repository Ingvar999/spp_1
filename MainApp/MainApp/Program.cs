using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace MainApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            Tracer tracer = new Tracer();
            tracer.StartTrace();
            Console.ReadKey();
        }
    }

    public interface ITracer
    {
        void StartTrace();
        void StopTrace();
        TraceResult GetTraceResult();
    }

    public interface ISerializer
    {
        MemoryStream GetJSON();
        MemoryStream GetXML();
    }

    public class TraceResult
    {

    }

    public class Tracer: ITracer, ISerializer
    {
        private TraceResult TraceInfo;

        public void StartTrace()
        {
            StackTrace trace = new StackTrace();
            Console.WriteLine(trace.GetFrame(1).GetMethod().ReflectedType.ToString());
            Console.WriteLine(trace.GetFrame(1).GetMethod().ToString());
            Stopwatch watch = new Stopwatch();
            watch.Start();
            int a = 6;
            int b = a * 90;
            int c = b * 60;
            watch.Stop();
            Console.WriteLine(watch.ElapsedTicks * ((1000L * 1000L * 1000L) / Stopwatch.Frequency));
        }

        public void StopTrace()
        {

        }

        public TraceResult GetTraceResult()
        {
            return TraceInfo;
        }

        public MemoryStream GetJSON()
        {
            MemoryStream stream = new MemoryStream();
            return stream;
        }

        public MemoryStream GetXML()
        {
            MemoryStream stream = new MemoryStream();
            return stream;
        }
    }

}
