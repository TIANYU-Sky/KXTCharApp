using IKXTServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using 快迅通_通讯服务器.Server;
using 快迅通_通讯服务器.Utils;

namespace 快迅通_通讯服务器
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IController
    {
        private bool ServerStarted;

        private ChatServer Server;

        public MainWindow()
        {
            InitializeComponent();

            ServerStarted = false;

            Server = null;
            (this as IController).ServerStateChanged(Utils.ServerState.STOPED);
        }

        private void GroupSelectButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folder = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "请选择用于存放群信息文件的根目录",
                ShowNewFolderButton = true
            };
            if (System.Windows.Forms.DialogResult.OK == folder.ShowDialog())
                GroupInfoRootPath.Text = folder.SelectedPath;
        }

        private void ChatSelectButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folder = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "请选择用于存放通讯记录文件的根目录",
                ShowNewFolderButton = true
            };
            if (System.Windows.Forms.DialogResult.OK == folder.ShowDialog())
                ChatRootPath.Text = folder.SelectedPath;
        }

        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (ServerStarted)
                StopServer();
            else
                StartServer();
        }
        private void StartServer()
        {
            if ("" == ServerIPAddress.Text || "" == ServerIPPort.Text)
            {
                System.Windows.MessageBox.Show("需要输入提供的接口IP地址与端口", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if ("" == GroupInfoRootPath.Text || "" == ChatRootPath.Text)
            {
                System.Windows.MessageBox.Show("需要选择用于操作的文件目录", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (int.TryParse(ServerIPPort.Text, out int port))
            {
                if (IPAddress.TryParse(ServerIPAddress.Text, out IPAddress address))
                {
                    try
                    {
                        Server = new ChatServer
                        (
                        new IPEndPoint(address, port),
                        GroupInfoRootPath.Text,
                        ChatRootPath.Text,
                        this
                        );

                        ServerStarted = true;

                        ServerIPAddress.IsReadOnly = true;
                        ServerIPPort.IsReadOnly = true;
                        GroupInfoRootPath.IsReadOnly = true;
                        ChatRootPath.IsReadOnly = true;

                        GroupSelectButton.IsEnabled = false;
                        ChatSelectButton.IsEnabled = false;

                        StartServerButton.Content = "停止服务";
                    }
                    catch (Exception e)
                    {
                        (this as IController).Notify(LogLevel.Error, e.Message);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("不能识别的IP地址", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("不能识别的IP地址端口", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        private void StopServer()
        {
            Server.StopServer();
            Server = null;
            (this as IController).ServerStateChanged(Utils.ServerState.STOPED);

            ServerStarted = false;

            ServerIPAddress.IsReadOnly = false;
            ServerIPPort.IsReadOnly = false;
            GroupInfoRootPath.IsReadOnly = false;
            ChatRootPath.IsReadOnly = false;

            GroupSelectButton.IsEnabled = true;
            ChatSelectButton.IsEnabled = true;

            StartServerButton.Content = "启动服务";
        }

        void IController.Notify(LogLevel level, string message)
        {
            NotifyList.Dispatcher.BeginInvoke
                (
                new Action
                (
                    () =>
                    {
                        NotifyList.Items.Add(new NotifyItem(new NotifyListData
                        {
                            Level = level,
                            Message = message
                        }));
                        //NotifyList.Items.Refresh();
                    }
                )
                );
        }
        void IController.ServerStateChanged(ServerState state)
        {
            string str = "";
            switch (state)
            {
                case Utils.ServerState.CONNECTED:
                    str = "已连接";
                    break;
                case Utils.ServerState.STOPED:
                    str = "服务未启动";
                    break;
                case Utils.ServerState.WAITING:
                    str = "等待连接";
                    break;
            }

            ServerState.Dispatcher.BeginInvoke
                (
                new Action(() => { ServerState.Text = str; })
                );
        }

        void IController.SetActuallPort(int port)
        {
            ServerIPPort.Dispatcher.BeginInvoke
                (
                new Action(() => { ServerIPPort.Text = port.ToString(); })
                );
        }
    }
}
