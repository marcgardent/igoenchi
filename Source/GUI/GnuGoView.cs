using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

namespace IGoEnchi
{
	public enum GnuGoColor {Black, White, Both, None};
	
	public class GnuGoSettings
	{		
		public static readonly GnuGoSettings Default = 
			new GnuGoSettings(19, 0, 6.5f, 10, GnuGoColor.Black);
		
		[XmlElement("boardSize")]
		public int BoardSize {get; set;}
		[XmlElement("handicap")]
		public int Handicap {get; set;}
		[XmlElement("komi")]
		public float Komi {get; set;}
		[XmlElement("gnuGoLevel")]
		public int Level {get; set;}
		[XmlElement("gnuGoColor")]
		public GnuGoColor Color {get; set;}
		
		public GnuGoSettings():
			this(Default.BoardSize,
			     Default.Handicap,
			     Default.Komi,
			     Default.Level,
			     Default.Color)
		{
			
		}
				
		public GnuGoSettings(int boardSize, int handicap, float komi, 
		                     int level, GnuGoColor color)
		{
			BoardSize = boardSize;
			Handicap = handicap;
			Komi = komi;
			Level = level;
			Color = color;
		}
	}
	
	public class GnuGoDialog: IGSDialog
	{
		private NumericUpDown boardSizeUpDown;						
		private NumericUpDown handicapUpDown;
		private TextBox komiTextBox;
		private TrackBar gnugoLevel;
		private RadioButton gnugoBlack;
		private RadioButton gnugoWhite;
		private RadioButton gnugoBoth;
		private RadioButton gnugoNone;
		
		public GnuGoDialog(GnuGoSettings defaults): this(defaults, false)
		{
			
		}
		
		public GnuGoDialog(GnuGoSettings defaults, bool resume)
		{
			boardSizeUpDown = new NumericUpDown()
			{
				Minimum = 5,
				Maximum = 19,
				Value = defaults.BoardSize,	
				Enabled = !resume
			};									
		
			handicapUpDown = new NumericUpDown()
			{							
				Minimum = 0,
				Maximum = 9,
				Value = defaults.Handicap,				
				Enabled = !resume
			};			
			
			komiTextBox = new TextBox()
			{				
				Text = ConfigManager.FloatToString(defaults.Komi),
				Enabled = !resume					
			};
			
			gnugoLevel = new TrackBar()
			{
				Minimum = 0,
				Maximum = 10,
				Value = defaults.Level
			};
			
			gnugoBlack = new RadioButton()
			{
				Text = "Black",
				Checked = defaults.Color == GnuGoColor.Black
			};
			
			gnugoWhite = new RadioButton()
			{
				Text = "White",
				Checked = defaults.Color == GnuGoColor.White
			};
									
			gnugoBoth = new RadioButton()
			{
				Text = "Both",
				Checked = defaults.Color == GnuGoColor.Both
			};
			
			gnugoNone = new RadioButton()
			{
				Text = "None",
				Checked = defaults.Color == GnuGoColor.None
			};
			
			foreach (var item in new Control[] 
			         {
			         	boardSizeUpDown, komiTextBox, handicapUpDown, 
			         	gnugoLevel, gnugoBlack, gnugoWhite, gnugoBoth, gnugoNone
			         })
			{
				Layout.Resize(item);
			}
			
			Layout.Bind(
				Layout.PropertyTable(
					Layout.Label("Board size"), boardSizeUpDown,
					Layout.Label("Handicap"), handicapUpDown,
					Layout.Label("Komi"), komiTextBox,
					Layout.Label("GNU Go Level"), gnugoLevel,
					Layout.Label("GNU Go Plays"), gnugoBlack,
					null, gnugoWhite,
					null, gnugoBoth,
					null, gnugoNone),
				this);
		}
	
		public GnuGoSettings GnuGoSettings
		{
			get 
			{
				var komi = 0.0f;				
				try
				{
					komi = ConfigManager.StringToFloat(komiTextBox.Text);
				}
				catch (Exception) {}
								
				return new GnuGoSettings(
					(int) boardSizeUpDown.Value,
					(int) handicapUpDown.Value,
					komi, gnugoLevel.Value,
					gnugoBlack.Checked ? GnuGoColor.Black :
					gnugoWhite.Checked ? GnuGoColor.White :
					gnugoBoth.Checked ? GnuGoColor.Both :
								    	GnuGoColor.None);
			}
		}
	}
	
	public delegate GTP GTPReset(string sgfFile);
	
