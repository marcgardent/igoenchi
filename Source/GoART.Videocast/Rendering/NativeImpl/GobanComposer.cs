using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using IGOEnchi.Videocast.Rendering.NativeImpl.Models;
using IGOPhoenix.GoGameAnalytic.Maps;

namespace IGOEnchi.Videocast.Rendering.NativeImpl
{
    class GobanComposer : IGobanRenderAsImage
    {
        private readonly GobanColor colors;
        private readonly Bitmap bitmap;
        private readonly Graphics context;
        private IGobanRender[] gobans => new []{main, seconde, tierce, quinte };
        private IGobanRender[] outlined => new []{main, seconde};
        private readonly ThresholdMapper<byte> influenceScale;

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


            this.influenceScale = new ThresholdMapper<byte>(0, (i, th) => i <= th);
            influenceScale.Add(1 / 16d, 1 * 13);
            influenceScale.Add(1 / 15d, 2 * 13);
            influenceScale.Add(1 / 14d, 3 * 13);
            influenceScale.Add(1 / 13d, 4 * 13);
            influenceScale.Add(1 / 12d, 5 * 13);
            influenceScale.Add(1 / 11d, 6 * 13);
            influenceScale.Add(1 / 10d, 7 * 13);
            influenceScale.Add(1 / 9d, 8 * 13);
            influenceScale.Add(1 / 8d, 9 * 13);
            influenceScale.Add(1 / 7d, 10 * 13);
            influenceScale.Add(1 / 6d, 11 * 13);
            influenceScale.Add(1 / 5d, 12 * 13);
            influenceScale.Add(1 / 4d, 13 * 13);
            influenceScale.Add(1 / 3d, 14 * 13);
            influenceScale.Add(1 / 2d, 15 * 13);
            influenceScale.Add(1d, 16 * 13);
            influenceScale.Add(2d, 17 * 13);
            influenceScale.Add(3d, 18 * 13);
            influenceScale.Add(4d, 19 * 13);
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

        public void Influence(byte x, byte y, double blackInf, double whiteInf)
        {
            var white = influenceScale.Map(whiteInf);
            var black = influenceScale.Map(blackInf);

            tierce.Outline(x, y, Color.FromArgb(black, 0, 0));
            quinte.Outline(x, y, Color.FromArgb(0, 0, white));
            if (Math.Abs(white - black) < 1/64d)
            {
                main.Outline(x, y, Color.FromArgb(128, black, 0, white));
            }
            else if (white > black)
            {
                main.Outline(x, y, Color.FromArgb(128, 0, 0, white));
            }
            else
            {
                main.Outline(x, y, Color.FromArgb(128, black, 0, 0));
            }
        }

        public void Impact(byte x, byte y, double intensity)
        {
            var mapped = influenceScale.Map(intensity);
            seconde.Outline(x, y, Color.FromArgb(mapped, mapped, mapped));
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
