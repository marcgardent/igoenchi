using System;
using System.IO;

namespace IGOEnchi.Videocast.Rendering
{
    internal interface IGobanRender : IDisposable
    {
        void ClearGoban();

        void SetBlack();
        void SetWhite();

        void Outline(byte x, byte y);
        void Stone(byte x, byte y);
        void Focus(byte x, byte y);


        void ReadPng(Stream outstream);
    }
}