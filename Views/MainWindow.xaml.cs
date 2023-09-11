﻿using System.Windows;
using System.Windows.Input;
using WomoMemo.Views;
using System.Windows.Media.Imaging;
using System;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Auth.UI;
using WomoMemo.Models;
using System.Collections.Generic;
using System.Windows.Controls;

namespace WomoMemo
{
    public partial class MainWindow : Window
    {
        // Window
        public MainWindow()
        {
            InitializeComponent();

            UpdateUser(FirebaseUI.Instance.Client.User);
            lstMemo.ItemsSource = App.Memos.Values;
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            App.MainWin = null;
        }

        // Func
        public void UpdateUser(User? user)
        {
            Dispatcher.Invoke(() =>
            {
                btnUser.Visibility = user == null ? Visibility.Collapsed : Visibility.Visible;
                btnLogin.Visibility = user == null ? Visibility.Visible : Visibility.Collapsed;

                txtName.Text = user == null ? "" : user.Info.DisplayName;
                txtEmail.Text = user == null ? "" : user.Info.Email;

                imgUser.ImageSource = user == null ? null :
                string.IsNullOrWhiteSpace(user.Info.PhotoUrl) ? null :
                new BitmapImage(new Uri(user.Info.PhotoUrl));
            });
        }
        public void UpdateMemos(IEnumerable<Memo> memos)
        {
            Dispatcher.Invoke(() => lstMemo.ItemsSource = memos);
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
            FirebaseUI.Instance.Client.SignOut();
            App.Memos.Clear();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Body
        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            string key = (string)((Border)sender).Tag;

            if (!App.MemoWins.ContainsKey(key))
            {
                App.MemoWins.Add(key, new MemoWindow(key, App.Memos[key]));
                Config.Save();
            }
            App.MemoWins[key].Show();
            App.MemoWins[key].Focus();
        }
    }
}
