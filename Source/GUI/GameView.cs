using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
 
namespace IGoEnchi
{			
	public class GameInfoDialog: IGSDialog
	{			
		private NumericUpDown boardSizeUpDown;
		private TextBox whitePlayerTextBox;
		private TextBox blackPlayerTextBox;
		private TextBox komiTextBox;
		private NumericUpDown handicapUpDown;
		
		public GameInfoDialog(GameInfo info, bool editBoardSize)
		{									
			if (editBoardSize)
			{							
				boardSizeUpDown = new NumericUpDown()
				{
					Minimum = 2,
					Maximum = 19,
					Value = 19,					
				};				
			}
						
			whitePlayerTextBox = new TextBox()
			{							
				Text = info.WhitePlayer
			};				
			blackPlayerTextBox = new TextBox()
			{							
				Text = info.BlackPlayer
			};
			handicapUpDown = new NumericUpDown()
			{							
				Minimum = 0,
				Maximum = 100,
				Value = info.Handicap,				
			};
			komiTextBox = new TextBox()
			{				
				Text = ConfigManager.FloatToString(info.Komi)
			};
			
			foreach (var item in new Control[] 
			         {
			         	boardSizeUpDown, whitePlayerTextBox, blackPlayerTextBox,
			         	handicapUpDown, komiTextBox
			         })
			{
				Layout.Resize(item);
			}
			
			Layout.Bind(
				Layout.PropertyTable(
					editBoardSize ? Layout.Label("Board size") : null, 
						boardSizeUpDown,
					Layout.Label("White"), whitePlayerTextBox,
					Layout.Label("Black"), blackPlayerTextBox,
					Layout.Label("Handicap"), handicapUpDown,
					Layout.Label("Komi"), komiTextBox),
				this);						
		}
		
		public byte BoardSize
		{
			get {return (byte) 
					(boardSizeUpDown == null ?
					 0 : (int) boardSizeUpDown.Value);}
		}
		
		public GameInfo GameInfo
		{
			get 
			{
				var komi = 0.0f;				
				try
				{
					komi = ConfigManager.StringToFloat(komiTextBox.Text);
				}
				catch (Exception) {}
				
				return new GameInfo(blackPlayerTextBox.Text,
		        	                whitePlayerTextBox.Text,
		            	            (int) handicapUpDown.Value,
		            	            komi);
			}
		}
	}
			
	public class GameView: FormView
	{
		public GoGame Game {get; protected set;}
		public GameRenderer Renderer {get; set;}
		public Rectangle BoardPosition {get; set;}
		public Point Cursor {get; set;}
		public bool UseCursor {get; private set;}
		
		protected bool AllowCursor {get; set;}
		protected bool AllowNavigation {get; set;}
		
		public GameView(Size size, GoGame game, GameRenderer renderer): base(size)
		{
			if ((game == null) || (renderer == null))
			{
				throw new ArgumentException("GoGame and Renderer arguments cannot be null");
			}
			
			Game = game;			
			Renderer = renderer;	
			
			Cursor = new Point(Game.BoardSize / 2, 
	    				       Game.BoardSize / 2);
			
			AllowCursor = true;
			AllowNavigation = true;
			
			OnResize();			
			EnableButtons();
		}
		
		protected GameView(Size size): base(size)
		{
			if (Game != null)
			{
				Cursor = new Point(Game.BoardSize / 2, 
	    					       Game.BoardSize / 2);
			}
			
			AllowCursor = true;
			AllowNavigation = true;
			
			OnResize();
			EnableButtons();
		}
		
		protected virtual void EnableButtons() {}
		
		protected override void OnResize()
		{
			BoardPosition = new Rectangle(0, 0, Size.Width, Size.Height);
		}
		
