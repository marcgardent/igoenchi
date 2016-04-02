using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IGOEnchi.SmartGameLib.models;

namespace IGOEnchi.SmartGameLib.io
{
    public class SgfWriter
    {
        private readonly TextWriter _writer;
        private readonly bool _smartNewLine;

        public SgfWriter(TextWriter writer, bool smartNewLine=true)
        {
            _writer = writer;
            _smartNewLine = smartNewLine;
        }

        private void NewLine()
        {
            if (_smartNewLine) _writer.WriteLine();
        }
        
        public void WriteSgfTree(SGFTree tree)
        {
            if (tree.HasContent)
            {
                _writer.Write("(");
                WriteSgfSequence(tree);
                _writer.Write(")");
                NewLine();
            }
        }

        private void WriteSgfSequence(SGFTree tree)
        {
            WriteProperties(tree.Properties);

            while (tree.ChildNodes.Count == 1)
            {
                NewLine();
                tree = tree.ChildNodes[0];
                WriteProperties(tree.Properties);
            }

            foreach (var childNode in tree.ChildNodes)
            {
                WriteSgfTree(childNode);
            }
        }

        private void WriteProperties(List<SGFProperty> properties)
        {
            _writer.Write(";");

            //Compress Properties multivalued
            foreach (var group in properties.GroupBy(x => x.Name))
            {
                _writer.Write(group.Key);
                foreach (var property in group)
                {
                    _writer.Write("[");
                    _writer.Write(property.Value);
                    _writer.Write("]");
                }
            }
        }
         
    }
}
