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
            webMain.Source = new Uri(Config.AuthUrl + "/login?callbackUrl=" + HttpUtility.UrlEncode(Config.MemoUrl));
        }

        private async void webMain_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
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
