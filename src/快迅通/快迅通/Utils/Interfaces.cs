using KXTNetStruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 快迅通.Utils
{
    public interface IPageReceive
    {
        string GetID { get; }
        void Receive(ChatMessage message);
    }

    public interface RequestSender
    {
        void RequestCallback(object response);
    }

    internal interface IMainPageCall
    {
        void CleanUIStack();
        void UpdateUI();
        void UpdatePhoto();
    }

    internal interface IDataSender
    {
        // 主服务器控制消息方法
        void Regist(Guid request_id);
        void Logout();

        // 登录服务器控制消息方法
        void Login(Guid request_id, LoginRequest request);
        bool RegistEmailReq(Guid request_id, RegistEmailReq req);
        bool RegistVerifyReq(Guid request_id, RegistVerifyReq req);
        bool RegistFinishReq(Guid request_id, RegistFinishReq req);

        bool UpdatePWEmailReq(Guid request_id, UpdatePWEmailReq req);
        bool UpdatePWVerifyReq(Guid request_id, UpdatePWVerifyReq req);
        bool UpdatePWFinishReq(Guid request_id, UpdatePWFinishReq req);

        void UpdatePhone(UpdatePhone update);
        void UpdateName(UpdateName update);
        void UpdateDescribe(UpdateDescribe update);
        void UpdatePicture(UpdatePicture update);
        bool UpdateEmailReq(Guid request_id, UpdateEmailReq req);
        bool UpdateEmailVerify(Guid request_id, UpdateEmailVerify verify);
        void UpdateGroupName(UpdateGroupName update);
        void UpdateGroupDescribe(UpdateGroupDescribe update);

        void UserInfoReq(Guid request_id, UserGroupInfoReq req);
        void GroupInfoReq(Guid request_id, UserGroupInfoReq req);
        void FriendsRequest(Guid request_id, FriendGroupApplyRequest request);
        void GroupsRequest(Guid request_id, FriendGroupApplyRequest request);
        void AppliesRequest(Guid request_id, FriendGroupApplyRequest request);
        void GroupMemberReq(Guid request_id, GroupMemberReq req);

        void CreateGroup(Guid request_id, CreateGroupReq req);
        void DeleteGroup(DeleteGroup delete);
        void ApplyUserReq(ApplyRequest request);
        void ApplyUserRes(ApplyResponse response);
        void DeleteUser(DeleteUser delete);
        void DropGroup(DeleteUser delete);

        void SearchReq(Guid request_id, SearchReq req);

        // 云文件服务器控制消息方法
        void CloudRequest(Guid request_id, CloudRequest request);
        void CreateFolder(CreateFolder create);
        void DeleteFolder(DeleteFolderFile delete);
        void DeleteFile(DeleteFolderFile delete);
        void FileUpload(Guid request_id, FileUploadReq req);
        void FileDownload(Guid request_id, FileDownloadReq req);
        void FileStreamReq(StreamReq req);
        void FileStreamRes(StreamRes res);
        void FileDownloadFinish(FileDownloadFinish finish);
        void FileUploadCancel(FileUploadCancel cancel);

        // 通讯服务器控制消息方法
        bool FriendChat(ChatMessage message);
        bool GroupChat(ChatMessage message);
        void FriendChatReq(Guid request_id, FriendChatsReq req);
        void GroupChatReq(Guid request_id, GroupChatsReq req);
        void ChatsFinish(ChatsFinishPackage finish);
    }
}
