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
    public class GroupDetailMember : KXTNetStruct.GroupMemberRes.GroupMemberItem
    {
        public ImageSource UserPhoto
        {
            get
            {
                return Utils.Utils.GetPhoto(UserPicture);
            }
        }
    }

    public sealed partial class GroupDetailInfo : ContentDialog, RequestSender
    {
        public string GID
        {
            get
            {
                return GroupID.Text;
            }
            set
            {
                GroupID.Text = value ?? "";
            }
        }
        public string GName
        {
            get
            {
                return GroupName.Text;
            }
            set
            {
                GroupName.Text = value ?? "";
            }
        }
        public string GDescribe
        {
            get
            {
                return GroupDescribe.Text;
            }
            set
            {
                GroupDescribe.Text = value ?? "";
            }
        }
        public string GCreateTime
        {
            get
            {
                return GroupCreateTime.Text;
            }
            set
            {
                GroupCreateTime.Text = value ?? "";
            }
        }
        public string GCreateID
        {
            get
            {
                return GroupCreateID;
            }
            set
            {
                GroupCreateID = value ?? "";
                if (GroupCreateID == RunningDatas.UserID)
                {
                    IsOwner = true;
                    DeleteGroup.Content = "删除群";
                }
            }
        }
        public string GCreateName
        {
            get
            {
                return GroupCreater.Text;
            }
            set
            {
                GroupCreater.Text = value ?? "";
            }
        }

        public bool IsOwner { get; private set; }
        public List<GroupDetailMember> Members { get; }

        private string GroupCreateID;
        private readonly Guid RequestID;

        public GroupDetailInfo()
        {
            RequestID = Guid.NewGuid();
            Members = new List<GroupDetailMember>();
            IsOwner = false;

            this.InitializeComponent();

            RunningDatas.RequestTable.TryAdd(RequestID, this);
        }

        public new IAsyncOperation<ContentDialogResult> ShowAsync()
        {
            RunningDatas.DataSender.GroupMemberReq(RequestID, new KXTNetStruct.GroupMemberReq
            {
                GroupID = GID
            });

            return base.ShowAsync();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            RunningDatas.RequestTable.TryRemove(RequestID, out _);
        }
        private void DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            if (IsOwner)
            {
                RunningDatas.DataSender.DeleteGroup(new KXTNetStruct.DeleteGroup
                {
                    GroupID = GID
                });
            }
            else
            {
                RunningDatas.DataSender.DropGroup(new DeleteUser
                {
                    UserID = RunningDatas.UserID,
                    FriendID = GID
                });
            }

            RunningDatas.RequestTable.TryRemove(RequestID, out _);

            RunningDatas.GroupsTable.TryRemove(GID, out _);

            Utils.RunningDatas.UIStack.Pop();
            Utils.RunningDatas.MainPage.UpdateUI();

            Hide();
        }

        public void RequestCallback(object response)
        {
            GroupMemberRes res = response as GroupMemberRes;

            foreach (var item in res.Members)
                Members.Add(new GroupDetailMember
                {
                    UserID = item.UserID,
                    UserName = item.UserName,
                    UserPicture = item.UserPicture
                });

            _ = Dispatcher.RunAsync
                (
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                new Windows.UI.Core.DispatchedHandler(
                    () =>
                    {
                        GroupMemberList.ItemsSource = Members;
                    }
                ));
        }

    }
}
