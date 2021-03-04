using IKXTServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using 快迅通_文件服务器.Utils;

namespace 快迅通_文件服务器.Server
{
    internal class CloudServer
    {
        private readonly Socket ServerSocket;

        private readonly string InfoRootPath;
        private readonly string FileRootPath;
        private readonly IController Controller;

        private MainConnect Connect;

        public CloudServer
            (
            IPEndPoint service_point,
            string info_root_path,
            string file_root_path,
            IController controller
            )
        {
            Connect = null;

            Controller = controller;
            InfoRootPath = info_root_path;
            FileRootPath = file_root_path;

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
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                Socket main = ServerSocket.EndAccept(ar);
                Connect = new MainConnect
                    (
                    main,
                    InfoRootPath,
                    FileRootPath,
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