		public override void Paint(PaintEventArgs args)
		{								
			Renderer.StartRendering(BoardPosition);
			Game.Render(Renderer);
			if (AllowCursor && (ConfigManager.Settings.KeepCursor || UseCursor))
			{
				Renderer.DrawCursor((byte) Cursor.X, (byte) Cursor.Y);
			}
			Renderer.FinishRendering(args.Graphics);
		}		
		
		protected virtual void CursorPress(Point cursor)
		{			
			var black =
				Game.CurrentNode is GoMoveNode ? 
					!(Game.CurrentNode as GoMoveNode).Stone.IsBlack :
					(Game.CurrentNode as GoSetupNode).BlackToPlay;
			var stone = new Stone((byte) cursor.X, 
			                      (byte) cursor.Y, 
			                      black ?? true);
			if (Game.CanPlace(stone))
			{
				Game.PlaceStone(stone);
				SoundUtils.Play("Stone");
			}
		}
				
	    public override void KeyDown(KeyEventArgs args) 
		{	
	    	if (AllowCursor && (ConfigManager.Settings.KeepCursor || UseCursor))
	    	{
	    		switch (args.KeyCode)
	    		{
	    			case Keys.Left:
	    				if (Cursor.X > 0)
	    				{
	    					Cursor = new Point(Cursor.X - 1, Cursor.Y);
	    				}
	    				break;
	    			case Keys.Right:
	    				if (Cursor.X < Game.BoardSize - 1)
	    				{
	    					Cursor = new Point(Cursor.X + 1, Cursor.Y);
	    				}
	    				break;
	    			case Keys.Down:
	    				if (Cursor.Y < Game.BoardSize - 1)
	    				{
	    					Cursor = new Point(Cursor.X, Cursor.Y + 1);
	    				}
	    				break;
	    			case Keys.Up:
	    				if (Cursor.Y > 0)
	    				{
	    					Cursor = new Point(Cursor.X, Cursor.Y - 1);
	    				}
	    				break;
	    			case Keys.Enter:	    				
	    				CursorPress(Cursor);
	    				UseCursor = false;
	    				break;
	    		}
	    	}
	    	else
	    	{
	    		switch (args.KeyCode)
	    		{
	    			case Keys.Left:
	    				if (AllowNavigation)
	    				{
	    					Game.ToPreviousMove();
	    				}
	    				break;
	    			case Keys.Right:
						if (AllowNavigation)
	    				{    					
		    				Game.ToNextMove();
						}
	    				break;
	    			case Keys.Down:
	    				if (AllowNavigation)
	    				{
		    				Game.ToNextVariation();
	    				}
	    				break;
	    			case Keys.Up:
	    				if (AllowNavigation)
	    				{
	    					Game.ToPreviousVariation();
	    				}
	    				break;
	    			case Keys.Enter:
	    				if (AllowCursor)
	    				{
		    				UseCursor = true;	    				
	    				}
	    				break;
	    		}
	    	}
			Invalidate();
			EnableButtons();
		}
	}
	
	public class GameBrowseView: GameView
	{
		protected TextBox comment;
		
		private ToolBarButton startButton;
		private ToolBarButton endButton;
		private ToolBarButton forwardButton;
		private ToolBarButton backButton;
		
		public GameBrowseView(Size size, 
		                      GoGame game, GameRenderer renderer): 
			base(size, game, renderer)
		{	
			CreateCommentBox();						
			AllowCursor = false;
		}
		
		public GameBrowseView(Size size): base(size) 
		{								
			AllowCursor = false;			
		}
		
		protected void CreateCommentBox()
		{
			comment = new TextBox();
			comment.ReadOnly = true;
			comment.Multiline = true;
			comment.ScrollBars = ScrollBars.Vertical;
			comment.KeyDown += delegate {};
			
			comment.Height = Size.Height - Size.Width;			
			comment.Dock = DockStyle.Bottom;
			comment.Text = Game.CurrentNode.Comment;
			Font oldFont = comment.Font;
			comment.Font = new Font(ConfigManager.Settings.DefaultFontName, 8, FontStyle.Regular);
			
			Controls.Add(comment);				
		}
		
