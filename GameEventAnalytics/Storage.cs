using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace GameEventAnalytics
{
    public class EventManager<T> where T : Serializer<Event>
    {
        public string FullPath { get; private set; }
        public Uri ServerUri { get; set; }

        private Serializer<Event> serializer = SerializerFactory<T, Event>.GetSerializer();
        private List<Event> buffer = new List<Event>();

        private const int MIN_FLUSH_NUMBER = 2;
        private const int MIN_EVENT_NUMBER_TO_SEND = 4;
        private const int MAX_EVENT_COUNT_TO_STORE = 10;

        public EventManager(string filename, Uri serverUri){
            string extension = null;

            Type tType = typeof(T);

            if (tType == typeof(JSONSerializer<Event>))
            {
                extension = "json";
            }
            else if (tType == typeof(XMLSerializer<Event>))
            {
                extension = "xml";
            }
            else if (tType == typeof(CSVSerializer<Event>)){
                extension = "csv";
            }

            FullPath = $"../Analytics/{filename}.{extension}";
            ServerUri = serverUri;
        }

        public void AddEvent(Event currEvent)
        {
            if (buffer.Count >= MIN_FLUSH_NUMBER)
            {
                Flush();
            }
            buffer.Add(currEvent);
        }

        public void Flush()
        {
            if (!File.Exists(FullPath))
                CreateNew();

            int fileCount = 0;
            try
            {
                List<Event> eventList = null;
                string newFileText = null;

                using (FileStream ReadingStream = new FileStream(FullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(ReadingStream))
                    {
                        string fileText = reader.ReadToEnd();

                        eventList = serializer.Deserialize(fileText);
                        eventList.AddRange(buffer);
                        buffer.Clear();

                        if (fileCount >= MAX_EVENT_COUNT_TO_STORE)
                            eventList.RemoveRange(0, fileCount - MAX_EVENT_COUNT_TO_STORE);

                        fileCount = eventList.Count;

                        newFileText = serializer.Serialize(eventList);
                    }
                }

                using (FileStream WritingStream = new FileStream(FullPath, FileMode.Open))
                {
                    using (StreamWriter writer = new StreamWriter(WritingStream))
                    {
                        writer.AutoFlush = true;
                        writer.Write(newFileText);
                    }
                }
            }
            catch (System.Security.SecurityException)
            {
                Console.Out.WriteLine("File is already in use by another process");
            }

            if (fileCount >= MIN_EVENT_NUMBER_TO_SEND)
                SendToServer();
        }

        public void SendToServer()
        {
            bool isAvailable = IsInternetConnectionAvailable();
            if (!isAvailable)
                return;

             HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ServerUri);

             byte[] eventsTextBytes = null;

             using (FileStream fileStream = new FileStream(FullPath, FileMode.Open))
             using (StreamReader reader = new StreamReader(fileStream))
             {
                 eventsTextBytes = Encoding.UTF8.GetBytes(reader.ReadToEnd());
             }

             request.Method = "POST";
             request.ContentType = "application/x-www-form-urlencoded";

             using (var requestStream = request.GetRequestStream())
             {
                 requestStream.Write(eventsTextBytes, 0, eventsTextBytes.Length);
             }

             HttpWebResponse response = (HttpWebResponse)request.GetResponse();

             CreateNew();
        }

        private bool IsInternetConnectionAvailable()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ServerUri);
            request.Timeout = 500;
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException)
            {
                return false;
            }

            if (response.StatusCode == HttpStatusCode.OK)
                return true;
            return false;
        }

        private void CreateNew()
        {
            if (!Directory.Exists("../Analytics"))
                Directory.CreateDirectory("../Analytics");

            List<Event> emptyList = new List<Event>();
            string structure = serializer.Serialize(emptyList);

            using (FileStream stream = new FileStream(FullPath, FileMode.Create))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(structure);
            }
        }
    }
}
