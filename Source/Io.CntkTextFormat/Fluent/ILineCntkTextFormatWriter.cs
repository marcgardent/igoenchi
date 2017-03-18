using System.Collections.Generic;

namespace Io.CNTKTextFormat
{
    public interface ILineCntkTextFormatWriter
    {
        ILineCntkTextFormatWriter Dense(string name, IEnumerable<float> data);
        ILineCntkTextFormatWriter Sparse(string name, IEnumerable<float> data);


        ILineCntkTextFormatWriter Dense(string name, IEnumerable<byte> data);
        ILineCntkTextFormatWriter Sparse(string name, IEnumerable<byte> data);


        ILineCntkTextFormatWriter Dense(string name, IEnumerable<int> data);
        ILineCntkTextFormatWriter Sparse(string name, IEnumerable<int> data);


        ILineCntkTextFormatWriter Dense(string name, IEnumerable<double> data);
        ILineCntkTextFormatWriter Sparse(string name, IEnumerable<double> data);
        ISequenceCntkTextFormatWriter CloseSequence();
        
    }
}