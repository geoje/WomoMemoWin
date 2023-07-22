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
            webMain.Source = new Uri(Config.loginUrl + "?callbackUrl=" + HttpUtility.UrlEncode(Config.memoUrl));
        }

        private async void webMain_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            List<CoreWebView2Cookie> cookieList = await webMain.CoreWebView2.CookieManager.GetCookiesAsync(Config.memoUrl);
            var sessinoTokenCookie = cookieList.Find(cookie => cookie.Name.Equals(Config.sessionTokenName));
            if (sessinoTokenCookie != null)
            {
                Config.sessionTokenValue = sessinoTokenCookie.Value;
                Close();
            }
        }
    }
}
