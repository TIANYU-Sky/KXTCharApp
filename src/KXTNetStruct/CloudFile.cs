using IKXTServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KXTNetStruct
{
    public enum FileType
    {
        Folder,
        UnKnown,
        Picture,
        Video,
        Music,
        Adobe_AeFile,
        Adobe_AiFile,
        Adobe_IdFile,
        Adobe_PrFile,
        Adobe_PsFile,
        Adobe_XdFile,
        Code_C_CppFile,
        Code_CodeFile,
        Code_CsFile,
        Code_CssFile,
        Microsoft_ExcelFile,
        Microsoft_WordFile,
        Microsoft_PptFile,
        Microsoft_VisioFile
    }

    public class CloudFile
    {
        public byte Folder;
        public string Name { get; private set; }
        public FileType Type { get; private set; }
        public DateTime Time;
        public int Size;

        public CloudFile()
        {
            Folder = 0x00;
            Name = "";
            Type = FileType.UnKnown;
            Time = DateTime.Now;
            Size = 0;
        }

        public string SetName
        {
            set
            {
                Name = value;
                Type = GetTypeFromExtension(value);
            }
        }

        public void FromByte(byte[] buffer, int index, out int length)
        {
            length = 0;

            try
            {
                Folder = buffer[index++];
                ++length;

                Type = (FileType)BitConverter.ToInt32(buffer, index);
                index += 4;
                length += 4;

                Size = BitConverter.ToInt32(buffer, index);
                index += 4;
                length += 4;

                Time = KXTBitConvert.ToDateTime(buffer, index);
                index += 8;
                length += 8;

                Name = KXTBitConvert.ToString(buffer, index, out int count);
                length += count;
            }
            catch
            {

            }
        }
        public byte[] ToByteArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.Add(Folder);
            buffer.AddRange(BitConverter.GetBytes((int)Type));
            buffer.AddRange(BitConverter.GetBytes(Size));
            buffer.AddRange(KXTBitConvert.ToBytes(Time));
            buffer.AddRange(KXTBitConvert.ToBytes(Name));

            return buffer.ToArray();
        }

        public static FileType GetTypeFromExtension(string name)
        {
            FileInfo file;

            try
            {
                file = new FileInfo(name);
            }
            catch
            {
                return FileType.UnKnown;
            }

            switch (file.Extension.ToLower())
            {
                case Picture_FileType_BMP:
                case Picture_FileType_JPG:
                case Picture_FileType_JPEG:
                case Picture_FileType_PNG:
                case Picture_FileType_TIG:
                case Picture_FileType_GIF:
                case Picture_FileType_EXIF:
                case Picture_FileType_RAW:
                case Picture_FileType_DNG:
                case Picture_FileType_CR2:
                case Picture_FileType_ICO:
                    return FileType.Picture;
                case Video_FileType_WMV:
                case Video_FileType_ASF:
                case Video_FileType_ASX:
                case Video_FileType_RM:
                case Video_FileType_RMVB:
                case Video_FileType_MP4:
                case Video_FileType_3GP:
                case Video_FileType_M4V:
                case Video_FileType_AVI:
                case Video_FileType_MKV:
                case Video_FileType_FLV:
                    return FileType.Video;
                case Music_FileType_ICO:
                case Music_FileType_FLAC:
                case Music_FileType_APE:
                case Music_FileType_ALAC:
                case Music_FileType_MP3:
                case Music_FileType_AAC:
                case Music_FileType_CDA:
                    return FileType.Music;
                case Adobe_AeFile_FileType_AEP:
                    return FileType.Adobe_AeFile;
                case Adobe_AiFile_FileType_IDEA:
                case Adobe_AiFile_FileType_AI:
                case Adobe_AiFile_FileType_AIT:
                case Adobe_AiFile_FileType_DRAW:
                case Adobe_AiFile_FileType_LINE:
                case Adobe_AiFile_FileType_EPS:
                case Adobe_AiFile_FileType_EPSF:
                case Adobe_AiFile_FileType_PS:
                    return FileType.Adobe_AiFile;
                case Adobe_PsFile_FileType_PSD:
                case Adobe_PsFile_FileType_PDD:
                case Adobe_PsFile_FileType_PSDT:
                case Adobe_PsFile_FileType_PSB:
                    return FileType.Adobe_PsFile;
                case Adobe_IdFile_FileType_INDD:
                    return FileType.Adobe_IdFile;
                case Adobe_PrFile_FileType_PRPROJ:
                    return FileType.Adobe_PrFile;
                case Adobe_XdFile_FileType_XD:
                    return FileType.Adobe_XdFile;
                case C_CPP_FileType_C:
                case C_CPP_FileType_C__:
                case C_CPP_FileType_CPP:
                case C_CPP_FileType_CC:
                case C_CPP_FileType_CP:
                case C_CPP_FileType_CXX:
                case C_CPP_FileType_H:
                case C_CPP_FileType_HPP:
                case C_CPP_FileType_HXX:
                    return FileType.Code_C_CppFile;
                case CS_FileType_CS:
                case CS_FileType_ASPX:
                case CS_FileType_ASCX:
                    return FileType.Code_CsFile;
                case CSS_FileType_CSS:
                    return FileType.Code_CssFile;
                case Code_FileType_JAVA:
                case Code_FileType_PHP:
                case Code_FileType_JSP:
                case Code_FileType_DFM:
                case Code_FileType_PAS:
                case Code_FileType_FRM:
                case Code_FileType_VB:
                case Code_FileType_VBS:
                case Code_FileType_CLASS:
                case Code_FileType_PY:
                case Code_FileType_SQL:
                case Code_FileType_FS:
                case Code_FileType_JS:
                case Code_FileType_ASM:
                case Code_FileType_OBJ:
                case Code_FileType_S:
                    return FileType.Code_CodeFile;
                case MS_Excel_FileType_XL:
                case MS_Excel_FileType_XLS:
                case MS_Excel_FileType_XLSX:
                case MS_Excel_FileType_XLSM:
                case MS_Excel_FileType_XLSB:
                case MS_Excel_FileType_XLAM:
                case MS_Excel_FileType_XLTX:
                case MS_Excel_FileType_XLTM:
                case MS_Excel_FileType_XLA:
                case MS_Excel_FileType_XLT:
                case MS_Excel_FileType_XLM:
                case MS_Excel_FileType_XLW:
                    return FileType.Microsoft_ExcelFile;
                case MS_Word_FileType_DOC:
                case MS_Word_FileType_DOCX:
                case MS_Word_FileType_DOCM:
                case MS_Word_FileType_DOT:
                case MS_Word_FileType_DOTX:
                case MS_Word_FileType_DOTM:
                    return FileType.Microsoft_WordFile;
                case MS_PPT_FileType_PPT:
                case MS_PPT_FileType_PPTX:
                case MS_PPT_FileType_PPTM:
                case MS_PPT_FileType_PPSX:
                case MS_PPT_FileType_PPS:
                case MS_PPT_FileType_PPSM:
                case MS_PPT_FileType_POTX:
                case MS_PPT_FileType_POT:
                case MS_PPT_FileType_POTM:
                case MS_PPT_FileType_PPAM:
                case MS_PPT_FileType_PPA:
                    return FileType.Microsoft_PptFile;
                case MS_Visio_FileType_VSD:
                case MS_Visio_FileType_VSDX:
                case MS_Visio_FileType_VSDM:
                case MS_Visio_FileType_VDX:
                case MS_Visio_FileType_VSSX:
                case MS_Visio_FileType_VSSM:
                case MS_Visio_FileType_VSX:
                case MS_Visio_FileType_VSTX:
                case MS_Visio_FileType_VSTM:
                case MS_Visio_FileType_VST:
                case MS_Visio_FileType_VTX:
                case MS_Visio_FileType_VSW:
                case MS_Visio_FileType_VDW:
                    return FileType.Microsoft_VisioFile;
                default:
                    return FileType.UnKnown;
            }
        }

        #region 图片格式后缀名
        public const string Picture_FileType_BMP = ".bmp";
        public const string Picture_FileType_JPG = ".jpg";
        public const string Picture_FileType_JPEG = ".jpeg";
        public const string Picture_FileType_PNG = ".png";
        public const string Picture_FileType_TIG = ".tig";
        public const string Picture_FileType_GIF = ".gif";
        public const string Picture_FileType_EXIF = ".exif";
        public const string Picture_FileType_RAW = ".raw";
        public const string Picture_FileType_DNG = ".dng";
        public const string Picture_FileType_CR2 = ".cr2";
        public const string Picture_FileType_ICO = ".ico";
        #endregion

        #region 视频格式后缀名
        public const string Video_FileType_WMV = ".wmv";
        public const string Video_FileType_ASF = ".asf";
        public const string Video_FileType_ASX = ".asx";
        public const string Video_FileType_RM = ".rm";
        public const string Video_FileType_RMVB = ".rmvb";
        public const string Video_FileType_MP4 = ".mp4";
        public const string Video_FileType_3GP = ".3gp";
        public const string Video_FileType_M4V = ".m4v";
        public const string Video_FileType_AVI = ".avi";
        public const string Video_FileType_MKV = ".mkv";
        public const string Video_FileType_FLV = ".flv";
        #endregion

        #region 音乐格式后缀名
        public const string Music_FileType_ICO = ".wav";
        public const string Music_FileType_FLAC = ".flac";
        public const string Music_FileType_APE = ".ape";
        public const string Music_FileType_ALAC = ".alac";
        public const string Music_FileType_MP3 = ".mp3";
        public const string Music_FileType_AAC = ".aac";
        public const string Music_FileType_CDA = ".cda";
        #endregion

        #region Adobe Ae文件格式组
        public const string Adobe_AeFile_FileType_AEP = ".aep";
        #endregion

        #region Adobe Ai文件格式组
        public const string Adobe_AiFile_FileType_IDEA = ".idea";
        public const string Adobe_AiFile_FileType_AI = ".ai";
        public const string Adobe_AiFile_FileType_AIT = ".ait";
        public const string Adobe_AiFile_FileType_DRAW = ".draw";
        public const string Adobe_AiFile_FileType_LINE = ".line";
        public const string Adobe_AiFile_FileType_EPS = ".eps";
        public const string Adobe_AiFile_FileType_EPSF = ".epsf";
        public const string Adobe_AiFile_FileType_PS = ".ps";
        #endregion

        #region Adobe Ps文件格式组
        public const string Adobe_PsFile_FileType_PSD = ".psd";
        public const string Adobe_PsFile_FileType_PDD = ".pdd";
        public const string Adobe_PsFile_FileType_PSDT = ".psdt";
        public const string Adobe_PsFile_FileType_PSB = ".psb";
        #endregion

        #region Adobe Id文件格式组
        public const string Adobe_IdFile_FileType_INDD = ".indd";
        #endregion

        #region Adobe Pr文件格式组
        public const string Adobe_PrFile_FileType_PRPROJ = ".prproj";
        #endregion

        #region Adobe Xd文件格式组
        public const string Adobe_XdFile_FileType_XD = ".xd";
        #endregion

        #region C/C++代码文件
        public const string C_CPP_FileType_C = ".c";
        public const string C_CPP_FileType_C__ = ".c++";
        public const string C_CPP_FileType_CC = ".cc";
        public const string C_CPP_FileType_CPP = ".cpp";
        public const string C_CPP_FileType_CP = ".cp";
        public const string C_CPP_FileType_CXX = ".cxx";
        public const string C_CPP_FileType_H = ".h";
        public const string C_CPP_FileType_HPP = ".hpp";
        public const string C_CPP_FileType_HXX = ".hxx";
        #endregion

        #region C#代码文件
        public const string CS_FileType_CS = ".cs";
        public const string CS_FileType_ASPX = ".aspx";
        public const string CS_FileType_ASCX = ".ascx";
        #endregion

        #region CSS代码文件
        public const string CSS_FileType_CSS = ".css";
        #endregion

        #region 代码文件
        public const string Code_FileType_JAVA = ".java";
        public const string Code_FileType_PHP = ".php";
        public const string Code_FileType_JSP = ".jsp";
        public const string Code_FileType_DFM = ".dfm";
        public const string Code_FileType_PAS = ".pas";
        public const string Code_FileType_FRM = ".frm";
        public const string Code_FileType_VB = ".vb";
        public const string Code_FileType_VBS = ".vbs";
        public const string Code_FileType_CLASS = ".class";
        public const string Code_FileType_LUA = ".lua";
        public const string Code_FileType_PY = ".py";
        public const string Code_FileType_SQL = ".sql";
        public const string Code_FileType_FS = ".fs";
        public const string Code_FileType_JS = ".js";
        public const string Code_FileType_ASM = ".asm";
        public const string Code_FileType_OBJ = ".obj";
        public const string Code_FileType_S = ".s";
        #endregion

        #region Microsoft Excel文件组
        public const string MS_Excel_FileType_XL = ".xl";
        public const string MS_Excel_FileType_XLS = ".xls";
        public const string MS_Excel_FileType_XLSX = ".xlsx";
        public const string MS_Excel_FileType_XLSM = ".xlsm";
        public const string MS_Excel_FileType_XLSB = ".xlsb";
        public const string MS_Excel_FileType_XLAM = ".xlam";
        public const string MS_Excel_FileType_XLTX = ".xltx";
        public const string MS_Excel_FileType_XLTM = ".xltm";
        public const string MS_Excel_FileType_XLA = ".xla";
        public const string MS_Excel_FileType_XLT = ".xlt";
        public const string MS_Excel_FileType_XLM = ".xlm";
        public const string MS_Excel_FileType_XLW = ".xlw";
        #endregion

        #region Microsoft Word文件组
        public const string MS_Word_FileType_DOC = ".doc";
        public const string MS_Word_FileType_DOCX = ".docx";
        public const string MS_Word_FileType_DOCM = ".docm";
        public const string MS_Word_FileType_DOT = ".dot";
        public const string MS_Word_FileType_DOTX = ".dotx";
        public const string MS_Word_FileType_DOTM = ".dotm";
        #endregion

        #region Microsoft PPT文件组
        public const string MS_PPT_FileType_PPT = ".ppt";
        public const string MS_PPT_FileType_PPTX = ".pptx";
        public const string MS_PPT_FileType_PPTM = ".pptm";
        public const string MS_PPT_FileType_PPSX = ".ppsx";
        public const string MS_PPT_FileType_PPS = ".pps";
        public const string MS_PPT_FileType_PPSM = ".ppsm";
        public const string MS_PPT_FileType_POTX = ".potx";
        public const string MS_PPT_FileType_POT = ".pot";
        public const string MS_PPT_FileType_POTM = ".potm";
        public const string MS_PPT_FileType_PPAM = ".ppam";
        public const string MS_PPT_FileType_PPA = ".ppa";
        #endregion

        #region Microsoft Visio文件组
        public const string MS_Visio_FileType_VSD = ".vsd";
        public const string MS_Visio_FileType_VSDX = ".vsdx";
        public const string MS_Visio_FileType_VSDM = ".vsdm";
        public const string MS_Visio_FileType_VDX = ".vdx";
        public const string MS_Visio_FileType_VSSX = ".vssx";
        public const string MS_Visio_FileType_VSSM = ".vssm";
        public const string MS_Visio_FileType_VSS = ".vss";
        public const string MS_Visio_FileType_VSX = ".vsx";
        public const string MS_Visio_FileType_VSTX = ".vstx";
        public const string MS_Visio_FileType_VSTM = ".vstm";
        public const string MS_Visio_FileType_VST = ".vst";
        public const string MS_Visio_FileType_VTX = ".vtx";
        public const string MS_Visio_FileType_VSW = ".vsw";
        public const string MS_Visio_FileType_VDW = ".vdw";
        #endregion
    }
}
