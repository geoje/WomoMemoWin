using System.Collections.Generic;

namespace WomoMemo.Models
{
    internal class ColorMap
    {
        public static Dictionary<string, string> BACKGROUND_COLORS = new Dictionary<string, string>()
        {
            {"pink", "#F8BBD0"},
            {"red", "#FFCDD2"},
            {"deepOrange", "#FFCCBC"},
            {"orange", "#FFE0B2"},
            {"amber", "#FFECB3"},
            {"yellow", "#FFF9C4"},
            {"lime", "#F0F4C3"},
            {"lightGreen", "#DCEDC8"},
            {"green", "#C8E6C9"},
            {"teal", "#B2DFDB"},
            {"cyan", "#B2EBF2"},
            {"lightBlue", "#B3E5FC"},
            {"blue", "#BBDEFB"},
            {"indigo", "#C5CAE9"},
            {"purple", "#E1BEE7"},
            {"deepPurple", "#D1C4E9"},
            {"blueGrey", "#CFD8DC"},
            {"brown", "#D7CCC8"},
            {"grey", "#F5F5F5"},
            {"clear", "#FFFFFF"}
        };

        public static string Background(string color)
        {
            if (!BACKGROUND_COLORS.ContainsKey(color)) color = "clear";
            return BACKGROUND_COLORS[color];
        }
        public static string Border(string color)
        {
            if (!BACKGROUND_COLORS.ContainsKey(color)) color = "clear";
            return color == "clear" ? "#1F000000" : BACKGROUND_COLORS[color];
        }
    }
}
