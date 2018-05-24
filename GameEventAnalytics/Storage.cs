using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace GameEventAnalytics
{
    /// <summary>
    /// The class that stores and transfers to the server information about the game events
    /// </summary>
    /// <typeparam name="T">Type of serializer</typeparam>
    public class EventManager<T> where T : Serializer<Event>
    {
        /// <summary>
        /// Full path to the file where events will be stored
        /// </summary>
        public string FullPath { get; private set; }
        /// <summary>
        /// Uri of the analytics server
        /// </summary>
        public Uri ServerUri { get; set; }

        private Serializer<Event> serializer = SerializerFactory<T, Event>.GetSerializer();
        /// <summary>
        /// Temporary storage of events
        /// </summary>
        private List<Event> buffer = new List<Event>();

        private const int MIN_FLUSH_NUMBER = 2;
        private const int MIN_EVENT_NUMBER_TO_SEND = 4;
        private const int MAX_EVENT_COUNT_TO_STORE = 10;

        /// <summary>
        /// Initializes a manager of event, that will store the events in file with specified name 
        /// and transmit them to server with specified Uri
        /// </summary>
        /// <param name="filename">Name of file</param>
        /// <param name="serverUri">Uri of server</param>
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

        /// <summary>
        /// Adds event to the buffer. If it's full, manager stores them in the file
        /// </summary>
        /// <param name="currEvent">New event</param>
        public void AddEvent(Event currEvent)
        {
            if (buffer.Count >= MIN_FLUSH_NUMBER)
            {
                Flush();
            }
            buffer.Add(currEvent);
        }

        /// <summary>
        /// Clears the buffer and stores the events in the file
        /// </summary>
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

                        // Manager removes old events to store new ones
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

        /// <summary>
        /// Tries to send the event information to the server
        /// </summary>
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

        /// <summary>
        /// Checks if there is a connection with the specified server
        /// </summary>
        /// <returns>State of connection</returns>
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

        /// <summary>
        /// Creates a new directory where files with event infomation will be stored
        /// </summary>
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
