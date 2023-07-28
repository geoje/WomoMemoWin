using System.Windows;
using System.Windows.Input;
using WomoMemo.Models;
using WomoMemo.Views;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using MaterialDesignThemes.Wpf;

namespace WomoMemo
{
    public partial class MainWindow : Window
    {
        int[] latestVer = { 1, 0, 0 };
        // Window
        public MainWindow()
        {
            App.MainWin = this;
            InitializeComponent();
            lstMemo.ItemsSource = App.Memos;
            UpdateControls();

            Task.Run(CheckUpdateAndDownload);
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            App.MainWin = null;
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
        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            await CheckUpdateAndDownload();
        }
        private async void btnUser_Click(object sender, RoutedEventArgs e)
        {
            if (User.Id == "") await App.GetUserProfile();
            if (User.Image == null) await App.DownloadUserProfileImage();
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
                    if (App.MemoWins.ContainsKey(id)) App.MemoWins[id].Show();
                    else
                    {
                        new MemoWindow(App.Memos[i]).Show();
                        Config.Save();
                    }
                    App.MemoWins[id].Focus();
                }
        }

        // Private
        private async Task CheckUpdateAndDownload()
        {
            int[] currentVer = App.GetCurrentVersion();
            latestVer = await App.GetLatestVersion();
            for (int i = 0; i < 3; i++)
                if (latestVer[i] > currentVer[i])
                {
                    Dispatcher.Invoke(() =>
                    {
                        btnUpdate.Visibility = Visibility.Visible;
                        txtUpdate.Text = "There is a new version is available.\nDo you want to update to the latest version?\n\n" +
                        $"Latest version: {string.Join(".", latestVer)}\nCurrent version: {string.Join('.', currentVer)}";
                        btnUpdateTrue.Content = "Yes";
                        btnUpdateTrue.Visibility = Visibility.Visible;
                        btnUpdateFalse.Content = "No";
                        btnUpdateFalse.Visibility = Visibility.Visible;
                        dlgUpdate.IsOpen = true;
                    });

                    break;
                }
        }

        // Public
        public void UpdateControls()
        {
            Dispatcher.Invoke(() =>
            {
                btnUser.Visibility = string.IsNullOrEmpty(Config.SessionTokenValue) ? Visibility.Collapsed : Visibility.Visible;
                btnLogin.Visibility = string.IsNullOrEmpty(Config.SessionTokenValue) ? Visibility.Visible : Visibility.Collapsed;
                imgUser.ImageSource = User.Image;

                imgProvider.Source = new BitmapImage(new Uri($"/Resources/{User.Provider}.png", UriKind.RelativeOrAbsolute));
                txtName.Text = User.Name;
                txtEmail.Text = User.Email;
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

        private async void dlgUpdate_DialogClosed(object sender, DialogClosedEventArgs eventArgs)
        {
            if (((string)(eventArgs.Parameter ?? string.Empty)) != "True") return;

            if (txtUpdate.Text.StartsWith("There is a new version is available"))
            {
                try
                {
                    string setupFilename = Path.Combine(Path.GetTempPath(), "Setup.msi");
                    using (var client = new HttpClient())
                    using (var webFileStream = await client.GetStreamAsync(Config.GITHUB_URL + $"/releases/download/{string.Join(".", latestVer)}/Setup.msi"))
                    using (var localFileStream = new FileStream(setupFilename, FileMode.Create))
                        await webFileStream.CopyToAsync(localFileStream);
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = setupFilename,
                        UseShellExecute = true
                    });
                    Application.Current.Shutdown();
                }
                catch
                {
                    txtUpdate.Text = $"Download failed.\n\nPlease enter the below URL on your browser or click OK button to open the URL and download the program directly.\nYou can download and install the Setup.msi file inside the Assets.\n\n{Config.GITHUB_URL}/releases/latest";
                    btnUpdateTrue.Content = "OK";
                    btnUpdateTrue.Visibility = Visibility.Visible;
                    btnUpdateFalse.Visibility = Visibility.Collapsed;
                    dlgUpdate.IsOpen = true;
                }
            }
            else if (txtUpdate.Text.StartsWith("Download failed"))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Config.GITHUB_URL + "/releases/latest",
                    UseShellExecute = true
                });
            }
        }
    }
}