		protected override void OnInit()
		{			
			startButton = new ToolBarButton() 
			{
				ToolTipText = "To start",				
			};
			
			backButton = new ToolBarButton() 
			{
				ToolTipText = "Back"
			};
			
			forwardButton = new ToolBarButton()
			{
				ToolTipText = "Forward"
			};
			
			endButton  = new ToolBarButton() 
			{
				ToolTipText = "To end"
			};
			
			Tools = new Tool[] 
			{
				new Tool(new ToolBarButton() {ToolTipText = "Expand comment",
				         					Style = ToolBarButtonStyle.ToggleButton}, 
				         	 ConfigManager.GetIcon("comment"), 
				         	 OnCommentClick),
				new Tool(startButton,
				         ConfigManager.GetIcon("to_start"), 
				         OnStartClick),
				new Tool(backButton, 
				         ConfigManager.GetIcon("back"), 
				         OnBackClick),
				new Tool(forwardButton, 
				         ConfigManager.GetIcon("forward"), 
				         OnForwardClick),
				new Tool(endButton, 
				         ConfigManager.GetIcon("to_end"), 
				         OnEndClick)
			};
		}				
		
		protected override void EnableButtons()
		{
			startButton.Enabled = Game.MoveNumber > 0;
			backButton.Enabled = Game.MoveNumber > 0;
			endButton.Enabled = Game.CurrentNode.HasChildren;
			forwardButton.Enabled = Game.CurrentNode.HasChildren;
		}
		
		protected void OnStartClick(object sender, EventArgs args)
		{
			Game.ToMove(0);		
			comment.Text = Game.CurrentComment;
			EnableButtons();
			Invalidate();
		}
		
		private void OnBackClick(object sender, EventArgs args)
		{
			Game.ToPreviousMove();
			comment.Text = Game.CurrentComment;
			EnableButtons();
			Invalidate();
		}
		
		private void OnForwardClick(object sender, EventArgs args)
		{
			Game.ToNextMove();
			comment.Text = Game.CurrentComment;
			EnableButtons();
			Invalidate();
		}
		
		protected void OnEndClick(object sender, EventArgs args)
		{
			while (Game.ToNextMove());
			comment.Text = Game.CurrentComment;
			EnableButtons();
			Invalidate();
		}
		
		protected void OnCommentClick(object sender, EventArgs args)
		{
			if ((sender as ToolBarButton).Pushed) 			
			{
				comment.Height = Size.Height;
			}
			else
			{
				comment.Height = Size.Height - Size.Width;
			}
		}
		
		public override void KeyDown(KeyEventArgs args)
		{
			var node = Game.CurrentNode;
			base.KeyDown(args);						
			if (node != Game.CurrentNode)
			{
				comment.Text = Game.CurrentComment;
			}
		}
		
		protected override void OnResize()
		{
			base.OnResize();
			if (comment != null)
			{
				OnCommentClick(Tools[0].Button, EventArgs.Empty);
			}
		}		
		
		public override void MouseDown(MouseEventArgs args)
		{
			if (Game.CurrentNode is GoMoveNode)
			{
				var node = Game.CurrentNode as GoMoveNode;
				if (node.ParentNode != null)
				{
					var stone = Renderer.GetCoords(args.X, args.Y, Game.BoardSize);
					foreach (var child in node.ParentNode.ChildNodes)
					{
						if (child is GoMoveNode && child != node &&
						    stone.AtPlaceOf((child as GoMoveNode).Stone))
						{
							Game.ToPreviousMove(false);
							Game.ToMove(child);
							Game.Update();
							comment.Text = Game.CurrentComment;
							Invalidate();
							return;
						}
					}
				}
			}
		}
		
	}
	
	public abstract class EditMode
	{
		private bool allowsDrag = true;
		
