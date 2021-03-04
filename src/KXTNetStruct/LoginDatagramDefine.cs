using IKXTServer;
using KXTNetStruct.Struct;
using System;
using System.Collections.Generic;
using System.Text;

namespace KXTNetStruct
{
    public class LoginMessageType
    {
        public const byte LoginRequest = 0x00;
        public const byte LoginResult = 0x01;

        public const byte RegistEmailReq = 0x10;
        public const byte RegistEmailRes = 0x11;
        public const byte RegistVerifyReq = 0x12;
        public const byte RegistVerifyRes = 0x13;
        public const byte RegistFinishReq = 0x14;
        public const byte RegistFinishRes = 0x15;

        public const byte UpdatePWEmailReq = 0x20;
        public const byte UpdatePWEmailRes = 0x21;
        public const byte UpdatePWVerifyReq = 0x22;
        public const byte UpdatePWVerifyRes = 0x23;
        public const byte UpdatePWFinishReq = 0x24;
        public const byte UpdatePWFinishRes = 0x25;

        public const byte UpdatePhone = 0x30;
        public const byte UpdateName = 0x31;
        public const byte UpdateDescribe = 0x32;
        public const byte UpdatePicture = 0x33;
        public const byte UpdateEmailReq = 0x34;
        public const byte UpdateEmailRes = 0x35;
        public const byte UpdateEmailVerify = 0x36;
        public const byte UpdateEmailResult = 0x37;
        public const byte UpdateGroupName = 0x38;
        public const byte UpdateGroupDescribe = 0x39;

        public const byte UserInfoReq = 0x40;
        public const byte UserInfoRes = 0x41;
        public const byte GroupInfoReq = 0x42;
        public const byte GroupInfoRes = 0x43;
        public const byte FriendsRequest = 0x44;
        public const byte FriendsResponse = 0x45;
        public const byte GroupsRequest = 0x46;
        public const byte GroupsResponse = 0x47;
        public const byte AppliesRequest = 0x48;
        public const byte AppliesResponse = 0x49;
        public const byte GroupMemberReq = 0x4A;
        public const byte GroupMemberRes = 0x4B;

        public const byte CreateGroupReq = 0x50;
        public const byte CreateGroupRes = 0x51;
        public const byte DeleteGroup = 0x52;
        public const byte ApplyUserReq = 0x53;
        public const byte ApplyUserRes = 0x54;
        public const byte DeleteUser = 0x55;
        public const byte DropGroup = 0x56;

        public const byte SearchReq = 0x60;
        public const byte SearchRes = 0x61;
    }

    public class LoginRequest : IKXTServer.IKXTSerialization
    {
        public LoginType UserIDType;
        public string UserID;
        public string UserPW;

        public LoginRequest()
        {
            UserIDType = LoginType.Identification;
            UserID = "";
            UserPW = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                UserIDType = (LoginType)buffer[index++];

                UserID = IKXTServer.KXTBitConvert.ToString(buffer, index, out int count);
                index += count;

                UserPW = IKXTServer.KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.Add((byte)UserIDType);

            buffer.AddRange(IKXTServer.KXTBitConvert.ToBytes(UserID));
            buffer.AddRange(IKXTServer.KXTBitConvert.ToBytes(UserPW));

            return buffer.ToArray();
        }

    }

    public class LoginResponse : IKXTServer.IKXTSerialization
    {
        public LoginResult LoginResult;

        public byte UserPicture;
        public string UserID;
        public string UserName;
        public string UserDesicribe;
        public string UserPhone;
        public string UserEmail;

        public LoginResponse()
        {
            LoginResult = LoginResult.Success;
            UserPicture = 0x00;
            UserID = "";
            UserName = "";
            UserDesicribe = "";
            UserPhone = "";
            UserEmail = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                LoginResult = (LoginResult)buffer[index++];

                UserPicture = buffer[index++];

                int length = 0;

                UserID = IKXTServer.KXTBitConvert.ToString(buffer, index, out length);
                index += length;

                UserName = IKXTServer.KXTBitConvert.ToString(buffer, index, out length);
                index += length;

                UserDesicribe = IKXTServer.KXTBitConvert.ToString(buffer, index, out length);
                index += length;

                UserPhone = IKXTServer.KXTBitConvert.ToString(buffer, index, out length);
                index += length;

                UserEmail = IKXTServer.KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.Add((byte)LoginResult);
            buffer.Add(UserPicture);

            buffer.AddRange(IKXTServer.KXTBitConvert.ToBytes(UserID));
            buffer.AddRange(IKXTServer.KXTBitConvert.ToBytes(UserName));
            buffer.AddRange(IKXTServer.KXTBitConvert.ToBytes(UserDesicribe));
            buffer.AddRange(IKXTServer.KXTBitConvert.ToBytes(UserPhone));
            buffer.AddRange(IKXTServer.KXTBitConvert.ToBytes(UserEmail));

            return buffer.ToArray();
        }
    }

