using KXTNetStruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Documents;
using 快迅通.Utils;

namespace 快迅通.Server
{
    internal enum ServerConnectResult
    {
        Success,
        SocketError,
        SystemError
    }

    internal class ClientConnect : IDisposable, IDataSender
    {
        private readonly byte[] Buffer;
        private readonly Socket Server;

        private bool IsConnect
        {
            get
            {
                bool result = false;
                if (0 != Interlocked.Exchange(ref ConnectLock, 1))
                    Thread.Sleep(10);

                result = is_connnect;

                Interlocked.Exchange(ref ConnectLock, 0);

                return result;
            }
            set
            {
                if (0 != Interlocked.Exchange(ref ConnectLock, 1))
                    Thread.Sleep(10);

                is_connnect = value;

                Interlocked.Exchange(ref ConnectLock, 0);
            }
        }
        private bool is_connnect;
        private int ConnectLock;

        public ClientConnect()
        {
            is_connnect = true;
            ConnectLock = 0;

            Buffer = new byte[Datagram.DatagramLengthMax];
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public ServerConnectResult Connect(IPEndPoint point)
        {
            try
            {
                if (!IsConnect)
                {
                    Server.Connect(point);
                    Server.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnReceice, null);

                    is_connnect = true;
                }

                return ServerConnectResult.Success;
            }
            catch (SocketException)
            {
                return ServerConnectResult.SocketError;
            }
            catch
            {
                return ServerConnectResult.SystemError;
            }
        }
        public void Disconnect()
        {
            if (0 != Interlocked.Exchange(ref ConnectLock, 1))
                Thread.Sleep(10);

            try
            {
                if (is_connnect)
                {
                    is_connnect = false;
                    Server.Disconnect(true);
                }
            }
            catch
            {

            }

            Interlocked.Exchange(ref ConnectLock, 0);
        }

        private void OnReceice(IAsyncResult ar)
        {
            try
            {
                int count = Server.EndReceive(ar);
                if (Datagram.DatagramDataHead <= count)
                {
                    Action<byte[]> action = OnProcess;
                    byte[] buffer = new byte[count];
                    Array.Copy(Buffer, 0, buffer, 0, count);
                    action.BeginInvoke(buffer, null, null);
                }
            }
            catch
            {

            }

            try
            {
                if (IsConnect)
                {
                    Server.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnReceice, null);
                }
            }
            catch
            {
                if (IsConnect)
                {
                    Disconnect();
                    RunningDatas.ErrorNotify("程序错误 无法接收网络消息 请重新启动软件后重试");
                }
            }
        }
        private void OnProcess(byte[] buffer)
        {
            Datagram datagram = new Datagram();
            
            if (datagram.FromBytes_S(buffer, 0))
            {
                switch (datagram.DataType)
                {
                    case DatagramType.Chat:
                        OnChat(datagram);
                        break;
                    case DatagramType.Cloud:
                        OnCloud(datagram);
                        break;
                    case DatagramType.Login:
                        OnLogin(datagram);
                        break;
                    case DatagramType.Main:
                        OnMain(datagram);
                        break;
                }
            }
        }

