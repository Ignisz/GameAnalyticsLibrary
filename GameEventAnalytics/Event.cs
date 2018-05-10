using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEventAnalytics
{
    [Serializable]
    public class Event
    {
        public int Id { get; set; }
        public Dictionary<string, string> EventParameters { get; set; }

        public Event(int id) : this(id, parameters: null) {}

        public Event(int id, Dictionary<string, string> parameters)
        {
            Id = id;
            EventParameters = parameters;
        }
    }
}