    public class RegistEmailReq : IKXTServer.IKXTSerialization
    {
        public string Email;

        public RegistEmailReq()
        {
            Email = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                Email = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            return KXTBitConvert.ToBytes(Email);
        }
    }

    public class RegistResponse : IKXTServer.IKXTSerialization
    {
        public Guid NextLabel;

        public RegistResponse()
        {
            NextLabel = UnNextLabel;
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                byte[] temp = new byte[16];
                Array.Copy(buffer, index, temp, 0, temp.Length);
                NextLabel = new Guid(temp);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            return NextLabel.ToByteArray();
        }

        public static Guid UnNextLabel = Guid.Empty;
    }

    public class RegistVerifyReq : IKXTServer.IKXTSerialization
    {
        public Guid NextLabel;
        public readonly byte[] Verifies;

        public RegistVerifyReq()
        {
            Verifies = new byte[VerifiesLength]
            {
                0,0,0,0,0,0,0,0
            };
            NextLabel = Guid.Empty;
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                byte[] temp = new byte[16];
                Array.Copy(buffer, index, temp, 0, temp.Length);
                NextLabel = new Guid(temp);
                index += temp.Length;

                Array.Copy(buffer, index + 16, Verifies, 0, VerifiesLength);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(NextLabel.ToByteArray());
            buffer.AddRange(Verifies);
            return buffer.ToArray();
        }

        public const int VerifiesLength = 8;
    }

    public class RegistFinishReq : IKXTServer.IKXTSerialization
    {
        public Guid NextLabel;
        public byte Picture;
        public string Name;
        public string Password;
        public string Describe;

        public RegistFinishReq()
        {
            NextLabel = Guid.Empty;
            Picture = 0x00;
            Name = "";
            Password = "";
            Describe = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                byte[] temp = new byte[16];
                Array.Copy(buffer, index, temp, 0, temp.Length);
                NextLabel = new Guid(temp);
                index += temp.Length;

                Picture = buffer[index++];
                int length = 0;
                
                Name = KXTBitConvert.ToString(buffer, index, out length);
                index += length;

                Password = KXTBitConvert.ToString(buffer, index, out length);
                index += length;

                Describe = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(NextLabel.ToByteArray());
            buffer.Add(Picture);
            buffer.AddRange(KXTBitConvert.ToBytes(Name));
            buffer.AddRange(KXTBitConvert.ToBytes(Password));
            buffer.AddRange(KXTBitConvert.ToBytes(Describe));

            return buffer.ToArray();
        }
    }

    public class UpdatePWEmailReq : IKXTServer.IKXTSerialization
    {
        public string Email;

        public UpdatePWEmailReq()
        {
            Email = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                Email = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            return KXTBitConvert.ToBytes(Email);
        }
    }

    public class UpdatePWResponse : IKXTServer.IKXTSerialization
    {
        public Guid NextLabel;

        public UpdatePWResponse()
        {
            NextLabel = UnNextLabel;
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                byte[] temp = new byte[16];
                Array.Copy(buffer, index, temp, 0, temp.Length);
                NextLabel = new Guid(temp);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            return NextLabel.ToByteArray();
        }

        public static Guid UnNextLabel = Guid.Empty;
    }

    public class UpdatePWVerifyReq : IKXTServer.IKXTSerialization
    {
        public Guid NextLabel;
        public readonly byte[] Verifies;

        public UpdatePWVerifyReq()
        {
            NextLabel = Guid.Empty;
            Verifies = new byte[VerifiesLength];
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                byte[] temp = new byte[16];
                Array.Copy(buffer, index, temp, 0, temp.Length);
                NextLabel = new Guid(temp);
                index += temp.Length;

                Array.Copy(buffer, index, Verifies, 0, VerifiesLength);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(NextLabel.ToByteArray());
            buffer.AddRange(Verifies);

            return buffer.ToArray();
        }

        public const int VerifiesLength = 8;
    }

    public class UpdatePWFinishReq : IKXTServer.IKXTSerialization
    {
        public Guid NextLabel;
        public string Password;

