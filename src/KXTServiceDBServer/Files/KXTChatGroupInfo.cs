using IKXTServer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KXTServiceDBServer.Files
{
    public class KXTChatGroupInfoReader
    {
        private readonly string RootPath;
        private readonly Action<LogLevel, string> Notify;

        public KXTChatGroupInfoReader(string root, Action<LogLevel, string> notify)
        {
            RootPath = root;
            Notify = notify;
        }


        public List<string> ReadMembers(string group_id)
        {
            List<string> list = new List<string>();

            try
            {
                KXTJson Json = new KXTJson(RootPath + "\\" + group_id + ".json");

                JArray array = Json["member"] as JArray;
                foreach (var item in array)
                    list.Add(item.ToString());
            }
            catch
            {
                Notify(LogLevel.Warning, "群组信息数据操作异常：文件异常");
            }

            return list;
        }
    }
}
