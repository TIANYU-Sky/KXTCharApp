using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using 快迅通_通讯服务器.Utils;

namespace 快迅通_通讯服务器
{
    /// <summary>
    /// NotifyItem.xaml 的交互逻辑
    /// </summary>
    public partial class NotifyItem : UserControl
    {
        public NotifyItem(NotifyListData data)
        {
            InitializeComponent();

            ItemIcon.Source = data.Icon;
            ItemMessage.Text = data.Message;
        }
    }
}