        public UpdatePWFinishReq()
        {
            NextLabel = Guid.Empty;
            Password = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                byte[] temp = new byte[16];
                Array.Copy(buffer, index, temp, 0, temp.Length);
                NextLabel = new Guid(temp);
                index += temp.Length;

                Password = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(NextLabel.ToByteArray());
            buffer.AddRange(KXTBitConvert.ToBytes(Password));

            return buffer.ToArray();
        }
    }

    public class UpdatePhone : IKXTServer.IKXTSerialization
    {
        public string UserID;
        public string UserPhone;

        public UpdatePhone()
        {
            UserID = "";
            UserPhone = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                int length = 0;
                UserID = KXTBitConvert.ToString(buffer, index, out length);
                index += length;

                UserPhone = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(KXTBitConvert.ToBytes(UserID));
            buffer.AddRange(KXTBitConvert.ToBytes(UserPhone));

            return buffer.ToArray();
        }
    }

    public class UpdateName : IKXTServer.IKXTSerialization
    {
        public string UserID;
        public string UserName;

        public UpdateName()
        {
            UserID = "";
            UserName = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                UserID = KXTBitConvert.ToString(buffer, index, out int length);
                index += length;

                UserName = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(KXTBitConvert.ToBytes(UserID));
            buffer.AddRange(KXTBitConvert.ToBytes(UserName));

            return buffer.ToArray();
        }
    }

    public class UpdateDescribe : IKXTServer.IKXTSerialization
    {
        public string UserID;
        public string UserDescribe;

        public UpdateDescribe()
        {
            UserID = "";
            UserDescribe = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                UserID = KXTBitConvert.ToString(buffer, index, out int length);
                index += length;

                UserDescribe = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(KXTBitConvert.ToBytes(UserID));
            buffer.AddRange(KXTBitConvert.ToBytes(UserDescribe));

            return buffer.ToArray();
        }
    }

    public class UpdatePicture : IKXTServer.IKXTSerialization
    {
        public string UserID;
        public byte UserPicture;

        public UpdatePicture()
        {
            UserID = "";
            UserPicture = 0x00;
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                UserID = KXTBitConvert.ToString(buffer, index, out int length);
                index += length;

                UserPicture = buffer[index];
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(KXTBitConvert.ToBytes(UserID));
            buffer.Add(UserPicture);

            return buffer.ToArray();
        }
    }

    public class UpdateEmailReq : IKXTServer.IKXTSerialization
    {
        public string UserID;
        public string UserEmail;

        public UpdateEmailReq()
        {
            UserID = "";
            UserEmail = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                UserID = KXTBitConvert.ToString(buffer, index, out int length);
                index += length;

                UserEmail = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(KXTBitConvert.ToBytes(UserID));
            buffer.AddRange(KXTBitConvert.ToBytes(UserEmail));

            return buffer.ToArray();
        }
    }

    public class UpdateEmailResponse : IKXTServer.IKXTSerialization
    {
        public Guid NextLabel;

        public UpdateEmailResponse()
        {
            NextLabel = Guid.Empty;
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                byte[] temp = new byte[16];
                Array.Copy(buffer, index, temp, 0, temp.Length);
                NextLabel = new Guid(temp);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            return NextLabel.ToByteArray();
        }
    }

    public class UpdateEmailVerify : IKXTServer.IKXTSerialization
    {
        public Guid NextLabel;
        public readonly byte[] Verifies;

        public UpdateEmailVerify()
        {
            NextLabel = Guid.Empty;
            Verifies = new byte[8];
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                byte[] temp = new byte[16];
                Array.Copy(buffer, index, temp, 0, temp.Length);
                NextLabel = new Guid(temp);
                index += temp.Length;

                Array.Copy(buffer, index, Verifies, 0, VerifiesLength);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(NextLabel.ToByteArray());
            buffer.AddRange(Verifies);

            return buffer.ToArray();
        }

        public const int VerifiesLength = 8;
    }

    public class UpdateGroupName : IKXTServer.IKXTSerialization
    {
        public string GroupID;
        public string GroupName;

        public UpdateGroupName()
        {
            GroupID = "";
            GroupName = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                GroupID = KXTBitConvert.ToString(buffer, index, out int length);
                index += length;

                GroupName = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(KXTBitConvert.ToBytes(GroupID));
            buffer.AddRange(KXTBitConvert.ToBytes(GroupName));

            return buffer.ToArray();
        }
    }

    public class UpdateGroupDescribe : IKXTServer.IKXTSerialization
    {
        public string GroupID;
        public string GroupDescribe;

