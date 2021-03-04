using KXTNetStruct;
using KXTNetStruct.Struct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using 快迅通.Utils;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace 快迅通.Dialogs
{
    public sealed partial class AddNewAccount : ContentDialog, RequestSender
    {
        private readonly System.Timers.Timer OperationTimer;
        private readonly Guid RequestID;

        private KXTNetStruct.LoginRequest Request;

        public AddNewAccount()
        {
            RequestID = Guid.NewGuid();
            Request = new KXTNetStruct.LoginRequest();

            OperationTimer = new System.Timers.Timer
            {
                Interval = OperationTimeInterval,
                AutoReset = false
            };
            OperationTimer.Elapsed += OperationTimer_Trigger;

            this.InitializeComponent();
        }

        private void ContentDialog_OKButtonClick(object sender, RoutedEventArgs args)
        {
            if (0 == UserIDBox.Text.Length || 0 == UserPassword.Password.Length)
            {
                RunningDatas.ErrorNotify("用户名与密码不能为空");
                return;
            }

            if (!Utils.Utils.LoginUserCheck(UserIDBox.Text, out KXTNetStruct.Struct.LoginType type))
            {
                RunningDatas.ErrorNotify("无法识别的用户名");
                return;
            }

            Request.UserID = UserIDBox.Text;
            Request.UserPW = Utils.Security.SHA1Encode(UserPassword.Password);
            Request.UserIDType = type;

            RunningDatas.RequestTable.TryAdd(RequestID, this);
            RunningDatas.DataSender.Login(RequestID, Request);

            OperationTimer.Start();

            UserIDBox.IsEnabled = false;
            UserPassword.IsEnabled = false;
            OKButton.IsEnabled = false;
            CancelButton.IsEnabled = false;

            ProgressWaitBar.Visibility = Visibility.Visible;
            ProgressWaitBar.IsIndeterminate = true;
            ProgressWaitBar.ShowPaused = false;
            ProgressWaitBar.ShowError = false;
        }

        private void ContentDialog_CancelButtonClick(object sender, RoutedEventArgs args)
        {
            RunningDatas.RequestTable.TryRemove(RequestID, out _);
            Hide();
        }

        public void RequestCallback(object response)
        {
            OperationTimer.Stop();

            UserIDBox.IsEnabled = true;
            UserPassword.IsEnabled = true;
            OKButton.IsEnabled = true;
            CancelButton.IsEnabled = true;

            ProgressWaitBar.Visibility = Visibility.Collapsed;
            ProgressWaitBar.IsIndeterminate = false;
            ProgressWaitBar.ShowPaused = false;
            ProgressWaitBar.ShowError = false;

            LoginResponse res = response as LoginResponse;

            if (LoginResult.Success != res.LoginResult)
            {
                RunningDatas.ErrorNotify("用户无法验证登录");
                return;
            }

            LocalDB.AddAccount(Request.UserID, Request.UserPW, Request.UserIDType);

            _ = Dispatcher.RunAsync
                (
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler(() =>
                {
                    Hide();
                }
                ));
        }

        private void OperationTimer_Trigger(object sender, System.Timers.ElapsedEventArgs args)
        {
            _ = Dispatcher.RunAsync
                  (Windows.UI.Core.CoreDispatcherPriority.Normal,
                  new Windows.UI.Core.DispatchedHandler(() =>
                  {
                      UserIDBox.IsEnabled = true;
                      UserPassword.IsEnabled = true;
                      OKButton.IsEnabled = true;
                      CancelButton.IsEnabled = true;

                      ProgressWaitBar.Visibility = Visibility.Collapsed;
                      ProgressWaitBar.IsIndeterminate = false;
                      ProgressWaitBar.ShowPaused = false;
                      ProgressWaitBar.ShowError = false;
                  }));

            RunningDatas.RequestTable.TryRemove(RequestID, out _);

            RunningDatas.ErrorNotify("登录超时 请重试");
        }
        private const double OperationTimeInterval = 30000;
    }
}
