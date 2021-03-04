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
using 快迅通.Utils;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 快迅通.Views.SettingSubViews
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ThemePage : Page
    {
        public ThemePage()
        {
            this.InitializeComponent();
            TurnBackBar.Tapped = TurnBackButton_Tapped;

            AcrylicType.IsOn = OperatingData.AcrylicModel;
            OpacitySlider.Value = OperatingData.AcrylicOpacity * 100;
            OpacityValue.Text = ((int)OpacitySlider.Value).ToString() + "%";
            UpdateUI();
        }
        private void UpdateUI()
        {
            switch (OperatingData._Theme)
            {
                case _Interface_Theme._White_:
                    DefaultTheme.ThemeSelected = false;
                    GreenTheme.ThemeSelected = false;
                    PinkTheme.ThemeSelected = false;
                    GrayTheme.ThemeSelected = true;
                    break;
                case _Interface_Theme._Red_:
                    DefaultTheme.ThemeSelected = false;
                    GreenTheme.ThemeSelected = false;
                    PinkTheme.ThemeSelected = true;
                    GrayTheme.ThemeSelected = false;
                    break;
                case _Interface_Theme._Green_:
                    DefaultTheme.ThemeSelected = false;
                    GreenTheme.ThemeSelected = true;
                    PinkTheme.ThemeSelected = false;
                    GrayTheme.ThemeSelected = false;
                    break;
                case _Interface_Theme._Default_:
                default:
                    DefaultTheme.ThemeSelected = true;
                    GreenTheme.ThemeSelected = false;
                    PinkTheme.ThemeSelected = false;
                    GrayTheme.ThemeSelected = false;
                    break;
            }
        }

        private void DefaultTheme_Tapped(object sender, TappedRoutedEventArgs e)
        {
            OperatingData._Theme = _Interface_Theme._Default_;
            UpdateUI();
        }

        private void GreenTheme_Tapped(object sender, TappedRoutedEventArgs e)
        {
            OperatingData._Theme = _Interface_Theme._Green_;
            UpdateUI();
        }

        private void PinkTheme_Tapped(object sender, TappedRoutedEventArgs e)
        {
            OperatingData._Theme = _Interface_Theme._Red_;
            UpdateUI();
        }

        private void GrayTheme_Tapped(object sender, TappedRoutedEventArgs e)
        {
            OperatingData._Theme = _Interface_Theme._White_;
            UpdateUI();
        }

        private void OpacitySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            OperatingData.AcrylicOpacity = OpacitySlider.Value / 100;
            OpacityValue.Text = ((int)OpacitySlider.Value).ToString() + "%";
        }

        private void TurnBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            LocalDB.SetTheme((int)OperatingData._Theme, OperatingData.AcrylicModel, OperatingData.AcrylicOpacity);

            RunningDatas.UIStack.Pop();
            RunningDatas.MainPage.UpdateUI();
        }

        private void AcrylicType_Checked(object sender, RoutedEventArgs e)
        {
            OperatingData.AcrylicModel = true;
        }
        private void AcrylicType_Unchecked(object sender, RoutedEventArgs e)
        {
            OperatingData.AcrylicModel = false;
        }

        private void AcrylicType_Toggled(object sender, RoutedEventArgs e)
        {
            OperatingData.AcrylicModel = AcrylicType.IsOn;
        }
    }
}
