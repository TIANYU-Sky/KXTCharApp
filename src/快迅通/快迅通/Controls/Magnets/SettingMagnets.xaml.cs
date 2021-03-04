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
    public sealed partial class SettingMagnets : UserControl
    {
        public SettingMagnets()
        {
            this.InitializeComponent();
            this.MagnetsGrid.Background = Utils.Utils.GetViewPre();
        }

        public Brush ICON
        {
            set
            {
                if (null != value)
                    ICONGrid.Background = value;
            }
        }
        public string Describe
        {
            set
            {
                if (null == value)
                    DescribeText.Text = "";
                else
                    DescribeText.Text = value;
            }
        }
    }
}
