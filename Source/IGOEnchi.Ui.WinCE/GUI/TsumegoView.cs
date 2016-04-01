using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using IGOEnchi.GoGameLib.Models;
using IGOEnchi.GoGameLogic.Models;

namespace IGoEnchi
{
	/* for .NET CF 3.5
	public static class NullableExt
	{
		public static Nullable<B> Map<A, B>(this Nullable<A> source,
		                                 	Func<A, B> map)
		{
			return source.HasValue ? map(source.Value) :
									 null;
		}
	}*/
	
	public interface ITsumegoFormat
	{
		bool IsCorrectFor(GoGame tsumego);
		bool IsRightVariant(GoNode node);
	}
	
	public static class TsumegoFormat
	{		
		private static List<ITsumegoFormat> formats =
			new List<ITsumegoFormat>
			{
				new GoProblemsTsumego(),
				new DefaultTsumego()
			};
		
		public static ITsumegoFormat GetFormatFor(GoGame tsumego)
		{
			var format = formats.Find(f => f.IsCorrectFor(tsumego));
			return format != null ? format : new DefaultTsumego() as ITsumegoFormat;
		}

		private class GoProblemsTsumego: ITsumegoFormat
		{
			public bool IsCorrectFor(GoGame tsumego)
			{
				return IsRightVariant(tsumego.RootNode);
			}
		
			public bool IsRightVariant(GoNode node)
			{
				while (node.ChildNodes.Count == 1)
				{
					node = node.ChildNodes[0];
				}				
				
				if (!node.HasChildren)
				{
					return 
						node.Comment != null &&
						node.Comment.IndexOf("RIGHT") >= 0;
				}
				else
				{
					return node.ChildNodes.Exists(IsRightVariant);
				}
			}
		}
	
		private class DefaultTsumego: ITsumegoFormat
		{
			public bool IsCorrectFor(GoGame tsumego)
			{
				return true;
			}
			
			public bool IsRightVariant(GoNode node)
			{
				return true;
			}
		}
	}
	
	public class TsumegoStatistics
	{
		public int Total {get; private set;}
		public int Solved {get; private set;}
		
		public TsumegoStatistics()
		{
			
		}
		
		public void Right()
		{
			Total += 1;
			Solved += 1;
		}
		
		public void Wrong()
		{
			Total += 1;
		}
	}
	
	public class TsumegoView: GameBrowseView
	{		
		private Random random;
		private bool playerTurn;
		private int startMove;
		private string message = string.Empty;
		private ITsumegoFormat format;				
		private IEnumerator<GoGame> gamesEnum;		
		private bool ended;		
		private TsumegoStatistics statistics;

		private ToolBarButton startButton;
		private ToolBarButton backButton;
		private ToolBarButton nextButton;
		private ToolBarButton browseButton;
		private ToolBarButton commentButton;
		
		public TsumegoView(Size size, IEnumerator<GoGame> games, 
		                   GameRenderer renderer):
			base(size)
		{						
			if (!games.MoveNext())
			{
				throw new ArgumentException("No games found");
			}
			if (renderer == null)
			{
				throw new ArgumentException("Renderer cannot be null");
			}
			
			AllowNavigation = false;
			AllowCursor = true;
			
			Renderer = renderer;
			gamesEnum = games;
			
			GetProblem();			
				
			CreateCommentBox();
			format = TsumegoFormat.GetFormatFor(Game);
			
			random = new Random((int) (DateTime.Now.Ticks % Int32.MaxValue));									
			ToStart();							
		}
		
		protected override void EnableButtons()
		{
			startButton.Enabled = Game != null && Game.MoveNumber > startMove;
			backButton.Enabled = Game != null && Game.MoveNumber > startMove;
		}
		
		protected override void OnInit()
		{	
			startButton = new ToolBarButton()
			{
				ToolTipText = "To start"
			};
			
			backButton = new ToolBarButton()
			{
				ToolTipText = "Undo"
			};
			
			nextButton = new ToolBarButton() 
			{
				ToolTipText = "Next problem"
			};
			
			browseButton = new ToolBarButton()
			{
				ToolTipText = "Browse solution",
				Style = ToolBarButtonStyle.ToggleButton
			};
			
			commentButton = new ToolBarButton()
			{
				ToolTipText = "Expand comment",
				Style = ToolBarButtonStyle.ToggleButton
			};
			
			Tools = new Tool[] 
			{
				new Tool(browseButton, 
				         ConfigManager.GetIcon("observe"),
				         delegate {Invalidate();}),
				new Tool(commentButton, 
				         ConfigManager.GetIcon("comment"), 
				         OnCommentClick),
				new Tool(startButton, 
				         ConfigManager.GetIcon("to_start"), 
				         delegate {ToStart();}),
				new Tool(backButton, 
				         ConfigManager.GetIcon("back"), 
				         delegate {Back();}),
				new Tool(nextButton, 
				         ConfigManager.GetIcon("forward"), 
				         delegate {GetProblem();
				         		   ToStart();})				
			};
		}
		
		private bool Browsing 
		{
			get {return browseButton.Pushed;}
		}

