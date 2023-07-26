using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WomoMemo.Models;

namespace WomoMemo.Views
{
    /// <summary>
    /// MemoWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MemoWindow : Window
    {
        DateTime _lastTextChanged = DateTime.UtcNow;

        Memo Memo;

        public MemoWindow(Memo memo)
        {
            Memo = memo;
            InitializeComponent();
            Title = txtTitle.Text = memo.Title;
            txtContent.Text = memo.Content;
            Background = grdHeader.Background = new BrushConverter().ConvertFrom(Memo.Color) as SolidColorBrush;

        }
        private void Window_Activated(object sender, EventArgs e)
        {
            Trace.WriteLine("Activated");
            foreach(Control control in grdHeader.Children)
                control.Visibility = Visibility.Visible;
        }
        private void Window_Deactivated(object sender, EventArgs e)
        {
            Trace.WriteLine("Deactivated");
            foreach (Control control in grdHeader.Children)
                control.Visibility = Visibility.Collapsed;
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
            // Sync title
            if (((Control)sender).Name == "txtTitle") Title = txtTitle.Text;

            // Throttle
            if (DateTime.UtcNow.Subtract(_lastTextChanged).TotalSeconds >= 1)
            {
                
            }
            _lastTextChanged = DateTime.UtcNow;
        }
    }
}