	public class GnuGoView: GameView
	{
		public static readonly string GNUGoFile = @"\__gnugo__.sgf";
		
		private GTP gtp;
		private bool blackToPlay;
		private bool playing;
		private int passes;
		private Timer timer;
		private GnuGoColor gnugoColor;
		private GTPReset reset;
		
		private ToolBarButton passButton;
		private ToolBarButton undoButton;
		private ToolBarButton resetButton;
		private Label statusLabel;
		
		public GnuGoView(Size size, GoGame game, GameRenderer renderer, 
		                 GTP gtp, GnuGoColor gnugoColor, GTPReset reset):
			base(size, game, renderer)
		{
			this.gtp = gtp;	
			this.reset = reset;
			this.gnugoColor = gnugoColor;
			blackToPlay = true;
			
			AllowCursor = true;
			AllowNavigation = false;						
		
			timer = new Timer()
			{
				Interval = 50,				
			};
			timer.Tick += (s, e) => this.gtp.Update();
			
			Close += (s, e) => CloseGTP();		
			
			statusLabel = new Label()
			{
				Dock = DockStyle.Bottom,				
			};
			
			using (var graphics = CreateGraphics())
			{
				var textSize = graphics.MeasureString("Text", statusLabel.Font);
				statusLabel.Height = (int) textSize.Height;
			}
			
			OnResize();	
			
			Controls = new List<Control>() {statusLabel};
			
			Init();
		}			
		
		private void Init()
		{			
			gtp.HandicapReceived += handicap =>
			{
				foreach (var stone in handicap)
				{
					Game.AddStone(new Stone((byte) stone.X, (byte) stone.Y, true));
				}
				blackToPlay = false;
				Invalidate();
			};
			
			gtp.LoadSGFCompleted += success =>
			{				
				RemoveSGF();
				if (success)
				{
					blackToPlay = Game.BlackToPlay();
				}
				else
				{
					MessageBox.Show("Unable to resume the game",
							        "Error",
							         MessageBoxButtons.OK,
							         MessageBoxIcon.Exclamation,
							         MessageBoxDefaultButton.Button1);
					CloseGTP();
					UpdateStatus();
				}
			};
			
			gtp.SetupCompleted += () =>
			{				
				playing = true;				
				if (!CanPlay)
				{
					gtp.RequestMove(blackToPlay);
				}
				UpdateStatus();
			};
			
			gtp.MoveReceived += (x, y, isBlack) =>
			{
				Game.PlaceStone(new Stone((byte) x, (byte) y, isBlack));
				passes = 0;
				blackToPlay = !blackToPlay;
				if (!CanPlay)
				{
					gtp.RequestMove(blackToPlay);
				}				
				UpdateStatus();
				Invalidate();				
			};
			
			gtp.PassReceived += isBlack =>
			{				
				Game.PlaceStone(new Stone(20, 20, isBlack));
				passes += 1;
				if (passes < 2)
				{
					blackToPlay = !blackToPlay;
					if (!CanPlay)
					{
						gtp.RequestMove(blackToPlay);
					}					
				}
				else
				{
					playing = false;
					gtp.RequestScore();
				}
				UpdateStatus();
				Invalidate();
			};
			
			gtp.UndoReceived += count =>	
			{				
				ApplyUndo(count);
				UpdateStatus();
				Invalidate();
			};
			
			gtp.ScoreReceived += result =>			
				Result("Result: " + result);							
			
			gtp.ResignReceived += isBlack =>			
				Result("Result: " + (isBlack ? "W" : "B") + "+Resign");			
			
			gtp.ErrorReceived += s =>			
				MessageBox.Show(s,
								"Error",
							    MessageBoxButtons.OK,
							    MessageBoxIcon.Exclamation,
							    MessageBoxDefaultButton.Button1);
			
			statusLabel.Text = "Initializing...";
			timer.Enabled = true;
		}
		
		protected override void OnResize()
		{
			if (statusLabel != null)
			{
				BoardPosition = new Rectangle(0, 0, Size.Width, Size.Height - statusLabel.Height);			
			}
		}
		
