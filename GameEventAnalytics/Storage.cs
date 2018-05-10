using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GameEventAnalytics
{
    class Storage<T> where T : Serializer<List<Event>>
    {
        public string fullPath { get; set; }

        private Serializer<List<Event>> serializer = SerializerFactory<T, List<Event>>.GetSerializer();
        private List<Event> buffer = new List<Event>();

        private int fileCount = 0;

        private const int MIN_EVENT_COUNT_TO_SEND = 10;
        private const int MAX_EVENT_COUNT_TO_STORE = 100;

        public Storage(string filename){
            Type tType = typeof(T);

            if (tType == typeof(JSONSerializer<List<Event>>))
            {
                fullPath = $"../Analytics/{filename}.json";
            }
            else if (tType == typeof(XMLSerializer<List<Event>>))
            {
                fullPath = $"../Analytics/{filename}.xml";
            }
            else if (tType == typeof(CSVSerializer<List<Event>>)){
                fullPath = $"../Analytics/{filename}.csv";
            }
        }

        public void AddEvent(Event currEvent)
        {
            if (buffer.Count >= MIN_EVENT_COUNT_TO_SEND)
            {
                Flush();
            }
            buffer.Add(currEvent);
        }

        public void Flush()
        {
            if (!File.Exists(fullPath))
                CreateNew();

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            using (StreamWriter writer = new StreamWriter(stream))
            using (StreamReader reader = new StreamReader(stream))
            {
                writer.AutoFlush = true;
                string fileText = reader.ReadToEnd();

                List<Event> eventList = serializer.Deserialize(fileText);
                eventList.AddRange(buffer);
                fileCount += buffer.Count;
                buffer.Clear();

                fileText = serializer.Serialize(eventList);
                writer.Write(fileText);
            }
        }

        private void CreateNew()
        {
            List<Event> emptyList = new List<Event>();
            string structure = serializer.Serialize(emptyList);

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(structure);
            }
        }
    }
}
