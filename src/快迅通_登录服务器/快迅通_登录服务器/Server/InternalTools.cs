using IKXTServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 快迅通_登录服务器.Server
{
    internal class InternalTools
    {
        private static byte[] UserIDPool = new byte[11]
        {
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00
        };
        private static byte[] GroupIDPool = new byte[8]
        {
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
        };

        public static string GetNewUserID()
        {
            for (int i = UserIDPool.Length - 1; i >= 0; --i)
            {
                UserIDPool[i] += 1;
                if (10 > UserIDPool[i])
                    break;
            }

            StringBuilder builder = new StringBuilder();

            builder.Append(UserIDPool[0]);
            builder.Append(UserIDPool[1]);
            builder.Append(UserIDPool[2]);
            builder.Append(UserIDPool[3]);
            builder.Append(UserIDPool[4]);
            builder.Append(UserIDPool[5]);
            builder.Append(UserIDPool[6]);
            builder.Append(UserIDPool[7]);
            builder.Append(UserIDPool[8]);
            builder.Append(UserIDPool[9]);
            builder.Append(UserIDPool[10]);

            return builder.ToString();
        }
        public static string GetNewGroupID()
        {
            for (int i = GroupIDPool.Length - 1; i >= 0; --i)
            {
                GroupIDPool[i] += 1;
                if (10 > GroupIDPool[i])
                    break;
            }

            StringBuilder builder = new StringBuilder();

            builder.Append(GroupIDPool[0]);
            builder.Append(GroupIDPool[1]);
            builder.Append(GroupIDPool[2]);
            builder.Append(GroupIDPool[3]);
            builder.Append(GroupIDPool[4]);
            builder.Append(GroupIDPool[5]);
            builder.Append(GroupIDPool[6]);
            builder.Append(GroupIDPool[7]);

            return builder.ToString();
        }
        public static void SetUserIDPool(byte[] source)
        {
            for (int i = 0; i < source.Length && i < UserIDPool.Length; ++i)
                UserIDPool[UserIDPool.Length - 1 - i] = source[source.Length - 1 - i];
        }
        public static void SetGroupIDPool(byte[] source)
        {
            for (int i = 0; i < source.Length && i < GroupIDPool.Length; ++i)
                GroupIDPool[UserIDPool.Length - 1 - i] = source[source.Length - 1 - i];
        }
        public static byte[] GetUserIDPool() => UserIDPool;
        public static byte[] GetGroupIDPool() => GroupIDPool;

        public static byte[] GenerateVerification()
        {
            byte[] buffer = new byte[VerifiesLength];

            Random random = new Random();

            for (int i = 0; i < buffer.Length; ++i)
                buffer[i] = (byte)random.Next(0, 36);

            return buffer;
        }
        public static string GenerateVerification(byte[] ver_code)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < ver_code.Length; ++i)
            {
                if (10 > ver_code[i])
                    builder.Append(ver_code[i]);
                else
                    builder.Append((char)(ver_code[i] + 55));
            }

            return builder.ToString();
        }
        public static bool VerificateCode(byte[] source_code, byte[] input_code)
        {
            if (source_code.Length != input_code.Length)
                return false;

            for (int i = 0; i < source_code.Length; ++i)
                if (source_code[i] != input_code[i])
                    return false;

            return true;
        }

        public static bool SendRegistVerificateEmail(string email, string verify_code)
        {
            EmailServer server = new EmailServer();

            string body = System.IO.File.ReadAllText(@"template\verify_email_source.html");
            body = body.Replace("{$$-USER_VERIFY_CODE-$$}", verify_code);
            body = body.Replace("{$$-USER_EMAIL_REASON-$$}", "注册验证时产生");
            body = body.Replace("{$$-USER_EMAIL_REMINDER-$$}", "在进行用户注册的关键过程中");

            return server.Send(email, "KXT令牌", body);
        }
        public static bool SendUpdatePWVerificateEmail(string email, string verify_code)
        {
            EmailServer server = new EmailServer();

            string body = System.IO.File.ReadAllText(@"template\verify_email_source.html");
            body = body.Replace("{$$-USER_VERIFY_CODE-$$}", verify_code);
            body = body.Replace("{$$-USER_EMAIL_REASON-$$}", "修改密码时产生");
            body = body.Replace("{$$-USER_EMAIL_REMINDER-$$}", "在进行用户密码修改的关键过程中");

            return server.Send(email, "KXT令牌", body);
        }
        public static bool SendUpdateEmailVerificateEmail(string email, string verify_code)
        {
            EmailServer server = new EmailServer();

            string body = System.IO.File.ReadAllText(@"template\verify_email_source.html");
            body = body.Replace("{$$-USER_VERIFY_CODE-$$}", verify_code);
            body = body.Replace("{$$-USER_EMAIL_REASON-$$}", "更改绑定邮箱时产生");
            body = body.Replace("{$$-USER_EMAIL_REMINDER-$$}", "在进行用户邮箱绑定修改的关键过程中");

            return server.Send(email, "KXT令牌", body);
        }

        public const int VerifiesLength = 8;
    }

    internal class EmailRequestPackage
    {
        public string UserID;
        public string Email;
        public byte[] Verify;
        public DateTime Time;
    }
}
