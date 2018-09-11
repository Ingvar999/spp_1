using System;
using System.Text;
using System.IO;
using System.Threading;
using TracerLib;

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
                Console.WriteLine(Encoding.UTF8.GetString(source.ToArray()));
                fileStream.Write(source.ToArray(), 0, (int)source.Length);
                fileStream.Close();
            }
            Console.WriteLine();
            {
                var fileStream = new FileStream("XMLTrace.XML", FileMode.OpenOrCreate);
                MemoryStream source = tracer.GetXML();
                Console.WriteLine(Encoding.UTF8.GetString(source.ToArray()));
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
}