        public UpdateGroupDescribe()
        {
            GroupID = "";
            GroupDescribe = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                GroupID = KXTBitConvert.ToString(buffer, index, out int length);
                index += length;

                GroupDescribe = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(KXTBitConvert.ToBytes(GroupID));
            buffer.AddRange(KXTBitConvert.ToBytes(GroupDescribe));

            return buffer.ToArray();
        }
    }

    public class UserGroupInfoReq : IKXTServer.IKXTSerialization
    {
        public string ID;

        public UserGroupInfoReq()
        {
            ID = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                ID = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            return KXTBitConvert.ToBytes(ID);
        }
    }

    public class UserInfoRes : IKXTServer.IKXTSerialization
    {
        public byte UserPicture;
        public string UserID;
        public string UserName;
        public string UserDescribe;
        public string UserPhone;
        public string UserEmail;

        public UserInfoRes()
        {
            UserPicture = 0x00;
            UserID = "";
            UserName = "";
            UserDescribe = "";
            UserPhone = "";
            UserEmail = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                UserPicture = buffer[index++];

                int length = 0;
                UserID = KXTBitConvert.ToString(buffer, index, out length);
                index += length;

                UserName = KXTBitConvert.ToString(buffer, index, out length);
                index += length;

                UserDescribe = KXTBitConvert.ToString(buffer, index, out length);
                index += length;

                UserPhone = KXTBitConvert.ToString(buffer, index, out length);
                index += length;

                UserEmail = KXTBitConvert.ToString(buffer, index, out length);
                index += length;
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.Add(UserPicture);
            buffer.AddRange(KXTBitConvert.ToBytes(UserID));
            buffer.AddRange(KXTBitConvert.ToBytes(UserName));
            buffer.AddRange(KXTBitConvert.ToBytes(UserDescribe));
            buffer.AddRange(KXTBitConvert.ToBytes(UserPhone));
            buffer.AddRange(KXTBitConvert.ToBytes(UserEmail));

            return buffer.ToArray();
        }
    }

    public class GroupInfoRes : IKXTServer.IKXTSerialization
    {
        public byte UserPicture;
        public string GroupID;
        public string GroupName;
        public string GroupDescribe;
        public string CreatorID;
        public string CreatorName;
        public DateTime CreateTime;

        public GroupInfoRes()
        {
            UserPicture = 0x00;
            GroupID = "";
            GroupName = "";
            GroupDescribe = "";
            CreatorID = "";
            CreatorName = "";
            CreateTime = DateTime.Now;
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                UserPicture = buffer[index++];

                int length = 0;

                GroupID = KXTBitConvert.ToString(buffer, index, out length);
                index += length;

                GroupName = KXTBitConvert.ToString(buffer, index, out length);
                index += length;

                GroupDescribe = KXTBitConvert.ToString(buffer, index, out length);
                index += length;

                CreatorID = KXTBitConvert.ToString(buffer, index, out length);
                index += length;

                CreatorName = KXTBitConvert.ToString(buffer, index, out length);
                index += length;

                CreateTime = KXTBitConvert.ToDateTime(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.Add(UserPicture);
            buffer.AddRange(KXTBitConvert.ToBytes(GroupID));
            buffer.AddRange(KXTBitConvert.ToBytes(GroupName));
            buffer.AddRange(KXTBitConvert.ToBytes(GroupDescribe));
            buffer.AddRange(KXTBitConvert.ToBytes(CreatorID));
            buffer.AddRange(KXTBitConvert.ToBytes(CreatorName));
            buffer.AddRange(KXTBitConvert.ToBytes(CreateTime));

            return buffer.ToArray();
        }
    }

    public class FriendGroupApplyRequest : IKXTServer.IKXTSerialization
    {
        public string UserID;

        public FriendGroupApplyRequest()
        {
            UserID = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                UserID = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            return KXTBitConvert.ToBytes(UserID);
        }
    }

    public class FriendsResponse : IKXTServer.IKXTSerialization
    {
        public string[] FriendsID;

        public FriendsResponse()
        {
            FriendsID = new string[0];
        }

        public void FromBytes(byte[] buffer, int index)
        {
            List<string> lists = new List<string>();
            try
            {
                int count = (buffer.Length - index) >> 4;
                if (0 < count)
                {
                    for (int i = 0; i < count; ++i) 
                    {
                        lists.Add(KXTBitConvert.ToString(buffer, index));
                        index += PreFriendIDLength;
                    }
                }
            }
            catch
            {

            }

            FriendsID = lists.ToArray();
        }
        public byte[] ToByteArray()
        {
            int count = 
                null == FriendsID ? 
                0 : 
                FriendsID.Length > FriendsMaxCount ? FriendsMaxCount : FriendsID.Length;

            List<byte> buffer = new List<byte>();

            for (int i = 0; i < count; ++i)
                buffer.AddRange(KXTBitConvert.ToBytes(FriendsID[i]));

            return buffer.ToArray();
        }

