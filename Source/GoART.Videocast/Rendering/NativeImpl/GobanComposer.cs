using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using IGOEnchi.Videocast.Rendering.NativeImpl.Models;

namespace IGOEnchi.Videocast.Rendering.NativeImpl
{
    class GobanComposer : IGobanRenderAsImage
    {
        private readonly GobanColor colors;
        private readonly Bitmap bitmap;
        private readonly Graphics context;
        private IGobanRender[] gobans => new []{main, seconde, tierce, quinte };
        private IGobanRender[] outlined => new []{main, seconde};

        private readonly IGobanRender main;
        private readonly IGobanRender seconde;
        private readonly IGobanRender tierce;
        private readonly IGobanRender quinte;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="gridSize"></param>
        /// <param name="resolution">FullHD=120, 4K=240</param>
        public GobanComposer(GobanColor colors, byte gridSize, int resolution=120)
        {
            var width = 16* resolution;
            var height = 9* resolution;

            this.colors = colors;
            this.bitmap = new Bitmap(width, height);
            this.context = Graphics.FromImage(bitmap);
            context.Clear(colors.Background);

            this.main = new GobanRenderTransform(new PointF(0, 0), context, new NativeGobanRenderImpl(this.context, new GobanGeometry(9 * resolution, gridSize), colors));
            this.quinte = new GobanRenderTransform(new PointF(10 * resolution, 0), context, new NativeGobanRenderImpl(this.context, new GobanGeometry(3 * resolution, gridSize), colors));
            this.tierce = new GobanRenderTransform(new PointF(13 * resolution, 0), context, new NativeGobanRenderImpl(this.context, new GobanGeometry(3 * resolution, gridSize), colors));
            this.seconde = new GobanRenderTransform(new PointF(10 * resolution, 3 * resolution), context, new NativeGobanRenderImpl(this.context, new GobanGeometry(6 * resolution, gridSize), colors));
        }
        
        public void ClearGoban()
        {
            foreach (var goban in gobans) goban.ClearGoban();
        }

        public void SetBlack()
        {
            foreach (var goban in gobans) goban.SetBlack();
        }

        public void SetWhite()
        {
            foreach (var goban in gobans) goban.SetWhite();
        }

        public void Outline(byte x, byte y, Color color)
        {
            foreach (var goban in outlined) goban.Outline(x, y, color);
        }

        public void Stone(byte x, byte y)
        {
            foreach (var goban in gobans) goban.Stone(x, y);
        }

        public void Focus(byte x, byte y)
        {
            foreach (var goban in gobans) goban.Focus(x, y);
        }

        public void Influence(byte x, byte y, byte black, byte white)
        {
            tierce.Outline(x, y, Color.FromArgb(128,0,0, black));
            quinte.Outline(x, y, Color.FromArgb(128, white, 0,0));
            if (white == black)
            {
                main.Outline(x, y, Color.FromArgb(128, white, white, white));
            }
            else if (white > black)
            {
                main.Outline(x, y, Color.FromArgb(128, white, 0, 0));
            }
            else
            {
                main.Outline(x, y, Color.FromArgb(128, 0, 0, black));
            }
        }

        public void Impact(byte x, byte y, byte intensity)
        {
            seconde.Outline(x, y, Color.FromArgb(128, intensity, intensity, intensity));
        }

        public void ReadPng(Stream outstream)
        {
            context.Flush();
            bitmap.Save(outstream, ImageFormat.Png);
        }
        
        public void Dispose()
        {
            context.Dispose();
            bitmap.Dispose();
        }

    }
}
