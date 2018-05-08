using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEventAnalytics
{
    public class SerializerFactory<T> where T : Serializer
    {
        public static Serializer GetSerializer()
        {
            Serializer serializer = null;

            if (typeof(T) == typeof(XMLSerializer))
                serializer = new XMLSerializer();

            else if (typeof(T) == typeof(JSONSerializer))
                serializer = new JSONSerializer();

            else if (typeof(T) == typeof(CSVSerializer))
                serializer = new CSVSerializer();

            return serializer;
        }
    }

    public abstract class Serializer
    {

    }

    public class XMLSerializer : Serializer
    {

    }

    public class JSONSerializer : Serializer
    {

    }

    public class CSVSerializer : Serializer
    {

    }
}
