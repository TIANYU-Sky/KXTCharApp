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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace 快迅通.Controls
{
    public sealed partial class NotifyTip : UserControl
    {
        public string Message
        {
            set
            {
                ContextTextBlock.Text = value ?? "";
            }
            get
            {
                return ContextTextBlock.Text;
            }
        }
        public Symbol Icon
        {
            set
            {
                ContextSymbolIcon.Symbol = value;
            }
            get
            {
                return ContextSymbolIcon.Symbol;
            }
        }

        private readonly System.Timers.Timer KeepTimer;
        private readonly Action<UIElement> EndNotify;

        public NotifyTip(Action<UIElement> end_notify)
        {
            EndNotify = end_notify;
            this.InitializeComponent();

            this.Background = Utils.Utils.GetViewIllustrator();
            ContextTextBlock.Foreground = Utils.Utils.GetViewIllustratorForce();
            ContextSymbolIcon.Foreground = Utils.Utils.GetViewIllustratorForce();

            KeepTimer = new System.Timers.Timer
            {
                Interval = 4000,
                AutoReset = false
            };
            KeepTimer.Elapsed += Timer_Trigger;
            KeepTimer.Start();

            var s = (Storyboard)Resources["StartAnimation"];
            s.Begin();
        }

        private void Timer_Trigger(object sender, System.Timers.ElapsedEventArgs args)
        {
            EndNotify(this);
        }
    }
}
