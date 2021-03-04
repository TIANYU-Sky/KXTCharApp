using KXTNetStruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace 快迅通.Utils
{
    public class TemporaryCloudList
    {
        public string GUID { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }
        public int Size { get; set; }
    }
    public class TemporaryFolderTree
    {
        public string Name { get; set; }
        public List<TemporaryFolderTree> Children { get; set; }
    }
    public class TemporaryFilesList
    {
        public string GUID { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }
        public int Size { get; set; }
        public ImageSource Icon
        {
            get
            {
                string url = "ms-appx:///ICONS/Files/FileLogo.png";
                switch (Type)
                {
                    case FileType.Adobe_AeFile:
                        url = "ms-appx:///ICONS/Files/AEFilesLogo.png";
                        break;
                    case FileType.Adobe_AiFile:
                        url = "ms-appx:///ICONS/Files/AIFilesLogo.png";
                        break;
                    case FileType.Adobe_XdFile:
                        url = "ms-appx:///ICONS/Files/XdFilesLogo.png";
                        break;
                    case FileType.Adobe_IdFile:
                        url = "ms-appx:///ICONS/Files/IDFilesLogo.png";
                        break;
                    case FileType.Adobe_PrFile:
                        url = "ms-appx:///ICONS/Files/PrFilesLogo.png";
                        break;
                    case FileType.Adobe_PsFile:
                        url = "ms-appx:///ICONS/Files/PSFilesLogo.png";
                        break;
                    case FileType.Code_CodeFile:
                        url = "ms-appx:///ICONS/Files/CodeFileLogo.png";
                        break;
                    case FileType.Code_CsFile:
                        url = "ms-appx:///ICONS/Files/CSFileLogo.png";
                        break;
                    case FileType.Code_CssFile:
                        url = "ms-appx:///ICONS/Files/CSSFileLogo.png";
                        break;
                    case FileType.Code_C_CppFile:
                        url = "ms-appx:///ICONS/Files/C_CppFileLogo.png";
                        break;
                    case FileType.Microsoft_ExcelFile:
                        url = "ms-appx:///ICONS/Files/ExcelFileLogo.png";
                        break;
                    case FileType.Microsoft_PptFile:
                        url = "ms-appx:///ICONS/Files/PPPTFileLogo.png";
                        break;
                    case FileType.Microsoft_VisioFile:
                        url = "ms-appx:///ICONS/Files/VisioFileLogo.png";
                        break;
                    case FileType.Microsoft_WordFile:
                        url = "ms-appx:///ICONS/Files/WordFileLogo.png";
                        break;
                    case FileType.Music:
                        url = "ms-appx:///ICONS/Files/MusicFileLogo2.png";
                        break;
                    case FileType.Picture:
                        url = "ms-appx:///ICONS/Files/PictureFileLogo2.png";
                        break;
                    case FileType.Video:
                        url = "ms-appx:///ICONS/Files/VideoFileLogo2.png";
                        break;
                    case FileType.Folder:
                        url = "ms-appx:///ICONS/Files/FolderLogo.png";
                        break;
                    case FileType.UnKnown:
                    default:
                        break;
                }
                return new BitmapImage(new Uri(url));
            }
        }
        public FileType Type { get; set; }
    }
    public class TemporaryWaitingObject
    {
        public Guid ID { get; set; }
        public ImageSource Icon
        {
            get
            {
                string url = "ms-appx:///ICONS/Files/FileLogo.png";
                switch (FileType)
                {
                    case FileType.Adobe_AeFile:
                        url = "ms-appx:///ICONS/Files/AEFilesLogo.png";
                        break;
                    case FileType.Adobe_AiFile:
                        url = "ms-appx:///ICONS/Files/AIFilesLogo.png";
                        break;
                    case FileType.Adobe_XdFile:
                        url = "ms-appx:///ICONS/Files/XdFilesLogo.png";
                        break;
                    case FileType.Adobe_IdFile:
                        url = "ms-appx:///ICONS/Files/IDFilesLogo.png";
                        break;
                    case FileType.Adobe_PrFile:
                        url = "ms-appx:///ICONS/Files/PrFilesLogo.png";
                        break;
                    case FileType.Adobe_PsFile:
                        url = "ms-appx:///ICONS/Files/PSFilesLogo.png";
                        break;
                    case FileType.Code_CodeFile:
                        url = "ms-appx:///ICONS/Files/CodeFileLogo.png";
                        break;
                    case FileType.Code_CsFile:
                        url = "ms-appx:///ICONS/Files/CSFileLogo.png";
                        break;
                    case FileType.Code_CssFile:
                        url = "ms-appx:///ICONS/Files/CSSFileLogo.png";
                        break;
                    case FileType.Code_C_CppFile:
                        url = "ms-appx:///ICONS/Files/C_CppFileLogo.png";
                        break;
                    case FileType.Microsoft_ExcelFile:
                        url = "ms-appx:///ICONS/Files/ExcelFileLogo.png";
                        break;
                    case FileType.Microsoft_PptFile:
                        url = "ms-appx:///ICONS/Files/PPPTFileLogo.png";
                        break;
                    case FileType.Microsoft_VisioFile:
                        url = "ms-appx:///ICONS/Files/VisioFileLogo.png";
                        break;
                    case FileType.Microsoft_WordFile:
                        url = "ms-appx:///ICONS/Files/WordFileLogo.png";
                        break;
                    case FileType.Music:
                        url = "ms-appx:///ICONS/Files/MusicFileLogo2.png";
                        break;
                    case FileType.Picture:
                        url = "ms-appx:///ICONS/Files/PictureFileLogo2.png";
                        break;
                    case FileType.Video:
                        url = "ms-appx:///ICONS/Files/VideoFileLogo2.png";
                        break;
                    case FileType.Folder:
                        url = "ms-appx:///ICONS/Files/FolderLogo.png";
                        break;
                    case FileType.UnKnown:
                    default:
                        break;
                }
                return new BitmapImage(new Uri(url));
            }
        }
        public FileType FileType { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public Symbol Type { get; set; }
    }
    public class TemporaryStreamItem
    {
        public Guid StreamID { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public string Schedult { get; set; }
        public string Type { get; set; }
        public string Time { get; set; }
    }

    public class TemporaryFriendCommunity
    {
        public DateTime Time;
        public string Message;
    }

    public class TemporaryFriendsList
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Describe { get; set; }
        public bool OnLine { get; set; }
        public ImageSource Photo
        {
            get
            {
                return Utils.GetPhoto(PhotoIndex);
            }
        }
        public int PhotoIndex { get; set; }
        public string MessageCount
        {
            get
            {
                return 100 > Message.Count ? Message.Count.ToString() : "99+";
            }
        }
        public List<TemporaryFriendCommunity> Message { get; }
        public Visibility OnLineState
        {
            get
            {
                return OnLine ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public Visibility MessageState
        {
            get
            {
                return 0 < Message.Count ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public TemporaryFriendsList()
        {
            Message = new List<TemporaryFriendCommunity>();
        }
    }

    public class TemporaryGroupMessage
    {
        public DateTime Time;
        public string UserID;
        public string Message;
    }

    public class TemporaryGroupsList
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }
        public string ManagerID { get; set; }
        public string ManagerName { get; set; }
        public string Describe { get; set; }
        public string MessageCount
        {
            get
            {
                return 100 > Message.Count ? Message.Count.ToString() : "99+";
            }
        }
        public List<TemporaryGroupMessage> Message { get; }
        public Visibility MessageState
        {
            get
            {
                return 0 < Message.Count ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public TemporaryGroupsList()
        {
            Message = new List<TemporaryGroupMessage>();
        }
    }


}
