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
    public sealed partial class FriendMessageMagnets : UserControl
    {
        public FriendMessageMagnets()
        {
            this.InitializeComponent();
            MessageGrid.Background = Utils.Utils.GetViewIllustrator();
            MessageBox.Foreground = Utils.Utils.GetViewIllustratorForce();
        }

        public string Text
        {
            set
            {
                MessageBox.Text = value ?? "";
            }
        }
        public string Time
        {
            set
            {
                MessageTime.Text = value ?? "";
            }
        }
        public ImageSource Photo
        {
            set
            {
                HeadPhoto.ImageSource = value ?? Utils.Utils.GetPhoto(0);
            }
        }
    }
}
