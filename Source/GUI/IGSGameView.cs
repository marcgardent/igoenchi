using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

namespace IGoEnchi
{
	public class IGSGameView: GameView
	{
		private Label timeLabel;
		private Timer timer;		
		private bool gameEnded;	
		private int lastMove;
		
		protected ObservedGame IGSGame
		{
			get {return Game as ObservedGame;}
		}
		
		protected bool Ended
		{
			get {return gameEnded;}
		}
		
		public IGSGameView(Size size, 
		                   ObservedGame game, 
		                   GameRenderer renderer,
		                   IGSGameListener gameListener): base(size, game, renderer)
		{		
			if (gameListener == null)
			{
				throw new ArgumentException("IGSGameListener argument cannot be null");
			}						
			
			gameListener.GameChanged += new IGSGameEventHandler(GameChanged);
			gameListener.GameEnded += new IGSGameEventHandler(GameEnded);

			timeLabel = new Label();
			
			SizeF textSize;
			using (var graphics = CreateGraphics())
			{
				textSize = graphics.MeasureString("Refresh", timeLabel.Font);
			}			
														
			timeLabel.Height = (int) textSize.Height;
			timeLabel.Dock = DockStyle.Bottom;
						
			timer = new Timer()
			{
				Interval = 1000,
				Enabled = true
			};
			timer.Tick += new EventHandler(OnTick);
									
			BoardPosition = new Rectangle(0, 0, Size.Width, Size.Height - timeLabel.Height);
			
			Controls = new List<Control>()
			{
				timeLabel
			};
			
			lastMove = game.MoveNumber;
		}
		
		protected override void OnResize()
		{
			if (timeLabel != null)
			{
				BoardPosition = new Rectangle(0, 0, Size.Width, Size.Height - timeLabel.Height);			
			}
		}
		
		private void OnTick(object sender, EventArgs args)
		{							
			if (IGSGame.MoveNumber % 2 != 0 )
			{
				if (IGSGame.WhiteTime > TimeSpan.Zero)
				{
					IGSGame.WhiteTime -= TimeSpan.FromSeconds(1);
				}
			}
			else
			{
				if (IGSGame.BlackTime > TimeSpan.Zero)
				{
					IGSGame.BlackTime -= TimeSpan.FromSeconds(1);
				}
			}			
			SetTimeLabel();
		}

		private void UpdateView(object sender, EventArgs args)
		{
			Invalidate();
			SetTimeLabel();
			
			if (IGSGame.MovesMade > lastMove)
			{
				SoundUtils.Play("Stone");
			}
			lastMove = IGSGame.MovesMade;
		}
		
		private void SetTimeLabel()
		{
			var blackByouyomi = "";
			var whiteByouyomi = "";			
			if (IGSGame.BlackByouyomiStones >= 0)
			{
				blackByouyomi = "(" + IGSGame.BlackByouyomiStones + ")";
			}
			if (IGSGame.WhiteByouyomiStones >= 0)
			{
				whiteByouyomi = "(" + IGSGame.WhiteByouyomiStones + ")";
			}
			timeLabel.Text = "B:" + IGSGame.BlackTime + blackByouyomi +
				             " W:" + IGSGame.WhiteTime + whiteByouyomi +
				             " Cp: " + IGSGame.BlackCaptures +
				             "/" + IGSGame.WhiteCaptures;
		}		
				
		private void GameChanged(object sender, IGSGameEventArgs args)
		{
			if (IGSGame == args.Game)
			{				
				Container.BaseForm.Invoke(new EventHandler(UpdateView));
				EventHandler enableButtons = delegate
				{
					EnableButtons();
				};
				Container.BaseForm.Invoke(enableButtons);
			}
		}
		
		private void StopTimer(object sender, EventArgs args)
		{
			timer.Enabled = false;
			UpdateView(this, EventArgs.Empty);
		}
		
		private void GameEnded(object sender, IGSGameEventArgs args)
		{
			if (IGSGame == args.Game)
			{
				Container.BaseForm.Invoke(new EventHandler(StopTimer));				
				gameEnded = true;
			}
		}
	}
	
	public class ObservedGameView: IGSGameView
	{
		private IGSObserver gameObserver;		
		
		private ToolBarButton startButton;
		private ToolBarButton endButton;
		private ToolBarButton forwardButton;
		private ToolBarButton backButton;
		
		public event EventHandler KibitzRequested;
		
