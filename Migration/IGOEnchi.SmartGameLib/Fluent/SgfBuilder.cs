using System;
using System.Globalization;
using IGOEnchi.SmartGameLib.models;

namespace IGOEnchi.SmartGameLib
{


    public class SgfBuilder
    {
        public delegate void ForkHandler(SgfBuilder builder);
        private readonly SGFTree _root;
        private SGFTree _cursor;

        public SgfBuilder()
        {
            _root = new SGFTree();
            _cursor = _root;
        }

        /// <summary>
        /// Generic property
        /// </summary>
        /// <param name="key">key of properties</param>
        /// <param name="value">pre-formated value</param>
        /// <returns></returns>
        public SgfBuilder p(string key, string value)
        {
            _cursor.AddAttribute(new SGFProperty(key,value));
            return this;
        }

        /// <summary>
        /// byte property
        /// </summary>
        /// <param name="key">key of properties</param>
        /// <param name="value">pre-formated value</param>
        /// <returns></returns>
        public SgfBuilder p(string key, byte value)
        {
            _cursor.AddAttribute(new SGFProperty(key, value.ToString()));
            return this;
        }
        
        public SgfBuilder p(string key, float value)
        {
            _cursor.AddAttribute(new SGFProperty(key, value.ToString(CultureInfo.InvariantCulture)));
            return this; 
        }

        public SgfBuilder Text(string key, string comment)
        {
            string escaped = EscapedText(comment);
            return p(key, escaped);
        }

        /// <summary>
        ///see "Text"  http://www.red-bean.com/sgf/sgf4.html
        /// </summary>
        private static string EscapedText(string comment)
        {
            return comment.Replace("\\", "\\\\").Replace("]", "\\]");
        }

        /// <summary>
        ///see "Text"  http://www.red-bean.com/sgf/sgf4.html
        /// </summary>
        private static string EscapedTextToCompose(string comment)
        {
            return EscapedText(comment).Replace(":","\\:");
        }

        /// <summary>
        ///see "Text"  http://www.red-bean.com/sgf/sgf4.html
        /// Darft - missing Info
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public SgfBuilder Compose(string key, string val1, string val2)
        {
            p(key, string.Concat(EscapedTextToCompose(val1), ":", EscapedTextToCompose(val2)));
            return this;
        }

        public SgfBuilder Root()
        {
            _cursor = _root;
            return this;
        }

        public SgfBuilder Next()
        {
            _cursor = _cursor.NewNode();
            return this;
        }
        
        public SgfBuilder Fork(ForkHandler handler)
        {
            var fork = new SgfBuilder();
            handler(fork);
            _cursor.AddNode(fork.ToSgfTree());

            return this;
        }

        public SGFTree ToSgfTree()
        {
            return _root;
        }

    }
}