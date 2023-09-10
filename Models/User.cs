﻿using System.Windows.Media.Imaging;

namespace WomoMemo.Models
{
    public class User
    {
        public static string Name = "";
        public static string Email = "";
        public static string ImageUrl = "";
        public static BitmapImage? Image;
        public static string Id = "";

        public static void Clear()
        {
            Name = "";
            Email = "";
            ImageUrl = "";
            Image = null;
            Id = "";
        }
    }
}