        public const int PreFriendIDLength = 16;
        public const int FriendsMaxCount = 250;
    }

    public class GroupsResponse : IKXTServer.IKXTSerialization
    {
        public string[] GroupsID;

        public GroupsResponse()
        {
            GroupsID = new string[0];
        }

        public void FromBytes(byte[] buffer, int index)
        {
            List<string> lists = new List<string>();
            try
            {
                int count = (buffer.Length - index) >> 4;
                if (0 < count)
                {
                    for (int i = 0; i < count; ++i)
                    {
                        lists.Add(KXTBitConvert.ToString(buffer, index));
                        index += PreGroupIDLength;
                    }
                }
            }
            catch
            {

            }

            GroupsID = lists.ToArray();
        }
        public byte[] ToByteArray()
        {
            int count =
                null == GroupsID ?
                0 :
                GroupsID.Length > GroupsMaxCount ? GroupsMaxCount : GroupsID.Length;

            List<byte> buffer = new List<byte>();

            for (int i = 0; i < count; ++i)
                buffer.AddRange(KXTBitConvert.ToBytes(GroupsID[i]));

            return buffer.ToArray();
        }

        public const int PreGroupIDLength = 10;
        public const int GroupsMaxCount = 400;
    }

    public class AppliesResponse : IKXTServer.IKXTSerialization
    {
        public ApplyPackage[] Applies;

        public void FromBytes(byte[] buffer, int index)
        {
            List<ApplyPackage> temp = new List<ApplyPackage>();
            try
            {
                int count = BitConverter.ToInt32(buffer, index);
                index += 4;

                for (int i = 0; i < count; ++i)
                {
                    ApplyPackage user = new ApplyPackage();
                    user.FromBytes(buffer, ref index);
                    temp.Add(user);
                }
            }
            catch
            {

            }
            Applies = temp.ToArray();
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            {
                List<byte> objs = new List<byte>();
                int count = 0;

                for (; count < Applies.Length; ++count)
                {
                    byte[] vs = Applies[count].ToByteArray();
                    if (Datagram.DatagramDataMax >= objs.Count + vs.Length)
                        break;

                    objs.AddRange(vs);
                }

                buffer.AddRange(BitConverter.GetBytes(count));
                buffer.AddRange(objs);
            }

            return buffer.ToArray();
        }

        public class ApplyPackage
        {
            public byte TargetType;
            public byte ApplierPicture;
            public string TargetID;
            public string ApplierID;
            public string ApplierName;
            public string ApplierDescribe;
            public DateTime ApplyTime;
            public string Message;

            public ApplyPackage()
            {
                TargetType = TargetType_Friend;
                TargetID = "";
                ApplierPicture = 0x00;
                ApplierID = "";
                ApplierName = "";
                ApplierDescribe = "";
                ApplyTime = DateTime.Now;
                Message = "";
            }

            public byte[] ToByteArray()
            {
                List<byte> buffer = new List<byte>();

                buffer.Add(TargetType);
                buffer.Add(ApplierPicture);
                buffer.AddRange(KXTBitConvert.ToBytes(TargetID));
                buffer.AddRange(KXTBitConvert.ToBytes(ApplierID));
                buffer.AddRange(KXTBitConvert.ToBytes(ApplierName));
                buffer.AddRange(KXTBitConvert.ToBytes(ApplierDescribe));
                buffer.AddRange(KXTBitConvert.ToBytes(ApplyTime));
                buffer.AddRange(KXTBitConvert.ToBytes(Message));

                return buffer.ToArray();
            }
            public void FromBytes(byte[] buffer, ref int index)
            {
                try
                {
                    TargetType = buffer[index++];
                    ApplierPicture = buffer[index++];

                    int length = 0;
                    TargetID = KXTBitConvert.ToString(buffer, index, out length);
                    index += length;

                    ApplierID = KXTBitConvert.ToString(buffer, index, out length);
                    index += length;

                    ApplierName = KXTBitConvert.ToString(buffer, index, out length);
                    index += length;

                    ApplierDescribe = KXTBitConvert.ToString(buffer, index, out length);
                    index += length;

                    ApplyTime = KXTBitConvert.ToDateTime(buffer, index);
                    index += 8;

                    Message = KXTBitConvert.ToString(buffer, index, out length);
                    index += length;
                }
                catch
                {

                }
            }

