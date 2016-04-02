/*  Copyright (C) 2006-2008 Valentin Kraevskiy
 * 	This file is part of IGoEnchi project
 *	Please read ..\license.txt for conditions of distribution and use
 */

using System.Collections.Generic;
using System.Linq;

namespace IGOEnchi.SmartGameLib.models
{
    public class SGFTree
    {
        public SGFTree()
        {
            ChildNodes = new List<SGFTree>();
            Properties = new List<SGFProperty>();
        }

        public SGFTree Parent { get; private set; }

        public List<SGFTree> ChildNodes { get; private set; }

        public List<SGFProperty> Properties { get; private set; }

        public bool HasContent
        {
            get
            {
                return Properties.Any();   
            }
        }

        public SGFTree NewNode()
        {
            var node = new SGFTree();
            AddNode(node);
            return node;
        }

        public void AddNode(SGFTree node)
        {
            ChildNodes.Add(node);
            node.Parent = this;
        }

        public void RemoveNode(SGFTree node)
        {
            ChildNodes.Remove(node);
        }

        public void AddAttribute(SGFProperty property)
        {
            Properties.Add(property);
        }

        public void RemoveAttribute(SGFProperty property)
        {
            Properties.Remove(property);
        }
    }
}