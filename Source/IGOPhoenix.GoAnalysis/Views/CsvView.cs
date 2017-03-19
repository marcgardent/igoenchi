using System.Collections.Generic;
using System.IO;
using IGOPhoenix.GoGameAnalytic.Models;

namespace IGOPhoenix.GameAnalysis.views
{

    /// <summary>
    /// Adapt GoPlayerStat to CSV
    /// </summary>
    public class CsvView
    {
        private readonly string _path;
        private readonly IEnumerable<GoStat> Statistics;

        public CsvView(IEnumerable<GoStat> statistics)
        {
            Statistics = statistics;
        }

        public void ToCsv(string path)
        {
            using (var s = File.Open(path, FileMode.Create))
            using (var writer = new StreamWriter(s))
            {

                writer.Write("Turn;Black Stones;Black Groups;Black liberties;");
                writer.WriteLine("White Stones;White Groups;White liberties;");
                foreach (var goStat in Statistics)
                {
                    writer.Write($"{goStat.Turn};");
                    WriteGroup(writer, goStat.BlackStat);
                    WriteGroup(writer, goStat.WhiteStat);
                    writer.WriteLine();
                }
            }
        }

        private static void WriteGroup(StreamWriter writer, GoPlayerStat stat)
        {
            writer.Write($"{stat.StoneCount};{ stat.Groups.Count};{stat.LibertyCount};");
        }
    }
}