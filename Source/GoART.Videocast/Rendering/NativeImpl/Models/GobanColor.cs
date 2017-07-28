using System.Drawing;

namespace IGOEnchi.Videocast.Rendering.NativeImpl.Models
{
    public class GobanColor
    {
        public static GobanColor BlackTheme = new GobanColor
        {
            Background = Color.Black,
            BlackStroke = Color.White,
            WhiteStroke = Color.White,
            GridStroke = Color.White,
            FocusStroke = Color.Red,
            BlackOutline = Color.DarkOrange,
            WhiteOutline = Color.DarkBlue
        };

        public static GobanColor WhiteTheme = new GobanColor
        {
            Background = Color.White,
            BlackStroke = Color.Black,
            WhiteStroke = Color.Black,
            GridStroke = Color.Black,
            FocusStroke = Color.Red,
            BlackOutline = Color.DarkOrange,
            WhiteOutline = Color.DarkBlue
        };

        public Color Background { get; private set; }
        public Color WhiteStroke { get; private set; }
        public Color BlackStroke { get; private set; }
        public Color GridStroke { get; private set; }
        public Color FocusStroke { get; private set; }

        public Color BlackOutline { get; private set; }
        public Color WhiteOutline { get; private set; }
    }
}