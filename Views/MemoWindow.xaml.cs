using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            // Color, Checkbox, Archive
            mnuColor.Visibility = mnuCheckbox.Visibility = mnuArchive.Visibility = Memo.Delete == null ? Visibility.Visible : Visibility.Collapsed;

            // Restore
            mnuRestore.Visibility = Memo.Delete == null ? Visibility.Collapsed : Visibility.Visible;

            // Checkbox, Archive, Delete
            UpdateCheckbox();
            UpdateArchive();
            UpdateDelete();
        }
        private void UpdateCheckbox()
        {
            txtCheckbox.Text = Memo.Checked == null ? "Enable Checkbox" : "Disable Checkbox";
            icoCheckbox.Kind = Memo.Checked == null ? PackIconKind.CheckAll : PackIconKind.CheckboxIndeterminateOutline;
        }
        private void UpdateArchive()
        {
            txtArchive.Text = Memo.Archive ? "Unarchive" : "Archive";
            icoArchive.Kind = Memo.Archive ? PackIconKind.ArchiveArrowUp : PackIconKind.ArchiveArrowDownOutline;
        }
        private void UpdateDelete()
        {
            icoDelete.Kind = Memo.Delete == null ? PackIconKind.TrashCanOutline : PackIconKind.DeleteForeverOutline;
            txtDelete.Text = Memo.Delete == null ? "Delete" : "Delete Forever";
            txtDelete.Foreground = icoDelete.Foreground = Memo.Delete == null ? Brushes.White : Brushes.Red;
        }

        // Header
        private void grdHeader_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }
        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            btnMenu.ContextMenu.PlacementTarget = btnMenu;
            btnMenu.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            btnMenu.ContextMenu.IsOpen = true;
            e.Handled = true;
        }
        private void mnuList_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWin == null) App.MainWin = new MainWindow();
            App.MainWin.Show();
            App.MainWin.Focus();
        }
        private async void mnuNew_Click(object sender, RoutedEventArgs e)
        {
            await App.CreateMemo();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            App.MemoWins.Remove(Memo.Key);
            Config.Save();

            if (Memo.Equals(Memo.Empty))
            {
                App.Memos.Remove(Memo.Key);
                if (App.MainWin != null) App.MainWin.UpdateMemosFromAppByView();
                Task.Run(() => App.DeleteMemo(Memo.Key));
            }
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
            else if (sndCon.Name == "mnuCheckbox")
            {
                Memo._checked = Memo.Checked == null ? new HashSet<int>() : null;
                UpdateCheckbox();
            }
            else if (sndCon.Name == "mnuArchive")
            {
                Memo.Archive = !Memo.Archive;
                UpdateArchive();
            }
            else if (sndCon.Name == "mnuRestore")
            {
                Memo.Delete = null;
                UpdateAppBar();
            }
            else if (sndCon.Name == "mnuDelete")
            {
                if (Memo.Delete == null) Memo.Delete = DateTime.Now;
                else
                {
                    App.Memos.Remove(Memo.Key);
                    App.MemoWins.Remove(Memo.Key);
                    App.MainWin?.UpdateMemosFromAppByView();
                    new Task(async () => await App.DeleteMemo(Memo.Key)).Start();
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