		public bool AllowsDrag
		{
			get {return allowsDrag;}
			protected set {allowsDrag = value;}
		}
		
		public abstract void Edit(GoGame game, byte x, byte y);
		
		protected void ClearMarkup(GoGame game, byte x, byte y)
		{
			if (game.CurrentNode.Markup != null)
			{
				game.CurrentNode.Markup.Marks.RemoveAll(
				delegate(Mark mark) 
				{
					return mark.X == x && mark.Y == y;
				});
				
				game.CurrentNode.Markup.Labels.RemoveAll(
				delegate(TextLabel label) 
				{
					return label.X == x && label.Y == y;
				});
			}
		}
	}
	
	public class MoveMode: EditMode
	{
		public MoveMode() 
		{
			AllowsDrag = false;
		}
				
		public override void Edit(GoGame game, byte x, byte y)
		{
			var stone = new Stone(x, y, true);
			stone.IsBlack = game.BlackToPlay();
			if (game.CanPlace(stone))
			{				
				if (!game.Continue(stone))
				{
					game.PlaceStone(stone);
				}
				SoundUtils.Play("Stone");
			}
		}						
	}
	
	public class AddStoneMode: EditMode
	{
		private bool black;
		
		public AddStoneMode(bool black)
		{
			this.black = black;	
		}
		
		public override void Edit(GoGame game, byte x, byte y)
		{
			game.AddStone(new Stone(x, y, black));
		}		
	}
	
	public class RemoveStoneMode: EditMode
	{			
		public RemoveStoneMode() {}
				
		public override void Edit(GoGame game, byte x, byte y)
		{						
			game.RemoveStone(new Stone(x, y, true));			
		}
	}
	
	public class ClearMarkupMode: EditMode
	{			
		public ClearMarkupMode() {}
				
		public override void Edit(GoGame game, byte x, byte y)
		{			
			ClearMarkup(game, x, y);
		}
	}
	
	public class MarkupMode: EditMode
	{
		private MarkType markType;
		
		public MarkupMode(MarkType markType)
		{
			this.markType = markType;
		}
		
		public override void Edit(GoGame game, byte x, byte y)
		{
			ClearMarkup(game, x, y);
			
			var node = game.CurrentNode;			
			node.EnsureMarkup();
			node.Markup.Marks.Add(new Mark(x, y, markType));
		}
	}
	
	public class LabelMode: EditMode
	{		
		public LabelMode() 
		{
			AllowsDrag = false;
		}
		
		public override void Edit(GoGame game, byte x, byte y)
		{												
			var input = new StringInput();
			var dialog = new ValueDialog<StringInput>("Label: ", input);
			if (dialog.ShowDialog() == DialogResult.OK && input.Text != string.Empty)
			{
				ClearMarkup(game, x, y);
				var node = game.CurrentNode;			
				node.EnsureMarkup();
				node.Markup.Labels.Add(new TextLabel(x, y, input.Text));
			}
		}
	}
	
	public enum EditAction {Change, Drag};
	
	public class GameEditView: GameBrowseView
	{
		private EditMode editMode;		
		private ContextMenu modeMenu;
		private ContextMenu actionMenu;
		
		private MenuItem blackItem;
		private MenuItem whiteItem;
		private MenuItem passItem;
		
		private bool drag = false;
		
		private Stone lastStone = new Stone();
		
		private ToolBarButton startButton;
		private ToolBarButton endButton;
		