		public ObservedGameView(Size size, 
		                        ObservedGame game, 
		                        GameRenderer renderer,
		                        IGSObserver gameObserver): base(size, game, renderer, gameObserver)
		{												
			this.gameObserver = gameObserver;						
			AllowNavigation = true;			
			AllowCursor = true;
		}	
		
		protected override void EnableButtons()
		{
			startButton.Enabled = Game.MoveNumber > 0;
			backButton.Enabled = Game.MoveNumber > 0;
			endButton.Enabled = Game.CurrentNode.HasChildren;
			forwardButton.Enabled = Game.CurrentNode.HasChildren;
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
				new Tool(new ToolBarButton() {Style = ToolBarButtonStyle.ToggleButton,
				         					  ToolTipText = "Kibitz"}, 
				         ConfigManager.GetIcon("talk"), 
				         OnKibitzClick),
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
		
		public int GameNumber
		{
			get {return IGSGame.GameNumber;}
		}
		
		private void OnStartClick(object sender, EventArgs args)
		{
			IGSGame.ToMove(0);
			EnableButtons();
			Invalidate();
		}
		
		private void OnBackClick(object sender, EventArgs args)
		{
			IGSGame.ToPreviousMove();
			EnableButtons();
			Invalidate();
		}
		
		private void OnForwardClick(object sender, EventArgs args)
		{
			IGSGame.ToNextMove();
			EnableButtons();
			Invalidate();
		}
		
		private void OnEndClick(object sender, EventArgs args)
		{
			IGSGame.ToMove(IGSGame.MovesMade);
			EnableButtons();
			Invalidate();
		}
		
		private void OnKibitzClick(object sender, EventArgs args)
		{
			if (!Ended)
			{		
				OnKibitzRequested(EventArgs.Empty);
			}
		}
		
		protected void OnKibitzRequested(EventArgs args)
		{
			if (KibitzRequested != null)
			{
				KibitzRequested(this, args);
			}
		}
		
		public override void OnClose(EventArgs args)
		{
			if (!Ended)
			{
				gameObserver.Unobserve(IGSGame);
			}
			base.OnClose(args);
		}				
		
		private ChatView FindKibitzView()
		{
			return Container.FindView("Kibitz in game " + 
			                          IGSGame.GameNumber) as ChatView;			
		}
		
		public void CoordsToKibitz(int x, int y)
		{
			var coords = 
				((char)('A' + ('A' + x < 'I' ? x : x + 1))).ToString() +
				(Game.BoardSize - y).ToString();
			var view = FindKibitzView();
			if (view != null)
			{
				view.AddInput(coords);
			}
			else
			{
				OnKibitzRequested(EventArgs.Empty);
				view = FindKibitzView();
				if (view != null)
				{
					view.AddInput(coords);
				}
			}
		}
		
		public override void MouseDown(MouseEventArgs args)
		{
			var stone = Renderer.GetCoords(args.X, args.Y, Game.BoardSize);
			
			if (stone.X >= 0 && stone.Y >= 0 &&
			    stone.X < Game.BoardSize && stone.Y < Game.BoardSize)
			{
				CoordsToKibitz(stone.X, stone.Y);
			}
		}
		
		protected override void CursorPress(Point cursor)
		{
			CoordsToKibitz(cursor.X, cursor.Y);
		}
	}		
	
	public class PlayedGameView: IGSGameView
	{
		private IGSPlayer gamePlayer;
		private ContextMenu menu;		
		
		public event EventHandler ChatRequested;
		
		public string OpponentName
		{
			get 
			{
				return gamePlayer.GetOpponentName(IGSGame);
			}
		}
		
		public PlayedGameView(Size size, 
		                        ObservedGame game, 
		                        GameRenderer renderer,
		                        IGSPlayer gamePlayer): base(size, game, renderer, gamePlayer)
		{			
			this.gamePlayer = gamePlayer;
				
			AllowNavigation = false;			
			AllowCursor = true;
			
			MenuItem menuItem = new MenuItem() {Text = "Pass"};
			menuItem.Click += new EventHandler(OnPassClick);
			menu.MenuItems.Add(menuItem);
			
			menuItem = new MenuItem() {Text = "Ask undo"};
			menuItem.Click += new EventHandler(OnAskUndoClick);
			menu.MenuItems.Add(menuItem);
			
			menuItem = new MenuItem() {Text = "Undo"};
			menuItem.Click += new EventHandler(OnUndoClick);
			menu.MenuItems.Add(menuItem);
			
			menuItem = new MenuItem() {Text = "Done"};
			menuItem.Click += new EventHandler(OnDoneClick);
			menu.MenuItems.Add(menuItem);
			
			menuItem = new MenuItem() {Text = "Adjourn"};
			menuItem.Click += new EventHandler(OnAdjournClick);
			menu.MenuItems.Add(menuItem);
			
			menuItem = new MenuItem() {Text = "Resign"};
			menuItem.Click += new EventHandler(OnResignClick);
			menu.MenuItems.Add(menuItem);						
			
			menuItem = new MenuItem() {Text = "Handicap"};
			menuItem.Click += new EventHandler(OnHandicapClick);
			menu.MenuItems.Add(menuItem);						
		}
		