            public const byte TargetType_Friend = 0x00;
            public const byte TargetType_Group = 0x01;
        }
    }

    public class GroupMemberReq : IKXTServer.IKXTSerialization
    {
        public string GroupID;

        public GroupMemberReq()
        {
            GroupID = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                GroupID = IKXTServer.KXTBitConvert.ToString(buffer, index);
            }
            catch 
            {
                
            }
        }
        public byte[] ToByteArray()
        {
            return IKXTServer.KXTBitConvert.ToBytes(GroupID);
        }
    }

    public class GroupMemberRes : IKXTServer.IKXTSerialization
    {
        public readonly List<GroupMemberItem> Members;

        public GroupMemberRes()
        {
            Members = new List<GroupMemberItem>();
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                int count = BitConverter.ToInt32(buffer, index);
                index += 4;

                for (int i = 0; i < count; ++i)
                {
                    GroupMemberItem item = new GroupMemberItem();
                    item.FromBytes(buffer, index, out int length);
                    index += length;

                    Members.Add(item);
                }
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            int count = 0;
            for (int i = 0; i < Members.Count; ++i)
            {
                byte[] vs = Members[i].ToByteArray();

                if (Datagram.DatagramDataMax - 4 < buffer.Count + vs.Length)
                    break;

                buffer.AddRange(vs);
                ++count;
            }

            List<byte> result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(count));
            result.AddRange(buffer);
            return result.ToArray();
        }
        public byte[][] ToByteArrays()
        {
            List<byte[]> result = new List<byte[]>();

            List<byte> buffer = new List<byte>();

            List<byte> temp = new List<byte>();
            int count = 0;
            foreach (GroupMemberItem item in Members)
            {
                byte[] vs = item.ToByteArray();

                if (Datagram.DatagramDataMax - 4 < buffer.Count + vs.Length)
                {
                    temp.Clear();
                    temp.AddRange(BitConverter.GetBytes(count));
                    temp.AddRange(buffer);
                    result.Add(temp.ToArray());

                    buffer.Clear();
                    count = 0;
                }
                
                buffer.AddRange(vs);
                ++count;
            }

            temp.Clear();
            temp.AddRange(BitConverter.GetBytes(count));
            temp.AddRange(buffer);
            result.Add(temp.ToArray());

            return result.ToArray();
        }

        public class GroupMemberItem : IKXTServer.IKXTSerialization
        {
            public byte UserPicture;
            public string UserID;
            public string UserName;

            public GroupMemberItem()
            {
                UserPicture = 0x00;
                UserID = "";
                UserName = "";
            }

