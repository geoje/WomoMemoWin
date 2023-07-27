using System.Windows;
using System.Windows.Input;
using WomoMemo.Models;
using WomoMemo.Views;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System;
using System.Diagnostics;

namespace WomoMemo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            App.MainWin = this;
            InitializeComponent();
            lstMemo.ItemsSource = App.Memos;
            UpdateControls();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            App.MainWin = null;
        }
        public void UpdateControls()
        {
            Dispatcher.Invoke(() =>
            {
                lblAlert.Content = App.ErrorMessage;

                btnUser.Visibility = string.IsNullOrEmpty(Config.SessionTokenValue) ? Visibility.Collapsed : Visibility.Visible;
                btnLogin.Visibility = string.IsNullOrEmpty(Config.SessionTokenValue) ? Visibility.Visible : Visibility.Collapsed;
                imgUser.ImageSource = User.Image;

                imgProvider.Source = new BitmapImage(new Uri($"/Resources/{User.Provider}.png", UriKind.RelativeOrAbsolute));
                txtName.Text = User.Name;
                txtEmail.Text = User.Email;
            });
        }

        // Header
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
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
            Config.SessionTokenValue = "";
            Config.Save();
            User.Clear();
            App.Memos.Clear();
            imgProvider.Source = null;
            imgUser.ImageSource = null;
            txtName.Text = "";
            txtEmail.Text = "";
            btnUser.Visibility = Visibility.Collapsed;
            btnLogin.Visibility = Visibility.Visible;
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Body
        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int id = (int)((Border)sender).Tag;
            for (int i = 0; i < App.Memos.Count; i++)
                if (App.Memos[i].Id == id)
                {
                    if (App.MemoWins.ContainsKey(id)) { App.MemoWins[id].Show(); Trace.WriteLine(App.MemoWins[id].ToString()); }
                    else
                    {
                        App.MemoWins.Add(id, new MemoWindow(App.Memos[i]));
                        App.MemoWins[id].Show();
                    }
                    App.MemoWins[id].Focus();
                }
        }
    }
}
