using System.Windows;
using System.Windows.Controls;
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
            Memo = memo;
            InitializeComponent();
            txtTitle.Text = memo.Title;
            txtContent.Text = memo.Content;
        }

        // Header
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }
        private void btnList_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWin == null) App.MainWin = new MainWindow();
            App.MainWin.Show();
            App.MainWin.Focus();
        }
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnColor_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Body
        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
