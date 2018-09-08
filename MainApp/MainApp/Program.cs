using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization.Json;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace MainApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Tracer tracer = new Tracer();
            tracer.StartTrace();
            AnyClass anyObject = new AnyClass(tracer);
            anyObject.AnyMethod();
            tracer.StopTrace();
            {
                var fileStream = new FileStream("JsonTrace.JSON", FileMode.OpenOrCreate);
                MemoryStream source = tracer.GetJSON();
                fileStream.Write(source.ToArray(), 0, (int)source.Length);
                fileStream.Close();
            }
            {
                var fileStream = new FileStream("XMLTrace.XML", FileMode.OpenOrCreate);
                MemoryStream source = tracer.GetXML();
                fileStream.Write(source.ToArray(), 0, (int)source.Length);
                fileStream.Close();
            }
            Console.ReadKey();
        }
    }

    public class AnyClass
    {
        private ITracer tracer;

        public AnyClass(ITracer _tracer)
        {
            tracer = _tracer;
        }

        public void AnyMethod()
        {
            tracer.StartTrace();
            Thread.Sleep(10);
            tracer.StopTrace();
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

    [Serializable]
    public class MethodInfo
    {
        [DataMember]
        public string name;
        [DataMember]
        public string className;
        [DataMember]
        public string time = "...";
        [DataMember]
        public List<MethodInfo> methods;

        public MethodInfo()
        {
            methods = new List<MethodInfo>();
        }
    }

    [Serializable]
    public class ThreadInfo
    {
        [DataMember]
        public int id;
        [DataMember]
        public string time = "...";
        [DataMember]
        public List<MethodInfo> methods;

        public ThreadInfo(int _id)
        {
            id = _id;
            methods = new List<MethodInfo>();
        }
    }

    [Serializable]
    [DataContract]
    public class TraceResult
    {
        [DataMember]
        public SortedDictionary<int, ThreadInfo> threads;

        public TraceResult()
        {
            threads = new SortedDictionary<int, ThreadInfo>();
        }
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
            currentMethod.methodInfo.time = Math.Round(currentMethod.methodWatch.ElapsedTicks * (1000000d / Stopwatch.Frequency)).ToString() + "μs";
        }

        public TraceResult GetTraceResult()
        {
            return traceInfo;
        }

        public MemoryStream GetJSON()
        {
            MemoryStream stream = new MemoryStream();
            var formatter = new DataContractJsonSerializer(traceInfo.GetType());
            formatter.WriteObject(stream, traceInfo);
            return stream;
        }

        public MemoryStream GetXML()
        {
            MemoryStream stream = new MemoryStream();
            var formatter = new XmlSerializer(traceInfo.GetType());
            formatter.Serialize(stream, traceInfo);
            return stream;
        }
    }

}
