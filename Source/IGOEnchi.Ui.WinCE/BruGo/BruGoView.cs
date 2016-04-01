using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using IGOEnchi.BruGoClientLib;
using IGOEnchi.GoGameLib.Models;
using IGOEnchi.GoGameLogic.Models;

namespace IGoEnchi
{
	public class BruGoView: GameView
	{
		private static Dictionary<string, Color> MoveColors = CreateColorMap();
		
		private bool isBlack;
		private HttpWebRequest lastRequest;
		private string lastId;
		private BruGoNode[] currentNodes;
		private string description;		
		
		private BruGoCache cache;
		private Timer requestTimer;
		
		private ToolBarButton backButton;
		private ToolBarButton startButton;
		private ToolBarButton reloadButton;
		
		private Label statusLabel;
		
		private static Dictionary<string, Color> CreateColorMap()
		{
			var colors = new Dictionary<string, Color>();
			colors["Joseki"] = Color.Blue;
			colors["Popular joseki"] = Color.Green;
			colors["Trickmove"] = Color.Yellow;
			colors["Usually a mistake"] = Color.Red;
			return colors;
		}
		
		public BruGoView(Size size, GoGame game, GameRenderer renderer): 
			base(size, game, renderer)
		{			
			cache = new BruGoCache();
			
			AllowCursor = true;
			AllowNavigation = false;
			
			statusLabel = new Label()
			{
				Dock = DockStyle.Bottom,				
			};
			
			using (var graphics = CreateGraphics())
			{
				var textSize = graphics.MeasureString("Text", statusLabel.Font);
				statusLabel.Height = (int) textSize.Height;
			}
									
			Controls = new List<Control>() {statusLabel};
			
			OnResize();
			
			requestTimer = new Timer() {Interval = 15000};
			requestTimer.Tick += Timeout;
			ToStart();
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
			backButton = new ToolBarButton()
			{
				ToolTipText = "Back",
				Enabled = false
			};
			
			startButton = new ToolBarButton()
			{
				ToolTipText = "To start",
				Enabled = false
			};
			
			reloadButton = new ToolBarButton()
			{
				ToolTipText = "Reload",
				Enabled = false				
			};
			
			Tools = new Tool[] 
			{			
				new Tool(backButton,
				         ConfigManager.GetIcon("back"),
				         Back),
				new Tool(startButton,
				         ConfigManager.GetIcon("to_start"),
				         delegate {ToStart();}),
				new Tool(reloadButton, 
				         ConfigManager.GetIcon("refresh"), 
				         Reload)				
			};
		}
		
		private Color GetColor(string move)
		{
			return MoveColors.ContainsKey(move) ? MoveColors[move] : Color.Gray;			
		}
		
		private void ToStart()
		{
			currentNodes = new BruGoNode[0];
			isBlack = true;
			description = "";			
			statusLabel.Text = "Ready";
			cache.Clear();
			
			backButton.Enabled = false;
			startButton.Enabled = false;
			reloadButton.Enabled = false;
			
			while (Game.CurrentNode.ParentNode != null)
			{
				var node = Game.CurrentNode;
				Game.ToPreviousMove(false);				
				Game.CurrentNode.RemoveNode(node);				
			}
			Game.Update();
			
			Request(StartPosition, isBlack);
		}
		
		private void Back(object sender, EventArgs args)
		{
			var item = cache.Get();
			if (item != null)
			{
				description = item.Description;				
				currentNodes = item.Nodes;
				
				backButton.Enabled = !cache.IsEmpty();
				startButton.Enabled = !cache.IsEmpty();
				reloadButton.Enabled = false;
				statusLabel.Text = "Ready";
				
				isBlack = cache.Size % 2 != 0;
				
				var node = Game.CurrentNode;
				Game.ToPreviousMove();				
				Game.CurrentNode.RemoveNode(node);
				
				Invalidate();
			}
		}
		
		private void Reload(object sender, EventArgs args)
		{
			reloadButton.Enabled = false;
			Request(lastId, isBlack);			
		}
		
