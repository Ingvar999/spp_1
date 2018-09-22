using System.IO;
using System;
using System.Text;

namespace TWriterLib
{
    public interface IWriter
    {
        void WriteToConsole(MemoryStream source);
        void WriteToFile(MemoryStream source, string fileName);
    }

    public class TWriter: IWriter
    {
        public void WriteToConsole(MemoryStream source)
        {
            Console.WriteLine(Encoding.UTF8.GetString(source.ToArray()));
        }

        public void WriteToFile(MemoryStream source, string fileName)
        {
            var fileStream = new FileStream(fileName, FileMode.OpenOrCreate);
            fileStream.Write(source.ToArray(), 0, (int)source.Length);
            fileStream.Close();
        }
    }
}
