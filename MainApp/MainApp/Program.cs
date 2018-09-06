using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MainApp
{
    class Program
    {
        static void Main(string[] args)
        {
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
