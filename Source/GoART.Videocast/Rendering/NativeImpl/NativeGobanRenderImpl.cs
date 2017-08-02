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
        
        private readonly Brush blackFill;
        private readonly Pen blackStroke;
        private readonly GobanColor colors;
        private readonly Graphics context;
        private readonly Pen focusStroke;
        private readonly GobanGeometry geometry;

        private readonly Pen gridStroke;

        private readonly Brush whiteFill;
        private readonly Pen whiteStroke;


        private bool isBlack;
        private readonly SolidBrush backgroundFill;

        public NativeGobanRenderImpl(Graphics context,GobanGeometry geometry, GobanColor colors)
        {
            this.geometry = geometry;
            this.colors = colors;
            this.context = context;
            
            
            context.SmoothingMode = SmoothingMode.HighQuality;
            gridStroke = new Pen(this.colors.GridStroke, this.geometry.strokePx);

            backgroundFill = new SolidBrush(colors.Background);
            blackFill = new SolidBrush(Color.Black);
            whiteFill = new SolidBrush(Color.White);
            blackStroke = new Pen(colors.BlackStroke, geometry.strokePx);
            whiteStroke = new Pen(colors.WhiteStroke, geometry.strokePx);
            focusStroke = new Pen(colors.FocusStroke, geometry.focusStrokePx);
        }
         
        private Brush StoneFill => isBlack ? blackFill : whiteFill;
        private Pen StoneStroke => isBlack ? blackStroke : whiteStroke;

        public void ClearGoban()
        {
            context.FillRectangle(backgroundFill,geometry.GobanRect);
            Grid();
        }

        public void Grid()
        {
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

        public void Hatch(int x, int y, Color color)
        {
            context.FillRectangle(new HatchBrush(HatchStyle.WideDownwardDiagonal, color), geometry.AeraBound(x, y));
        }

        public void Outline(int x, int y, Color color)
        {
            context.FillRectangle(new SolidBrush(color), geometry.AeraBound(x, y));
        }

        private void Border(PointF start, PointF end, Color color)
        {
            context.DrawLine(new Pen(color, geometry.strokePx * 2), start, end);
        }

        public void BorderLeft(int x, int y, Color color)
        {
            Border(geometry.LeftBottomCorner(x, y), geometry.LeftTopCorner(x, y), color);
        }

        public void BorderRight(int x, int y, Color color)
        {
            Border(geometry.RightBottomCorner(x, y), geometry.RightTopCorner(x, y), color);
        }

        public void BorderTop(int x, int y, Color color)
        {
            Border(geometry.RightTopCorner(x, y), geometry.LeftTopCorner(x, y), color);
        }

        public void BorderBottom(int x, int y, Color color)
        {
            Border(geometry.LeftBottomCorner(x, y), geometry.RightBottomCorner(x, y), color);
        }

        public void Stone(int x, int y)
        {
            context.FillEllipse(StoneFill, geometry.StoneBound(x, y));
            context.DrawEllipse(StoneStroke, geometry.StoneBound(x, y));
        }

        public void Focus(int x, int y)
        {
            var rect = geometry.FocusBound(x, y);

            context.DrawEllipse(focusStroke, rect);
        }
    }
}