            public byte[] ToByteArray()
            {
                List<byte> buffer = new List<byte>();

                buffer.Add(UserPicture);
                buffer.AddRange(IKXTServer.KXTBitConvert.ToBytes(UserID));
                buffer.AddRange(IKXTServer.KXTBitConvert.ToBytes(UserName));

                return buffer.ToArray();
            }
            public void FromBytes(byte[] buffer, int index)
            {
                FromBytes(buffer, index, out _);
            }
            public void FromBytes(byte[] buffer, int index, out int length)
            {
                length = 0;

                try
                {
                    UserPicture = buffer[index++];
                    ++length;

                    UserID = IKXTServer.KXTBitConvert.ToString(buffer, index, out int count);
                    length += count;

                    UserName = IKXTServer.KXTBitConvert.ToString(buffer, index, out count);
                    length += count;
                }
                catch
                {

                }
            }
        }
    }

    public class CreateGroupReq : IKXTServer.IKXTSerialization
    {
        public string GroupName;
        public string CreatorID;
        public string GroupDescribe;

        public CreateGroupReq()
        {
            GroupName = "";
            CreatorID = "";
            GroupDescribe = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                int length = 0;
                GroupName = KXTBitConvert.ToString(buffer, index, out length);
                index += length;

                CreatorID = KXTBitConvert.ToString(buffer, index, out length);
                index += length;

                GroupDescribe = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(KXTBitConvert.ToBytes(GroupName));
            buffer.AddRange(KXTBitConvert.ToBytes(CreatorID));
            buffer.AddRange(KXTBitConvert.ToBytes(GroupDescribe));

            return buffer.ToArray();
        }
    }

    public class CreateGroupRes : IKXTServer.IKXTSerialization
    {
        public string GroupID;

        public CreateGroupRes()
        {
            GroupID = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                GroupID = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            return KXTBitConvert.ToBytes(GroupID);
        }
    }

    public class DeleteGroup : IKXTServer.IKXTSerialization
    {
        public string GroupID;

        public DeleteGroup()
        {
            GroupID = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                GroupID = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            return KXTBitConvert.ToBytes(GroupID);
        }
    }

    public class ApplyRequest : IKXTServer.IKXTSerialization
    {
        public byte TargetType;
        public string TargetID;
        public DateTime ApplyTime;
        public string Message;

        public ApplyRequest()
        {
            TargetType = TargetType_Friend;
            TargetID = "";
            ApplyTime = DateTime.Now;
            Message = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                TargetType = buffer[index++];
                TargetID = KXTBitConvert.ToString(buffer, index, out int length);
                index += length;
                ApplyTime = KXTBitConvert.ToDateTime(buffer, index);
                index += 8;
                Message = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.Add(TargetType);
            buffer.AddRange(KXTBitConvert.ToBytes(TargetID));
            buffer.AddRange(KXTBitConvert.ToBytes(ApplyTime));
            buffer.AddRange(KXTBitConvert.ToBytes(Message));

            return buffer.ToArray();
        }

        public const byte TargetType_Friend = 0x00;
        public const byte TargetType_Group = 0x01;
    }

    public class ApplyResponse : IKXTServer.IKXTSerialization
    {
        public string TargetID;
        public string ApplierID;
        public byte ApplyResult;

        public ApplyResponse()
        {
            TargetID = "";
            ApplierID = "";
            ApplyResult = ApplyResult_Submit;
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                ApplyResult = buffer[index++];

                TargetID = KXTBitConvert.ToString(buffer, index, out int length);
                index += length;

                ApplierID = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.Add(ApplyResult);
            buffer.AddRange(KXTBitConvert.ToBytes(TargetID));
            buffer.AddRange(KXTBitConvert.ToBytes(ApplierID));

            return buffer.ToArray();
        }

        public const byte ApplyResult_Submit = 0x00;
        public const byte ApplyResult_Ignor = 0x01;
        public const byte ApplyResult_Refuse = 0x02;
    }

    public class DeleteUser : IKXTServer.IKXTSerialization
    {
        public string UserID;
        public string FriendID;

        public DeleteUser()
        {
            UserID = "";
            FriendID = "";
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                UserID = KXTBitConvert.ToString(buffer, index, out int count);
                FriendID = KXTBitConvert.ToString(buffer, index + count);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(KXTBitConvert.ToBytes(UserID));
            buffer.AddRange(KXTBitConvert.ToBytes(FriendID));

            return buffer.ToArray();
        }
    }

    public class SearchReq : IKXTServer.IKXTSerialization
    {
        public string SearchString;
        public byte SearchType;

        public SearchReq()
        {
            SearchString = "";
            SearchType = SearchType_User;
        }

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                SearchType = buffer[index++];

                SearchString = KXTBitConvert.ToString(buffer, index);
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.Add(SearchType);
            buffer.AddRange(KXTBitConvert.ToBytes(SearchString));

            return buffer.ToArray();
        }

        public const byte SearchType_User = 0x00;
        public const byte SearchType_Group = 0x01;
    }

    public class SearchRes : IKXTServer.IKXTSerialization
    {
        public byte SearchType;
        public object SearchResult;

        public void FromBytes(byte[] buffer, int index)
        {
            try
            {
                SearchType = buffer[index++];

                if (SearchType_User == SearchType)
                {
                    List<UserSearchResult> temp = new List<UserSearchResult>();

                    try
                    {
                        int count = BitConverter.ToInt32(buffer, index);
                        index += 4;

                        for (int i = 0; i < count; ++i)
                        {
                            UserSearchResult user = new UserSearchResult();
                            user.FromBytes(buffer, ref index);
                            temp.Add(user);
                        }
                    }
                    catch
                    {

                    }

                    SearchResult = temp.ToArray();
                }
                else
                {
                    List<GroupSearchResult> temp = new List<GroupSearchResult>();

                    try
                    {
                        int count = BitConverter.ToInt32(buffer, index);
                        index += 4;

                        for (int i = 0; i < count; ++i)
                        {
                            GroupSearchResult group = new GroupSearchResult();
                            group.FromBytes(buffer, ref index);
                            temp.Add(group);
                        }
                    }
                    catch
                    {

                    }

                    SearchResult = temp.ToArray();
                }
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();
            buffer.Add(SearchType);

            if (null != SearchResult)
            {
                List<byte> objs = new List<byte>();
                int count = 0;

                try
                {
                    if (SearchType_User == SearchType)
                    {
                        UserSearchResult[] temp = SearchResult as UserSearchResult[];

                        for (; count < temp.Length; ++count)
                        {
                            byte[] vs = temp[count].ToByteArray();
                            if (Datagram.DatagramDataMax >= objs.Count + vs.Length)
                                break;

                            objs.AddRange(vs);
                        }
                    }
                    else
                    {
                        GroupSearchResult[] temp = SearchResult as GroupSearchResult[];

                        for (; count < temp.Length; ++count)
                        {
                            byte[] vs = temp[count].ToByteArray();
                            if (Datagram.DatagramDataMax >= objs.Count + vs.Length)
                                break;

                            objs.AddRange(vs);
                        }
                    }
                }
                catch
                {

                }

                buffer.AddRange(BitConverter.GetBytes(count));
                buffer.AddRange(objs);
            }

            return buffer.ToArray();
        }

        public const byte SearchType_User = 0x00;
        public const byte SearchType_Group = 0x01;

        public class UserSearchResult
        {
            public byte UserPicture;
            public string UserID;
            public string UserName;
            public string UserDescribe;
            public string UserPhone;
            public string UserEmail;

            public UserSearchResult()
            {
                UserPicture = 0x00;
                UserID = "";
                UserName = "";
                UserDescribe = "";
                UserPhone = "";
                UserEmail = "";
            }

            public byte[] ToByteArray()
            {
                List<byte> buffer = new List<byte>();

                buffer.Add(UserPicture);
                buffer.AddRange(KXTBitConvert.ToBytes(UserID));
                buffer.AddRange(KXTBitConvert.ToBytes(UserName));
                buffer.AddRange(KXTBitConvert.ToBytes(UserDescribe));
                buffer.AddRange(KXTBitConvert.ToBytes(UserPhone));
                buffer.AddRange(KXTBitConvert.ToBytes(UserEmail));

                return buffer.ToArray();
            }
            public void FromBytes(byte[] buffer, ref int index)
            {
                try
                {
                    UserPicture = buffer[index++];

                    int length = 0;
                    UserID = KXTBitConvert.ToString(buffer, index, out length);
                    index += length;

                    UserName = KXTBitConvert.ToString(buffer, index, out length);
                    index += length;

                    UserDescribe = KXTBitConvert.ToString(buffer, index, out length);
                    index += length;

                    UserPhone = KXTBitConvert.ToString(buffer, index, out length);
                    index += length;

                    UserEmail = KXTBitConvert.ToString(buffer, index, out length);
                    index += length;
                }
                catch
                {

                }
            }
        }

        public class GroupSearchResult
        {
            public string GroupID;
            public string GroupName;
            public string GroupDescribe;
            public string CreatorID;
            public string CreatorName;
            public DateTime CreateTime;

            public GroupSearchResult()
            {
                GroupID = "";
                GroupName = "";
                GroupDescribe = "";
                CreatorID = "";
                CreatorName = "";
                CreateTime = DateTime.Now;
            }

            public byte[] ToByteArray()
            {
                List<byte> buffer = new List<byte>();

                buffer.AddRange(KXTBitConvert.ToBytes(GroupID));
                buffer.AddRange(KXTBitConvert.ToBytes(GroupName));
                buffer.AddRange(KXTBitConvert.ToBytes(GroupDescribe));
                buffer.AddRange(KXTBitConvert.ToBytes(CreatorID));
                buffer.AddRange(KXTBitConvert.ToBytes(CreatorName));
                buffer.AddRange(KXTBitConvert.ToBytes(CreateTime));

                return buffer.ToArray();
            }
            public void FromBytes(byte[] buffer, ref int index)
            {
                try
                {
                    int length = 0;

                    GroupID = KXTBitConvert.ToString(buffer, index, out length);
                    index += length;

                    GroupName = KXTBitConvert.ToString(buffer, index, out length);
                    index += length;

                    GroupDescribe = KXTBitConvert.ToString(buffer, index, out length);
                    index += length;

                    CreatorID = KXTBitConvert.ToString(buffer, index, out length);
                    index += length;

                    CreatorName = KXTBitConvert.ToString(buffer, index, out length);
                    index += length;

                    CreateTime = KXTBitConvert.ToDateTime(buffer, index);
                }
                catch
                {

                }
            }
        }
    }
}
