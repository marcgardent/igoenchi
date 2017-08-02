using System;
using System.Drawing;
using System.IO;

namespace IGOEnchi.Videocast.Rendering
{
    internal interface IGobanRender
    {
        void ClearGoban();

        void SetBlack();
        void SetWhite();
        
        void Outline(int x, int y, Color color);
        void Stone(int x, int y);
        void Focus(int x, int y);
        
    }

    internal interface IGobanRenderAsImage : IGobanRender, IDisposable
    {

        void Influence(int x, int y, double black, double white);

        void ReadPng(Stream outstream);
    }

}