using IKXTServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 快迅通_登录服务器.Utils
{
    internal enum ServerState
    {
        STOPED,
        WAITING,
        CONNECTED
    }

    internal interface IController
    {
        void Notify(LogLevel level, string message);
        void ServerStateChanged(ServerState state);
        void SetActuallPort(int port);
    }
}
