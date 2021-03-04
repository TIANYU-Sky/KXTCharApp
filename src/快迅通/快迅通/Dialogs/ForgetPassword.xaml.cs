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
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using 快迅通.Utils;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace 快迅通.Dialogs
{
    internal enum ForgetPWState
    {
        Email,
        Verification,
        Update,
        Finish
    }

    public sealed partial class ForgetPassword : ContentDialog, RequestSender
    {
        private readonly Guid RequestID;

        private System.Timers.Timer OperationTimer;
        private ForgetPWState State;
        private Guid[] NextLabel;

        public ForgetPassword()
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

            State = ForgetPWState.Email;
            LoadGrid();
        }
        private void LoadGrid()
        {
            switch (State)
            {
                case ForgetPWState.Email:
                    ForgetPWFirstGrid.Visibility = Visibility.Visible;
                    ForgetPWSecondGrid.Visibility = Visibility.Collapsed;
                    ForgetPWThirdGrid.Visibility = Visibility.Collapsed;
                    ForgetPWFourthGrid.Visibility = Visibility.Collapsed;

                    CancelButton.Visibility = Visibility.Visible;
                    PreButton.Visibility = Visibility.Collapsed;
                    break;
                case ForgetPWState.Verification:
                    ForgetPWFirstGrid.Visibility = Visibility.Collapsed;
                    ForgetPWSecondGrid.Visibility = Visibility.Visible;
                    ForgetPWThirdGrid.Visibility = Visibility.Collapsed;
                    ForgetPWFourthGrid.Visibility = Visibility.Collapsed;

                    CancelButton.Visibility = Visibility.Collapsed;
                    PreButton.Visibility = Visibility.Visible;
                    break;
                case ForgetPWState.Update:
                    ForgetPWFirstGrid.Visibility = Visibility.Collapsed;
                    ForgetPWSecondGrid.Visibility = Visibility.Collapsed;
                    ForgetPWThirdGrid.Visibility = Visibility.Visible;
                    ForgetPWFourthGrid.Visibility = Visibility.Collapsed;

                    CancelButton.Visibility = Visibility.Collapsed;
                    PreButton.Visibility = Visibility.Visible;
                    break;
                case ForgetPWState.Finish:
                    ForgetPWFirstGrid.Visibility = Visibility.Collapsed;
                    ForgetPWSecondGrid.Visibility = Visibility.Collapsed;
                    ForgetPWThirdGrid.Visibility = Visibility.Collapsed;
                    ForgetPWFourthGrid.Visibility = Visibility.Visible;

                    CancelButton.Visibility = Visibility.Collapsed;
                    PreButton.Visibility = Visibility.Collapsed;
                    NextButton.Content = "关闭";
                    break;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void PreButton_Click(object sender, RoutedEventArgs e)
        {
            switch (State)
            {
                case ForgetPWState.Verification:
                    State = ForgetPWState.Email;
                    break;
                case ForgetPWState.Update:
                    State = ForgetPWState.Verification;
                    break;
            }

            LoadGrid();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            switch (State)
            {
                case ForgetPWState.Email:
                    EmailNext();
                    break;
                case ForgetPWState.Verification:
                    VerificationNext();
                    break;
                case ForgetPWState.Update:
                    UpdateNext();
                    break;
                case ForgetPWState.Finish:
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
            if (RunningDatas.DataSender.UpdatePWEmailReq
                (
                RequestID,
                new KXTNetStruct.UpdatePWEmailReq
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
            KXTNetStruct.UpdatePWVerifyReq req = new KXTNetStruct.UpdatePWVerifyReq
            {
                NextLabel = NextLabel[0]
            };
            Array.Copy(req.Verifies, 0, value, 0, 8);
            if (RunningDatas.DataSender.UpdatePWVerifyReq
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
        private void UpdateNext()
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
            if (RunningDatas.DataSender.UpdatePWFinishReq
                (
                RequestID,
                new KXTNetStruct.UpdatePWFinishReq
                {
                    NextLabel = NextLabel[1],
                    Password = Utils.Security.SHA1Encode(UserPWBox.Password)
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

        public void RequestCallback(object response)
        {
            UpdatePWResponse res = response as UpdatePWResponse;

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
                case ForgetPWState.Email:
                    EmailCallBack(res.NextLabel);
                    break;
                case ForgetPWState.Verification:
                    VerificationCallBack(res.NextLabel);
                    break;
                case ForgetPWState.Update:
                    UpdateCallBack(res.NextLabel);
                    break;
            }
        }
        private void EmailCallBack(Guid next)
        {
            if (Guid.Empty == next)
            {
                RunningDatas.ErrorNotify("邮箱无效 无法找到对应用户");
                return;
            }

            NextLabel[0] = next;
            State = ForgetPWState.Verification;

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
            State = ForgetPWState.Update;

            _ = Dispatcher.RunAsync
                (
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler(() =>
                {
                    LoadGrid();
                }));
        }
        private void UpdateCallBack(Guid next)
        {
            State = ForgetPWState.Finish;

            _ = Dispatcher.RunAsync
                (
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler(() =>
                {
                    ResultBox.Text = Guid.Empty == next ? "密码修改失败" : "密码修改成功";
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
