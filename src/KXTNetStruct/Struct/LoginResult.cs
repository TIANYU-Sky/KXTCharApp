using System;
using System.Collections.Generic;
using System.Text;

namespace KXTNetStruct.Struct
{
    public enum LoginResult : byte
    {
        Success,
        Error_User,
        Error_Phone,
        Error_Email,
        Error_Password,
        Error_Server
    }
}
