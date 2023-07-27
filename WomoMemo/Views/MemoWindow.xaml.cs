using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WomoMemo.Models;

namespace WomoMemo.Views
{
    public partial class MemoWindow : Window
    {
        public Timer? PostMemoTimer;

        public Memo Memo;

        public MemoWindow(Memo memo)
        {
            InitializeComponent();
            Memo = memo;
            UpdateMemo(memo);
        }
        private void Window_Activated(object sender, EventArgs e)
        {
            foreach(Control control in grdHeader.Children)
                control.Visibility = Visibility.Visible;
        }
        private void Window_Deactivated(object sender, EventArgs e)
        {
            foreach (Control control in grdHeader.Children)
                control.Visibility = Visibility.Collapsed;
        }

        public void UpdateMemo(Memo memo)
        {
            Memo = memo;
            Title = txtTitle.Text = memo.Title;
            txtContent.Text = memo.Content;
            Background = grdHeader.Background = new BrushConverter().ConvertFrom(memo.BgColor) as SolidColorBrush;
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
        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Memo? appMemo = App.Memos.Where(memo => memo.Id == Memo.Id).FirstOrDefault();
                if (appMemo != null)
                {
                    int idx = App.Memos.IndexOf(appMemo);
                    App.Memos.RemoveAt(idx);
                    App.MemoWins.Remove(Memo.Id);
                }

                await App.Client.DeleteAsync("/api/memos/" + Memo.Id);

                App.UpdateErrorMessage("Error on deleting memo", true);
                Close();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                App.UpdateErrorMessage("Error on deleting memo");
            }
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
            // Sync title and Content
            if (((Control)sender).Name == "txtTitle") Title = Memo.Title = txtTitle.Text;
            else Memo.Content = txtContent.Text;
            Memo? appMemo = App.Memos.Where(memo => memo.Id == Memo.Id).FirstOrDefault();
            if (appMemo != null)
            {
                int idx = App.Memos.IndexOf(appMemo);
                App.Memos.Insert(idx, Memo);
                App.Memos.RemoveAt(idx + 1);
            }

            // Post memo to server
            if (PostMemoTimer == null)
                PostMemoTimer = new Timer(async _ =>
                {
                    try
                    {
                        JObject jObj = new JObject
                        {
                            { "title", Memo.Title },
                            { "content", Memo.Content },
                            { "color", Memo.Color },
                            { "checkBox", Memo.Checkbox },
                        };
                        StringContent content = new StringContent(jObj.ToString(Newtonsoft.Json.Formatting.None));

                        (await App.Client.PutAsync("/api/memos/" + Memo.Id, content)).EnsureSuccessStatusCode();
                        App.UpdateErrorMessage("Error on posting memo", true);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(ex.ToString());
                        App.UpdateErrorMessage("Error on posting memo");
                    }

                    PostMemoTimer?.Dispose();
                    PostMemoTimer = null;
                }, null, 1000, Timeout.Infinite);
            // Throttle
            else
                PostMemoTimer.Change(1000, Timeout.Infinite);
        }
    }
}
