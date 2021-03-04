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
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace 快迅通.Controls.Magnets
{
    public sealed partial class UserAccountMagnets : UserControl
    {
        public UserAccountMagnets()
        {
            this.InitializeComponent();
            UserAccountMagnetGrid.Background = Utils.Utils.GetViewPre();

        }

        public Brush UserPicture
        {
            set
            {
                if (null != value)
                    UserPictureBox.Background = value;
            }
        }
        public string UserName
        {
            set
            {
                if (null == value)
                    UserNameText.Text = "";
                else
                    UserNameText.Text = value;
            }
        }
        public string UserID
        {
            set
            {
                if (null == value)
                    UserIDText.Text = "";
                else
                    UserIDText.Text = value;
            }
        }
        public bool CurrentUser
        {
            set
            {
                Selected.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
