using System.Windows;
using System.Windows.Input;
using WomoMemo.Views;
using System.Windows.Media.Imaging;
using System;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Auth.UI;

namespace WomoMemo
{
    public partial class MainWindow : Window
    {
        // Window
        public MainWindow()
        {
            InitializeComponent();

            FirebaseUI.Instance.Client.AuthStateChanged += AuthStateChanged;
            lstMemo.ItemsSource = App.Memos;
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            App.MainWin = null;
        }
        private void AuthStateChanged(object? sender, UserEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                btnUser.Visibility = e.User == null ? Visibility.Collapsed : Visibility.Visible;
                btnLogin.Visibility = e.User == null ? Visibility.Visible : Visibility.Collapsed;

                txtName.Text = e.User == null ? "" : e.User.Info.DisplayName;
                txtEmail.Text = e.User == null ? "" : e.User.Info.Email;

                imgUser.ImageSource = e.User == null ? null :
                string.IsNullOrWhiteSpace(e.User.Info.PhotoUrl) ? null :
                new BitmapImage(new Uri(e.User.Info.PhotoUrl));
            });
        }
        public void ShowAlert(string message)
        {
            Dispatcher.Invoke(() =>
            {
                if (snkAlert.MessageQueue is { } messageQueue)
                    Task.Factory.StartNew(() => messageQueue.Enqueue(message));
            });
        }

        // Header
        private void GridAppBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }
        private async void btnNew_Click(object sender, RoutedEventArgs e)
        {
            await App.CreateNewMemo();
        }
        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            btnUser.ContextMenu.PlacementTarget = btnUser;
            btnUser.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            btnUser.ContextMenu.IsOpen = true;
            e.Handled = true;
        }
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
        }
        private void mnuLogout_Click(object sender, RoutedEventArgs e)
        {
            App.Memos.Clear();
            FirebaseUI.Instance.Client.SignOut();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Body
        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //int id = (int)((Border)sender).Tag;
            //for (int i = 0; i < App.Memos.Count; i++)
            //    if (App.Memos[i].Id == id)
            //    {
            //        if (App.MemoWins.ContainsKey(id)) App.MemoWins[id].Show();
            //        else
            //        {
            //            new MemoWindow(App.Memos[i]).Show();
            //            Config.Save();
            //        }
            //        App.MemoWins[id].Focus();
            //    }
        }
    }
}
