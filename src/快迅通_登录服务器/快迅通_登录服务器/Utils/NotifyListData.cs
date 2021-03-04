using IKXTServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace 快迅通_登录服务器.Utils
{
    public class NotifyListData
    {
        public string Message;
        public LogLevel Level;
        public ImageSource Icon
        {
            get
            {
                switch (Level)
                {
                    case LogLevel.Error:
                        return new BitmapImage(new Uri("pack://application:,,,/快迅通_登录服务器;component/Icons/Error.png"));
                    case LogLevel.Warning:
                        return new BitmapImage(new Uri("pack://application:,,,/快迅通_登录服务器;component/Icons/Warning.png"));
                    case LogLevel.Info:
                    default:
                        return new BitmapImage(new Uri("pack://application:,,,/快迅通_登录服务器;component/Icons/Info.png"));
                }
            }
        }
    }
}
