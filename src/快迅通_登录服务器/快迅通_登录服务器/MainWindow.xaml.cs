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
using 快迅通_登录服务器.Server;
using 快迅通_登录服务器.Utils;

namespace 快迅通_登录服务器
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IController
    {
        private bool ServerStarted;

        private LoginServer Server;

        public MainWindow()
        {
            InitializeComponent();

            ServerStarted = false;

            Server = null;
            (this as IController).ServerStateChanged(Utils.ServerState.STOPED);
        }

        private void UserSelectButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folder = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "请选择用于存放用户信息文件的根目录",
                ShowNewFolderButton = true
            };
            if (System.Windows.Forms.DialogResult.OK == folder.ShowDialog())
                UserInfoRootPath.Text = folder.SelectedPath;
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
                Description = "请选择用于存放通讯信息文件的根目录",
                ShowNewFolderButton = true
            };
            if (System.Windows.Forms.DialogResult.OK == folder.ShowDialog())
                ChatRootPath.Text = folder.SelectedPath;
        }
        private void AppliesSelectButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folder = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "请选择用于存放申请信息文件的根目录",
                ShowNewFolderButton = true
            };
            if (System.Windows.Forms.DialogResult.OK == folder.ShowDialog())
                AppliesRootPath.Text = folder.SelectedPath;
        }
        private void UserIDPoolSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (11 != UserIDPool.Text.Length)
            {
                MessageBox.Show("无效的用户ID池数据 长度应为：11", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            byte[] buffer = new byte[11];
            for (int i = 0; i < 11; ++i)
            {
                if ('0' <= UserIDPool.Text[i] && '9' >= UserIDPool.Text[i])
                    buffer[i] = (byte)(UserIDPool.Text[i] - '0');
                else
                {
                    MessageBox.Show("无效的用户ID池数据 数据只能在范围：0-9", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            InternalTools.SetUserIDPool(buffer);
        }
        private void GroupIDPoolSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (8 != UserIDPool.Text.Length)
            {
                MessageBox.Show("无效的群ID池数据 长度应为：8", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            byte[] buffer = new byte[8];
            for (int i = 0; i < 8; ++i)
            {
                if ('0' <= UserIDPool.Text[i] && '9' >= UserIDPool.Text[i])
                    buffer[i] = (byte)(UserIDPool.Text[i] - '0');
                else
                {
                    MessageBox.Show("无效的群ID池数据 数据只能在范围：0-9", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            InternalTools.SetGroupIDPool(buffer);
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

            if (
                "" == UserInfoRootPath.Text 
                || "" == GroupInfoRootPath.Text
                || "" == ChatRootPath.Text
                || "" == AppliesRootPath.Text
                || "" == CloudRootPath.Text
                || "" == CloudInfoRootPath.Text
                )
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
                        Server = new LoginServer
                        (
                        new IPEndPoint(address, port),
                        UserInfoRootPath.Text,
                        GroupInfoRootPath.Text,
                        AppliesRootPath.Text,
                        ChatRootPath.Text,
                        CloudRootPath.Text,
                        CloudInfoRootPath.Text,
                        this
                        );

                        ServerStarted = true;

                        ServerIPAddress.IsReadOnly = true;
                        ServerIPPort.IsReadOnly = true;

                        UserInfoRootPath.IsReadOnly = true;
                        GroupInfoRootPath.IsReadOnly = true;
                        ChatRootPath.IsReadOnly = true;
                        AppliesRootPath.IsReadOnly = true;
                        CloudRootPath.IsReadOnly = true;
                        CloudInfoRootPath.IsReadOnly = true;
                        UserIDPool.IsReadOnly = true;
                        GroupIDPool.IsReadOnly = true;

                        UserSelectButton.IsEnabled = false;
                        GroupSelectButton.IsEnabled = false;
                        ChatSelectButton.IsEnabled = false;
                        AppliesSelectButton.IsEnabled = false;
                        CloudSelectButton.IsEnabled = false;
                        CloudInfoSelectButton.IsEnabled = false;
                        UserIDPoolSaveButton.IsEnabled = false;
                        GroupIDPoolSaveButton.IsEnabled = false;

                        StartServerButton.Content = "停止服务";
                    }
                    catch (Exception e)
                    {
                        Server = null;
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

            UserInfoRootPath.IsReadOnly = false;
            GroupInfoRootPath.IsReadOnly = false;
            ChatRootPath.IsReadOnly = false;
            AppliesRootPath.IsReadOnly = false;
            CloudRootPath.IsReadOnly = false;
            CloudInfoRootPath.IsReadOnly = false;
            UserIDPool.IsReadOnly = false;
            GroupIDPool.IsReadOnly = false;

            UserSelectButton.IsEnabled = true;
            GroupSelectButton.IsEnabled = true;
            ChatSelectButton.IsEnabled = true;
            AppliesSelectButton.IsEnabled = true;
            CloudSelectButton.IsEnabled = true;
            CloudInfoSelectButton.IsEnabled = true;
            UserIDPoolSaveButton.IsEnabled = true;
            GroupIDPoolSaveButton.IsEnabled = true;

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

        private void CloudSelectButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloudInfoSelectButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
