using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Faux.Banque.Domain.Interfaces;
using System.Runtime.Serialization.Formatters;

namespace Faux.Banque.Domain.Storage
{
    public class JSONEventStoreSerializer : IEventStoreSerializer
    {
        JsonSerializer serializer;
        public JSONEventStoreSerializer()
        {
            serializer = new JsonSerializer();
            serializer.TypeNameHandling = TypeNameHandling.All;
            serializer.TypeNameAssemblyFormat = FormatterAssemblyStyle.Full;
        }
        public List<IEvent> DeserializeEvent(byte[] data)
        {
            List<IEvent> results = new List<IEvent>();
            using (MemoryStream stream = new MemoryStream(data))
            using (StreamReader reader = new StreamReader(stream))
            using (JsonReader jsonReader = new JsonTextReader(reader))
            {
                var result = serializer.Deserialize<IEvent[]>(jsonReader);
                results.AddRange(result);
            }
            return results;
        }

        public byte[] SerializeEvents(Interfaces.IEvent[] events)
        {
            byte[] results;

            using (MemoryStream stream = new MemoryStream(1000))
            using (StreamWriter writer = new StreamWriter(stream))
            using (JsonWriter jsonWriter = new JsonTextWriter(writer))
            {
                serializer.Serialize(jsonWriter, events);
                jsonWriter.Flush();
                stream.Position = 0;
                results = stream.ToArray();
            }
            return results;
        }
    }
}
