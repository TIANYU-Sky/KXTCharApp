using KXTNetStruct;
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
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using 快迅通.Utils;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace 快迅通.Dialogs
{
    internal enum RegistState
    {
        Email,
        Verification,
        Regist,
        Finish
    }

    public sealed partial class RegisterNewAccount : ContentDialog, RequestSender
    {
        private readonly Guid RequestID;

        private System.Timers.Timer OperationTimer;
        private RegistState State;
        private Guid[] NextLabel;

        public RegisterNewAccount()
        {
            RequestID = Guid.NewGuid();
            NextLabel = new Guid[]
            {
                Guid.Empty,
                Guid.Empty
            };

            this.InitializeComponent();

            OperationTimer = new System.Timers.Timer
            {
                Interval = OperationTimeInterval,
                AutoReset = false
            };
            OperationTimer.Elapsed += OperationTimer_Trigger;

            State = RegistState.Email;
            LoadGrid();
        }
        private void LoadGrid()
        {
            switch (State)
            {
                case RegistState.Email:
                    RegisterFirstGrid.Visibility = Visibility.Visible;
                    RegisterSecondGrid.Visibility = Visibility.Collapsed;
                    RegisterThirdGrid.Visibility = Visibility.Collapsed;
                    RegisterFourthGrid.Visibility = Visibility.Collapsed;

                    CancelButton.Visibility = Visibility.Visible;
                    PreButton.Visibility = Visibility.Collapsed;
                    break;
                case RegistState.Verification:
                    RegisterFirstGrid.Visibility = Visibility.Collapsed;
                    RegisterSecondGrid.Visibility = Visibility.Visible;
                    RegisterThirdGrid.Visibility = Visibility.Collapsed;
                    RegisterFourthGrid.Visibility = Visibility.Collapsed;

                    CancelButton.Visibility = Visibility.Collapsed;
                    PreButton.Visibility = Visibility.Visible;
                    break;
                case RegistState.Regist:
                    RegisterFirstGrid.Visibility = Visibility.Collapsed;
                    RegisterSecondGrid.Visibility = Visibility.Collapsed;
                    RegisterThirdGrid.Visibility = Visibility.Visible;
                    RegisterFourthGrid.Visibility = Visibility.Collapsed;

                    CancelButton.Visibility = Visibility.Collapsed;
                    PreButton.Visibility = Visibility.Visible;
                    break;
                case RegistState.Finish:
                    RegisterFirstGrid.Visibility = Visibility.Collapsed;
                    RegisterSecondGrid.Visibility = Visibility.Collapsed;
                    RegisterThirdGrid.Visibility = Visibility.Collapsed;
                    RegisterFourthGrid.Visibility = Visibility.Visible;

                    CancelButton.Visibility = Visibility.Collapsed;
                    PreButton.Visibility = Visibility.Collapsed;
                    NextButton.Content = "关闭";
                    break;
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            switch (State)
            {
                case RegistState.Email:
                    EmailNext();
                    break;
                case RegistState.Verification:
                    VerificationNext();
                    break;
                case RegistState.Regist:
                    RegistNext();
                    break;
                case RegistState.Finish:
                    Hide();
                    break;
            }
        }
        private void EmailNext()
        {
            if (0 == EmailBox.Text.Length)
            {
                RunningDatas.ErrorNotify("注册邮箱不能为空");
                return;
            }

            if (!Utils.Utils.IsEmail(EmailBox.Text)) 
            {
                RunningDatas.ErrorNotify("注册邮箱无效");
                return;
            }

            RunningDatas.RequestTable.TryAdd(RequestID, this);
            if (RunningDatas.DataSender.RegistEmailReq
                (
                RequestID,
                new KXTNetStruct.RegistEmailReq
                {
                    Email = EmailBox.Text
                }
                ))
            {
                NextButton.Visibility = Visibility.Collapsed;
                RingControl.Visibility = Visibility.Visible;
                RingControl.IsActive = true;

                OperationTimer.Start();
            }
            else
                RunningDatas.InfoNotify("无法连接到网络");
        }
        private void VerificationNext()
        {
            if (0 == VerificationBox.Text.Length)
            {
                RunningDatas.ErrorNotify("验证码不能为空");
                return;
            }

            if (8 > VerificationBox.Text.Length)
            {
                RunningDatas.ErrorNotify("验证码无效");
                return;
            }

            if (!Utils.Utils.ConvertVerifyCode(VerificationBox.Text, out byte[] value)) 
            {
                RunningDatas.ErrorNotify("验证码无效");
                return;
            }

            RunningDatas.RequestTable.TryAdd(RequestID, this);
            KXTNetStruct.RegistVerifyReq req = new KXTNetStruct.RegistVerifyReq
            {
                NextLabel = NextLabel[0]
            };
            Array.Copy(req.Verifies, 0, value, 0, 8);
            if (RunningDatas.DataSender.RegistVerifyReq
                (
                RequestID,
                req
                ))
            {
                NextButton.Visibility = Visibility.Collapsed;
                RingControl.Visibility = Visibility.Visible;
                RingControl.IsActive = true;

                OperationTimer.Start();
            }
            else
                RunningDatas.InfoNotify("无法连接到网络");
        }
        private void RegistNext()
        {
            if (0 == UserPWBox.Password.Length || 0 == UserPWRBox.Password.Length)
            {
                RunningDatas.ErrorNotify("密码不能为空");
                return;
            }

            if (UserPWBox.Password != UserPWRBox.Password)
            {
                RunningDatas.ErrorNotify("两次输入的密码不同");
                return;
            }

            RunningDatas.RequestTable.TryAdd(RequestID, this);
            if (RunningDatas.DataSender.RegistFinishReq
                (
                RequestID,
                new KXTNetStruct.RegistFinishReq
                {
                    NextLabel=NextLabel[1],
                    Name=UserNameBox.Text,
                    Password=Utils.Security.SHA1Encode(UserPWBox.Password),
                }
                ))
            {
                NextButton.Visibility = Visibility.Collapsed;
                RingControl.Visibility = Visibility.Visible;
                RingControl.IsActive = true;

                OperationTimer.Start();
            }
            else
                RunningDatas.InfoNotify("无法连接到网络");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
        private void PreButton_Click(object sender, RoutedEventArgs e)
        {
            switch (State)
            {
                case RegistState.Verification:
                    State = RegistState.Email;
                    break;
                case RegistState.Regist:
                    State = RegistState.Verification;
                    break;
            }

            LoadGrid();
        }

        void RequestSender.RequestCallback(object response)
        {
            RegistResponse res = response as RegistResponse;

            OperationTimer.Stop();

            _ = Dispatcher.RunAsync
                (
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler(() =>
                {
                    NextButton.Visibility = Visibility.Visible;
                    RingControl.Visibility = Visibility.Collapsed;
                    RingControl.IsActive = false;
                }));

            switch (State)
            {
                case RegistState.Email:
                    EmailCallBack(res.NextLabel);
                    break;
                case RegistState.Verification:
                    VerificationCallBack(res.NextLabel);
                    break;
                case RegistState.Regist:
                    RegistCallBack(res.NextLabel);
                    break;
            }
        }
        private void EmailCallBack(Guid next)
        {
            if (Guid.Empty == next)
            {
                RunningDatas.ErrorNotify("邮箱无效或已绑定用户 请更换邮箱后重试");
                return;
            }

            NextLabel[0] = next;
            State = RegistState.Verification;

            _ = Dispatcher.RunAsync
                (
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler(() =>
                {
                    LoadGrid();
                }));
        }
        private void VerificationCallBack(Guid next)
        {
            if (Guid.Empty == next)
            {
                RunningDatas.ErrorNotify("验证码无效");
                return;
            }

            NextLabel[1] = next;
            State = RegistState.Regist;

            _ = Dispatcher.RunAsync
                (
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler(() =>
                {
                    LoadGrid();
                }));
        }
        private void RegistCallBack(Guid next)
        {
            if (Guid.Empty == next)
            {
                RunningDatas.ErrorNotify("注册失败 请稍后再试");
                return;
            }

            NextLabel[1] = next;
            State = RegistState.Finish;

            _ = Dispatcher.RunAsync
                (
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler(() =>
                {
                    UserIDBox.Text = IKXTServer.DataConvert.GetString(next);
                    LoadGrid();
                }));
        }

        private void OperationTimer_Trigger(object sender, System.Timers.ElapsedEventArgs args)
        {
            _ = Dispatcher.RunAsync
                  (Windows.UI.Core.CoreDispatcherPriority.Normal,
                  new Windows.UI.Core.DispatchedHandler(() =>
                  {
                      NextButton.Visibility = Visibility.Visible;
                      RingControl.Visibility = Visibility.Collapsed;
                      RingControl.IsActive = false;
                  }));

            RunningDatas.RequestTable.TryRemove(RequestID, out _);

            RunningDatas.ErrorNotify("登录超时 请重试");
        }
        private const double OperationTimeInterval = 30000;
    }
}
