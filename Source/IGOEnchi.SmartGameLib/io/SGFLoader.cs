/*  Copyright (C) 2006-2009 Valentin Kraevskiy
 * 	This file is part of IGoEnchi project
 *	Please read ..\license.txt for conditions of distribution and use
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IGOEnchi.SmartGameLib.models;

namespace IGOEnchi.SmartGameLib
{
    public class SGFLoader
    {
        /// <summary>
        ///     Set Defaut Encoding to load file
        /// </summary>
        public static Encoding DefaultEncodingConfiguration = Encoding.UTF8;

        private readonly StringBuilder propertyValue;
        private readonly StreamReader stream;

        private int charCount;

        private SGFLoader(StreamReader stream)
        {
            propertyValue = new StringBuilder();
            this.stream = stream;
        }

        private static List<int> BuildFileIndex(string path)
        {
            return BuildFileIndex(path, false);
        }

        private static List<int> BuildFileIndex(string path, bool permute)
        {
            if (File.Exists(path))
            {
                using (var fs = File.OpenRead(path))
                using (var reader = new StreamReader(fs))
                {
                    var index = new List<int>();
                    var level = 0;
                    var offset = 0;

                    while (!reader.EndOfStream)
                    {
                        var next = (char)reader.Read();
                        if (next == '(')
                        {
                            if (level == 0)
                            {
                                index.Add(offset);
                            }
                            level += 1;
                        }
                        else if (next == ')')
                        {
                            level -= 1;
                        }
                        offset += 1;
                    }
                

                    if (permute)
                    {
                        var permutation = new List<int>(index.Count);
                        var random = new Random();
                        while (index.Count > 0)
                        {
                            var next = random.Next(0, index.Count);
                            permutation.Add(index[next]);
                            index.RemoveAt(next);
                        }
                        return permutation;
                    }

                    return index;

                }
            }
            else
            {
                throw new IOException($"File doesn't exist {path}");
            }
        }

        public static IEnumerable<SGFTree> LoadAndParse(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    Directory.GetFiles(path, "*.sgf");
                }
                else
                {
                    var index = BuildFileIndex(path, true);
                    foreach (var tree in LoadAndParse(path, index))
                    {
                        yield return tree;
                    }
                }
            }
        }

        private static IEnumerable<SGFTree> LoadAndParse(string path, List<int> index)
        {
            if (File.Exists(path))
            {
                var stream = new StreamReader(
                    File.OpenRead(path),
                    DefaultEncodingConfiguration);

                try
                {
                    var loader = new SGFLoader(stream);

                    if (index == null)
                    {
                        while (loader.ReadUntil('(') == '(')
                        {
                            var tree = new SGFTree();
                            if (loader.ReadTree(tree))
                            {
                                yield return tree;
                            }
                        }
                    }
                    else
                    {
                        while (index.Count > 0)
                        {
                            stream.BaseStream.Seek(index[0], SeekOrigin.Begin);
                            stream.DiscardBufferedData();
                            index.RemoveAt(0);
                            loader.ReadUntil('(');
                            var tree = new SGFTree();
                            if (loader.ReadTree(tree))
                            {
                                yield return tree;
                            }
                        }
                    }
                }
                finally
                {
                    stream.Close();
                }
            }
        }

        public static SGFTree LoadAndParseSingle(string path)
        {
            if (File.Exists(path))
            {
                var stream = new StreamReader(
                    File.OpenRead(path),
                    DefaultEncodingConfiguration);

                SGFTree tree = null;

                try
                {
                    var loader = new SGFLoader(stream);

                    if (loader.ReadUntil('(') == '(')
                    {
                        tree = new SGFTree();

                        if (!loader.ReadTree(tree))
                        {
                            tree = null;
                        }
                    }
                }
                finally
                {
                    stream.Close();
                }

                if (tree != null)
                {
                    return tree;
                }
                throw new Exception("Couldn't load sgf from " + path);
            }
            throw new Exception("Couldn't open " + path);
        }

        public static SGFTree LoadFromStream(Stream baseStream)
        {
            var stream = new StreamReader(
                baseStream,
                DefaultEncodingConfiguration);

            SGFTree tree = null;

            try
            {
                var loader = new SGFLoader(stream);
                if (loader.ReadUntil('(') == '(')
                {
                    tree = new SGFTree();
                    if (!loader.ReadTree(tree))
                    {
                        tree = null;
                    }
                }
            }
            finally
            {
                stream.Close();
            }

            if (tree != null)
            {
                return tree;
            }
            throw new Exception("Couldn't load sgf from stream");
        }

        private bool ReadTree(SGFTree currentNode)
        {
            var nextChar = '\0';
            var propertyName = string.Empty;
            do
            {
                nextChar = ConsumeWhiteSpaces();
                if (nextChar == ';')
                {
                    currentNode = currentNode.NewNode();
                }
                else if (nextChar == '(')
                {
                    ReadTree(currentNode);
                }
                else if (nextChar == ')')
                {
                    return true;
                }
                else if (nextChar == '[')
                {
                    var attribute = ReadAttribute();
                    if (propertyName != string.Empty)
                    {
                        currentNode.AddAttribute(
                            new SGFProperty(propertyName, attribute));
                    }
                }
                else if (char.IsLetter(nextChar))
                {
                    propertyName = "";
                    do
                    {
                        propertyName = string.Concat(propertyName, nextChar);
                        nextChar = ConsumeWhiteSpaces();
                    } while (nextChar != '[' && !stream.EndOfStream);

                    if (stream.EndOfStream)
                    {
                        throw new Exception("Unexpected end of stream");
                    }

                    currentNode.AddAttribute(
                        new SGFProperty(propertyName, ReadAttribute()));
                }
                else if (stream.EndOfStream)
                {
                    throw new Exception("Unexpected end of stream");
                }
            } while (nextChar != ')');
            return true;
        }

        private char ConsumeWhiteSpaces()
        {
            var nextChar = '\n';
            while (char.IsWhiteSpace(nextChar) &&
                   !stream.EndOfStream)
            {
                nextChar = Convert.ToChar(stream.Read());
            }
            return nextChar;
        }

        private char ReadUntil(char target)
        {
            var nextChar = '\0';
            while (nextChar != target &&
                   !stream.EndOfStream)
            {
                nextChar = ConsumeWhiteSpaces();
            }
            return nextChar;
        }

        private string ReadAttribute()
        {
            char nextChar;
            propertyValue.Remove(0, propertyValue.Length);
            charCount = 0;
            nextChar = ConsumeWhiteSpaces();

            while (nextChar != ']' && !stream.EndOfStream)
            {
                propertyValue.Append(nextChar);
                if (!char.IsWhiteSpace(nextChar))
                {
                    charCount = propertyValue.Length;
                }

                nextChar = (char) stream.Read();
                if (nextChar == '\\')
                {
                    nextChar = (char) stream.Read();
                    if (char.IsWhiteSpace(nextChar))
                    {
                        nextChar = ' ';
                    }
                    else
                    {
                        charCount = propertyValue.Length;
                    }

                    propertyValue.Append(nextChar);
                    nextChar = Convert.ToChar(stream.Read());
                }
            }
            return propertyValue.ToString(0, charCount);
        }
    }
}