using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace 快迅通.Dialogs
{
    public class FolderNamePakcage
    {
        public bool OKState { get; set; }
        public string Name { get; set; }
    }

    public sealed partial class CloudNewFolder : ContentDialog
    {
        private readonly Func<string, bool> NameCheck;
        private readonly FolderNamePakcage NamePackage;

        public CloudNewFolder(FolderNamePakcage pakcage, Func<string, bool> check_func)
        {
            NamePackage = pakcage;
            NameCheck = check_func;

            this.InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (0 == FolderNameBox.Text.Length)
            {
                MessageFlyoutText.Text = "文件夹名字不能为空";
                MessageFlyout.ShowAt(FolderNameBox);
                return;
            }

            if (!NameStringCheck(FolderNameBox.Text))
            {
                MessageFlyoutText.Text = "文件夹名称无效";
                MessageFlyout.ShowAt(FolderNameBox);
                return;
            }

            if (!NameCheck(FolderNameBox.Text))
            {
                MessageFlyoutText.Text = "文件夹已存在";
                MessageFlyout.ShowAt(FolderNameBox);
                return;
            }

            NamePackage.Name = FolderNameBox.Text;
            NamePackage.OKState = true;

            Hide();
        }
        private bool NameStringCheck(string name)
        {
            foreach (char i in name)
            {
                switch (i)
                {
                    case '\\':
                    case '/':
                    case ':':
                    case '*':
                    case '?':
                    case '\"':
                    case '<':
                    case '>':
                    case '|':
                        return false;
                }
            }
            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NamePackage.Name = FolderNameBox.Text;
            NamePackage.OKState = false;

            Hide();
        }
    }
}
