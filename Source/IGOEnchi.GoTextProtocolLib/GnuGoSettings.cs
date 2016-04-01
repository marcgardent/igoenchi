using System.Xml.Serialization;

namespace IGoEnchi
{
    public class GnuGoSettings
    {
        public static readonly GnuGoSettings Default =
            new GnuGoSettings(19, 0, 6.5f, 10, GnuGoColor.Black);

        public GnuGoSettings() :
            this(Default.BoardSize,
                Default.Handicap,
                Default.Komi,
                Default.Level,
                Default.Color)
        {
        }

        public GnuGoSettings(int boardSize, int handicap, float komi,
            int level, GnuGoColor color)
        {
            BoardSize = boardSize;
            Handicap = handicap;
            Komi = komi;
            Level = level;
            Color = color;
        }

        [XmlElement("boardSize")]
        public int BoardSize { get; set; }

        [XmlElement("handicap")]
        public int Handicap { get; set; }

        [XmlElement("komi")]
        public float Komi { get; set; }

        [XmlElement("gnuGoLevel")]
        public int Level { get; set; }

        [XmlElement("gnuGoColor")]
        public GnuGoColor Color { get; set; }
    }
}