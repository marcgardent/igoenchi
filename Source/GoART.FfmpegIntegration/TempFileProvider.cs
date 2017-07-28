using System;
using System.Collections.Generic;
using System.IO;

namespace GoART.FfmpegIntegration
{
    public class TempFileProvider : IDisposable
    {
        public readonly string Folder;
        private List<TempFile> TempFiles = new List<TempFile>();

        struct TempFile
        {
            public string filename;
            public FileStream stream;
        }

        public Stream CreateReadWriteStream(string name)
        {
            var t = new TempFile
            {
                filename = name,
                stream = File.Open(name, FileMode.Create, FileAccess.ReadWrite)
            };
            TempFiles.Add(t);
            return t.stream;
        }

        public TempFileProvider(string tempFolder)
        {
            Folder = tempFolder;
        }

        public void Dispose()
        {
            foreach (var tempFile in TempFiles)
            {
                try
                {
                    tempFile.stream.Dispose();
                    File.Delete(tempFile.filename);
                }
                catch (Exception)
                {
                    
                }
            }
        }
    }
}