		private string GetUrl(string id, bool isBlack)
		{
			return "http://www.brugo.be/api.php?id=" + id +
				   "&color=" + (isBlack ? "B" : "W");
		}
		
		private string StartPosition
		{
			get {return "__00000000";}
		}
				
		private bool Waiting
		{
			get {return requestTimer.Enabled;}
		}
		
		private void OnRequestFailed(string reason)
		{
			lastRequest = null;			
			reloadButton.Enabled = true;
			
			statusLabel.Text = "Error - " + reason;
			
			backButton.Enabled = !cache.IsEmpty();
			startButton.Enabled = !cache.IsEmpty();
		}
		
		private void Timeout(object sender, EventArgs args)
		{
			requestTimer.Enabled = false;
			if (lastRequest != null)
			{
				var request = lastRequest;
				OnRequestFailed("Timeout");
				request.Abort();				
			}
		}

		private void Request(string id, bool isBlack)
		{
			if (!Waiting) 
			{
				lastId = id;				
				var request = WebRequest.Create(GetUrl(id, isBlack)) as HttpWebRequest;
				request.Method = "GET";
				request.UserAgent = "IGoEnchi/WM";							
			
				statusLabel.Text = "Loading...";
				try 
				{
					request.BeginGetResponse(RequestCallback, null);
					lastRequest = request;
					requestTimer.Enabled = true;					
					currentNodes = new BruGoNode[0];
				}
				catch (Exception)
				{
					OnRequestFailed("Unable to send request");					
				}						
			}
		}		
		
		private void RequestCallback(IAsyncResult result)
		{
			if (lastRequest != null)
			{
				try
				{						
					var response = lastRequest.EndGetResponse(result);
					try
					{
						var reader = new StreamReader(response.GetResponseStream());
						var position = reader.ReadToEnd();						
						Apply(BruGoAPIParser.Parse(position));												
					}
					finally
					{						
						response.Close();						
					}										
				}
				catch (Exception)
				{
					Container.BaseForm.Invoke(new EventHandler(
						delegate
						{
							OnRequestFailed("Unable to access the server");							
						}));
				}
				requestTimer.Enabled = false;
				lastRequest = null;
			}
		}				
		
		private void Apply(BruGoNode[] nodes)
		{			
			currentNodes = nodes;		
			isBlack = !isBlack;				
						
			Container.BaseForm.Invoke(new EventHandler(delegate 
			{
			    backButton.Enabled = !cache.IsEmpty();
				startButton.Enabled = !cache.IsEmpty();
				statusLabel.Text = "Ready";
				Invalidate();
			}));
		}
		
		public override void Paint(PaintEventArgs args)
		{
			Renderer.StartRendering(BoardPosition);
			Game.Render(Renderer);
			
			foreach (var node in currentNodes)
			{
				Renderer.DrawColoredMark(GetColor(node.Description), 
				                         (byte) node.X, (byte) node.Y);
			}
			
			if (AllowCursor && (ConfigManager.Settings.KeepCursor || UseCursor))
			{
				Renderer.DrawCursor((byte) Cursor.X, (byte) Cursor.Y);			
			}
			if (!String.IsNullOrEmpty(description))
			{
				Renderer.DrawMessage(description);
			}
			Renderer.FinishRendering(args.Graphics);
		}
		
		protected override void CursorPress(Point cursor)
		{		
			if (!Waiting)
			{
				var stone = new Stone((byte) cursor.X, (byte) cursor.Y, !isBlack);
						
				foreach (var node in currentNodes)
				{					
					if (node.X == stone.X && node.Y == stone.Y)
					{									
						cache.Add(description, currentNodes);						
						Request(node.ID, isBlack);
						
						backButton.Enabled = false;
						startButton.Enabled = false;
							
						stone.IsBlack = !isBlack;
						Game.PlaceStone(stone);						
						description = node.Description;						
						
						SoundUtils.Play("Stone");
						Invalidate();		
						return;
					}
				}
			}
		}
		
		public override void MouseDown(MouseEventArgs args)
		{
			var stone = Renderer.GetCoords(args.X, args.Y, Game.BoardSize);
			CursorPress(new Point(stone.X, stone.Y));			
		}
	}
}