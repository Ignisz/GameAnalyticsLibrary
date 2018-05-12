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
                                                where U : Event
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
        public abstract String Serialize(List<U> input);
        public abstract List<U> Deserialize(string serializedString);
    }

    public class XMLSerializer<U> : Serializer<U>
    {
        public XmlSerializer serializer = new XmlSerializer(typeof(List<U>));

        public override List<U> Deserialize(string serializedString)
        {
            List<U> result = null;

            using (MemoryStream stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(serializedString);
                writer.Flush();
                stream.Position = 0;

                StreamReader reader = new StreamReader(stream);
                result = (List<U>)serializer.Deserialize(stream);

                writer.Close();
                reader.Close();
            }
            
            return result;
        }

        public override string Serialize(List<U> input)
        {
            string result = null;

            using (MemoryStream stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                serializer.Serialize(writer, input);

                stream.Position = 0;

                StreamReader reader = new StreamReader(stream);
                result = reader.ReadToEnd();

                writer.Close();
                reader.Close();
            }
            return result;
        }
    }

    public class JSONSerializer<U> : Serializer<U>
    {
        public override List<U> Deserialize(string serializedString)
        {
            return JsonConvert.DeserializeObject<List<U>>(serializedString);
        }

        public override string Serialize(List<U> input)
        {
            return JsonConvert.SerializeObject(input);
        }
    }

    public class CSVSerializer<U> : Serializer<U> where U : Event
    {
        public override List<U> Deserialize(string serializedString)
        {
            List<U> result = new List<U>();
            using (MemoryStream stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.AutoFlush = true;

                writer.Write(serializedString);

                stream.Position = 0;

                StreamReader reader = new StreamReader(stream);
                CsvReader csvReader = new CsvReader(reader);
                csvReader.Configuration.HasHeaderRecord = false;

                ReadingContext context = csvReader.Parser.Context;
                string[] fields = null;

                while ((fields = csvReader.Parser.Read()) != null)
                {
                    Int32.TryParse(fields[0], out int res);
                    Event currentEvent = new Event(res);

                    for (int i = 1; i < fields.Length; i++)
                    {
                        string[] args = fields[i].Split(':');
                        currentEvent.AddEvent(args[0], args[1]);
                    }

                    result.Add((U)currentEvent);
                }

                writer.Close();
                reader.Close();
            }
            return result;
        }

        public override string Serialize(List<U> input)
        {
            string result = null;

            using (MemoryStream stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.AutoFlush = true;

                CsvWriter csvWriter = new CsvWriter(writer);
                csvWriter.Configuration.HasHeaderRecord = false;

                for (int i = 0; i < input.Count; i++)
                {
                    csvWriter.WriteRecord<U>(input[i]);
                    csvWriter.NextRecord();
                }
                stream.Position = 0;

                StreamReader reader = new StreamReader(stream);
                result = reader.ReadToEnd();

                writer.Close();
                reader.Close();
            }

            return result;
        }
    }
}
