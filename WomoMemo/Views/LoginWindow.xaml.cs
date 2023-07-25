using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Web;
using System.Windows;
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
            if (sessinoTokenCookie != null)
            {
                Config.SessionTokenValue = sessinoTokenCookie.Value;
                Config.Save();
                Close();
            }
        }
    }
}
