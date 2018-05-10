using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using CsvHelper;
using System.IO;

namespace GameEventAnalytics
{
    public sealed class SerializerFactory<T, U> where T : Serializer<U>
                                                where U : List<Event>
    {
        public static Serializer<U> GetSerializer()
        {
            Type tType = typeof(T);
            Serializer<U> serializer = null;

            if (tType == typeof(XMLSerializer<U>))
                serializer = new XMLSerializer<U>();

            else if (tType == typeof(JSONSerializer<U>))
                serializer = new JSONSerializer<U>();

            else if (tType == typeof(CSVSerializer<U>))
                serializer = new CSVSerializer<U>();

            return serializer;
        }
    }

    public abstract class Serializer<U>
    {
        public abstract String Serialize(U input);
        public abstract U Deserialize(string serializedString);
    }

    public class XMLSerializer<U> : Serializer<U>
    {
        public XmlSerializer serializer = new XmlSerializer(typeof(U));

        public override U Deserialize(string serializedString)
        {
            U result = default(U);

            using (MemoryStream stream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(serializedString);
                writer.Flush();
                stream.Position = 0;

                result = (U)serializer.Deserialize(stream);
            }
            
            return result;
        }

        public override string Serialize(U input)
        {
            string result = null;

            using (MemoryStream stream = new MemoryStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                serializer.Serialize(stream, input);
                reader.ReadToEnd();
            }
            return result;
        }
    }

    public class JSONSerializer<U> : Serializer<U>
    {
        public override U Deserialize(string serializedString)
        {
            return JsonConvert.DeserializeObject<U>(serializedString);
        }

        public override string Serialize(U input)
        {
            return JsonConvert.SerializeObject(input);
        }
    }

    public class CSVSerializer<U> : Serializer<U>
    {
        public override U Deserialize(string serializedString)
        {
            U result = default(U);
            using (MemoryStream stream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(stream))
            using (StreamReader reader = new StreamReader(stream))
            {
                writer.Write(serializedString);
                writer.Flush();
                stream.Position = 0;
                
                CsvReader csvReader = new CsvReader(reader);
                result = csvReader.GetRecords<U>().First<U>();
            }
            return result;
        }

        public override string Serialize(U input)
        {
            string result = null;

            using (MemoryStream stream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(stream))
            using (StreamReader reader = new StreamReader(stream))
            {
                CsvWriter csvWriter = new CsvWriter(writer);
                csvWriter.WriteRecord<U>(input);

                result = reader.ReadToEnd();
            }

            return result;
        }
    }
}
