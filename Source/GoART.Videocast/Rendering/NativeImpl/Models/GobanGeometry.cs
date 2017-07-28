using System;
using System.Collections.Generic;
using System.Drawing;

namespace IGOEnchi.Videocast.Rendering.NativeImpl.Models
{
    /// <summary>
    ///     Compute coordinates of elements
    /// </summary>
    public class GobanGeometry
    {
        public static GobanGeometry TumblrResolution = new GobanGeometry(500, 500, 19);

        public static GobanGeometry DebugResolution = new GobanGeometry(640, 320, 19);
        public static GobanGeometry HighResolution = new GobanGeometry(1920, 1080, 19);

        public static GobanGeometry UltraHighResolution = new GobanGeometry(3840, 2160, 19);
        public readonly int focusStrokePx;
        private readonly byte gridsize;
        private readonly int gridStepPx;
        private readonly int gridWidthPx;
        public readonly int heightPx;
        private readonly int marginLeftPx;

        private readonly int marginTopPx;
        public readonly int stoneSizePx;
        public readonly int strokePx;
        public readonly int widthPx;

        /// <summary>
        /// </summary>
        /// <param name="widthPx">video size</param>
        /// <param name="heightPx">video size</param>
        /// <param name="gridsize">grid of goban (ex 19x19)</param>
        public GobanGeometry(int widthPx, int heightPx, byte gridsize)
        {
            this.gridsize = gridsize;
            this.widthPx = widthPx;
            this.heightPx = heightPx;
            var min = Math.Min(widthPx, heightPx);
            strokePx = Math.Max(min/480, 1);
            gridStepPx = (int) (min*0.8/this.gridsize);

            gridWidthPx = gridStepPx*(this.gridsize - 1);
            stoneSizePx = (int) (gridStepPx*0.9);
            focusStrokePx = stoneSizePx/6;
            marginTopPx = gridStepPx;
            marginLeftPx = (widthPx - gridWidthPx)/2;
        }

        public IEnumerable<Rectangle> Lines()
        {
            for (byte y = 0; y < gridsize; y++)
            {
                yield return new Rectangle(
                    marginLeftPx,
                    marginTopPx + y*gridStepPx,
                    gridWidthPx, 0
                    );
            }

            for (byte x = 0; x < gridsize; x++)
            {
                yield return new Rectangle(
                    marginLeftPx + x*gridStepPx,
                    marginTopPx,
                    0, gridWidthPx
                    );
            }
        }

        public Point Intersection(int x, int y)
        {
            return new Point(marginLeftPx + x*gridStepPx, marginTopPx + y*gridStepPx);
        }

        public Rectangle IntersectionBound(int x, int y)
        {
            var intesection = Intersection(x, y);

            return new Rectangle(intesection.X - stoneSizePx/2, intesection.Y - stoneSizePx/2, stoneSizePx, stoneSizePx);
        }

        public Rectangle IntersectionSemiBound(int x, int y)
        {
            var intesection = Intersection(x, y);
            return new Rectangle(intesection.X - stoneSizePx/4, intesection.Y - stoneSizePx/4, stoneSizePx/2,
                stoneSizePx/2);
        }
    }
}