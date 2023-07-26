using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Media.Imaging;
using WomoMemo.Models;

namespace WomoMemo.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await webMain.EnsureCoreWebView2Async();
            webMain.CoreWebView2.CookieManager.DeleteAllCookies();
            webMain.Source = new Uri(Config.AuthUrl + "/login?callbackUrl=" + HttpUtility.UrlEncode(Config.MemoUrl));
        }

        private async void webMain_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            List<CoreWebView2Cookie> cookieList = await webMain.CoreWebView2.CookieManager.GetCookiesAsync(Config.MemoUrl);
            var sessinoTokenCookie = cookieList.Find(cookie => cookie.Name.Equals(Config.SessionTokenName));
            if (sessinoTokenCookie == null) return;

            // Save sessionToken to file
            Config.SessionTokenValue = sessinoTokenCookie.Value;
            Config.Save();

            // Save cookie to App
            App.Handler.CookieContainer = new CookieContainer();
            App.Handler.CookieContainer.Add(new Uri(Config.MemoUrl), new Cookie(Config.SessionTokenName, Config.SessionTokenValue));

            // Get and Download user profile
            await App.GetUserProfile();
            await App.DownloadUserProfileImage();

            // Update controls and close window
            App.MainWin?.UpdateControls();
            Close();
        }
    }
}
