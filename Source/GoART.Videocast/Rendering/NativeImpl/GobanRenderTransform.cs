using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace IGOEnchi.Videocast.Rendering.NativeImpl
{
    class GobanRenderTransform : IGobanRender 
    {
        private readonly IGobanRender goban;
        private readonly Graphics context;
        private readonly PointF origin;
        private GraphicsState state;

        public GobanRenderTransform(PointF origin, Graphics context, IGobanRender goban)
        {
            this.goban = goban;
            this.context = context;
            this.origin = origin;
        }
         
        private void Transform(Action action)
        {
            this.state = context.Save();
            context.TranslateTransform(origin.X, origin.Y);
            action();
            context.Restore(state);
        }
       
        public void ClearGoban() => Transform(this.goban.ClearGoban);

        public void SetBlack() => goban.SetBlack();

        public void SetWhite() => goban.SetWhite();

        public void Outline(int x, int y,Color color) => Transform(() => goban.Outline(x, y, color));

        public void Stone(int x, int y) => Transform(() => goban.Stone(x, y));

        public void Focus(int x, int y) => Transform(() => goban.Focus(x, y));
         
    }
}