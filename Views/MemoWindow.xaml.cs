using Newtonsoft.Json;
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
            var colorKeyList = ColorMap.COLORS.Keys.ToArray();
            foreach (Button btnCol in pnlColor.Children)
            {
                string colorKey = colorKeyList[i++];
                btnCol.Tag = colorKey;
                btnCol.Background = new BrushConverter().ConvertFrom(ColorMap.Background(colorKey)) as SolidColorBrush;
                btnCol.BorderThickness = new Thickness();
                btnCol.Margin = new Thickness(2);
                btnCol.Click += handleMemoChanged;
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

            grdHeader.Background =
                    new BrushConverter()
                    .ConvertFrom(ColorMap.Border(Memo.Color))
                    as SolidColorBrush;
        }
        private void window_Deactivated(object sender, EventArgs e)
        {
            foreach (Control control in grdHeader.Children)
                control.Visibility = Visibility.Collapsed;
            txtContent.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

            grdHeader.Background =
                    new BrushConverter()
                    .ConvertFrom(ColorMap.Background(Memo.Color))
                    as SolidColorBrush;
        }
        private void window_SizeOrPosChanged(object sender, EventArgs e)
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
            // Dismiss response to update when editing
            if (PutMemoTimer != null) return;

            Dispatcher.Invoke(() =>
            {
                Memo = memo;
                Title = txtTitle.Text = memo.Title;
                txtContent.Text = memo.Content;
                Background = grdHeader.Background =
                    new BrushConverter()
                    .ConvertFrom(ColorMap.Background(memo.Color))
                    as SolidColorBrush;
                UpdateAppBar();
            });
        }
        private void UpdateAppBar()
        {
            // btnNewOrRestore
            icoNewOrRestore.Kind = Memo.Delete == null ?
                MaterialDesignThemes.Wpf.PackIconKind.Plus :
                MaterialDesignThemes.Wpf.PackIconKind.History;
            btnNewOrRestore.ToolTip = Memo.Delete == null ? "New Memo" : "Restore";

            // Color, Checkbox, Archive
            btnColor.IsEnabled = btnCheckbox.IsEnabled = btnArchive.IsEnabled = Memo.Delete == null;
            btnColor.Opacity = btnCheckbox.Opacity = btnArchive.Opacity = Memo.Delete == null ? 1 : 0;

            // Checkbox, Archive, Delete
            UpdateCheckbox();
            UpdateArchive();
            UpdateDelete();
        }
        private void UpdateCheckbox()
        {
            btnCheckbox.ToolTip = Memo.Checked == null ? "Enable Checkbox" : "Disable Checkbox";
            icoCheckbox.Kind = Memo.Checked == null ?
                MaterialDesignThemes.Wpf.PackIconKind.CheckAll :
                MaterialDesignThemes.Wpf.PackIconKind.CheckboxIndeterminateOutline;
        }
        private void UpdateArchive()
        {
            btnArchive.ToolTip = Memo.Archive ? "Unarchive" : "Archive";
            icoArchive.Kind = Memo.Archive ?
                MaterialDesignThemes.Wpf.PackIconKind.ArchiveArrowUp :
                MaterialDesignThemes.Wpf.PackIconKind.ArchiveArrowDownOutline;
        }
        private void UpdateDelete()
        {
            icoDelete.Kind = Memo.Delete == null ?
                MaterialDesignThemes.Wpf.PackIconKind.TrashCanOutline :
                MaterialDesignThemes.Wpf.PackIconKind.DeleteForeverOutline;
            btnDelete.ToolTip = Memo.Delete == null ? "Delete" : "Delete Forever";
            btnDelete.Foreground = icoDelete.Foreground = Memo.Delete == null ? Brushes.DimGray : Brushes.Red;
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
        private async void btnNewOrRestore_Click(object sender, RoutedEventArgs e)
        {
            await App.CreateMemo();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            App.MemoWins.Remove(Memo.Key);
            Config.Save();
            Close();
        }

        // Body
        private void handleMemoChanged(object sender, EventArgs e)
        {
            if (!_loaded) return;

            // Update my memo instance
            Control sndCon = (Control)sender;
            if (sndCon.Name == "txtTitle") Title = Memo.Title = txtTitle.Text;
            else if (sndCon.Name == "txtContent") Memo.Content = txtContent.Text;
            else if (sndCon.Name == "btnCheckbox")
            {
                Memo._checked = Memo.Checked == null ? new HashSet<int>() : null;
                UpdateCheckbox();
            }
            else if (sndCon.Name == "btnArchive")
            {
                Memo.Archive = !Memo.Archive;
                UpdateArchive();
            }
            else if (sndCon.Name == "btnDelete")
            {
                if (Memo.Delete == null) Memo.Delete = DateTime.Now;
                else
                {
                    App.Memos.Remove(Memo.Key);
                    App.MemoWins.Remove(Memo.Key);
                    App.MainWin?.UpdateMemosFromAppByView();
                    App.DeleteMemo(Memo.Key).Start();
                    Config.Save();
                    Close();
                    return;
                }
                UpdateAppBar();
            }
            else // Changed color
            {
                Memo.Color = (string)sndCon.Tag;
                Background = grdHeader.Background = sndCon.Background;
            }

            // Update Main Window
            App.MainWin?.UpdateMemosFromAppByView();

            //Put memo to server
            if (PutMemoTimer == null)
                PutMemoTimer = new Timer(async _ =>
                {
                    if (!App.Memos.ContainsKey(Memo.Key)) return;

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
