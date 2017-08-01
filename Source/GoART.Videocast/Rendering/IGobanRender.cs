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
        
        void Outline(byte x, byte y, Color color);
        void Stone(byte x, byte y);
        void Focus(byte x, byte y);
        
    }

    internal interface IGobanRenderAsImage : IGobanRender, IDisposable
    {

        void Influence(byte x, byte y, double black, double white);

        void ReadPng(Stream outstream);
    }

}