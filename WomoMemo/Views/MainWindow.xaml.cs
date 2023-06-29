using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using WomoMemo.Models;

namespace WomoMemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var memos = GetMemos();
            if (memos.Count > 0) lstMemo.ItemsSource = memos;
        }

        private List<Memo> GetMemos()
        {
            return new List<Memo>()
            {
                new Memo(0, false, "Hello Jake,\nThis is sample memo."),
                new Memo(0, true, "ToDo\nList\nWe\nHave\nCheck\nBox"),
                new Memo(0, false, "I'd like to go to the home.\nPlease make me free!")
            };
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
