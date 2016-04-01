/*  Copyright (C) 2006-2009
 * 	This file is part of IGoEnchi project
 *	Please read ..\license.txt for conditions of distribution and use
 */ 

using System;
using System.IO;
using System.Text;

namespace IGoEnchi
{
	public class SGFWriter
	{
		public static void SaveGame(GoGame game, string path)
		{
			var writer = new StreamWriter(File.OpenWrite(path));
			SaveGame(game, writer);			
			writer.Close();
		}
		
		public static void SaveGame(GoGame game, StreamWriter writer)
		{
			var node = game.RootNode;

			writer.Write("(;GM[1]FF[4]SZ[" + game.BoardSize +
			             "]PW[" + game.Info.WhitePlayer +
			             "]PB[" + game.Info.BlackPlayer +
			             "]");			
			SaveNode(writer, node);
			
			writer.Write(")");			
		}			
		
		private static void SaveNode(StreamWriter writer, GoNode goNode)
		{			
			GoNode node = goNode;
			while (node != null)
			{
				if (!(node is GoRootNode))
				{
					writer.Write(";");
				}
				if (node is GoMoveNode)
				{
					var currentNode = node as GoMoveNode;										
					writer.Write(DecompilePointValue(currentNode.Stone));					
				}
				else if (node is GoSetupNode)
				{
					var currentNode = node as GoSetupNode;
					var builder = new StringBuilder();
					BitPlane stones;
					var currentBoard = currentNode.BoardCopy;
					Board lastBoard = null;
					if (currentNode.ParentNode != null)
					{
						lastBoard = currentNode.ParentNode.BoardCopy;
					}
					
					
					if (lastBoard == null)
					{
						stones = currentBoard.Black;
					}
					else
					{						
						stones = 
							currentBoard.Black - lastBoard.Black;
					}
					if (!stones.Empty())
					{
						builder.Append("AB" + DecompilePlaneValue(stones));
					}
					if (lastBoard == null)
					{
						stones = currentBoard.White;
					}
					else
					{
						stones = 
							currentBoard.White - lastBoard.White;
					}
					if (!stones.Empty())
					{
						builder.Append("AW" + DecompilePlaneValue(stones));
					}
					if (lastBoard != null)
					{
						stones = (lastBoard.White - currentBoard.White).And(
							lastBoard.Black - currentBoard.Black);
						if (!stones.Empty())
						{
							builder.Append("AE" + DecompilePlaneValue(stones));
						}
					}					
					if (currentNode.BlackToPlay.HasValue)
					{
						builder.Append("PL[");
						builder.Append(
							currentNode.BlackToPlay.Value ?	"B" : "W");
						builder.Append("]");
					}
					var move = builder.ToString();
					if (move != string.Empty)
					{						
						writer.Write(move);
					}
				}
				
				if (node.Comment != null)
				{
					var comment = 
						node.Comment.Replace("]", "\\]").Replace("\\", "\\\\");
					writer.Write("C[" + comment + "]");
				}
				if (node.Markup != null)
				{
					var builder = new StringBuilder();
					if (node.Markup.Labels.Count > 0)
					{		
						builder.Append("LB");
						foreach (var label in node.Markup.Labels)
						{
							builder.Append("[");
							builder.Append(DecompilePointValue(label.X, label.Y));
							builder.Append(":");
							builder.Append(label.Text);
							builder.Append("]");
						}
					}
					if (node.Markup.Marks.Count > 0)
					{														
						foreach (var mark in node.Markup.Marks)
						{
							switch (mark.MarkType)
							{
								case MarkType.Square:
									builder.Append("SQ[");
									break;
								case MarkType.Circle:
									builder.Append("CR[");
									break;
								case MarkType.Triangle:
									builder.Append("TR[");
									break;
								case MarkType.Mark:
									builder.Append("MA[");
									break;
								case MarkType.Selected:
									builder.Append("SL[");
									break;
							}							
							builder.Append(DecompilePointValue(mark.X, mark.Y));
							builder.Append("]");
						}
					}
					writer.Write(builder.ToString());
				}
					
				if (node.ChildNodes.Count == 1)
				{
					node = node.ChildNodes[0];					
				}
				else
				{
					foreach (var child in node.ChildNodes)
					{
						writer.Write('(');
						SaveNode(writer, child);
						writer.Write(')');
					}
					node = null;
				}
			}
		}
		
		private static string DecompilePlaneValue(BitPlane plane)
		{
			var result = new StringBuilder();
			for (byte i = 0; i < plane.Width; i++)
			{
				for (byte j = 0; j < plane.Height; j++)
				{
					if (plane[i, j] == true)
					{
						result.Append('[');
						result.Append(Convert.ToChar(i + 'a').ToString());
						result.Append(Convert.ToChar(j + 'a').ToString());
						result.Append(']');
					}
				}
			}
			return result.ToString();
		}
		
		private static string DecompilePointValue(byte x, byte y)
		{
			return Convert.ToChar(x + 'a').ToString() +
				Convert.ToChar(y + 'a').ToString();			
		}
		
		private static string DecompilePointValue(Stone stone)
		{
			var coords = 
				Convert.ToChar(stone.X + 'a').ToString() +
				Convert.ToChar(stone.Y + 'a').ToString();
			if (stone.IsBlack)
			{
				return "B[" + coords + "]";
			}
			else
			{
				return "W[" + coords + "]";
			}
		}
	}
}
