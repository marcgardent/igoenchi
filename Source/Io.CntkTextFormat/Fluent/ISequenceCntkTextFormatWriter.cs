namespace Io.CNTKTextFormat
{
    public interface ISequenceCntkTextFormatWriter
    {
        ILineCntkTextFormatWriter Sequence();

        ISerieCNTKTextFormatWriter CloseSerie();
    }
}