using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameEventAnalytics;

namespace LibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var manager = new EventManager<CSVSerializer<Event>>("analytics", new Uri("http://127.0.0.1:8080/"));
            manager.AddEvent(new Event(1, new Dictionary<string, string>() { { "Date", DateTime.Now.ToString() }, { "Test1", "2324" } }));
            manager.AddEvent(new Event(2, new Dictionary<string, string>() { { "Date", DateTime.Now.ToString() }, { "Test2", "1235" } }));
            manager.AddEvent(new Event(3, new Dictionary<string, string>() { { "Date", DateTime.Now.ToString() }, { "Test3", "2626" }, { "sdfhyr", "26rgege26" } }));
            manager.AddEvent(new Event(4, new Dictionary<string, string>() { { "Date", DateTime.Now.ToString() }, { "Test4", "24426262626324" } }));
            manager.AddEvent(new Event(5, new Dictionary<string, string>() { { "Date", DateTime.Now.ToString() }, { "Test5", "23246262424" }, { "sdgsggs", "26r3326" } }));
            manager.AddEvent(new Event(6, new Dictionary<string, string>() { { "Date", DateTime.Now.ToString() }, { "Test6", "2324" } }));

            manager.Flush();
            Console.Read();
        }
    }
}
