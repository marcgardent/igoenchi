using System.Diagnostics;
using System.IO;

namespace GoART.FfmpegIntegration
{
    public class FfmpegEncoding
    {
        private readonly TempFileProvider temp;


        private readonly string ffmpeg;
        private int framerate = 24;
        private int currentframe = 0;
        private string audioFile;

        public FfmpegEncoding(TempFileProvider temp, int inputFramerate)
        {
            this.temp = temp;
            this.ffmpeg = DefaultPath();
            this.framerate = inputFramerate;
            this.audioFile = $"{temp.Folder}\\encoding-ffmpeg.wav";
            if (!File.Exists(ffmpeg))
            {
                throw new FileNotFoundException("not found ffmpeg.exe", ffmpeg);
            }
        }
         

        public Stream WavStream()
        {
            return temp.CreateReadWriteStream($"{temp.Folder}\\encoding-ffmpeg.wav");
        }

        public Stream NextFrame()
        {
            return temp.CreateReadWriteStream($"{temp.Folder}\\encoding-ffmpeg-{currentframe++:00000}.png");
        }


        string DefaultPath()
        {

            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var directory = System.IO.Path.GetDirectoryName(path);

            return Path.Combine(directory, "ffmpeg", "ffmpeg.exe");
        }

        public void SaveAsMP4(string output)
        {
            var arguments = $"-y -r {framerate} -start_number 0  -i \"{temp.Folder}\\encoding-ffmpeg-%5d.png\" -i \"{audioFile}\" -c:v libx264 -preset veryfast -crf 1  -c:a aac -b:a 128k -vf \"fps=24,format=yuv420p\" -metadata title=\"GoART Ludopant\"   \"{output}\"";
            var handler = Process.Start(new ProcessStartInfo(ffmpeg, arguments));
            handler.WaitForExit();
        }
        public void SaveAsMP4Release(string output)
        {
            var arguments = $"-y -r {framerate} -start_number 0  -i \"{temp.Folder}\\encoding-ffmpeg-%5d.png\" -i \"{audioFile}\" -c:v libx264 -preset veryslow -crf 1  -c:a aac -b:a 128k -vf \"fps=24,format=yuv420p\" -metadata title=\"GoART Ludopant\"   \"{output}\"";
            var handler = Process.Start(new ProcessStartInfo(ffmpeg, arguments));
            handler.WaitForExit();
        }
    }
}