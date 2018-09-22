using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace TracerLib
{
    public interface ITracer
    {
        void StartTrace();
        void StopTrace();
        TraceResult GetTraceResult();
    }

    [Serializable]
    public class MethodInfo: ICloneable
    {
        [DataMember]
        public string Name;
        [DataMember]
        public string ClassName;
        [DataMember]
        public uint Time;
        [DataMember]
        public List<MethodInfo> Methods;

        public MethodInfo()
        {
            Methods = new List<MethodInfo>();
            Time = 0;
        }

        public object Clone()
        {
            var res = new MethodInfo {
                ClassName = (string)this.ClassName.Clone(),
                Name = (string)this.Name.Clone(),
                Time = this.Time,
                Methods = new List<MethodInfo>()
            };
            foreach(var method in Methods)
            {
                res.Methods.Add((MethodInfo)method.Clone());
            }
            return res;
        }
    }

    [Serializable]
    public class ThreadInfo: ICloneable
    {
        [DataMember]
        public int Id;
        [DataMember]
        public uint Time;
        [DataMember]
        public List<MethodInfo> Methods;

        public ThreadInfo() { }

        public ThreadInfo(int _id)
        {
            Time = 0;
            Id = _id;
            Methods = new List<MethodInfo>();
        }

        public object Clone()
        {
            var res = new ThreadInfo
            {
                Id = this.Id,
                Time = this.Time,
                Methods = new List<MethodInfo>()
            };
            foreach (var method in Methods)
            {
                res.Methods.Add((MethodInfo)method.Clone());
            }
            return res;
        }
    }

    class InternalTraceResult
    {
        public ConcurrentDictionary<int, ThreadInfo> Threads;

        public InternalTraceResult()
        {
            Threads = new ConcurrentDictionary<int, ThreadInfo>();
        }
    }

    [Serializable]
    [DataContract]
    public class TraceResult
    {

        [DataMember]
        public readonly List<ThreadInfo> threads;

        public TraceResult() { }

        public TraceResult(List<ThreadInfo> source)
        {
            threads = source;
        }
    }

    public class Tracer : ITracer
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

        private InternalTraceResult traceInfo;
        private ConcurrentDictionary<int, Stack<Pair>> methodsStacks;

        public Tracer()
        {
            traceInfo = new InternalTraceResult();
            methodsStacks = new ConcurrentDictionary<int, Stack<Pair>>();
        }

        public void StartTrace()
        {
            var method = new StackTrace().GetFrame(1).GetMethod();
            int threadId = Thread.CurrentThread.ManagedThreadId;
            if (!traceInfo.Threads.ContainsKey(threadId))
            {
                traceInfo.Threads.TryAdd(threadId, new ThreadInfo(threadId));
                methodsStacks.TryAdd(threadId, new Stack<Pair>());
            }

            MethodInfo newInfo = new MethodInfo
            {
                Name = method.ToString(),
                ClassName = method.ReflectedType.ToString()
            };
            Stopwatch stopwatch = new Stopwatch();

            if (methodsStacks[threadId].Count == 0)
            {
                traceInfo.Threads[threadId].Methods.Add(newInfo);
            }
            else
            {
                methodsStacks[threadId].Peek().methodInfo.Methods.Add(newInfo);
            }
            methodsStacks[threadId].Push(new Pair(stopwatch, newInfo));
            stopwatch.Start();
        }

        public void StopTrace()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            Pair currentMethod = methodsStacks[threadId].Pop();
            currentMethod.methodWatch.Stop();
            currentMethod.methodInfo.Time = (uint)Math.Round(currentMethod.methodWatch.ElapsedTicks * (1000000d / Stopwatch.Frequency));
            if (methodsStacks[threadId].Count == 0)
            {
                traceInfo.Threads[threadId].Time += currentMethod.methodInfo.Time;
            }
        }

        public TraceResult GetTraceResult()
        {
            List<ThreadInfo> list = new List<ThreadInfo>();
            foreach (var thread in traceInfo.Threads)
            {
                list.Add((ThreadInfo)thread.Value.Clone());
            }
            return new TraceResult(list);
        }
    }
}

