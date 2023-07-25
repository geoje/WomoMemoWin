using System.Windows;
using System.Windows.Input;
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
            txtTitle.Text = memo.Title;
            txtContent.Text = memo.Content;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void btnList_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
