using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
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
        bool _loaded = false;

        public Memo Memo;
        public Timer? ResizeTimer, PutMemoTimer;

        public MemoWindow(Memo memo)
        {
            InitializeComponent();
            App.MemoWins.Add(memo.Id, this);
            Memo = memo;

            // Update title, content, bgcolor, ...
            UpdateMemo(memo);

            // Add color changer panel
            int i = 0;
            var colorKeyList = Memo.COLORS.Keys.ToArray();
            foreach (Button btnCol in pnlColor.Children)
            {
                string colorKey = colorKeyList[i++];
                btnCol.Tag = colorKey;
                btnCol.Background = new BrushConverter().ConvertFrom(Memo.COLORS[colorKey]) as SolidColorBrush;
                btnCol.BorderThickness = new Thickness();
                btnCol.Margin = new Thickness(2);
                btnCol.Click += memoChanged;
            }
        }
        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            _loaded = true;
        }
        private void window_Activated(object sender, EventArgs e)
        {
            foreach (Control control in grdHeader.Children)
                control.Visibility = Visibility.Visible;
            txtContent.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
        }
        private void window_Deactivated(object sender, EventArgs e)
        {
            foreach (Control control in grdHeader.Children)
                control.Visibility = Visibility.Collapsed;
            txtContent.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }
        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_loaded) return;

            // Save config
            if (ResizeTimer == null)
                ResizeTimer = new Timer(_ => Dispatcher.Invoke(() => Config.Save()), null, 400, Timeout.Infinite);
            // Throttle
            else
                ResizeTimer.Change(400, Timeout.Infinite);
        }
        private void window_Closing(object sender, CancelEventArgs e)
        {
            App.MemoWins.Remove(Memo.Id);
        }

        public void UpdateMemo(Memo memo)
        {
            Memo = memo;
            Title = txtTitle.Text = memo.Title;
            txtContent.Text = memo.Content;
            Background = grdHeader.Background = new BrushConverter().ConvertFrom(memo.BgColor) as SolidColorBrush;
        }

        // Header
        private void grdHeader_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
                Config.Save();
            }
        }
        private void btnList_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWin == null) App.MainWin = new MainWindow();
            App.MainWin.Show();
            App.MainWin.Focus();
        }
        private async void btnNew_Click(object sender, RoutedEventArgs e)
        {
            await App.CreateNewMemo();
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

                Config.Save();
                Close();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                App.MainWin?.ShowAlert("Error on deleting memo");
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            App.MemoWins.Remove(Memo.Id);
            Config.Save();
            Close();
        }

        // Body
        private void memoChanged(object sender, EventArgs e)
        {
            // Update my memo instance
            Control sndCon = (Control)sender;
            if (sndCon.Name == "txtTitle") Title = Memo.Title = txtTitle.Text;
            else if (sndCon.Name == "txtContent") Memo.Content = txtContent.Text;
            else if (((WrapPanel)sndCon.Parent).Name == "pnlColor")
            {
                Memo.Color = (string)sndCon.Tag;
                Background = grdHeader.Background = sndCon.Background;
            }

            // Update App memo
            Memo? appMemo = App.Memos.Where(memo => memo.Id == Memo.Id).FirstOrDefault();
            if (appMemo != null)
            {
                int idx = App.Memos.IndexOf(appMemo);
                App.Memos.Insert(idx, Memo);
                App.Memos.RemoveAt(idx + 1);
            }

            // Put memo to server
            if (PutMemoTimer == null)
                PutMemoTimer = new Timer(async _ =>
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
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(ex.ToString());
                        App.MainWin?.ShowAlert("Error on putting memo");
                    }

                    PutMemoTimer?.Dispose();
                    PutMemoTimer = null;
                }, null, 1000, Timeout.Infinite);
            // Throttle
            else
                PutMemoTimer.Change(1000, Timeout.Infinite);
        }
    }
}