		public GameEditView(Size size, 
		                    GoGame game, GameRenderer renderer): 
			base(size, game, renderer)
		{			
			editMode = new MoveMode();						
			comment.ReadOnly = false;
			comment.TextChanged += delegate 
			{
				game.CurrentNode.Comment = comment.Text;
			};
			var names = new string[]
			{
				"Move",
				"Add black",
				"Add white",
				"Remove stone",
				"Circle",
				"Square",
				"Triangle",
				"Mark",
				"Select",
				"Label",
				"Clear markup"
			};
			
			var actions = new EventHandler[]
			{
				delegate {editMode = new MoveMode();},
				delegate {editMode = new AddStoneMode(true);},
				delegate {editMode = new AddStoneMode(false);},
				delegate {editMode = new RemoveStoneMode();},
				delegate {editMode = new MarkupMode(MarkType.Circle);},
				delegate {editMode = new MarkupMode(MarkType.Square);},
				delegate {editMode = new MarkupMode(MarkType.Triangle);},
				delegate {editMode = new MarkupMode(MarkType.Mark);},
				delegate {editMode = new MarkupMode(MarkType.Selected);},
				delegate {editMode = new LabelMode();},
				delegate {editMode = new ClearMarkupMode();}
			};
			
			var breaks = new List<int>() {1, 4, 10};
			
			for (var i = 0; i < names.Length; i++)
			{
				if (breaks.Count > 0 && i == breaks[0])
				{
					modeMenu.MenuItems.Add(
						new MenuItem() {Text = "-"});
					breaks.RemoveAt(0);
				}
				var item = new MenuItem() {Text = names[i]};
				item.Click += actions[i];
				modeMenu.MenuItems.Add(item);				
			}
						
			modeMenu.MenuItems[0].Checked = true;
			
			for (var j = 0; j < modeMenu.MenuItems.Count; j++)
			{
				var item = modeMenu.MenuItems[j];
				if (item.Text != "-")
				{
					item.Click += delegate
					{					
						for (var i = 0; i < modeMenu.MenuItems.Count; i++)
						{
							modeMenu.MenuItems[i].Checked = false;
						}
						item.Checked = true;
					};
				}
			}
			
			var menuItem = new MenuItem() {Text = "Game Info"};
			menuItem.Click += OnGameInfoClick;

			actionMenu.MenuItems.Add(menuItem);
			actionMenu.MenuItems.Add(new MenuItem() {Text = "-"});

			menuItem = new MenuItem() {Text = "Delete node"};
			menuItem.Click += OnDeleteClick;
			
			actionMenu.MenuItems.Add(menuItem);
			actionMenu.MenuItems.Add(new MenuItem() {Text = "-"});
			
			blackItem = new MenuItem() {Text = "Black to play"};
			whiteItem = new MenuItem() {Text = "White to play"};
			passItem = new MenuItem() {Text = "Pass"};			
			
			blackItem.Click += delegate 
			{
				if (Game.CurrentNode is GoSetupNode)
				{
					(Game.CurrentNode as GoSetupNode).BlackToPlay = true;
					SetPlayer();
				}
			};
			
			whiteItem.Click += delegate 
			{
				if (Game.CurrentNode is GoSetupNode)
				{
					(Game.CurrentNode as GoSetupNode).BlackToPlay = false;
					SetPlayer();
				}
			};
			
			passItem.Click += delegate 
			{
				Game.PlaceStone(new Stone(20, 20, Game.BlackToPlay()));
				Invalidate();
			};
			
			actionMenu.MenuItems.Add(blackItem);
			actionMenu.MenuItems.Add(whiteItem);
			
			actionMenu.MenuItems.Add(new MenuItem() {Text = "-"});
			
			actionMenu.MenuItems.Add(passItem);
			
			actionMenu.MenuItems.Add(new MenuItem() {Text = "-"});
			
			var commentItem = new MenuItem() {Text = "Expand comment"};
			commentItem.Click += delegate (object sender, EventArgs args)
			{
				if (!commentItem.Checked)
				{
					comment.Height = Size.Height;
				}
				else
				{
					comment.Height = Size.Height - Size.Width;
				}				
				commentItem.Checked = !commentItem.Checked;
			};
			actionMenu.MenuItems.Add(commentItem);
			
			SetPlayer();
			AllowCursor = true;
		}
		
