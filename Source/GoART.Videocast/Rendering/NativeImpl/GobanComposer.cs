using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using IGOEnchi.Videocast.Rendering.NativeImpl.Models;
using IGOPhoenix.GoGameAnalytic;
using IGOPhoenix.GoGameAnalytic.BitPlaneParsing;
using IGOPhoenix.GoGameAnalytic.Maps;

namespace IGOEnchi.Videocast.Rendering.NativeImpl
{
    class GobanComposer : IGobanRenderAsImage
    {
        
        private readonly Bitmap bitmap;
        private readonly Graphics context;
        private IGobanRender[] gobans => new []{main, seconde, tierce, quarte, quinte };
        private IGobanRender[] outlined => new []{main, seconde};
        private readonly ThresholdMapper<byte> influenceScale;

        private readonly IGobanRender main;
        private readonly IGobanRender seconde;
        private readonly IGobanRender tierce;
        private readonly GobanRenderTransform quarte;
        private readonly IGobanRender quinte;
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="gridSize"></param>
        /// <param name="resolution">FullHD=120, 4K=240</param>
        public GobanComposer(GobanColor colors, int gridSize, int resolution=120)
        {
            var width = 16* resolution;
            var height = 9* resolution;
            
            this.bitmap = new Bitmap(width, height);
            this.context = Graphics.FromImage(bitmap);
            context.Clear(colors.Background);

            this.main = new GobanRenderTransform(new PointF(0, 0), context, new NativeGobanRenderImpl(this.context, new GobanGeometry(9 * resolution, gridSize), colors));
            this.tierce = new GobanRenderTransform(new PointF(10 * resolution, 0), context, new NativeGobanRenderImpl(this.context, new GobanGeometry(2 * resolution, gridSize), colors));
            this.quarte = new GobanRenderTransform(new PointF(12 * resolution, 0), context, new NativeGobanRenderImpl(this.context, new GobanGeometry(2 * resolution, gridSize), colors));
            this.quinte = new GobanRenderTransform(new PointF(14 * resolution, 0), context, new NativeGobanRenderImpl(this.context, new GobanGeometry(2 * resolution, gridSize), colors));
            this.seconde = new GobanRenderTransform(new PointF(10 * resolution, 3 * resolution), context, new NativeGobanRenderImpl(this.context, new GobanGeometry(6 * resolution, gridSize), colors));


            this.influenceScale = new ThresholdMapper<byte>();
            influenceScale.Le(1 / 16d, 1 * 13);
            influenceScale.Le(1 / 15d, 2 * 13);
            influenceScale.Le(1 / 14d, 3 * 13);
            influenceScale.Le(1 / 13d, 4 * 13);
            influenceScale.Le(1 / 12d, 5 * 13);
            influenceScale.Le(1 / 11d, 6 * 13);
            influenceScale.Le(1 / 10d, 7 * 13);
            influenceScale.Le(1 / 9d, 8 * 13);
            influenceScale.Le(1 / 8d, 9 * 13);
            influenceScale.Le(1 / 7d, 10 * 13);
            influenceScale.Le(1 / 6d, 11 * 13);
            influenceScale.Le(1 / 5d, 12 * 13);
            influenceScale.Le(1 / 4d, 13 * 13);
            influenceScale.Le(1 / 3d, 14 * 13);
            influenceScale.Le(1 / 2d, 15 * 13);
            influenceScale.Le(1d, 16 * 13);
            influenceScale.Le(2d, 17 * 13);
            influenceScale.Le(3d, 18 * 13);
            influenceScale.Le(4d, 19 * 13);
            influenceScale.Gt(4d, 255);
        }

        public void Grid()
        {
            foreach (var goban in gobans) goban.Grid();
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

        public void Outline(int x, int y, Color color)
        {
            foreach (var goban in outlined) goban.Outline(x, y, color);
        }

        public void Stone(int x, int y)
        {
            foreach (var goban in gobans) goban.Stone(x, y);
        }

        public void Focus(int x, int y)
        {
            foreach (var goban in gobans) goban.Focus(x, y);
        }

        public void Influence(InfluenceAnalyser influences)
        {
            foreach (var coord in influences.whiteInfluence.AllCoords)
            {
                var white = influenceScale.Map(influences.whiteInfluence[coord]);
                var black = influenceScale.Map(influences.blackInfluence[coord]);
                var impact = influenceScale.Map(influences.MoveInfluence[coord]);
                tierce.Outline(coord.X, coord.Y, Color.FromArgb(black, 0, 0));
                quarte.Outline(coord.X, coord.Y, Color.FromArgb(black, 0, white));
                quinte.Outline(coord.X, coord.Y, Color.FromArgb(0, 0, white));
                seconde.Outline(coord.X, coord.Y, Color.FromArgb(impact, impact, impact));
                
            }

            foreach (var territory in influences.ContestTerritories)
            {
                //main.Outline(territory.X, territory.Y, Color.FromArgb(128,255, 255, 255));
                main.Hatch(territory.X, territory.Y, Color.Red);
            }

            foreach (var territory in influences.WhiteTerritories)
            {
                main.Outline(territory.X, territory.Y, Color.FromArgb(128,255, 255, 255));
                DrawBorders(main, territory, Color.Red);

            }

            foreach (var territory in influences.BlackTerritories)
            {
                main.Outline(territory.X, territory.Y, Color.FromArgb(128, 0, 0, 0));
                DrawBorders(main, territory, Color.Red);
            }

            Grid();
        }

        private void DrawBorders(IGobanRender render, TerritoryParser.TerritoryCoords coords, Color color)
        {
            foreach (var orientation in coords.Orientations)
            {
                if(orientation == TerritoryParser.BorderState.bottom) render.BorderBottom(coords.X, coords.Y, color);
                if(orientation == TerritoryParser.BorderState.right) render.BorderRight(coords.X, coords.Y, color);
                if(orientation == TerritoryParser.BorderState.left) render.BorderLeft(coords.X, coords.Y, color);
                if(orientation == TerritoryParser.BorderState.top) render.BorderTop(coords.X, coords.Y, color);
            }
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
