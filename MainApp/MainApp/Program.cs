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

    public class MethodInfo
    {
        public string name;
        public string className;
        public string time = "...";
        public List<MethodInfo> methods;
    }

    public class ThreadInfo
    {
        public int id;
        public string time = "...";
        public List<MethodInfo> methods;
    }

    public class TraceResult
    {
        public SortedDictionary<int, ThreadInfo> threads;
    }

    public class Tracer: ITracer, ISerializer
    {
        private class Pair
        {
            public Stopwatch methodWatch;
            public MethodInfo methodInfo;

            public Pair(Stopwatch _methodWatch, MethodInfo _methodInfo)
            {
                methodWatch = _methodWatch;
                methodInfo = _methodInfo;
            }
        }

        private TraceResult traceInfo;
        private Dictionary<int, Stack<Pair>> methodsStacks;

        public Tracer()
        {
            traceInfo = new TraceResult();
            methodsStacks = new Dictionary<int, Stack<Pair>>();
        }

        public void StartTrace()
        {
            var method = new StackTrace().GetFrame(1).GetMethod();
            int threadId = Thread.CurrentThread.ManagedThreadId;
            if (!traceInfo.threads.ContainsKey(threadId))
            {
                traceInfo.threads.Add(threadId, new ThreadInfo());
                methodsStacks.Add(threadId, new Stack<Pair>());
            }

            MethodInfo newInfo = new MethodInfo();
            newInfo.name = method.ToString();
            newInfo.className = method.ReflectedType.ToString();
            Stopwatch stopwatch = new Stopwatch();

            methodsStacks[threadId].Push(new Pair(stopwatch, newInfo));

            //add to result info

            StackTrace trace = new StackTrace();
            Console.WriteLine(trace.GetFrame(1).GetMethod().ReflectedType.ToString());
            Console.WriteLine(trace.GetFrame(1).GetMethod().ToString());
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Thread.Sleep(10);
            watch.Stop();
            Console.WriteLine(Math.Round(watch.ElapsedTicks * ((double)(1000L * 1000L) / Stopwatch.Frequency)));
        }

        public void StopTrace()
        {

        }

        public TraceResult GetTraceResult()
        {
            return traceInfo;
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
