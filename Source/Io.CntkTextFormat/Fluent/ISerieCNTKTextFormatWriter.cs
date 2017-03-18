using System;

namespace Io.CNTKTextFormat
{
    public interface ISerieCNTKTextFormatWriter : IDisposable
    {
        ISequenceCntkTextFormatWriter Serie();
    }
}