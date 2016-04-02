using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using IGOEnchi.GoGameSgf;
using IGOEnchi.SmartGameLib;
using IGOEnchi.SmartGameLib.io;
using IGOEnchi.SmartGameLib.models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IGOEnchi.GoGame.UnitTest
{
    /// <summary>
    /// Save as *.sgf at *.ex.sgf.
    /// To Check manualy file with tiers softwares.
    /// Check console for warn
    /// </summary>
    [TestClass]
    public class EndToEndTest
    {
        private static readonly string AssemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        
        [TestMethod]
        public void When_SaveAs_Kogo_s_Joseki_Dictionary_Do_Same() => SaveAsTest("Kogo's Joseki Dictionary.sgf");


        private static void SaveAsTest(string usecase)
        {
            SGFTree actual;

            //deserialize data
            using (var stream = GetUseCases(usecase))
            {
                var excepted = SgfReader.LoadFromStream(stream);
                //read
                var gogame = SgfCompiler.Compile(excepted);

                //build
                actual = new GoSgfBuilder(gogame).ToSGFTree();
            }
            
            //Check with fs
            SaveActual(usecase, actual);
            
            //Check with Sgf models 
            using (var stream = GetUseCases(usecase))
            {
                var excepted = SgfReader.LoadFromStream(stream);
                CompareSgfHelper.CompareNode(excepted.ChildNodes[0], actual, ""); //TODO Algorithm Style  - Remove useless First Node?! see SgfCompiler.Compile
            }
        }

        private static Stream GetUseCases(string name)
        {    
            return File.OpenRead(Path.Combine(AssemblyLocation, "usecases", name));
        }

        /// <summary>
        /// Save to debugging with tiers softwares
        /// /!\ Test Chain SgfReader+SgfCompiler+GoSgfBuilder+SgfWriter!
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static void SaveActual(string name, SGFTree actual)
        {
            var file = Path.Combine(AssemblyLocation, "usecases", string.Concat(name, "ex.sgf"));
            Trace.WriteLine($"saveAs result to {file}");
            using (var stream = File.Open(file, FileMode.Truncate, FileAccess.ReadWrite))
            using (var sw = new StreamWriter(stream, Encoding.UTF8))
            {
                var writer = new SgfWriter(sw, true);
                writer.WriteSgfTree(actual);
            }
        }
    }
}
