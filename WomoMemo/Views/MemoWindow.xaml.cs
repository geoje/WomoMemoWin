using System.Windows;
using WomoMemo.Models;

namespace WomoMemo.Views
{
    /// <summary>
    /// MemoWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MemoWindow : Window
    {
        Memo Memo;

        public MemoWindow(Memo memo)
        {
            InitializeComponent();
            Memo = memo;
        }
    }
}
