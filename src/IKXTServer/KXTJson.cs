using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace IKXTServer
{
    public enum KXTRootJsonType
    {
        Object,
        Array
    }

    public class KXTJson
    {
        private readonly string JsonFile;
        private readonly JObject JsonObject;

        public KXTJson(string json_file)
        {
            JsonFile = json_file;

            System.IO.StreamReader stream = System.IO.File.OpenText(json_file);
            JsonTextReader reader = new JsonTextReader(stream);

            JsonObject = JToken.ReadFrom(reader) as JObject;

            reader.Close();
            stream.Close();
        }
        public bool Write()
        {
            try
            {
                System.IO.File.WriteAllText
                (
                JsonFile,
                JsonConvert.SerializeObject(JsonObject)
                );
                return true;
            }
            catch
            {
                return false;
            }
        }

        public JToken this[string prop_name]
        {
            get
            {
                return JsonObject[prop_name];
            }
            set
            {
                JsonObject[prop_name] = value;
            }
        }
        public JToken this[object key]
        {
            get
            {
                return JsonObject[key];
            }
            set
            {
                JsonObject[key] = value;
            }
        }

        public static bool CreateFile(string path, KeyValuePair<string, KXTRootJsonType>[] pairs)
        {
            try
            {
                JObject obj = new JObject();
                if (null != pairs)
                {
                    foreach (var item in pairs)
                    {
                        obj.Add
                        (
                        item.Key,
                        item.Value == KXTRootJsonType.Object ? (JToken)new JObject() : (JToken)new JArray()
                        );
                    }
                }
                CreateFileSub(path, obj);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private static void CreateFileSub(string path, JObject json)
        {
            System.IO.File.WriteAllText
                (
                path,
                JsonConvert.SerializeObject(json)
                );
        }
    }
}