		protected override void OnInit()
		{
			menu = new ContextMenu();
			
			Tools = new Tool[] 
			{
				new Tool(new ToolBarButton() {ToolTipText = "Say"}, 
				         ConfigManager.GetIcon("talk"), 
				         OnSayClick),
				new Tool(new ToolBarButton() {ToolTipText = "Game",
				         					  Style = ToolBarButtonStyle.DropDownButton,
				         					  DropDownMenu = menu}, 	
				     	 ConfigManager.GetIcon("game"), 
				     	 delegate {})
			};
		}
		
		private void PlaceStone(int x, int y)
		{
			if (!Ended && x >= 0 && y >= 0 &&
			    x < Game.BoardSize && y < Game.BoardSize)
			{
				var node = Game.CurrentNode;
				node.EnsureMarkup();
				if (node.Markup.DeadStones != null &&
					Game.CurrentNode.Markup.DeadStones[(byte) x, (byte) y])
				{
					gamePlayer.Undo(IGSGame);
				}
				else
				{
					gamePlayer.PlaceStone(
						IGSGame, new Stone((byte) x, (byte) y, true));
				}
			}			
		}			
		
		protected override void CursorPress(Point cursor)
		{
			PlaceStone(cursor.X, cursor.Y);
		}
		
		public override void MouseDown(MouseEventArgs args)
		{
			var stone = Renderer.GetCoords(args.X, args.Y, Game.BoardSize);		
			PlaceStone(stone.X, stone.Y);
		}
		
		private void OnPassClick(object sender, EventArgs args)
		{
			if (!Ended)
			{
				gamePlayer.Pass(IGSGame);
			}
		}
		
		private void OnResignClick(object sender, EventArgs args)
		{
			if (!Ended)
			{
				gamePlayer.Resign(IGSGame);
			}
		}
		
		private void OnAdjournClick(object sender, EventArgs args)
		{
			if (!Ended)
			{
				gamePlayer.Adjourn(IGSGame);
			}
		}
		
		private void OnDoneClick(object sender, EventArgs args)
		{
			if (!Ended)
			{		
				gamePlayer.Done(IGSGame);
			}
		}
		
		private void OnAskUndoClick(object sender, EventArgs args)
		{
			if (!Ended)
			{		
				gamePlayer.AskUndo(IGSGame);
			}
		}
		
		private void OnUndoClick(object sender, EventArgs args)
		{
			if (!Ended)
			{		
				gamePlayer.Undo(IGSGame);
			}
		}
		
		private void OnSayClick(object sender, EventArgs args)
		{
			if (!Ended)
			{		
				OnChatRequested(EventArgs.Empty);
			}
		}
		
		private void OnKomiClick(object sender, EventArgs args)
		{
			if (!Ended)
			{	
				var input = new FloatInput()
				{
					Value = 0.5f,
					Step = 0.5f
				};
				var dialog = new ValueDialog<FloatInput>("Komi value: ", input);
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					gamePlayer.SetKomi(Convert.ToSingle(input.Value));
				}
			}
		}
		
		private void OnHandicapClick(object sender, EventArgs args)
		{
			if (!Ended)
			{	
				var input = new IntInput()
				{
					MinValue = 0,
					MaxValue = 9
				};
				var dialog = new ValueDialog<IntInput>("Handicap count: ", input);
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					gamePlayer.SetHandicap(input.Value);
				}
			}
		}
		
		public override void OnClose(EventArgs args)
		{
			OnResignClick(this, EventArgs.Empty);
			base.OnClose(args);
		}
		
		protected void OnChatRequested(EventArgs args)
		{
			if (ChatRequested != null)
			{
				ChatRequested(this, args);
			}
		}
	}
}