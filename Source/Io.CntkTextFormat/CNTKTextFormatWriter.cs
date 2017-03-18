using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Io.CNTKTextFormat
{
    /// <summary>
    /// https://github.com/Microsoft/CNTK/wiki/CNTKTextFormat-Reader
    /// eg.
    /// 1 |Oranges 100:3 123:4 |Bananas 8 |Apples 0 1 2 3 4 5 6 7 8 9
    /// 2 |Apples 0 1.1 22 0.3 14 54 0.06 0.7 1.8 9.9 |Bananas 123917 |Oranges 1134:1.911 13331:0.014
    /// 3 |Bananas -0.001 |Apples 3.9 1.11 121.2 99.13 0.04 2.95 1.6 7.19 10.8 -9.9 |Oranges 999:0.001 918918:-9.19
    /// </summary>
    public class CntkTextFormatWriter : ISerieCNTKTextFormatWriter, ISequenceCntkTextFormatWriter, ILineCntkTextFormatWriter, IDisposable
    {
        static readonly IFormatProvider enUS = new CultureInfo("en-US");
        static readonly string SEPARATOR = " "; // or \t
        static readonly string SEPARATOR_KEY = "|";
        static readonly string SEPARATOR_VALUE = ":"; 

        private readonly StreamWriter writer;
        private long sequenceCounter= 0;
        
        private CntkTextFormatWriter(StreamWriter w)
        {
            this.writer = w;
        }
        
        public static ISerieCNTKTextFormatWriter CreateCtf(string file)
        {
            return new CntkTextFormatWriter(File.CreateText(file));
        }

        public static ISerieCNTKTextFormatWriter CreateCtf(StreamWriter w)
        {
            return new CntkTextFormatWriter(w);
        }
        
        public ISequenceCntkTextFormatWriter Serie()
        {
            sequenceCounter++;
            return this;
        }

        public ILineCntkTextFormatWriter Sequence()
        {
            writer.Write(sequenceCounter);
            return this;
        }

        public ISerieCNTKTextFormatWriter CloseSerie()
        {
            return this;
        }
         

        private void WriteInputName(string name)
        {
            writer.Write(string.Concat(SEPARATOR, SEPARATOR_KEY, name));
        }


        public ISequenceCntkTextFormatWriter CloseSequence()
        {
            writer.Write("\r\n");
            return this;
        }
         
        
        public ILineCntkTextFormatWriter Dense(string name, IEnumerable<float> data)
        {
            WriteInputName(name);
            foreach(var d in data)
            {
                writer.Write(SEPARATOR);
                writer.Write(d.ToString(enUS));
            }
            
            return this;
        }

        public ILineCntkTextFormatWriter Dense(string name, IEnumerable<int> data)
        {
            WriteInputName(name);
            foreach (var d in data)
            {
                writer.Write(SEPARATOR);
                writer.Write(d.ToString(enUS));
            }

            return this;
        }


        public ILineCntkTextFormatWriter Dense(string name, IEnumerable<double> data)
        {
            WriteInputName(name);
            foreach (var d in data)
            {
                writer.Write(SEPARATOR);
                writer.Write(d.ToString(enUS));
            }

            return this;
        }


        public ILineCntkTextFormatWriter Dense(string name, IEnumerable<byte> data)
        {
            WriteInputName(name);
            foreach (var d in data)
            {
                writer.Write(SEPARATOR);
                writer.Write(d.ToString(enUS));
            }

            return this;
        }

        public ILineCntkTextFormatWriter Sparse(string name, IEnumerable<int> data)
        {
            WriteInputName(name);
            long position = 0;
            foreach (var d in data)
            {
                if (d != 0)
                {
                    writer.Write(SEPARATOR);
                    writer.Write(position.ToString(enUS));
                    writer.Write(SEPARATOR_VALUE);
                    writer.Write(d.ToString(enUS));
                }

                position++;
            }

            return this;
        }



        public ILineCntkTextFormatWriter Sparse(string name, IEnumerable<double> data)
        {
            WriteInputName(name);
            long position = 0;
            foreach (var d in data)
            {

                if (d != 0)
                {
                    writer.Write(SEPARATOR);
                    writer.Write(position.ToString(enUS));
                    writer.Write(SEPARATOR_VALUE);
                    writer.Write(d.ToString(enUS));
                }

                position++;
            }

            return this;
        }



        public ILineCntkTextFormatWriter Sparse(string name, IEnumerable<byte> data)
        {
            WriteInputName(name);
            long position = 0;
            foreach (var d in data)
            {

                if (d != 0)
                {
                    writer.Write(SEPARATOR);
                    writer.Write(position.ToString(enUS));
                    writer.Write(SEPARATOR_VALUE);
                    writer.Write(d.ToString(enUS));
                }

                position++;
            }

            return this;
        }


        public ILineCntkTextFormatWriter Sparse(string name, IEnumerable<float> data)
        {
            WriteInputName(name);
            long position = 0;
            foreach (var d in data)
            {

                if (d != 0)
                {
                    writer.Write(SEPARATOR);
                    writer.Write(position.ToString(enUS));
                    writer.Write(SEPARATOR_VALUE);
                    writer.Write(d.ToString(enUS));
                }

                position++;
            }

            return this;
        }

        public void Dispose()
        {
            this.writer.Dispose();
        }
    }
}
