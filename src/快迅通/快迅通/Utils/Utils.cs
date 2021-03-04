using KXTNetStruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI;
using System.Security.Cryptography;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using KXTNetStruct.Struct;

namespace 快迅通.Utils
{
    internal class Utils
    {
        public static Windows.UI.Xaml.Media.Brush GetViewBackground()
        {
            Windows.UI.Xaml.Media.Brush brush = null;
            if (OperatingData.AcrylicModel)
            {
                switch (OperatingData._Theme)
                {
                    case _Interface_Theme._Green_:
                        brush = new Windows.UI.Xaml.Media.AcrylicBrush()
                        {
                            BackgroundSource = Windows.UI.Xaml.Media.AcrylicBackgroundSource.HostBackdrop,
                            TintOpacity = OperatingData.AcrylicOpacity,
                            TintColor = Color.FromArgb(0x7F, 0xAF, 0xF0, 0x00),
                            FallbackColor = Color.FromArgb(0x7F, 0xAF, 0xF0, 0x00)
                        };
                        break;
                    case _Interface_Theme._Red_:
                        brush = new Windows.UI.Xaml.Media.AcrylicBrush()
                        {
                            BackgroundSource = Windows.UI.Xaml.Media.AcrylicBackgroundSource.HostBackdrop,
                            TintOpacity = OperatingData.AcrylicOpacity,
                            TintColor = Color.FromArgb(0xFF, 0xF0, 0xA9, 0xFF),
                            FallbackColor = Color.FromArgb(0xFF, 0xF0, 0xA9, 0xFF)
                        };
                        break;
                    case _Interface_Theme._White_:
                        brush = new Windows.UI.Xaml.Media.AcrylicBrush()
                        {
                            BackgroundSource = Windows.UI.Xaml.Media.AcrylicBackgroundSource.HostBackdrop,
                            TintOpacity = OperatingData.AcrylicOpacity,
                            TintColor = Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC),
                            FallbackColor = Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC)
                        };
                        break;
                    case _Interface_Theme._Default_:
                        brush = new Windows.UI.Xaml.Media.AcrylicBrush()
                        {
                            BackgroundSource = Windows.UI.Xaml.Media.AcrylicBackgroundSource.HostBackdrop,
                            TintOpacity = OperatingData.AcrylicOpacity,
                            TintColor = Color.FromArgb(0xFF, 0x66, 0xCC, 0xFF),
                            FallbackColor = Color.FromArgb(0xFF, 0x66, 0xCC, 0xFF)
                        };
                        break;
                }
            }
            else
            {
                switch (OperatingData._Theme)
                {
                    case _Interface_Theme._Green_:
                        brush = new Windows.UI.Xaml.Media.SolidColorBrush()
                        {
                            Color = Color.FromArgb(0x7F, 0xAF, 0xF0, 0x00),
                            Opacity = OperatingData.AcrylicOpacity
                        };
                        break;
                    case _Interface_Theme._Red_:
                        brush = new Windows.UI.Xaml.Media.SolidColorBrush()
                        {
                            Color = Color.FromArgb(0xFF, 0xF0, 0xA9, 0xFF),
                            Opacity = OperatingData.AcrylicOpacity
                        };
                        break;
                    case _Interface_Theme._White_:
                        brush = new Windows.UI.Xaml.Media.SolidColorBrush()
                        {
                            Color = Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC),
                            Opacity = OperatingData.AcrylicOpacity
                        };
                        break;
                    case _Interface_Theme._Default_:
                        brush = new Windows.UI.Xaml.Media.SolidColorBrush()
                        {
                            Color = Color.FromArgb(0xFF, 0x66, 0xCC, 0xFF),
                            Opacity = OperatingData.AcrylicOpacity
                        };
                        break;
                }
            }
            return brush;
        }
        public static Windows.UI.Xaml.Media.Brush GetViewPre()
        {
            Windows.UI.Xaml.Media.Brush brush = null;
            brush = new Windows.UI.Xaml.Media.SolidColorBrush()
            {
                Color = Color.FromArgb(0xFF, 0x22, 0x22, 0x22),
                Opacity = OperatingData.AcrylicOpacity
            };
            //switch (OperatingData._Theme)
            //{
            //    case KXTDataStruct._Interface_Theme._Green_:
            //        brush = new Windows.UI.Xaml.Media.SolidColorBrush()
            //        {
            //            Color = Color.FromArgb(0x7F, 0xAF, 0xF0, 0x00),
            //            Opacity = OperatingData.AcrylicOpacity
            //        };
            //        break;
            //    case KXTDataStruct._Interface_Theme._Red_:
            //        brush = new Windows.UI.Xaml.Media.SolidColorBrush()
            //        {
            //            Color = Color.FromArgb(0xFF, 0xF0, 0xA9, 0xFF),
            //            Opacity = OperatingData.AcrylicOpacity
            //        };
            //        break;
            //    case KXTDataStruct._Interface_Theme._White_:
            //        brush = new Windows.UI.Xaml.Media.SolidColorBrush()
            //        {
            //            Color = Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC),
            //            Opacity = OperatingData.AcrylicOpacity
            //        };
            //        break;
            //    case KXTDataStruct._Interface_Theme._Default_:
            //        brush = new Windows.UI.Xaml.Media.SolidColorBrush()
            //        {
            //            Color = Color.FromArgb(0xFF, 0x66, 0xCC, 0xFF),
            //            Opacity = OperatingData.AcrylicOpacity
            //        };
            //        break;
            //}
            return brush;
        }
        public static Windows.UI.Xaml.Media.Brush GetViewIllustrator()
        {
            Windows.UI.Xaml.Media.Brush brush = null;
            brush = new Windows.UI.Xaml.Media.SolidColorBrush()
            {
                Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF),
                Opacity = OperatingData.AcrylicOpacity
            };
            //switch (OperatingData._Theme)
            //{
            //    case KXTDataStruct._Interface_Theme._Green_:
            //        brush = new Windows.UI.Xaml.Media.SolidColorBrush()
            //        {
            //            Color = Color.FromArgb(0x7F, 0xAF, 0xF0, 0x00),
            //            Opacity = OperatingData.AcrylicOpacity
            //        };
            //        break;
            //    case KXTDataStruct._Interface_Theme._Red_:
            //        brush = new Windows.UI.Xaml.Media.SolidColorBrush()
            //        {
            //            Color = Color.FromArgb(0xFF, 0xF0, 0xA9, 0xFF),
            //            Opacity = OperatingData.AcrylicOpacity
            //        };
            //        break;
            //    case KXTDataStruct._Interface_Theme._White_:
            //        brush = new Windows.UI.Xaml.Media.SolidColorBrush()
            //        {
            //            Color = Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC),
            //            Opacity = OperatingData.AcrylicOpacity
            //        };
            //        break;
            //    case KXTDataStruct._Interface_Theme._Default_:
            //        brush = new Windows.UI.Xaml.Media.SolidColorBrush()
            //        {
            //            Color = Color.FromArgb(0xFF, 0x66, 0xCC, 0xFF),
            //            Opacity = OperatingData.AcrylicOpacity
            //        };
            //        break;
            //}
            return brush;
        }
        public static Windows.UI.Xaml.Media.Brush GetViewIllustratorForce()
        {
            Windows.UI.Xaml.Media.Brush brush = null;
            brush = new Windows.UI.Xaml.Media.SolidColorBrush()
            {
                Color = Color.FromArgb(0xFF, 0x00, 0x00, 0x00),
                Opacity = OperatingData.AcrylicOpacity
            };
            //switch (OperatingData._Theme)
            //{
            //    case KXTDataStruct._Interface_Theme._Green_:
            //        brush = new Windows.UI.Xaml.Media.SolidColorBrush()
            //        {
            //            Color = Color.FromArgb(0x7F, 0xAF, 0xF0, 0x00),
            //            Opacity = OperatingData.AcrylicOpacity
            //        };
            //        break;
            //    case KXTDataStruct._Interface_Theme._Red_:
            //        brush = new Windows.UI.Xaml.Media.SolidColorBrush()
            //        {
            //            Color = Color.FromArgb(0xFF, 0xF0, 0xA9, 0xFF),
            //            Opacity = OperatingData.AcrylicOpacity
            //        };
            //        break;
            //    case KXTDataStruct._Interface_Theme._White_:
            //        brush = new Windows.UI.Xaml.Media.SolidColorBrush()
            //        {
            //            Color = Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC),
            //            Opacity = OperatingData.AcrylicOpacity
            //        };
            //        break;
            //    case KXTDataStruct._Interface_Theme._Default_:
            //        brush = new Windows.UI.Xaml.Media.SolidColorBrush()
            //        {
            //            Color = Color.FromArgb(0xFF, 0x66, 0xCC, 0xFF),
            //            Opacity = OperatingData.AcrylicOpacity
            //        };
            //        break;
            //}
            return brush;
        }

        public static _File_Size_Units_ PraseFileSizeUnit(int size_bytes, out double size_unit)
        {
            size_unit = size_bytes;
            if (1024 <= size_unit)
            {
                size_unit /= 1024;
                if (1024 <= size_unit)
                {
                    size_unit /= 1024;
                    if (1024 <= size_unit)
                    {
                        size_unit /= 1024;
                        return _File_Size_Units_._G_Bytes_;
                    }
                    return _File_Size_Units_._M_Bytes_;
                }
                return _File_Size_Units_._K_Bytes_;
            }
            return _File_Size_Units_._Bytes_;
        }

        public static bool LoginUserCheck(string input, out LoginType type)
        {
            type = LoginType.Identification;

            if (IsKXTID(input))
                return true;

            if (IsPhone(input))
            {
                type = LoginType.Phone;
                return true;
            }

            if (IsEmail(input))
            {
                type = LoginType.Email;
                return true;
            }

            return false;
        }

        public static ImageSource GetPhoto(int index)
        {
            ImageSource image = null;
            if (0 == index)
                image = new BitmapImage(new Uri("ms-appx:///Assets/action-item-login.png"));
            else
            {
                string url = "ms-appx:///ICONS/Photo/50" + index.ToString("D3") + ".png";
                image = new BitmapImage(new Uri(url));
            }
            return image;
        }

        public static bool IsKXTID(string source)
        {
            return Regex.IsMatch(source, @"^[0-9]{6}$");
        }
        public static bool IsPhone(string source)
        {
            return Regex.IsMatch(source, @"^1[0-9]{10}$");
        }
        public static bool IsEmail(string source)
        {
            return Regex.IsMatch(source, @"^\w+([-+.](_|\w)+)*@(\w+(.|\w)*\.)+\w+(.|\w)*$");
        }

        public static bool ConvertVerifyCode(string code, out byte[] value)
        {
            value = new byte[8];

            for (int i = 0; i < value.Length; ++i)
            {
                if ('0' <= code[i] && '9' >= code[i])
                    value[i] = (byte)(code[i] - '0');
                else if ('a' <= code[i] && 'z' >= code[i])
                    value[i] = (byte)(code[i] - 'a');
                else if ('A' <= code[i] && 'Z' >= code[i])
                    value[i] = (byte)(code[i] - 'A');
                else
                    return false;
            }

            return true;
        }
    }

    internal class Security
    {
        public static string SHA1Encode(string source)
        {
            SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
            byte[] passbytes = Encoding.UTF8.GetBytes(source);
            passbytes = sha256.ComputeHash(passbytes);
            return BitConverter.ToString(passbytes).Replace("-", "");
        }
    }
}