		protected override void OnInit()
		{
			passButton = new ToolBarButton()
			{
				ToolTipText = "Pass",				
			};
			
			undoButton = new ToolBarButton() 
			{
				ToolTipText = "Undo",				
			};						
			
			resetButton = new ToolBarButton()
			{
				ToolTipText = "Reset GNU Go"
			};
			
			Tools = new Tool[] 
			{
				new Tool(passButton,
				     	 ConfigManager.GetIcon("game"), 
				     	 delegate {Pass();}),
				new Tool(undoButton,
				     	 ConfigManager.GetIcon("back"),
				     	 delegate {Undo();}),
				new Tool(resetButton,
				         ConfigManager.GetIcon("refresh"),
				         delegate {Reset();})
			};
		}
		
		protected override void EnableButtons()
		{
			passButton.Enabled = CanPlay;
			undoButton.Enabled = CanPlay && Game.MoveNumber > 1 && gtp.SupportsUndo;			
			resetButton.Enabled = playing;
		}
		
		private void UpdateStatus()
		{
			statusLabel.Text =
				gtp.Closed ? "Game ended" :
				gtp.Waiting ? "Waiting for GNU Go..." :
							  "Ready";
			EnableButtons();
		}
		
		private void CloseGTP()			
		{
			playing = false;
			timer.Enabled = false;
			gtp.Quit();			
		}
		
		private void Result(string result) 
		{
			CloseGTP();
			UpdateStatus();
			MessageBox.Show(result,
							"Score",
							MessageBoxButtons.OK,
							MessageBoxIcon.Asterisk,
							MessageBoxDefaultButton.Button1);						
		}
		
		private bool CanPlay
		{
			get
			{
				return 
					playing && 
			    	(blackToPlay && gnugoColor == GnuGoColor.White ||
					 !blackToPlay && gnugoColor == GnuGoColor.Black ||
					 gnugoColor == GnuGoColor.None);
			}
		}
		
		private void ApplyUndo(int count)
		{
			Game.ToMove(Game.MoveNumber - count);
			Game.CurrentNode.RemoveNode(Game.CurrentNode.DefaultChildNode);
		}
		
		private void Play(int x, int y)
		{
			if (CanPlay &&
			    x >= 0 && y >= 0 &&
			    x < Game.BoardSize && y < Game.BoardSize)
			{
				gtp.Play(x, y, blackToPlay);
			}
		}
		
		private void Pass()
		{
			if (CanPlay)
			{
				gtp.Pass(blackToPlay);
				UpdateStatus();
			}
		}
		
		private void Undo()
		{
			if (gtp.SupportsUndo && CanPlay && Game.MoveNumber > 1)
			{				
				gtp.Undo(2);	
				UpdateStatus();
			}
		}
		
		public static void RemoveSGF()
		{
			try
			{
				File.Delete(GNUGoFile);
			}
			catch (Exception) {}
		}
		
		private void Reset()
		{
			if (MessageBox.Show("This will attempt to restart GNU Go and resume " +
			                    "the game. Do you want to proceed?",
							    "Reset",
							    MessageBoxButtons.YesNo,
							    MessageBoxIcon.Question,
							    MessageBoxDefaultButton.Button1) == DialogResult.Yes)
			{
				timer.Enabled = false;
				playing = false;
				if ((gnugoColor == GnuGoColor.Black && blackToPlay ||
				     gnugoColor == GnuGoColor.White && !blackToPlay) &&
				    Game.MoveNumber > 0)
				{
					ApplyUndo(1);	
					blackToPlay = !blackToPlay;
				}				
				System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;												
				try
				{					
					SGFWriter.SaveGame(Game, GNUGoFile);
					gtp = reset(GNUGoFile);
					Init();
				}
				catch (GnuGoException)
				{
					MessageBox.Show("Unable to restart GNU Go",
					                "Error",
					                MessageBoxButtons.OK,
					                MessageBoxIcon.Exclamation,
					                MessageBoxDefaultButton.Button1);
					CloseGTP();
					UpdateStatus();
					RemoveSGF();
				}
				catch (Exception e)
				{
					MessageBox.Show("Error while restarting GNU Go: " + e.Message,
					                "Error",
					                MessageBoxButtons.OK,
					                MessageBoxIcon.Exclamation,
					                MessageBoxDefaultButton.Button1);
					CloseGTP();
					UpdateStatus();
					RemoveSGF();
				}
				System.Windows.Forms.Cursor.Current = Cursors.Default;				
			}
		}
		
		protected override void CursorPress(Point cursor)
		{			
			Play(cursor.X, cursor.Y);
		}
				
		public override void MouseDown(MouseEventArgs args)
		{
			var stone = Renderer.GetCoords(args.X, args.Y, Game.BoardSize);
			Play(stone.X, stone.Y);							
		}
	}
}