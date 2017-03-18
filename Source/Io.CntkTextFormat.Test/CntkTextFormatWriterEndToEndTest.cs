using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Io.CNTKTextFormat;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Io.CntkTextFormat.Test
{
    [TestClass]
    public class CntkTextFormatWriterEndToEndTest
    {

 

        [TestMethod]
        public void FullShowCase()
        {

            using (var ms = new MemoryStream())
            using (var streamWriter = new StreamWriter(ms, Encoding.ASCII))
            using (var target = CntkTextFormatWriter.CreateCtf(streamWriter))
            {
                target.Serie()
                        .Sequence()
                            .Dense("dense", new[] { 1, 1, 1 })
                            .Sparse("sparse", new[] { 1, 0, 0, 0, 1 })
                        .CloseSequence()
                        .Sequence()
                            .Dense("dense", new[] { 2, 2, 2 })
                            .Sparse("sparse", new[] { 0, 0, 1, 0, 0 })
                        .CloseSequence()
                    .CloseSerie()
                    .Serie()
                        .Sequence()
                            .Dense("dense", new[] { 0, 0, 0 })
                            .Sparse("sparse", new[] { 3, 3, 3, 3, 3 })
                        .CloseSequence();

                streamWriter.Flush();
                ms.Position = 0;
                var sr = new StreamReader(ms, Encoding.ASCII);
                var actual = sr.ReadToEnd();
                Debug.WriteLine(actual);

                Assert.AreEqual("1 |dense 1 1 1 |sparse 0:1 4:1\r\n1 |dense 2 2 2 |sparse 2:1\r\n2 |dense 0 0 0 |sparse 0:3 1:3 2:3 3:3 4:3\r\n", actual);
            }
        }
    }
}
