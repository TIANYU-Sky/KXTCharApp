using IKXTServer;
using KXTServiceDBServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using 快迅通_登录服务器.Utils;

namespace 快迅通_登录服务器.Server
{
    internal class LoginServer
    {
        private readonly Socket ServerSocket;
        private readonly ISQLDataBase SQLDB;
        private readonly string UserInfoPath;
        private readonly string GroupInfoPath;
        private readonly string AppliesInfoPath;
        private readonly string ChatPath;
        private readonly string CloudPath;
        private readonly string CloudInfoPath;

        private readonly IController Controller;

        private MainConnect Connect;

        public LoginServer
            (
            IPEndPoint service_point,
            string user_info_path,
            string group_info_path,
            string applies_info_path,
            string chat_path,
            string cloud_path,
            string cloud_info_path,
            IController controller
            )
        {
            Controller = controller;

            Connect = null;

            try
            {
                SQLDB = new SQLDataBase(DBStaticData.DataBaseConnString);
                SQLDB.Open();
            }
            catch (Exception e)
            {
                throw e;
            }

            UserInfoPath = user_info_path;
            GroupInfoPath = group_info_path;
            AppliesInfoPath = applies_info_path;
            ChatPath = chat_path;
            CloudPath = cloud_path;
            CloudInfoPath = cloud_info_path;

            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(service_point);

            Controller.SetActuallPort((ServerSocket.LocalEndPoint as IPEndPoint).Port);

            ServerSocket.Listen(1000);
            ServerSocket.BeginAccept(OnReceive, null);

            Controller.ServerStateChanged(ServerState.WAITING);
            Controller.Notify(LogLevel.Info, "已在 " + ServerSocket.LocalEndPoint + " 上启动服务");
        }

        public void StopServer()
        {
            Controller.Notify(LogLevel.Info, "已在 " + ServerSocket.LocalEndPoint + " 上停止服务");

            if (null != Connect)
                Connect.Stop();
            ServerSocket.Close();

            SQLDB.Close();
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                Socket main = ServerSocket.EndAccept(ar);
                Connect = new MainConnect
                    (
                    main,
                    SQLDB,
                    UserInfoPath,
                    GroupInfoPath,
                    AppliesInfoPath,
                    ChatPath,
                    CloudPath,
                    CloudInfoPath,
                    Controller.Notify,
                    Close
                    );
                Controller.ServerStateChanged(ServerState.CONNECTED);
                Controller.Notify(LogLevel.Info, "已连接 " + main.RemoteEndPoint + " 上的远程服务");
            }
            catch
            {

            }
        }

        private void Close()
        {
            try
            {
                Controller.ServerStateChanged(ServerState.WAITING);
                Controller.Notify(LogLevel.Info, "已从远程服务中断开连接");
                ServerSocket.BeginAccept(OnReceive, null);
            }
            catch
            {

            }
        }
    }
}
