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
        // В XML сериализации не поддерживается Dictionary
        public List<EventKeyValuePair> EventParameters { get; set; }

        public Event() : this(0, new Dictionary<string, string>()) {}

        public Event(int id) : this(id, new Dictionary<string, string>()) {}

        public Event(int id, Dictionary<string, string> parameters)
        {
            Id = id;
            EventParameters = parameters.Select(x => new EventKeyValuePair(x.Key, x.Value)).ToList();
        }

        public void AddEvent(string key, string value)
        {
            EventParameters.Add(new EventKeyValuePair(key, value));
        }
    }
    //Special for XML
    [Serializable]
    public class EventKeyValuePair
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public EventKeyValuePair() : this(null, null) { }

        public EventKeyValuePair(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Key}:{Value}";
        }
    }
}
