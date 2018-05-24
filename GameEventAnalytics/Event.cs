using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEventAnalytics
{
    /// <summary>
    /// Model of incoming event
    /// </summary>
    [Serializable]
    public class Event
    {
        /// <summary>
        /// Id of event
        /// </summary>
        public int Id { get; set; }
        // XML serialization doesn't support Dictionary
        /// <summary>
        /// List of event's parameters
        /// </summary>
        public List<EventKeyValuePair> EventParameters { get; set; }

        /// <summary>
        /// Initializes an event with 0 id and empty list of parameters
        /// </summary>
        public Event() : this(0, new Dictionary<string, string>()) {}

        /// <summary>
        /// Initializes an event with specified id and empty list of parameters
        /// </summary>
        /// <param name="id">Id of event</param>
        public Event(int id) : this(id, new Dictionary<string, string>()) {}

        /// <summary>
        /// Initializes an empty event with specified id and list of parameters
        /// </summary>
        /// <param name="id">Id of event</param>
        /// <param name="parameters">Parameters of event</param>
        public Event(int id, Dictionary<string, string> parameters)
        {
            Id = id;
            EventParameters = parameters.Select(x => new EventKeyValuePair(x.Key, x.Value)).ToList();
        }

        /// <summary>
        /// Adds a new pair of parameters to list
        /// </summary>
        /// <param name="key">A key of new pair</param>
        /// <param name="value">A value of new pair</param>
        public void AddParamsPair(string key, string value)
        {
            EventParameters.Add(new EventKeyValuePair(key, value));
        }
    }

    /// <summary>
    /// Class for emulating Dictionary key-value pairs for XML serialization
    /// </summary>
    [Serializable]
    public class EventKeyValuePair
    {
        /// <summary>
        /// The key of pair
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// The value of pair
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Initializes an pair with nulls
        /// </summary>
        public EventKeyValuePair() : this(null, null) { }

        /// <summary>
        /// Initializes a pair with specified key and value
        /// </summary>
        /// <param name="key">Key of pair</param>
        /// <param name="value">Value of pair</param>
        public EventKeyValuePair(string key, string value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Returns a string in the following format: "Key:Value"
        /// </summary>
        public override string ToString()
        {
            return $"{Key}:{Value}";
        }
    }
}
