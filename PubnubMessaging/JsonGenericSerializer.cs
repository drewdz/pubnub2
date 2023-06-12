using Newtonsoft.Json;
using System;
using System.IO;

namespace PubnubMessaging
{
    public class JsonGenericSerializer : ISerializer
    {
        #region Fields

        private readonly JsonSerializer _Serializer;

        #endregion Fields

        #region Constructors

        public JsonGenericSerializer()
        {
            _Serializer = new JsonSerializer();
        }

        #endregion Constructors

        #region Operations

        public string Serialize<TItem>(TItem item)
        {
            using (var sw = new StringWriter())
            {
                using (var jw = new JsonTextWriter(sw))
                {
                    _Serializer.Serialize(jw, item);
                    jw.Flush();
                }
                return sw.ToString();
            }
        }

        public TItem Deserialize<TItem>(string json)
        {
            using (var tr = new StringReader(json))
            {
                using (var jr = new JsonTextReader(tr))
                {
                    return _Serializer.Deserialize<TItem>(jr);
                }
            }
        }

        public object Deserialize(Type type, string json)
        {
            using (var tr = new StringReader(json))
            {
                using (var jr = new JsonTextReader(tr))
                {
                    return _Serializer.Deserialize(jr, type);
                }
            }
        }

        #endregion Operations
    }
}
