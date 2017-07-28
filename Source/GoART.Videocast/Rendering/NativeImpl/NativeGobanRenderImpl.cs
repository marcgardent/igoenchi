using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using IGOEnchi.Videocast.Rendering.NativeImpl.Models;

namespace IGOEnchi.Videocast.Rendering.NativeImpl
{
    public class NativeGobanRenderImpl : IGobanRender
    {
        private readonly Bitmap bitmap;
        private readonly Brush blackFill;
        private readonly Pen blackStroke;
        private readonly GobanColor color;
        private readonly Graphics context;
        private readonly Pen focusStroke;
        private readonly GobanGeometry geometry;

        private readonly Pen gridStroke;

        private readonly Brush whiteFill;
        private readonly Pen whiteStroke;


        private bool isBlack;

        public NativeGobanRenderImpl(GobanGeometry geometry, GobanColor color)
        {
            this.geometry = geometry;
            this.color = color;
            bitmap = new Bitmap(this.geometry.widthPx, this.geometry.heightPx);
            context = Graphics.FromImage(bitmap);
            context.SmoothingMode = SmoothingMode.HighQuality;
            gridStroke = new Pen(this.color.GridStroke, this.geometry.strokePx);

            blackFill = new SolidBrush(Color.Black);
            whiteFill = new SolidBrush(Color.White);
            blackStroke = new Pen(color.BlackStroke, geometry.strokePx);
            whiteStroke = new Pen(color.WhiteStroke, geometry.strokePx);
            focusStroke = new Pen(color.FocusStroke, geometry.focusStrokePx);
        }


        private Brush StoneFill => isBlack ? blackFill : whiteFill;
        private Pen StoneStroke => isBlack ? blackStroke : whiteStroke;

        public void ClearGoban()
        {
            context.Clear(color.Background);

            foreach (var horizontal in geometry.Lines())
            {
                context.DrawLine(gridStroke, horizontal.X, horizontal.Y, horizontal.Right, horizontal.Bottom);
            }
        }

        public void SetBlack()
        {
            isBlack = true;
        }

        public void SetWhite()
        {
            isBlack = false;
        }

        public void Outline(byte x, byte y)
        {
            throw new NotImplementedException();
        }

        public void Stone(byte x, byte y)
        {
            context.FillEllipse(StoneFill, geometry.IntersectionBound(x, y));
            context.DrawEllipse(StoneStroke, geometry.IntersectionBound(x, y));
        }

        public void Focus(byte x, byte y)
        {
            var rect = geometry.IntersectionSemiBound(x, y);

            context.DrawEllipse(focusStroke, rect);
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