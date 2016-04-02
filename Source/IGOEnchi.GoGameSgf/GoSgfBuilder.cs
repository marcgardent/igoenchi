/*  Copyright (C) 2006-2009
 * 	This file is part of IGoEnchi project
 *	Please read ..\license.txt for conditions of distribution and use
 */

using System.Collections.Generic;
using System.IO;
using System.Text;
using IGOEnchi.GoGameLogic.Models;
using IGOEnchi.SmartGameLib.models;

namespace IGOEnchi.GoGameSgf
{

    /// <summary>
    /// Convert GoGame to SGFTree
    /// </summary>
    public class GoSgfBuilder
    {
        private readonly GoGame g; 

        public GoSgfBuilder(GoGame goGame)
        {
            g = goGame;
            
        }

        public SGFTree ToSGFTree()
        {
            var bb = new GoNodeSgfBuilder(g.RootNode);
            bb.BuidInfoGame(g);
            
            return bb.ToSGFTree();
        }
    }
}