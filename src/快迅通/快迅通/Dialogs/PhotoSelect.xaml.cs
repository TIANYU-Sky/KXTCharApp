using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using 快迅通.Utils;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace 快迅通.Dialogs
{
    public sealed partial class PhotoSelect : ContentDialog
    {
        private int SelectedIndex;

        public PhotoSelect()
        {
            this.InitializeComponent();
            SelectedIndex = RunningDatas.UserPhoto;
            SelectedPhotoBox.Source = Utils.Utils.GetPhoto(SelectedIndex);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            RunningDatas.UserPhoto = PhotoGridView.SelectedIndex;

            RunningDatas.DataSender.UpdatePicture(new KXTNetStruct.UpdatePicture
            {
                UserID = RunningDatas.UserID,
                UserPicture = (byte)RunningDatas.UserPhoto
            });

            RunningDatas.MainPage.UpdatePhoto();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
        }

        private void PhotoGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedIndex = PhotoGridView.SelectedIndex;
            SelectedPhotoBox.Source = Utils.Utils.GetPhoto(SelectedIndex);
        }
    }
}
