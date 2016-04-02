using System;
using System.Diagnostics;
using System.IO;
using IGOEnchi.SmartGameLib.io;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IGOEnchi.SmartGameLib.Test
{
    [TestClass]
    public class SgfWriterTest
    {
        [TestMethod]
        public void When_MultiValue_Do_Properties_Keep_Initial_Sorting()
        {
            var b = new SgfBuilder();
            b
                .p("p1", "v1")
                .p("p3", "v1")
                .p("p1", "v2")
                .p("p2", "v1")
                .p("p1", "v3")
                .p("p2", "v2")
                .p("p2", "v3");
            using (var sw = new StringWriter())
            {
                var target = new SgfWriter(sw, false);
                target.WriteSgfTree(b.ToSgfTree());
                var excepted = "(;p1[v1][v2][v3]p3[v1]p2[v1][v2][v3])";
                Trace.WriteLine(excepted);
                Trace.WriteLine(sw);
                Assert.AreEqual(excepted, sw.ToString());
            }
        }

        [TestMethod]
        public void When_Fork_Do_Use_PreOrder()
        {
            var b = new SgfBuilder();
            b.p("b", "M1")
                .Fork(x => x.p("b", "M2").Next().p("b", "M3"))
                .Fork(x => x.p("c", "M2").Next().p("c", "M3"));

            using (var sw = new StringWriter())
            {
                var target = new SgfWriter(sw, false);
                target.WriteSgfTree(b.ToSgfTree());
                var excepted = "(;b[M1](;b[M2];b[M3])(;c[M2];c[M3]))";
                Trace.WriteLine(excepted);
                Trace.WriteLine(sw);
                Assert.AreEqual(excepted, sw.ToString());
            }
        }

        [TestMethod]
        public void When_EmptyTree_Do_not_Write()
        {
            var b = new SgfBuilder();
            b.p("b", "M1").Fork(x =>{ }).Fork(x => { x.Next(); });

            using (var sw = new StringWriter())
            {
                var target = new SgfWriter(sw, false);
                target.WriteSgfTree(b.ToSgfTree());
                var excepted = "(;b[M1])";
                Trace.WriteLine(excepted);
                Trace.WriteLine(sw);
                Assert.AreEqual(excepted, sw.ToString());
            }
        }
    }
}
