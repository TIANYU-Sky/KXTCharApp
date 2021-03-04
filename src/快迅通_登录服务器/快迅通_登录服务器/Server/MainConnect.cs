using IKXTServer;
using KXTNetStruct;
using KXTNetStruct.Struct;
using KXTServiceDBServer;
using KXTServiceDBServer.Cloud;
using KXTServiceDBServer.Files;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace 快迅通_登录服务器.Server
{

    internal class MainConnect
    {
        /// <summary>
        /// 邮件请求缓存
        /// 用于保存当前通过邮件发起的注册、修改密码、更改邮箱的操作
        /// 保存 请求ID、与操作相关的邮箱地址/用户ID、生成的验证码
        /// 有效时间：10分钟
        /// </summary>
        private readonly ConcurrentDictionary<Guid, EmailRequestPackage> EmailRequestCache;
        /// <summary>
        /// 操作终结缓存
        /// 用于保存已经经过邮箱验证操作的最后数据
        /// 保存 请求ID、与操作相关的邮箱地址/用户ID、请求时间
        /// 有效时间：10分钟
        /// </summary>
        private readonly ConcurrentDictionary<Guid, KeyValuePair<string, DateTime>> OperationFinishCache;

        private readonly Socket MainServer;
        private readonly byte[] Buffer;

        private readonly Action<LogLevel, string> Notify;
        private readonly Action CloseServer;

        private readonly ISQLDataBase SQLDB;
        private readonly KXTUserInfoReader UserInfoReader;
        private readonly KXTGroupInfoReader GroupInfoReader;
        private readonly KXTUserAppliesReader UserAppliesReader;
        private readonly KXTUserChatReader UserChatReader;
        private readonly CloudInfoReader CloudReader;
        private readonly string CloudRootPath;

        private readonly System.Timers.Timer Timer;
        private bool HeartTest;

        private readonly System.Timers.Timer CacheTime;

        private int CloseLock;
        private bool IsClosed;

        public MainConnect
            (
            Socket main_server,
            ISQLDataBase sqldb,
            string user_info_path,
            string group_info_path,
            string applies_info_path,
            string chat_path,
            string cloud_path,
            string cloud_info_path,
            Action<LogLevel, string> notify,
            Action close
            )
        {
            IsClosed = false;
            CloseLock = 0;

            Notify = notify;
            SQLDB = sqldb;
            UserInfoReader = new KXTUserInfoReader(user_info_path, Notify);
            GroupInfoReader = new KXTGroupInfoReader(group_info_path, Notify);
            UserAppliesReader = new KXTUserAppliesReader(applies_info_path, Notify);
            UserChatReader = new KXTUserChatReader(chat_path, Notify);
            CloudReader = new CloudInfoReader(cloud_info_path, Notify);
            CloudRootPath = cloud_path;

            CloseServer = close;

            HeartTest = false;
            Timer = new System.Timers.Timer
            {
                Interval = HeartRequestInterval,
                AutoReset = false
            };
            Timer.Elapsed += HeartRequest_Trigger;
            Timer.Start();

            CacheTime = new System.Timers.Timer
            {
                Interval = CacheVaildTime,
                AutoReset = false
            };
            CacheTime.Elapsed += CacheFresh_Trigger;
            Timer.Start();

            EmailRequestCache = new ConcurrentDictionary<Guid, EmailRequestPackage>();
            OperationFinishCache = new ConcurrentDictionary<Guid, KeyValuePair<string, DateTime>>();

            MainServer = main_server;
            Buffer = new byte[Datagram.DatagramLengthMax];
            MainServer.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnReceive, null);
        }

        public void Stop()
        {
            if (0 != System.Threading.Interlocked.Exchange(ref CloseLock, 1))
                System.Threading.Thread.Sleep(5);

            if (!IsClosed)
            {
                MainServer.Shutdown(SocketShutdown.Both);
                MainServer.Dispose();

                Timer.Stop();
                CacheTime.Close();

                UserInfoReader.Close();
                GroupInfoReader.Close();
                UserAppliesReader.Close();
                UserChatReader.Close();

                CloseServer();
            }

            IsClosed = true;

            System.Threading.Interlocked.Exchange(ref CloseLock, 0);
        }

        private void Close()
        {
            Stop();
            CloseServer();
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int length = MainServer.EndReceive(ar);
                if (Datagram.DatagramLengthMin <= length)
                {
                    Action<byte[], int> action = OnProcess;
                    byte[] temp = new byte[length];
                    Array.Copy(Buffer, 0, temp, 0, length);
                    action.BeginInvoke(temp, length, null, null);
                }
            }
            catch
            {
                Notify(IKXTServer.LogLevel.Warning, "消息接收异常");
            }

            try
            {
                if (!IsClosed)
                    MainServer.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnReceive, null);
            }
            catch
            {
                Notify(IKXTServer.LogLevel.Error, "套接字启动接收异常");
                Close();
            }
        }
        private void OnProcess(byte[] buffer, int count)
        {
            Datagram datagram = new Datagram();
            if (!datagram.FromBytes_S(buffer, 0))
            {
                Notify(LogLevel.Info, "无效的消息：" + Encoding.UTF8.GetString(buffer));
                return;
            }

            if (DatagramType.Main == datagram.DataType)
                if (MainMessageType.HeartResponse == datagram.MessageType)
                {
                    HeartTest = false;
                    return;
                }

            switch (datagram.MessageType)
            {
                case LoginMessageType.LoginRequest:
                    OnLoginRequest(datagram);
                    break;

                case LoginMessageType.RegistEmailReq:
                    OnRegistEmailReq(datagram);
                    break;
                case LoginMessageType.RegistVerifyReq:
                    OnRegistVerifyReq(datagram);
                    break;
                case LoginMessageType.RegistFinishReq:
                    OnRegistFinishReq(datagram);
                    break;

                case LoginMessageType.UpdatePWEmailReq:
                    OnUpdatePWEmailReq(datagram);
                    break;
                case LoginMessageType.UpdatePWVerifyReq:
                    OnUpdatePWVerifyReq(datagram);
                    break;
                case LoginMessageType.UpdatePWFinishReq:
                    OnUpdatePWFinishReq(datagram);
                    break;

                case LoginMessageType.UpdatePhone:
                    OnUpdatePhone(datagram);
                    break;
                case LoginMessageType.UpdateName:
                    OnUpdateName(datagram);
                    break;
                case LoginMessageType.UpdateDescribe:
                    OnUpdateDescribe(datagram);
                    break;
                case LoginMessageType.UpdatePicture:
                    OnUpdatePicture(datagram);
                    break;
                case LoginMessageType.UpdateEmailReq:
                    OnUpdateEmailReq(datagram);
                    break;
                case LoginMessageType.UpdateEmailVerify:
                    OnUpdateEmailVerify(datagram);
                    break;
                case LoginMessageType.UpdateGroupName:
                    OnUpdateGroupName(datagram);
                    break;
                case LoginMessageType.UpdateGroupDescribe:
                    OnUpdateGroupDescribe(datagram);
                    break;

                case LoginMessageType.UserInfoReq:
                    OnUserInfoReq(datagram);
                    break;
                case LoginMessageType.GroupInfoReq:
                    OnGroupInfoReq(datagram);
                    break;
                case LoginMessageType.FriendsRequest:
                    OnFriendsRequest(datagram);
                    break;
                case LoginMessageType.GroupsRequest:
                    OnGroupsRequest(datagram);
                    break;
                case LoginMessageType.AppliesRequest:
                    OnAppliesRequest(datagram);
                    break;
                case LoginMessageType.GroupMemberReq:
                    OnGroupMemberReq(datagram);
                    break;

                case LoginMessageType.CreateGroupReq:
                    OnCreateGroupReq(datagram);
                    break;
                case LoginMessageType.DeleteGroup:
                    OnDeleteGroup(datagram);
                    break;
                case LoginMessageType.ApplyUserReq:
                    OnApplyUserReq(datagram);
                    break;
                case LoginMessageType.ApplyUserRes:
                    OnApplyUserRes(datagram);
                    break;
                case LoginMessageType.DeleteUser:
                    OnDeleteUser(datagram);
                    break;
                case LoginMessageType.DropGroup:
                    OnDropGroup(datagram);
                    break;

                case LoginMessageType.SearchReq:
                    OnSearchReq(datagram);
                    break;
            }
        }

        private bool Send(byte[] datas)
        {
            for (int i = 0; i < 3; ++i)
            {
                try
                {
                    MainServer.Send(datas);
                    return true;
                }
                catch
                {

                }
            }
            return false;
        }

        private void HeartRequest_Trigger(object sender, ElapsedEventArgs args)
        {
            // 发送心跳连接
            Datagram datagram = new Datagram
            {
                DataType = DatagramType.Main,
                MessageType = MainMessageType.HeartRequest,
                Time = DateTime.Now,
                RequestID = Guid.Empty,
                Sender = Guid.Empty,
                Datas = null
            };
            if (Send(datagram.ToByteArray()))
            {
                HeartTest = true;
                Timer.Elapsed -= HeartRequest_Trigger;
                Timer.Elapsed += HeartResponse_Trigger;
                Timer.Interval = HeartResponseInterval;
                Timer.Start();
            }
            else
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }
        private void HeartResponse_Trigger(object sender, ElapsedEventArgs args)
        {
            if (HeartTest)
            {
                Notify(IKXTServer.LogLevel.Error, "心跳连接超时");
                Close();
            }
            else
            {
                Timer.Elapsed -= HeartResponse_Trigger;
                Timer.Elapsed += HeartRequest_Trigger;
                Timer.Interval = HeartRequestInterval;
                Timer.Start();
            }
        }

        public const double HeartRequestInterval = 5000;
        public const double HeartResponseInterval = 3000;

        #region 操作实现块
        private void OnLoginRequest(Datagram datagram)
        {
            LoginRequest request = datagram.UnSerialData<LoginRequest>();

            LoginResponse response = new LoginResponse();
            response.LoginResult = SQLDB.Login
                (
                request.UserID,
                request.UserPW,
                request.UserIDType,
                out string user_id
                );

            if (LoginResult.Success == response.LoginResult)
            {
                if (SQLDB.SelectUserInfor(user_id, out UserInfoPackage info))
                {
                    response.UserPicture = (byte)info.Photo;
                    response.UserID = info.ID;
                    response.UserName = info.Name;
                    response.UserDesicribe = info.Describe;
                    response.UserPhone = info.Phone;
                    response.UserEmail = info.Email;
                }
            }

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = LoginMessageType.LoginResult;
            datagram.Datas = response.ToByteArray();

            if (!Send(datagram.ToByteArray()))
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }

        private void OnRegistEmailReq(Datagram datagram)
        {
            RegistEmailReq req = datagram.UnSerialData<RegistEmailReq>();

            RegistResponse response = new RegistResponse();
            if (SQLDB.CheckEmail(req.Email))
            {
                Guid request_id = Guid.NewGuid();

                byte[] ver_code = InternalTools.GenerateVerification();
                if (EmailRequestCache.TryAdd(request_id, new EmailRequestPackage
                {
                    UserID = null,
                    Email = req.Email,
                    Verify = ver_code,
                    Time = DateTime.Now
                }))
                {
                    if (
                        InternalTools.SendRegistVerificateEmail
                        (
                        req.Email,
                        InternalTools.GenerateVerification(ver_code)
                        )
                      )
                        response.NextLabel = request_id;
                }
            }

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = LoginMessageType.RegistEmailRes;
            datagram.Datas = response.ToByteArray();

            if (!Send(datagram.ToByteArray()))
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }
        private void OnRegistVerifyReq(Datagram datagram)
        {
            RegistVerifyReq req = datagram.UnSerialData<RegistVerifyReq>();

            RegistResponse response = new RegistResponse();
            if (EmailRequestCache.TryRemove(req.NextLabel, out EmailRequestPackage value))
            {
                if (InternalTools.VerificateCode(value.Verify, req.Verifies))
                {
                    Guid guid = Guid.NewGuid();
                    if (OperationFinishCache.TryAdd
                        (
                        guid,
                        new KeyValuePair<string, DateTime>(value.Email, DateTime.Now)
                        ))
                    {
                        response.NextLabel = guid;
                    }
                }
            }

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = LoginMessageType.RegistVerifyRes;
            datagram.Datas = response.ToByteArray();

            if (!Send(datagram.ToByteArray()))
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }
        private void OnRegistFinishReq(Datagram datagram)
        {
            RegistFinishReq req = datagram.UnSerialData<RegistFinishReq>();

            RegistResponse response = new RegistResponse();
            if (OperationFinishCache.TryGetValue
                (
                req.NextLabel,
                out KeyValuePair<string, DateTime> value
                ))
            {
                string user_id = InternalTools.GetNewUserID();
                if (SQLDB.Register(new RegisterPackage
                {
                    ID = user_id,
                    Name = req.Name,
                    Email = value.Key,
                    Describe = req.Describe,
                    Password = req.Password,
                    Phone = null,
                    Photo = req.Picture
                }))
                {
                    response.NextLabel = DataConvert.GetGuid(user_id);
                    UserInfoReader.CreateUser(user_id);
                    UserAppliesReader.CreateUser(user_id);
                    CloudReader.AddUser(user_id);
                    System.IO.Directory.CreateDirectory(CloudRootPath + "\\" + IKXTServer.DataConvert.GetGuid(user_id).ToString());
                }
            }

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = LoginMessageType.RegistFinishRes;
            datagram.Datas = response.ToByteArray();

            if (!Send(datagram.ToByteArray()))
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }

        private void OnUpdatePWEmailReq(Datagram datagram)
        {
            UpdatePWEmailReq req = datagram.UnSerialData<UpdatePWEmailReq>();

            UpdatePWResponse response = new UpdatePWResponse();
            if (!SQLDB.CheckEmail(req.Email))
            {
                Guid request_id = Guid.NewGuid();

                byte[] ver_code = InternalTools.GenerateVerification();
                if (EmailRequestCache.TryAdd(request_id, new EmailRequestPackage
                {
                    UserID = null,
                    Email = req.Email,
                    Verify = ver_code,
                    Time = DateTime.Now
                }))
                {
                    if (
                        InternalTools.SendUpdatePWVerificateEmail
                        (
                        req.Email,
                        InternalTools.GenerateVerification(ver_code)
                        )
                      )
                        response.NextLabel = request_id;
                }
            }

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = LoginMessageType.UpdatePWEmailRes;
            datagram.Datas = response.ToByteArray();

            if (!Send(datagram.ToByteArray()))
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }
        private void OnUpdatePWVerifyReq(Datagram datagram)
        {
            UpdatePWVerifyReq req = datagram.UnSerialData<UpdatePWVerifyReq>();

            UpdatePWResponse response = new UpdatePWResponse();
            if (EmailRequestCache.TryRemove(req.NextLabel, out EmailRequestPackage value))
            {
                if (InternalTools.VerificateCode(value.Verify, req.Verifies))
                {
                    Guid guid = Guid.NewGuid();
                    if (OperationFinishCache.TryAdd
                        (
                        guid,
                        new KeyValuePair<string, DateTime>(value.Email, DateTime.Now)
                        ))
                    {
                        response.NextLabel = guid;
                    }
                }
            }

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = LoginMessageType.UpdatePWVerifyRes;
            datagram.Datas = response.ToByteArray();

            if (!Send(datagram.ToByteArray()))
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }
        private void OnUpdatePWFinishReq(Datagram datagram)
        {
            UpdatePWFinishReq req = datagram.UnSerialData<UpdatePWFinishReq>();

            UpdatePWResponse response = new UpdatePWResponse();
            if (OperationFinishCache.TryGetValue
                (
                req.NextLabel,
                out KeyValuePair<string, DateTime> value
                ))
            {
                string user_id = InternalTools.GetNewUserID();
                if (SQLDB.UpdatePassword(value.Key, req.Password))
                {
                    response.NextLabel = Guid.NewGuid();
                }
            }

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = LoginMessageType.UpdatePWFinishRes;
            datagram.Datas = response.ToByteArray();

            if (!Send(datagram.ToByteArray()))
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }

        private void OnUpdatePhone(Datagram datagram)
        {
            UpdatePhone update = datagram.UnSerialData<UpdatePhone>();

            SQLDB.UpdatePhone(update.UserID, update.UserPhone);
        }
        private void OnUpdateName(Datagram datagram)
        {
            UpdateName update = datagram.UnSerialData<UpdateName>();

            SQLDB.UpdateName(update.UserID, update.UserName);
        }
        private void OnUpdateDescribe(Datagram datagram)
        {
            UpdateDescribe update = datagram.UnSerialData<UpdateDescribe>();

            SQLDB.UpdateName(update.UserID, update.UserDescribe);
        }
        private void OnUpdatePicture(Datagram datagram)
        {
            UpdatePicture update = datagram.UnSerialData<UpdatePicture>();

            SQLDB.UpdateProfilePhoto(update.UserID, update.UserPicture);
        }
        private void OnUpdateEmailReq(Datagram datagram)
        {
            UpdateEmailReq req = datagram.UnSerialData<UpdateEmailReq>();

            UpdateEmailResponse response = new UpdateEmailResponse();

            if (!SQLDB.CheckEmail(req.UserEmail))
            {
                if (SQLDB.SelectUserInfor(req.UserID, out UserInfoPackage info))
                {
                    Guid request_id = Guid.NewGuid();

                    byte[] ver_code = InternalTools.GenerateVerification();
                    if (EmailRequestCache.TryAdd(request_id, new EmailRequestPackage
                    {
                        UserID = info.ID,
                        Email = req.UserEmail,
                        Verify = ver_code,
                        Time = DateTime.Now
                    }))
                    {
                        if (
                            InternalTools.SendUpdateEmailVerificateEmail
                            (
                            req.UserEmail,
                            InternalTools.GenerateVerification(ver_code)
                            )
                          )
                            response.NextLabel = request_id;
                    }
                }
            }

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = LoginMessageType.UpdateEmailRes;
            datagram.Datas = response.ToByteArray();

            if (!Send(datagram.ToByteArray()))
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }
        private void OnUpdateEmailVerify(Datagram datagram)
        {
            UpdateEmailVerify req = datagram.UnSerialData<UpdateEmailVerify>();

            UpdateEmailResponse response = new UpdateEmailResponse();
            if (EmailRequestCache.TryRemove(req.NextLabel, out EmailRequestPackage value))
            {
                if (InternalTools.VerificateCode(value.Verify, req.Verifies))
                {
                    Guid guid = Guid.NewGuid();
                    if (SQLDB.UpdateEmail(value.UserID, value.Email))
                        response.NextLabel = guid;
                }
            }

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = LoginMessageType.UpdateEmailResult;
            datagram.Datas = response.ToByteArray();

            if (!Send(datagram.ToByteArray()))
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }
        private void OnUpdateGroupName(Datagram datagram)
        {
            UpdateGroupName update = datagram.UnSerialData<UpdateGroupName>();

            SQLDB.UpdateGroupName(update.GroupID, update.GroupName);
        }
        private void OnUpdateGroupDescribe(Datagram datagram)
        {
            UpdateGroupDescribe update = datagram.UnSerialData<UpdateGroupDescribe>();

            SQLDB.UpdateGroupDescribe(update.GroupID, update.GroupDescribe);
        }

        private void OnUserInfoReq(Datagram datagram)
        {
            UserGroupInfoReq req = datagram.UnSerialData<UserGroupInfoReq>();

            UserInfoRes res = new UserInfoRes();
            if (SQLDB.SelectUserInfor(req.ID, out UserInfoPackage info))
            {
                res.UserID = info.ID;
                res.UserName = info.Name;
                res.UserDescribe = info.Describe;
                res.UserPhone = info.Phone;
                res.UserEmail = info.Email;
                res.UserPicture = (byte)info.Photo;
            }

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = LoginMessageType.UserInfoRes;
            datagram.Datas = res.ToByteArray();

            if (!Send(datagram.ToByteArray()))
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }
        private void OnGroupInfoReq(Datagram datagram)
        {
            UserGroupInfoReq req = datagram.UnSerialData<UserGroupInfoReq>();

            GroupInfoRes res = new GroupInfoRes();
            if (SQLDB.SelectGroupInfo(req.ID, out GroupInfoPackage info))
            {
                res.GroupID = info.ID;
                res.GroupName = info.Name;
                res.GroupDescribe = info.Describe;
                res.CreatorID = info.ManagerID;
                res.CreatorName = info.ManagerName;
                res.CreateTime = info.Time;
                res.UserPicture = (byte)info.ManagerPhoto;
            }

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = LoginMessageType.GroupInfoRes;
            datagram.Datas = res.ToByteArray();

            if (!Send(datagram.ToByteArray()))
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }
        private void OnFriendsRequest(Datagram datagram)
        {
            FriendGroupApplyRequest request = datagram.UnSerialData<FriendGroupApplyRequest>();

            FriendsResponse response = new FriendsResponse
            {
                FriendsID = UserInfoReader.ReadFriends(request.UserID)
            };

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = LoginMessageType.FriendsResponse;
            datagram.Datas = response.ToByteArray();

            if (!Send(datagram.ToByteArray()))
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }
        private void OnGroupsRequest(Datagram datagram)
        {
            FriendGroupApplyRequest request = datagram.UnSerialData<FriendGroupApplyRequest>();

            GroupsResponse response = new GroupsResponse
            {
                GroupsID = UserInfoReader.ReadGroups(request.UserID)
            };

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = LoginMessageType.GroupsResponse;
            datagram.Datas = response.ToByteArray();

            if (!Send(datagram.ToByteArray()))
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }
        private void OnAppliesRequest(Datagram datagram)
        {
            FriendGroupApplyRequest request = datagram.UnSerialData<FriendGroupApplyRequest>();

            KXTUserAppliesPackage[] applies = UserAppliesReader.ReadApplies(request.UserID);

            List<AppliesResponse.ApplyPackage> list = new List<AppliesResponse.ApplyPackage>();
            foreach (KXTUserAppliesPackage i in applies)
            {
                UserInfoPackage user = new UserInfoPackage();
                SQLDB.SelectUserInfor(i.ApplierID, out user);

                list.Add(new AppliesResponse.ApplyPackage
                {
                    TargetType = i.TargetType,
                    TargetID = i.GroupID,
                    ApplierID = i.ApplierID,
                    ApplierName = user.Name,
                    ApplierDescribe = user.Describe,
                    ApplierPicture = (byte)user.Photo,
                    ApplyTime = i.ApplyTime,
                    Message = i.Message
                });
            }

            AppliesResponse response = new AppliesResponse
            {
                Applies = list.ToArray()
            };

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = LoginMessageType.AppliesResponse;
            datagram.Datas = response.ToByteArray();

            if (!Send(datagram.ToByteArray()))
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }
        private void OnGroupMemberReq(Datagram datagram)
        {
            GroupMemberReq req = datagram.UnSerialData<GroupMemberReq>();
            string[] members = GroupInfoReader.ReadMembers(req.GroupID);

            GroupMemberRes res = new GroupMemberRes();

            if (0 < members.Length)
            {
                if (SQLDB.SelectUserInfors(members, out UserInfoPackage[] infos))
                {
                    for (int i = 0; i < infos.Length; ++i)
                        res.Members.Add(new GroupMemberRes.GroupMemberItem
                        {
                            UserID = infos[i].ID,
                            UserName = infos[i].Name,
                            UserPicture = (byte)infos[i].Photo
                        });
                }
            }

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = LoginMessageType.GroupMemberRes;

            byte[][] buffer = res.ToByteArrays();
            for (int i = 0; i < buffer.Length; ++i)
            {
                datagram.Datas = res.ToByteArrays()[i];

                if (!Send(datagram.ToByteArray()))
                {
                    Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                    Close();
                    return;
                }
            }
        }

        private void OnCreateGroupReq(Datagram datagram)
        {
            CreateGroupReq req = datagram.UnSerialData<CreateGroupReq>();

            CreateGroupRes res = new CreateGroupRes();

            string group_id = InternalTools.GetNewGroupID();
            if (SQLDB.CreateGroup(new CreateGroupPackage
            {
                ID = group_id,
                Name = req.GroupName,
                Describe = req.GroupDescribe,
                Manager = req.CreatorID,
                Time = DateTime.Now
            }))
            {
                if (GroupInfoReader.CreateGroup(group_id))
                {
                    UserInfoReader.AddGroup(req.CreatorID, group_id);
                    UserChatReader.AddGroup(group_id);

                    res.GroupID = group_id;
                }
                else
                    SQLDB.DeleteGroup(group_id);
            }

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = LoginMessageType.CreateGroupRes;
            datagram.Datas = res.ToByteArray();

            if (!Send(datagram.ToByteArray()))
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }
        private void OnDeleteGroup(Datagram datagram)
        {
            DeleteGroup delete = datagram.UnSerialData<DeleteGroup>();

            if (SQLDB.DeleteGroup(delete.GroupID))
            {
                string[] members = GroupInfoReader.ReadMembers(delete.GroupID);

                if (null != members)
                    foreach (string i in members)
                        UserInfoReader.DelGroup(i, delete.GroupID);

                GroupInfoReader.DelGroup(delete.GroupID);
                UserChatReader.DelGroup(delete.GroupID);
            }
        }
        private void OnApplyUserReq(Datagram datagram)
        {
            ApplyRequest request = datagram.UnSerialData<ApplyRequest>();

            if (ApplyRequest.TargetType_Friend == request.TargetType)
                UserAppliesReader.AddApply(datagram.Sender, request.TargetID, request);
            else
            {
                if (SQLDB.SelectGroupInfo(request.TargetID, out GroupInfoPackage info))
                    UserAppliesReader.AddApply(datagram.Sender, info.ManagerID, request);
            }
        }
        private void OnApplyUserRes(Datagram datagram)
        {
            ApplyResponse response = datagram.UnSerialData<ApplyResponse>();

            string user_id = IKXTServer.DataConvert.GetString(datagram.Sender);

            UserAppliesReader.EndApply(user_id, response);

            if (ApplyResponse.ApplyResult_Submit == response.ApplyResult)
            {
                if ("" == response.TargetID)
                {
                    UserInfoReader.AddFriend(user_id, response.ApplierID);
                    UserInfoReader.AddFriend(response.ApplierID, user_id);

                    UserChatReader.AddFriend(user_id, response.ApplierID);
                }
                else
                {
                    UserInfoReader.AddGroup(response.ApplierID, response.TargetID);
                    GroupInfoReader.AddMember(response.TargetID, response.ApplierID);
                }
            }
        }
        private void OnDeleteUser(Datagram datagram)
        {
            DeleteUser delete = datagram.UnSerialData<DeleteUser>();

            if (UserChatReader.DelFriend(delete.UserID, delete.FriendID))
            {
                UserInfoReader.DelFriend(delete.UserID, delete.FriendID);
                UserInfoReader.DelFriend(delete.FriendID, delete.UserID);
            }
        }
        private void OnDropGroup(Datagram datagram)
        {
            DeleteUser delete = datagram.UnSerialData<DeleteUser>();

            GroupInfoReader.DelMember(delete.FriendID, delete.UserID);
            UserInfoReader.DelGroup(delete.UserID, delete.FriendID);
        }

        private void OnSearchReq(Datagram datagram)
        {
            SearchReq req = datagram.UnSerialData<SearchReq>();

            SearchRes res = new SearchRes();
            if (SearchReq.SearchType_Group == req.SearchType)
            {
                // 搜索群
                res.SearchType = SearchRes.SearchType_Group;
                if (SQLDB.SearchGroups(req.SearchString, out GroupInfoPackage[] infos))
                {
                    List<SearchRes.GroupSearchResult> list = new List<SearchRes.GroupSearchResult>();
                    foreach (GroupInfoPackage i in infos)
                    {
                        list.Add(new SearchRes.GroupSearchResult
                        {
                            CreatorID = i.ManagerID,
                            GroupDescribe = i.Describe,
                            CreateTime = i.Time,
                            GroupID = i.ID,
                            CreatorName = i.ManagerName,
                            GroupName = i.Name
                        });
                    }
                    res.SearchResult = list.ToArray();
                }
            }
            else
            {
                // 搜索用户
                res.SearchType = SearchRes.SearchType_User;
                if (SQLDB.SearchFriends(req.SearchString, out UserInfoPackage[] infos))
                {
                    List<SearchRes.UserSearchResult> list = new List<SearchRes.UserSearchResult>();
                    foreach (UserInfoPackage i in infos)
                    {
                        list.Add(new SearchRes.UserSearchResult
                        {
                            UserID = i.ID,
                            UserName = i.Name,
                            UserPhone = i.Phone,
                            UserEmail = i.Email,
                            UserDescribe = i.Describe,
                            UserPicture = (byte)i.Photo
                        });
                    }
                    res.SearchResult = list.ToArray();
                }
            }

            datagram.DataType = DatagramType.Client;
            datagram.MessageType = LoginMessageType.SearchRes;
            datagram.Datas = res.ToByteArray();

            if (!Send(datagram.ToByteArray()))
            {
                Notify(IKXTServer.LogLevel.Error, "数据发送异常");
                Close();
            }
        }
        #endregion

        private void CacheFresh_Trigger(object sender, ElapsedEventArgs args)
        {
            FreshEmailCache();
            FreshFinishCache();

            CacheTime.Start();
        }
        private void FreshEmailCache()
        {
            if (0 < EmailRequestCache.Count)
            {
                foreach (var i in EmailRequestCache)
                {
                    if ((DateTime.Now - i.Value.Time).TotalMilliseconds > CacheVaildTime)
                        EmailRequestCache.TryRemove(i.Key, out _);
                }
            }
        }
        private void FreshFinishCache()
        {
            if (0 < OperationFinishCache.Count)
            {
                foreach (var i in OperationFinishCache)
                {
                    if ((DateTime.Now - i.Value.Value).TotalMilliseconds > CacheVaildTime)
                        OperationFinishCache.TryRemove(i.Key, out _);
                }
            }
        }

        private const double CacheVaildTime = 600000;
    }
}
