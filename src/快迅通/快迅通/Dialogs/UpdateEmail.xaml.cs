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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace 快迅通.Dialogs
{
    internal enum UpdateEmailState
    {
        Email,
        Verification,
        Finish
    }

    public sealed partial class UpdateEmail : ContentDialog, RequestSender
    {
        private readonly Guid RequestID;
        private readonly System.Timers.Timer OperationTimer;

        private UpdateEmailState State;
        private Guid NextLabel;

        public UpdateEmail()
        {
            RequestID = Guid.NewGuid();
            NextLabel = Guid.Empty;

            this.InitializeComponent();

            OperationTimer = new System.Timers.Timer
            {
                Interval = OperationTimeInterval,
                AutoReset = false
            };
            OperationTimer.Elapsed += OperationTimer_Trigger;

            State = UpdateEmailState.Email;
            LoadGrid();
        }
        private void LoadGrid()
        {
            switch (State)
            {
                case UpdateEmailState.Email:
                    UpdateEmailFirstGrid.Visibility = Visibility.Visible;
                    UpdateEmailSecondGrid.Visibility = Visibility.Collapsed;
                    UpdateEmailThirdGrid.Visibility = Visibility.Collapsed;

                    CancelButton.Visibility = Visibility.Visible;
                    PreButton.Visibility = Visibility.Collapsed;
                    break;
                case UpdateEmailState.Verification:
                    UpdateEmailFirstGrid.Visibility = Visibility.Collapsed;
                    UpdateEmailSecondGrid.Visibility = Visibility.Visible;
                    UpdateEmailThirdGrid.Visibility = Visibility.Collapsed;

                    CancelButton.Visibility = Visibility.Collapsed;
                    PreButton.Visibility = Visibility.Visible;
                    break;
                case UpdateEmailState.Finish:
                    UpdateEmailFirstGrid.Visibility = Visibility.Collapsed;
                    UpdateEmailSecondGrid.Visibility = Visibility.Collapsed;
                    UpdateEmailThirdGrid.Visibility = Visibility.Visible;

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
            State = UpdateEmailState.Email;
            LoadGrid();
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            switch (State)
            {
                case UpdateEmailState.Email:
                    EmailNext();
                    break;
                case UpdateEmailState.Verification:
                    VerificationNext();
                    break;
                case UpdateEmailState.Finish:
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
            if (RunningDatas.DataSender.UpdateEmailReq
                (
                RequestID,
                new KXTNetStruct.UpdateEmailReq
                {
                    UserID = RunningDatas.UserID,
                    UserEmail = EmailBox.Text
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
            KXTNetStruct.UpdateEmailVerify req = new KXTNetStruct.UpdateEmailVerify
            {
                NextLabel = NextLabel
            };
            Array.Copy(req.Verifies, 0, value, 0, 8);
            if (RunningDatas.DataSender.UpdateEmailVerify
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

        public void RequestCallback(object response)
        {
            UpdateEmailResponse res = response as UpdateEmailResponse;

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
                case UpdateEmailState.Email:
                    EmailCallBack(res.NextLabel);
                    break;
                case UpdateEmailState.Verification:
                    VerificationCallBack(res.NextLabel);
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

            NextLabel = next;
            State = UpdateEmailState.Verification;

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
            State = UpdateEmailState.Finish;

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
