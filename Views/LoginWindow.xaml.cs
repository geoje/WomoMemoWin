using Firebase.Auth;
using Firebase.Auth.UI;
using System.Windows;

namespace WomoMemo.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            FirebaseUI.Instance.Client.AuthStateChanged += AuthStateChanged;
        }

        private void AuthStateChanged(object? sender, UserEventArgs e)
        {
            if (e.User != null) Dispatcher.Invoke(Close);
        }
    }
}