		private void SetPlayer()
		{							
			if (Game.CurrentNode is GoSetupNode)
			{
			    if ((Game.CurrentNode as GoSetupNode).BlackToPlay ?? true)
				{
					blackItem.Checked = true;
					whiteItem.Checked = false;
				}
				else
				{
					blackItem.Checked = false;
					whiteItem.Checked = true;
				}
			}
			else
			{
				blackItem.Checked = false;
				whiteItem.Checked = false;
			}			
		}
		
		public override void KeyDown(KeyEventArgs args)
		{
			base.KeyDown(args);
			SetPlayer();
		}
		
		protected new void OnStartClick(object sender, EventArgs args)
		{
			base.OnStartClick(sender, args);
			SetPlayer();
		}
		
		protected new void OnEndClick(object sender, EventArgs args)
		{
			base.OnEndClick(sender, args);
			SetPlayer();
		}
		
		protected override void EnableButtons()
		{
			startButton.Enabled = Game.MoveNumber > 0;
			endButton.Enabled = Game.CurrentNode.HasChildren;
		}
		
		protected override void OnInit()
		{			
			modeMenu = new ContextMenu();
			actionMenu = new ContextMenu();
		
			startButton = new ToolBarButton() 
			{
				ToolTipText = "To start",				
			};
			
			endButton  = new ToolBarButton() 
			{
				ToolTipText = "To end"
			};
			
			Tools = new Tool[] 
			{
				new Tool(new ToolBarButton() {ToolTipText = "Mode",
				         					  Style = ToolBarButtonStyle.DropDownButton,
				         					  DropDownMenu = modeMenu}, 	
				     	 ConfigManager.GetIcon("edit"), 
				     	 delegate {}),
				new Tool(new ToolBarButton() {ToolTipText = "Edit",
				         					  Style = ToolBarButtonStyle.DropDownButton,
				         					  DropDownMenu = actionMenu}, 	
				     	 ConfigManager.GetIcon("game"), 
				     	 delegate {}),				
				new Tool(startButton, 
				         ConfigManager.GetIcon("to_start"), 
				         OnStartClick),
				new Tool(endButton, 
				         ConfigManager.GetIcon("to_end"), 
				         OnEndClick)				
			};
		}
		
		private void OnGameInfoClick(object sender, EventArgs args)
		{
			var dialog = new GameInfoDialog(Game.Info, false);
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				Game.Info = dialog.GameInfo;
			}
		}
		
		private void OnDeleteClick(object sender, EventArgs args)
		{
			if (!(Game.CurrentNode is GoRootNode))
			{
				Game.CurrentNode.ParentNode.RemoveNode(Game.CurrentNode);
				Game.ToPreviousMove();
				Invalidate();
				SetPlayer();
				comment.Text = Game.CurrentNode.Comment ?? string.Empty;
			}
		}
		
		private void Edit(byte x, byte y)
		{
			editMode.Edit(Game, x, y);
			Invalidate();
			SetPlayer();
			comment.Text = Game.CurrentNode.Comment ?? string.Empty;
			EnableButtons();
		}
		
		public override void MouseDown(MouseEventArgs args)
		{
			Stone stone = Renderer.GetCoords(args.X, args.Y, Game.BoardSize);
			if (stone.X < Game.BoardSize && stone.Y < Game.BoardSize)
			{
				Edit(stone.X, stone.Y);
				if (editMode.AllowsDrag)
				{
					drag = true;
					lastStone = stone;
				}				
			}						
		}
		
		public override void MouseUp(MouseEventArgs args)
		{
			drag = false;
		}
		
		public override void MouseMove(MouseEventArgs args)
		{
			if (drag)
			{
				Stone stone = Renderer.GetCoords(args.X, args.Y, Game.BoardSize);
				if (stone.X < Game.BoardSize && stone.Y < Game.BoardSize)
				{
					if (!stone.SameAs(lastStone))
					{
						Edit(stone.X, stone.Y);						
					}
				}
			}
		}
	}
}

