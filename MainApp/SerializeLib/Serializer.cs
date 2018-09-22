using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace SerializeLib
{
    public interface ISerialize
    {
        MemoryStream Serialize(object source);
    }

    public class TJsonSerializer: ISerialize
    {
        public MemoryStream Serialize(object source)
        {
            MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(JsonConvert.SerializeObject(source, Formatting.Indented)));
            return stream;
        }
    }

    public class TXmlSerializer: ISerialize
    {
        public MemoryStream Serialize(object source)
        {
            MemoryStream stream = new MemoryStream();
            var formatter = new XmlSerializer(source.GetType());
            formatter.Serialize(stream, source);
            return stream;
        }
    }
}
