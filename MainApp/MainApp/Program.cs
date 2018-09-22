using System;
using System.Text;
using System.IO;
using System.Threading;
using TracerLib;
using TWriterLib;
using SerializeLib;

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
            tracer.GetTraceResult().threads[0].Id = 0;
            var writer = new TWriter();
            {
                var formatter = new TXmlSerializer();
                writer.WriteToConsole(formatter.Serialize(tracer.GetTraceResult()));
                writer.WriteToFile(formatter.Serialize(tracer.GetTraceResult()), "XmlTrace.XML");
            }
            {
                var formatter = new TJsonSerializer();
                writer.WriteToConsole(formatter.Serialize(tracer.GetTraceResult()));
                writer.WriteToFile(formatter.Serialize(tracer.GetTraceResult()), "JsonTrace.json");
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
}