        private void OnChat(Datagram datagram)
        {
            switch (datagram.MessageType)
            {
                case ChatDatagramDefine.FriendChat:
                    OnChat_FriendChat(datagram);
                    break;
                case ChatDatagramDefine.GroupChat:
                    OnChat_GroupChat(datagram);
                    break;
                case ChatDatagramDefine.FriendChatsRes:
                    OnChat_FriendChatsRes(datagram);
                    break;
                case ChatDatagramDefine.GroupChatsRes:
                    OnChat_GroupChatsRes(datagram);
                    break;
            }
        }
        private void OnChat_FriendChat(Datagram datagram)
        {
            ChatMessage message = datagram.UnSerialData<ChatMessage>();

            RunningDatas.ReceiveMessage(ChatPackageType.Friend, message);
        }
        private void OnChat_GroupChat(Datagram datagram)
        {
            ChatMessage message = datagram.UnSerialData<ChatMessage>();

            RunningDatas.ReceiveMessage(ChatPackageType.Group, message);
        }
        private void OnChat_FriendChatsRes(Datagram datagram)
        {
            FriendChatsRes res = datagram.UnSerialData<FriendChatsRes>();

            if (RunningDatas.RequestTable.TryGetValue(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }
        private void OnChat_GroupChatsRes(Datagram datagram)
        {
            GroupChatPackage res = datagram.UnSerialData<GroupChatPackage>();

            if (RunningDatas.RequestTable.TryGetValue(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }

        private void OnCloud(Datagram datagram)
        {
            switch (datagram.MessageType)
            {
                case CloudDatagramDefine.CloudResponse:
                    OnCloud_CloudResponse(datagram);
                    break;
                case CloudDatagramDefine.CloudResFinish:
                    OnCloud_CloudResFinish(datagram);
                    break;
                case CloudDatagramDefine.FileUploadRes:
                    OnCloud_FileUploadRes(datagram);
                    break;
                case CloudDatagramDefine.FileDownloadRes:
                    OnCloud_FileDownloadRes(datagram);
                    break;
                case CloudDatagramDefine.StreamReq:
                    OnCloud_StreamReq(datagram);
                    break;
                case CloudDatagramDefine.StreamRes:
                    OnCloud_StreamRes(datagram);
                    break;
                case CloudDatagramDefine.FileUploadFinish:
                    OnCloud_FileUploadFinish(datagram);
                    break;
            }
        }
        private void OnCloud_FileUploadFinish(Datagram datagram)
        {
            FileUploadFinish finish = datagram.UnSerialData<FileUploadFinish>();

            RunningDatas.FileService.FinishUpload(finish.UploadID);
        }
        private void OnCloud_StreamRes(Datagram datagram)
        {
            StreamRes stream = datagram.UnSerialData<StreamRes>();
            RunningDatas.FileService.Receive(stream.StreamID, stream.Block, stream.Stream);
        }
        private void OnCloud_StreamReq(Datagram datagram)
        {
            StreamReq stream = datagram.UnSerialData<StreamReq>();
            RunningDatas.FileService.Request(stream.StreamID, stream.Block);
        }
        private void OnCloud_FileDownloadRes(Datagram datagram)
        {
            FileDownloadRes response = datagram.UnSerialData<FileDownloadRes>();

            if (RunningDatas.RequestTable.TryRemove(datagram.RequestID, out RequestSender value))
                value.RequestCallback(response);
        }
        private void OnCloud_FileUploadRes(Datagram datagram)
        {
            FileUploadRes response = datagram.UnSerialData<FileUploadRes>();

            if (RunningDatas.RequestTable.TryRemove(datagram.RequestID, out RequestSender value))
                value.RequestCallback(response);
        }
        private void OnCloud_CloudResponse(Datagram datagram)
        {
            CloudResponse response = datagram.UnSerialData<CloudResponse>();

            if (RunningDatas.RequestTable.TryGetValue(datagram.RequestID, out RequestSender value))
                value.RequestCallback(response);
        }
        private void OnCloud_CloudResFinish(Datagram datagram)
        {
            if (RunningDatas.RequestTable.TryGetValue(datagram.RequestID, out RequestSender value))
                value.RequestCallback(null);
        }

        private void OnLogin(Datagram datagram)
        {
            switch (datagram.MessageType)
            {
                case LoginMessageType.LoginResult:
                    OnLogin_LoginResult(datagram);
                    break;

                case LoginMessageType.RegistEmailRes:
                    OnLogin_RegistEmailRes(datagram);
                    break;
                case LoginMessageType.RegistVerifyRes:
                    OnLogin_RegistVerifyRes(datagram);
                    break;
                case LoginMessageType.RegistFinishRes:
                    OnLogin_RegistFinishRes(datagram);
                    break;

                case LoginMessageType.UpdatePWEmailRes:
                    OnLogin_UpdatePWEmailRes(datagram);
                    break;
                case LoginMessageType.UpdatePWVerifyRes:
                    OnLogin_UpdatePWVerifyRes(datagram);
                    break;
                case LoginMessageType.UpdatePWFinishRes:
                    OnLogin_UpdatePWFinishRes(datagram);
                    break;

                case LoginMessageType.UpdateEmailRes:
                    OnLogin_UpdateEmailRes(datagram);
                    break;
                case LoginMessageType.UpdateEmailResult:
                    OnLogin_UpdateEmailResult(datagram);
                    break;

                case LoginMessageType.UserInfoRes:
                    OnLogin_UserInfoRes(datagram);
                    break;
                case LoginMessageType.GroupInfoRes:
                    OnLogin_GroupInfoRes(datagram);
                    break;
                case LoginMessageType.FriendsResponse:
                    OnLogin_FriendsResponse(datagram);
                    break;
                case LoginMessageType.GroupsResponse:
                    OnLogin_GroupsResponse(datagram);
                    break;
                case LoginMessageType.AppliesResponse:
                    OnLogin_AppliesResponse(datagram);
                    break;
                case LoginMessageType.GroupMemberRes:
                    OnLogin_GroupMemberRes(datagram);
                    break;

                case LoginMessageType.CreateGroupRes:
                    OnLogin_CreateGroupRes(datagram);
                    break;

                case LoginMessageType.SearchRes:
                    OnLogin_SearchRes(datagram);
                    break;
            }
        }
        private void OnLogin_SearchRes(Datagram datagram)
        {
            SearchRes res = datagram.UnSerialData<SearchRes>();

            if (RunningDatas.RequestTable.TryGetValue(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }
        private void OnLogin_CreateGroupRes(Datagram datagram)
        {
            CreateGroupRes res = datagram.UnSerialData<CreateGroupRes>();

            if (RunningDatas.RequestTable.TryGetValue(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }
        private void OnLogin_GroupMemberRes(Datagram datagram)
        {
            GroupMemberRes res = datagram.UnSerialData<GroupMemberRes>();

            if (RunningDatas.RequestTable.TryGetValue(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }
        private void OnLogin_AppliesResponse(Datagram datagram)
        {
            AppliesResponse res = datagram.UnSerialData<AppliesResponse>();

            if (RunningDatas.RequestTable.TryGetValue(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }
        private void OnLogin_GroupsResponse(Datagram datagram)
        {
            GroupsResponse res = datagram.UnSerialData<GroupsResponse>();

            if (RunningDatas.RequestTable.TryGetValue(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }
        private void OnLogin_FriendsResponse(Datagram datagram)
        {
            FriendsResponse res = datagram.UnSerialData<FriendsResponse>();

            if (RunningDatas.RequestTable.TryGetValue(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }
        private void OnLogin_GroupInfoRes(Datagram datagram)
        {
            GroupInfoRes res = datagram.UnSerialData<GroupInfoRes>();

            if (RunningDatas.RequestTable.TryGetValue(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }
        private void OnLogin_UserInfoRes(Datagram datagram)
        {
            UserInfoRes res = datagram.UnSerialData<UserInfoRes>();

            if (RunningDatas.RequestTable.TryGetValue(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }
        private void OnLogin_UpdateEmailResult(Datagram datagram)
        {
            UpdateEmailResponse res = datagram.UnSerialData<UpdateEmailResponse>();

            if (RunningDatas.RequestTable.TryRemove(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }
        private void OnLogin_UpdateEmailRes(Datagram datagram)
        {
            UpdateEmailResponse res = datagram.UnSerialData<UpdateEmailResponse>();

            if (RunningDatas.RequestTable.TryRemove(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }
        private void OnLogin_UpdatePWFinishRes(Datagram datagram)
        {
            UpdatePWResponse res = datagram.UnSerialData<UpdatePWResponse>();

            if (RunningDatas.RequestTable.TryRemove(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }
        private void OnLogin_UpdatePWVerifyRes(Datagram datagram)
        {
            UpdatePWResponse res = datagram.UnSerialData<UpdatePWResponse>();

            if (RunningDatas.RequestTable.TryRemove(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }
        private void OnLogin_UpdatePWEmailRes(Datagram datagram)
        {
            UpdatePWResponse res = datagram.UnSerialData<UpdatePWResponse>();

            if (RunningDatas.RequestTable.TryRemove(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }
        private void OnLogin_RegistFinishRes(Datagram datagram)
        {
            RegistResponse res = datagram.UnSerialData<RegistResponse>();

            if (RunningDatas.RequestTable.TryRemove(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }
        private void OnLogin_RegistVerifyRes(Datagram datagram)
        {
            RegistResponse res = datagram.UnSerialData<RegistResponse>();

            if (RunningDatas.RequestTable.TryRemove(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }
        private void OnLogin_RegistEmailRes(Datagram datagram)
        {
            RegistResponse res = datagram.UnSerialData<RegistResponse>();

            if (RunningDatas.RequestTable.TryRemove(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }
        private void OnLogin_LoginResult(Datagram datagram)
        {
            LoginResponse res = datagram.UnSerialData<LoginResponse>();

            if (RunningDatas.RequestTable.TryRemove(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }

        private void OnMain(Datagram datagram)
        {
            switch (datagram.MessageType)
            {
                case MainMessageType.HeartRequest:
                    OnMain_HeartRequest(datagram);
                    break;
                case MainMessageType.RegistRes:
                    OnMain_RegistRes(datagram);
                    break;
            }
        }
        private void OnMain_HeartRequest(Datagram datagram)
        {
            datagram.DataType = DatagramType.Client;
            datagram.MessageType = MainMessageType.HeartResponse;

            if (!Send(datagram))
            {
                Disconnect();
                RunningDatas.ErrorNotify("网络连接失败 请重启软件后重试");
            }
        }
        private void OnMain_RegistRes(Datagram datagram)
        {
            MainRegistRes res = datagram.UnSerialData<MainRegistRes>();

            if (RunningDatas.RequestTable.TryRemove(datagram.RequestID, out RequestSender value))
                value.RequestCallback(res);
        }

        private bool Send(Datagram datagram)
        {
            byte[] buffer = datagram.ToByteArray();

            for (int i = 0; i < 3; ++i)
            {
                try
                {
                    if (IsConnect)
                    {
                        Server.Send(buffer);
                        return true;
                    }
                }
                catch
                {

                }
            }

            return false;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    if (null != Server)
                        Server.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~ClientConnect()
        // {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }

        #endregion


        #region IDataSender Support

        void IDataSender.Regist(Guid request_id)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Main,
                MessageType = MainMessageType.RegistReq,
                RequestID = request_id,
                Datas = new MainRegistReq
                {
                    UserID = IKXTServer.DataConvert.GetGuid(RunningDatas.UserID)
                }.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.Logout()
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Main,
                MessageType = MainMessageType.LogoutMessage
            };

            Send(datagram);
        }

        void IDataSender.Login(Guid request_id, LoginRequest request)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.LoginRequest,
                RequestID = request_id,
                Datas = request.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        bool IDataSender.RegistEmailReq(Guid request_id, RegistEmailReq req)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.RegistEmailReq,
                RequestID = request_id,
                Datas = req.ToByteArray()
            };

            return Send(datagram);
        }
        bool IDataSender.RegistVerifyReq(Guid request_id, RegistVerifyReq req)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.RegistVerifyReq,
                RequestID = request_id,
                Datas = req.ToByteArray()
            };

            return Send(datagram);
        }
        bool IDataSender.RegistFinishReq(Guid request_id, RegistFinishReq req)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.RegistFinishReq,
                RequestID = request_id,
                Datas = req.ToByteArray()
            };

            return Send(datagram);
        }
        bool IDataSender.UpdatePWEmailReq(Guid request_id, UpdatePWEmailReq req)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.UpdatePWEmailReq,
                RequestID = request_id,
                Datas = req.ToByteArray()
            };

            return Send(datagram);
        }
        bool IDataSender.UpdatePWVerifyReq(Guid request_id, UpdatePWVerifyReq req)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.UpdatePWVerifyReq,
                RequestID = request_id,
                Datas = req.ToByteArray()
            };

            return Send(datagram);
        }
        bool IDataSender.UpdatePWFinishReq(Guid request_id, UpdatePWFinishReq req)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.UpdatePWFinishReq,
                RequestID = request_id,
                Datas = req.ToByteArray()
            };

            return Send(datagram);
        }
        void IDataSender.UpdatePhone(UpdatePhone update)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.UpdatePhone,
                Datas = update.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.UpdateName(UpdateName update)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.UpdateName,
                Datas = update.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.UpdateDescribe(UpdateDescribe update)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.UpdateDescribe,
                Datas = update.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.UpdatePicture(UpdatePicture update)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.UpdatePicture,
                Datas = update.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        bool IDataSender.UpdateEmailReq(Guid request_id, UpdateEmailReq req)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.UpdateEmailReq,
                RequestID = request_id,
                Datas = req.ToByteArray()
            };

            return Send(datagram);
        }
        bool IDataSender.UpdateEmailVerify(Guid request_id, UpdateEmailVerify verify)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.UpdateEmailVerify,
                RequestID = request_id,
                Datas = verify.ToByteArray()
            };

            return Send(datagram);
        }
        void IDataSender.UpdateGroupName(UpdateGroupName update)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.UpdateGroupName,
                Datas = update.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.UpdateGroupDescribe(UpdateGroupDescribe update)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.UpdateGroupDescribe,
                Datas = update.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.UserInfoReq(Guid request_id, UserGroupInfoReq req)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.UserInfoReq,
                RequestID = request_id,
                Datas = req.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.GroupInfoReq(Guid request_id, UserGroupInfoReq req)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.GroupInfoReq,
                RequestID = request_id,
                Datas = req.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.FriendsRequest(Guid request_id, FriendGroupApplyRequest request)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.FriendsRequest,
                RequestID = request_id,
                Datas = request.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.GroupsRequest(Guid request_id, FriendGroupApplyRequest request)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.GroupsRequest,
                RequestID = request_id,
                Datas = request.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.AppliesRequest(Guid request_id, FriendGroupApplyRequest request)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.AppliesRequest,
                RequestID = request_id,
                Datas = request.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.GroupMemberReq(Guid request_id, GroupMemberReq req)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.GroupMemberReq,
                RequestID = request_id,
                Datas = req.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.CreateGroup(Guid request_id, CreateGroupReq req)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.CreateGroupReq,
                RequestID = request_id,
                Datas = req.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.DeleteGroup(DeleteGroup delete)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.DeleteGroup,
                Datas = delete.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.ApplyUserReq(ApplyRequest request)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.ApplyUserReq,
                Datas = request.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.ApplyUserRes(ApplyResponse response)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.ApplyUserRes,
                Datas = response.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.DeleteUser(DeleteUser delete)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.DeleteUser,
                Datas = delete.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.DropGroup(DeleteUser delete)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.DropGroup,
                Datas = delete.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.SearchReq(Guid request_id, SearchReq req)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Login,
                MessageType = LoginMessageType.SearchReq,
                RequestID = request_id,
                Datas = req.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }

        void IDataSender.CloudRequest(Guid request_id, CloudRequest request)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Cloud,
                MessageType = CloudDatagramDefine.CloudRequest,
                RequestID = request_id,
                Datas = request.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.CreateFolder(CreateFolder create)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Cloud,
                MessageType = CloudDatagramDefine.CreateFolder,
                Datas = create.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.DeleteFolder(DeleteFolderFile delete)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Cloud,
                MessageType = CloudDatagramDefine.DeleteFolder,
                Datas = delete.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.DeleteFile(DeleteFolderFile delete)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Cloud,
                MessageType = CloudDatagramDefine.DeleteFile,
                Datas = delete.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.FileUpload(Guid request_id, FileUploadReq req)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Cloud,
                MessageType = CloudDatagramDefine.FileUploadReq,
                RequestID = request_id,
                Datas = req.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.FileDownload(Guid request_id, FileDownloadReq req)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Cloud,
                MessageType = CloudDatagramDefine.FileDownloadReq,
                RequestID = request_id,
                Datas = req.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.FileStreamReq(StreamReq req)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Cloud,
                MessageType = CloudDatagramDefine.StreamReq,
                Datas = req.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.FileStreamRes(StreamRes res)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Cloud,
                MessageType = CloudDatagramDefine.StreamRes,
                Datas = res.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.FileDownloadFinish(FileDownloadFinish finish)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Cloud,
                MessageType = CloudDatagramDefine.FileDownloadFinish,
                Datas = finish.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.FileUploadCancel(FileUploadCancel cancel)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Cloud,
                MessageType = CloudDatagramDefine.FileUploadCancel,
                Datas = cancel.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }

        bool IDataSender.FriendChat(ChatMessage message)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Chat,
                MessageType = ChatDatagramDefine.FriendChat,
                Datas = message.ToByteArray()
            };

            return Send(datagram);
        }
        bool IDataSender.GroupChat(ChatMessage message)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Chat,
                MessageType = ChatDatagramDefine.GroupChat,
                Datas = message.ToByteArray()
            };

            return Send(datagram);
        }
        void IDataSender.FriendChatReq(Guid request_id, FriendChatsReq req)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Chat,
                MessageType = ChatDatagramDefine.FriendChatsReq,
                RequestID = request_id,
                Datas = req.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.GroupChatReq(Guid request_id, GroupChatsReq req)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Chat,
                MessageType = ChatDatagramDefine.GroupChatsReq,
                RequestID = request_id,
                Datas = req.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        void IDataSender.ChatsFinish(ChatsFinishPackage finish)
        {
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Chat,
                MessageType = ChatDatagramDefine.GroupChat,
                Datas = finish.ToByteArray()
            };

            if (!Send(datagram))
                RunningDatas.InfoNotify("网络连接失败 请重启软件后重试");
        }
        #endregion
    }
}
