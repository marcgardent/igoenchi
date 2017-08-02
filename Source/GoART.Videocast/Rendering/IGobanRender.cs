using System;
using System.Drawing;
using System.IO;

namespace IGOEnchi.Videocast.Rendering
{
    internal interface IGobanRender
    {
        void ClearGoban();
        void Grid();

        void SetBlack();
        void SetWhite();

        void Hatch(int x, int y, Color color);
        void Outline(int x, int y, Color color);
        void Stone(int x, int y);
        void Focus(int x, int y);

        void BorderLeft(int x, int y, Color color);
        void BorderTop(int x, int y, Color color);
        void BorderBottom(int x, int y, Color color);
        void BorderRight(int x, int y, Color color);
    }

    internal interface IGobanRenderAsImage : IDisposable
    {
         
        void ReadPng(Stream outstream);
    }

}