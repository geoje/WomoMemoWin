using System.Collections.Generic;

namespace WomoMemo.Models
{
    internal class ColorSet
    {
        public string Background, Border;

        public ColorSet(string background, string border)
        {
            Background = background;
            Border = border;
        }
    }
    internal class ColorMap
    {
        public static Dictionary<string, ColorSet> COLORS = new Dictionary<string, ColorSet>()
        {
            { "pink", new ColorSet("#FCE4EC", "#F8BBD0") },
            { "red", new ColorSet("#FFEBEE", "#FFCDD2") },
            { "deepOrange", new ColorSet("#FBE9E7", "#FFCCBC") },
            { "orange", new ColorSet("#FFF3E0", "#FFE0B2") },
            { "amber", new ColorSet("#FFF8E1", "#FFECB3") },
            { "yellow", new ColorSet("#FFFDE7", "#FFF9C4") },
            { "lime", new ColorSet("#F9EBE7", "#F0F4C3") },
            { "lightGreen", new ColorSet("#F1F8E9", "#DCEDC8") },
            { "green", new ColorSet("#E8F5E9", "#C8E6C9") },
            { "teal", new ColorSet("#E0F2F1", "#B2DFDB") },
            { "cyan", new ColorSet("#E0F7FA", "#B2EBF2") },
            { "lightBlue", new ColorSet("#E1F5FE", "#B3E5FC") },
            { "blue", new ColorSet("#E3F2FD", "#BBDEFB") },
            { "indigo", new ColorSet("#E8EAF6", "#C5CAE9") },
            { "purple", new ColorSet("#F3E5F5", "#E1BEE7") },
            { "deepPurple", new ColorSet("#EDE7F6", "#D1C4E9") },
            { "blueGrey", new ColorSet("#ECEFF1", "#CFD8DC") },
            { "brown", new ColorSet("#EFEBE9", "#D7CCC8") },
            { "grey", new ColorSet("#FAFAFA", "#F5F5F5") },
            { "clear", new ColorSet("#FFFFFF", "#1F000000") }
        };

        public static string Background(string color)
        {
            if (!COLORS.ContainsKey(color)) color = "clear";
            return COLORS[color].Background;
        }
        public static string Border(string color)
        {
            if (!COLORS.ContainsKey(color)) color = "clear";
            return COLORS[color].Border;
        }
    }
}