		private void GetProblem()
		{
			if (!ended)
			{
				try
				{
					Game = gamesEnum.Current;						
					Game.ShowVariants = Variants.None;
					browseButton.Pushed = false;
				
					Cursor = new Point(Game.BoardSize / 2, 
	    				       		   Game.BoardSize / 2);
					
					ended = !gamesEnum.MoveNext();
					nextButton.Enabled = !ended;
				}
				catch (Exception exception)
				{
					MessageBox.Show(exception.Message,
					               	"Error",
						            MessageBoxButtons.OK,
									MessageBoxIcon.Exclamation, 
						            MessageBoxDefaultButton.Button1);
					ended = true;
					nextButton.Enabled = false;
				}
			}						
		}
		
		private void ToStart()
		{
			Game.UpdateBoard = false;	
			Game.ToMove(0);						
			while (Game.CurrentNode.ChildNodes.Count > 0 &&
			       Game.CurrentNode.ChildNodes[0] is GoSetupNode)
			{
				Game.ToMove(Game.CurrentNode.ChildNodes[0]);				
			}
			startMove = Game.MoveNumber;
			Game.UpdateBoard = true;	
			Game.Update();
			
			Invalidate();
			SetComment();
			playerTurn = true;
			message = string.Empty;
			
			EnableButtons();
			/*if (Game.CurrentNode is GoSetupNode)
			{
				var node = (Game.CurrentNode as GoSetupNode);
			 	message = 
					node.BlackToPlay.HasValue ? 
						(node.BlackToPlay.Value ? "Black to play" :
					 							  "White to play") :
						string.Empty;										
			}
			else
			{
				message = string.Empty;
			}*/
		}
		
		private void Back()
		{		
			Game.UpdateBoard = false;
			if (Game.MoveNumber != startMove)
			{
				if (playerTurn)
				{
					Game.ToPreviousMove();
				}
				Game.ToPreviousMove();
			}
			Game.UpdateBoard = true;
			Game.Update();
			Invalidate();
			SetComment();
			playerTurn = true;
			message = string.Empty;
			
			EnableButtons();
		}
		
		private void SetComment()
		{
			if (Game.CurrentNode.Comment != null)
			{
				comment.Text = Game.CurrentNode.Comment.
					Replace("RIGHT", "").Replace("CHOICE", "");
			}
			else
			{
				comment.Text = String.Empty;
			}
		}
		
		private void PlaceStone(Stone stone)
		{
			GoNode node = null;		
			foreach (var child in Game.CurrentNode.ChildNodes)
			{
				if (child is GoMoveNode)
				{
					if ((child as GoMoveNode).Stone.X == stone.X &&
					    (child as GoMoveNode).Stone.Y == stone.Y)
					{
						node = child;
						message = string.Empty;
						break;
					}
				}
			}
			if (node != null)
			{							
				Game.ToMove(node);
				if (Game.CurrentNode is GoMoveNode)
				{
					SoundUtils.Play("Stone");
				}
				var count = node.ChildNodes.Count;
				if (count > 0)
				{
					var variant = random.Next(0, count - 1);
					Game.ToMove(node.ChildNodes[variant]);
					if (Game.CurrentNode is GoMoveNode)
					{
						SoundUtils.Play("Stone");
					}
					playerTurn = true;
				}
				else
				{
					playerTurn = false;
				}
				
				if (!Game.CurrentNode.HasChildren)
				{
					if (format.IsRightVariant(Game.CurrentNode))
					{
						message = "Solved";						
					}
					else
					{
						message = "Wrong";						
					}					
				}
				
				Invalidate();
				SetComment();
			}
			else
			{				
				message = "No path";				
				Invalidate();
			}
		}
		
		protected override void CursorPress(Point cursor)
		{
			var stone = new Stone((byte) cursor.X, 
			                      (byte) cursor.Y, 
			                      true);
			PlaceStone(stone);
		}
		
		public override void MouseDown(MouseEventArgs args)
		{
			var stone = Renderer.GetCoords(args.X, args.Y, Game.BoardSize);
			PlaceStone(stone);
			EnableButtons();
		}
		
		public override void KeyDown(KeyEventArgs args)
		{
			base.KeyDown(args);
			if (!UseCursor && 
			    !ConfigManager.Settings.KeepCursor &&
			    args.KeyCode == Keys.Left)
			{
				Back();
			}
		}
		
		public override void Paint(PaintEventArgs args)
		{					
			Renderer.StartRendering(BoardPosition);
			Game.Render(Renderer);
			
			if (Browsing)
			{
				foreach (var node in Game.CurrentNode.ChildNodes)
				{
					if (node is GoMoveNode)
					{
						var stone = (node as GoMoveNode).Stone;
						Renderer.DrawColoredMark(
								format.IsRightVariant(node) ? Color.Green : Color.Red, 
								stone.X, stone.Y);					
					}				
				}
			}
				
			if (UseCursor)
			{
				Renderer.DrawCursor((byte) Cursor.X, (byte) Cursor.Y);
			}
			if (message != string.Empty)
			{
				Renderer.DrawMessage(message);
			}
			if (ConfigManager.Settings.KeepCursor || UseCursor)
			{
				Renderer.DrawCursor((byte) Cursor.X, (byte) Cursor.Y);
			}
			Renderer.FinishRendering(args.Graphics);
		}		
	}
}