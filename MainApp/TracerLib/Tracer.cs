using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace TracerLib
{
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
        public uint time = 0;
        public List<MethodInfo> methods;

        public MethodInfo()
        {
            methods = new List<MethodInfo>();
        }
    }

    public class ThreadInfo
    {
        public int id;
        public uint time = 0;
        public List<MethodInfo> methods;

        public ThreadInfo(int _id)
        {
            id = _id;
            methods = new List<MethodInfo>();
        }
    }

    public class TraceResult
    {
        public SortedDictionary<int, ThreadInfo> threads;

        public TraceResult()
        {
            threads = new SortedDictionary<int, ThreadInfo>();
        }
    }

    public class Tracer : ITracer, ISerializer
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
                traceInfo.threads.Add(threadId, new ThreadInfo(threadId));
                methodsStacks.Add(threadId, new Stack<Pair>());
            }

            MethodInfo newInfo = new MethodInfo();
            newInfo.name = method.ToString();
            newInfo.className = method.ReflectedType.ToString();
            Stopwatch stopwatch = new Stopwatch();

            if (methodsStacks[threadId].Count == 0)
            {
                traceInfo.threads[threadId].methods.Add(newInfo);
            }
            else
            {
                methodsStacks[threadId].Peek().methodInfo.methods.Add(newInfo);
            }
            methodsStacks[threadId].Push(new Pair(stopwatch, newInfo));
            stopwatch.Start();
        }

        public void StopTrace()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            Pair currentMethod = methodsStacks[threadId].Pop();
            currentMethod.methodWatch.Stop();
            currentMethod.methodInfo.time = (uint)Math.Round(currentMethod.methodWatch.ElapsedTicks * (1000000d / Stopwatch.Frequency));
        }

        public TraceResult GetTraceResult()
        {
            return traceInfo;
        }

        private string GetLongTab(int tabNumber)
        {
            string res = "\t";
            for (int i = 1; i < tabNumber; ++i)
            {
                res += "\t";
            }
            return res;
        }

        private string GetMethodsJSON(List<MethodInfo> methods, int tab)
        {
            string res = GetLongTab(tab) + "\"methods\":\n";
            res += GetLongTab(tab + 1) + "{\n";
            foreach (var method in methods)
            {
                res += GetLongTab(tab + 2) + $"\"name\": \"{method.name}\"\n";
                res += GetLongTab(tab + 2) + $"\"class\": \"{method.className}\"\n";
                res += GetLongTab(tab + 2) + $"\"time\": \"{method.time}mks\"\n";
                res += GetMethodsJSON(method.methods, tab + 2);
            }
            res += GetLongTab(tab + 1) + "}\n";
            return res;
        }

        public MemoryStream GetJSON()
        {
            string res = "{\n\tthreads: [";
            foreach (var thread in traceInfo.threads)
            {
                res += GetLongTab(2) + "{\n";
                res += GetLongTab(3) + $"\"id\": \"{thread.Value.id}\"\n";
                res += GetLongTab(3) + $"\"time\": \"{thread.Value.time}mks\"\n";
                res += GetMethodsJSON(thread.Value.methods, 3);
            }
            res += "\t]\n}";
            MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(res));
            return stream;
        }

        private string GetMethodsXML(List<MethodInfo> methods, int tab)
        {
            string res = "";
            foreach (var method in methods)
            {
                res += GetLongTab(tab) + $"<method name=\"{method.name}\" class=\"{method.className}\" time=\"{method.time}mks\">\n";
                res += GetMethodsXML(method.methods, tab + 1);
                res += GetLongTab(tab) + "</method>\n";
            }
            return res;
        }

        public MemoryStream GetXML()
        {
            string res = "<root>\n";
            foreach (var thread in traceInfo.threads)
            {
                res += $"\t<thread id=\"{thread.Value.id.ToString()}\" time=\"{thread.Value.time}mks\">\n";
                res += GetMethodsXML(thread.Value.methods, 2);
                res += "\t</thread>\n";
            }
            res += "</root>";
            MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(res));
            return stream;
        }
    }
}

