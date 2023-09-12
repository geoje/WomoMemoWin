using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

        // Window
        public MemoWindow(Memo memo)
        {
            InitializeComponent();
            Memo = memo;

            // Update controls
            UpdateMemo(Memo);

            // Add color changer panel
            int i = 0;
            var colorKeyList = ColorMap.BACKGROUND_COLORS.Keys.ToArray();
            foreach (Button btnCol in pnlColor.Children)
            {
                string colorKey = colorKeyList[i++];
                btnCol.Tag = colorKey;
                btnCol.Background = new BrushConverter().ConvertFrom(ColorMap.Background(colorKey)) as SolidColorBrush;
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
            App.MemoWins.Remove(Memo.Key);
        }

        // Func
        public void UpdateMemo(Memo memo)
        {
            Dispatcher.Invoke(() =>
            {
                Memo = memo;
                Title = txtTitle.Text = memo.Title;
                txtContent.Text = memo.Content;
                Background = grdHeader.Background =
                    new BrushConverter()
                    .ConvertFrom(ColorMap.Background(memo.Color))
                    as SolidColorBrush;
                btnCheckbox.Visibility = Memo.Checked == null ? Visibility.Visible : Visibility.Collapsed;
                btnUncheckbox.Visibility = Memo.Checked == null ? Visibility.Collapsed : Visibility.Visible;
                btnArchive.Visibility = Memo.Archive ? Visibility.Collapsed : Visibility.Visible;
                btnUnarchive.Visibility = Memo.Archive ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        // Header
        private void grdHeader_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }
        private void btnList_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWin == null) App.MainWin = new MainWindow();
            App.MainWin.Show();
            App.MainWin.Focus();
        }
        private async void btnNew_Click(object sender, RoutedEventArgs e)
        {
            await App.CreateMemo();
        }
        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            await App.DeleteMemo(Memo.Key);
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            App.MemoWins.Remove(Memo.Key);
            Config.Save();
            Close();
        }

        // Body
        private void memoChanged(object sender, EventArgs e)
        {
            if (!_loaded) return;
            Trace.WriteLine(((Control)sender).Name, "[memoChanged]");

            // Update my memo instance
            Control sndCon = (Control)sender;
            if (sndCon.Name == "txtTitle") Title = Memo.Title = txtTitle.Text;
            else if (sndCon.Name == "txtContent") Memo.Content = txtContent.Text;
            else if (((WrapPanel)sndCon.Parent).Name == "pnlColor")
            {
                Memo.Color = (string)sndCon.Tag;
                Background = grdHeader.Background = sndCon.Background;
            }
            else if (sndCon.Name == "btnCheckbox")
            {
                Memo.Checked = new HashSet<int>();
                btnCheckbox.Visibility = Visibility.Collapsed;
                btnUncheckbox.Visibility = Visibility.Visible;
            }
            else if (sndCon.Name == "btnUncheckbox")
            {
                Memo.Checked = null;
                btnCheckbox.Visibility = Visibility.Visible;
                btnUncheckbox.Visibility = Visibility.Collapsed;
            }
            else if (sndCon.Name == "btnArchive")
            {
                Memo.Archive = true;
                btnArchive.Visibility = Visibility.Collapsed;
                btnUnarchive.Visibility = Visibility.Visible;
            }
            else if (sndCon.Name == "btnUnarchive")
            {
                Memo.Archive = false;
                btnArchive.Visibility = Visibility.Visible;
                btnUnarchive.Visibility = Visibility.Collapsed;
            }

            // Put memo to server
            if (PutMemoTimer == null)
                PutMemoTimer = new Timer(async _ =>
                {
                    if (!App.Memos.Any(memo => memo.Key == Memo.Key)) return;

                    await App.UpdateMemo(Memo);

                    PutMemoTimer?.Dispose();
                    PutMemoTimer = null;
                }, null, 1000, Timeout.Infinite);
            // Throttle
            else
                PutMemoTimer.Change(1000, Timeout.Infinite);
        }
    }
}
