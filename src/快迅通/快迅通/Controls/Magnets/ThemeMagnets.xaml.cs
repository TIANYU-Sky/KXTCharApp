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
    public sealed partial class ThemeMagnets : UserControl
    {
        public ThemeMagnets()
        {
            this.InitializeComponent();
        }

        public Brush ThemeBrush
        {
            set
            {
                if (null != value)
                    ThemeColor.Background = value;
            }
        }
        public string ThemeName
        {
            set
            {
                if (null == value)
                    ThemeDescribeText.Text = "";
                else
                    ThemeDescribeText.Text = value;
            }
        }
        public bool ThemeSelected
        {
            set
            {
                Selected.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
            get
            {
                return Visibility.Visible == Selected.Visibility;
            }
        }
    }
}
