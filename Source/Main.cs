using System;
using System.Data;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Resources;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.WindowsCE.Forms;

namespace IGoEnchi
{
	public class MainForm : Form
	{
		private MainMenu mainMenu;
		private CustomToolBar toolBar;
		private GameRenderer renderer;
		
		private IGSClient client;
		private IGSServerInfo serverInfo;
		private IGSObserver gameObserver;
		private IGSPlayer gamePlayer;
		private IGSTellManager tellManager;
		private IGSSayManager sayManager;
		private IGSKibitzManager kibitzManager;
		private IGSFileManager fileManager;
		private IGSFriendManager friendManager;
		
		private FormViewContainer viewContainer;
		private ConsoleView consoleView;

		[STAThread]
		public static void Main(string[] args)
		{
			Application.Run(new MainForm());
		}
		
		public MainForm()
		{
			ConfigManager.Reset();
			ConfigManager.Load();
			
			ConfigManager.MainForm = this;
			
			mainMenu = new MainMenu();
			MenuItem menuItem = new MenuItem();
			menuItem.Text = "File";
			mainMenu.MenuItems.Add(menuItem);
			
			menuItem = new MenuItem();
			menuItem.Text = "IGS";
			mainMenu.MenuItems[0].MenuItems.Add(menuItem);

			menuItem = new MenuItem();
			menuItem.Text = "Connect";
			menuItem.Click += new EventHandler(OnConnectClick);
			mainMenu.MenuItems[0].MenuItems[0].MenuItems.Add(menuItem);
			
			menuItem = new MenuItem();
			menuItem.Text = "Disconnect";
			menuItem.Click += new EventHandler(OnDisconnectClick);
			mainMenu.MenuItems[0].MenuItems[0].MenuItems.Add(menuItem);
			
			mainMenu.MenuItems[0].MenuItems[0].MenuItems.Add(new MenuItem() {Text = "-"});
			mainMenu.MenuItems[0].MenuItems.Add(new MenuItem() {Text = "-"});
			
			menuItem = new MenuItem();
			menuItem.Text = "Games";
			menuItem.Click += new EventHandler(OnGamesClick);
			mainMenu.MenuItems[0].MenuItems[0].MenuItems.Add(menuItem);
			
			menuItem = new MenuItem();
			menuItem.Text = "Players";
			menuItem.Click += new EventHandler(OnPlayersClick);
			mainMenu.MenuItems[0].MenuItems[0].MenuItems.Add(menuItem);
			
			menuItem = new MenuItem()
			{
				Text = "Friends"
			};
			menuItem.Click += delegate
			{
				var view = viewContainer.FindView("Friends");
				if (view == null)
				{
					var friendsView = new FriendView(ClientSize, friendManager)
					{
						Container = viewContainer,
						Text = "Friends"
					};
					friendsView.ChatRequested += ChatRequested;
					friendsView.ObserveRequested += ObserveRequested;
					friendsView.MatchRequested += MatchRequested;
				}
				else
				{
					viewContainer.SwitchTo(view);
				}
			};
			mainMenu.MenuItems[0].MenuItems[0].MenuItems.Add(menuItem);

			mainMenu.MenuItems[0].MenuItems[0].MenuItems.Add(new MenuItem() {Text = "-"});
			
			menuItem = new MenuItem();
			menuItem.Text = "Toggle";
			menuItem.Click += new EventHandler(OnToggleClick);
			mainMenu.MenuItems[0].MenuItems[0].MenuItems.Add(menuItem);
			
			menuItem = new MenuItem();
			menuItem.Text = "Player stats";
			menuItem.Click +=
				delegate
			{
				var input = new StringInput();
				var dialog = new ValueDialog<StringInput>("Player name: ", input);
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					var view = viewContainer.FindView("Stats of " + input.Text) as PlayerStatsView;
					if (view != null)
					{
						viewContainer.SwitchTo(view);
					}
					else
					{
						view = new PlayerStatsView(ClientSize, serverInfo, input.Text)
						{
							Container = viewContainer,
							Text = "Stats of " + input.Text
						};
					}
				}
			};
			mainMenu.MenuItems[0].MenuItems[0].MenuItems.Add(menuItem);
			
			menuItem = new MenuItem();
			menuItem.Text = "Stored games";
			menuItem.Click += new EventHandler(OnStoredGamesClick);
			mainMenu.MenuItems[0].MenuItems[0].MenuItems.Add(menuItem);
			
			mainMenu.MenuItems[0].MenuItems[0].MenuItems.Add(new MenuItem() {Text = "-"});
			
			menuItem = new MenuItem();
			menuItem.Text = "Chat";
			menuItem.Click += new EventHandler(OnChatClick);
			mainMenu.MenuItems[0].MenuItems[0].MenuItems.Add(menuItem);
			
			menuItem = new MenuItem();
			menuItem.Text = "Match";
			menuItem.Click += new EventHandler(OnMatchClick);
			mainMenu.MenuItems[0].MenuItems[0].MenuItems.Add(menuItem);
			
			menuItem = new MenuItem();
			menuItem.Text = "NMatch";
			menuItem.Click += new EventHandler(OnNMatchClick);
			mainMenu.MenuItems[0].MenuItems[0].MenuItems.Add(menuItem);
			
			menuItem = new MenuItem();
			menuItem.Text = "Seek game";
			menuItem.Click += new EventHandler(OnSeekGameClick);
			mainMenu.MenuItems[0].MenuItems[0].MenuItems.Add(menuItem);
			
			menuItem = new MenuItem();
			menuItem.Text = "Observe";
			menuItem.Click += new EventHandler(OnObserveClick);
			mainMenu.MenuItems[0].MenuItems[0].MenuItems.Add(menuItem);
			
			mainMenu.MenuItems[0].MenuItems[0].MenuItems.Add(new MenuItem() {Text = "-"});
			
			menuItem = new MenuItem();
			menuItem.Text = "Help";
			menuItem.Click += delegate
			{
				var input = new StringInput();
				var dialog = new ValueDialog<StringInput>("Command name: ", input);
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					fileManager.RequestFile(input.Text);
				}
			};
			mainMenu.MenuItems[0].MenuItems[0].MenuItems.Add(menuItem);

			menuItem = new MenuItem();
			menuItem.Text = "SGF";
			mainMenu.MenuItems[0].MenuItems.Add(menuItem);
			
			var sgfIndex = 1;
			menuItem = new MenuItem();
			menuItem.Text = "Create board";
			menuItem.Click += delegate
			{
				var dialog = new GameInfoDialog(new GameInfo(), true);
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					var view = new GameEditView(
						ClientSize,
						new GoGame(dialog.BoardSize >= 2 ? dialog.BoardSize : (byte) 19),
						renderer)
					{
						Container = viewContainer,
						Text = "Board " + sgfIndex
					};
					view.Game.Info = dialog.GameInfo;
					sgfIndex += 1;
				}
			};
			mainMenu.MenuItems[0].MenuItems[2].MenuItems.Add(menuItem);
			
			menuItem = new MenuItem() {Text = "Edit game"};
			menuItem.Click += new EventHandler(OnOpenClick);
			mainMenu.MenuItems[0].MenuItems[2].MenuItems.Add(menuItem);
			
			menuItem = new MenuItem() {Text = "Browse game"};
			menuItem.Click += new EventHandler(OnOpenClick);
			mainMenu.MenuItems[0].MenuItems[2].MenuItems.Add(menuItem);
			
			menuItem = new MenuItem();
			menuItem.Text = "Load problems";
			menuItem.Click += new EventHandler(OnOpenClick);
			mainMenu.MenuItems[0].MenuItems[2].MenuItems.Add(menuItem);
			
			menuItem = new MenuItem();
			menuItem.Text = "Save game";
			menuItem.Click += new EventHandler(OnSaveClick);
			mainMenu.MenuItems[0].MenuItems[2].MenuItems.Add(menuItem);
			
			mainMenu.MenuItems[0].MenuItems[2].MenuItems.Add(new MenuItem() {Text = "-"});
			
			menuItem = new MenuItem();
			menuItem.Text = "Copy board";
			menuItem.Click += delegate
			{
				if (viewContainer.ActiveView is GameView)
				{
					var view = viewContainer.ActiveView as GameView;
					var existingView = viewContainer.FindView("Copy of " + view.Text);
					if (existingView != null)
					{
						viewContainer.SwitchTo(existingView);
					}
					else
					{
						Cursor.Current = Cursors.WaitCursor;
						try
						{
							var newView = new GameEditView(
								ClientSize, view.Game.Copy(), renderer)
							{
								Container = viewContainer,
								Text = "Copy of " + view.Text
							};
						}						
						catch (Exception e)
						{
							MessageBox.Show("Unexpected error: " + e.Message,
							                "Error",
							                MessageBoxButtons.OK,
							                MessageBoxIcon.Exclamation,
							                MessageBoxDefaultButton.Button1);
						}
						Cursor.Current = Cursors.Default;
					}
				}
			};
			mainMenu.MenuItems[0].MenuItems[2].MenuItems.Add(menuItem);
			
			menuItem = new MenuItem();
			menuItem.Text = "GNU Go";
			mainMenu.MenuItems[0].MenuItems.Add(menuItem);
			
			menuItem = new MenuItem();
			menuItem.Text = "New game";
			menuItem.Click += delegate
			{
				var view = viewContainer.FindView("GNU Go") as GnuGoView;
				if (view != null)
				{
					viewContainer.SwitchTo(view);
				}
				else
				{		
					var dialog = new GnuGoDialog(ConfigManager.Settings.GnuGoSettings);
					if (dialog.ShowDialog() == DialogResult.OK)
					{
						ConfigManager.Settings.GnuGoSettings = dialog.GnuGoSettings;
						ConfigManager.Save();
						try
						{					
							Cursor.Current = Cursors.WaitCursor;
							var gtp = GnuGoLauncher.Run(dialog.GnuGoSettings, null);
							view = new GnuGoView(
								ClientSize, 
								new GoGame((byte) dialog.GnuGoSettings.BoardSize),
								renderer, gtp, dialog.GnuGoSettings.Color, 
								file =>
								{
									var newGtp = GnuGoLauncher.Run(dialog.GnuGoSettings, file);
									return newGtp;
								})
							{
								Container = viewContainer,
								Text = "GNU Go"
							};							
						}
						catch (GnuGoException e)
						{
							var message = 
								e.Kind == GnuGoError.ExecutableNotFound ?
									"Could not find GnuGoCE.exe" :
								e.Kind == GnuGoError.CouldNotStart ?
									"Unable to launch GnuGoCE.exe" :
									"Could not connect to GNU Go";
								
							MessageBox.Show(message,
							                "Error",
							                MessageBoxButtons.OK,
							                MessageBoxIcon.Exclamation,
							                MessageBoxDefaultButton.Button1);
						}
						catch (Exception e)
						{	
							MessageBox.Show("Connection to GNU Go failed: " + e.Message,
								        	"Error",
								        	MessageBoxButtons.OK,
								        	MessageBoxIcon.Exclamation,
								        	MessageBoxDefaultButton.Button1);
						}
						Cursor.Current = Cursors.Default;
					}													
				}
			};				
			mainMenu.MenuItems[0].MenuItems[3].MenuItems.Add(menuItem);
			
			menuItem = new MenuItem();
			menuItem.Text = "Resume game";
			menuItem.Click += OnGnuGoResumeClick;
			mainMenu.MenuItems[0].MenuItems[3].MenuItems.Add(menuItem);
			
			menuItem = new MenuItem();
			menuItem.Text = "BruGo Joseki";
			menuItem.Click += delegate
			{				
				var view = viewContainer.FindView("BruGo Jouseki") as BruGoView;
				if (view != null)
				{
					viewContainer.SwitchTo(view);
				}
				else
				{
					view = new BruGoView(ClientSize, new GoGame(19), renderer)
					{
						Container = viewContainer,
						Text = "BruGo Jouseki"						
					};
				}
			};
			mainMenu.MenuItems[0].MenuItems.Add(menuItem);
			
			mainMenu.MenuItems[0].MenuItems.Add(new MenuItem() {Text = "-"});
			
			menuItem = new MenuItem()
			{
				Text = "Hide taskbar",
			};
			menuItem.Click += delegate(object sender, EventArgs e)
			{
				if (WindowState == FormWindowState.Maximized)
				{
					WindowState = FormWindowState.Normal;
					WinCEUtils.ShowTaskbar(this);
					(sender as MenuItem).Checked = false;
				}
				else
				{
					WindowState = FormWindowState.Maximized;
					WinCEUtils.HideTaskbar(this);
					(sender as MenuItem).Checked = true;
				}
			};
			mainMenu.MenuItems[0].MenuItems.Add(menuItem);
			
			mainMenu.MenuItems[0].MenuItems.Add(new MenuItem() {Text = "-"});
			
			menuItem = new MenuItem() {Text = "Settings"};
			menuItem.Click += delegate
			{
				SettingsView view = viewContainer.FindView("Settings") as SettingsView;
				if (view != null)
				{
					viewContainer.SwitchTo(view);
				}
				else
				{
					view = new SettingsView(ClientSize)
					{
						Container = viewContainer,
						Text = "Settings"
					};
				}
			};
			mainMenu.MenuItems[0].MenuItems.Add(menuItem);
			
			menuItem = new MenuItem();
			menuItem.Text = "About";
			menuItem.Click += new EventHandler(OnAboutClick);
			mainMenu.MenuItems[0].MenuItems.Add(menuItem);

			mainMenu.MenuItems[0].MenuItems.Add(new MenuItem() {Text = "-"});
			
			menuItem = new MenuItem();
			menuItem.Text = "Exit";
			menuItem.Click += new EventHandler(OnExitClick);
			mainMenu.MenuItems[0].MenuItems.Add(menuItem);
			
			this.Menu = mainMenu;
			this.Text = "IGoEnchi";
			
			if (ConfigManager.Settings.Renderer == RendererType.Direct3D)
			{
				try
				{
					renderer = new Direct3DRenderer(ClientSize, this);
				}
				catch (Exception) {}
			}
			if (renderer == null)
			{
				renderer = new GdiRenderer(ClientSize);
				ConfigManager.Settings.Renderer = RendererType.GDI;
			}
			
			this.Resize += delegate
			{
				foreach (var view in viewContainer.Views)
				{
					view.Size = ClientSize;
				}
			};
			
			client = new IGSClient();
			
			client.AddHandler(IGSMessages.Info, PrintInfo);
			client.AddHandler(IGSMessages.Error, PrintError);
			
			serverInfo = new IGSServerInfo(client);
			gameObserver = new IGSObserver(client, serverInfo);
			gameObserver.GameAdded += ObservedGameAdded;
			gamePlayer = new IGSPlayer(client, serverInfo);
			gamePlayer.GameAdded += PlayedGameAdded;
			gamePlayer.MatchRequestReceived += MatchRequestReceived;
			gamePlayer.NMatchRequestReceived += NMatchRequestReceived;
			gamePlayer.SeekStarted += OnSeekStarted;
			gamePlayer.SeekEnded += OnSeekEnded;
			
			tellManager = new IGSTellManager(client);
			tellManager.DefaultCallback += NewChatView;
			sayManager = new IGSSayManager(gamePlayer);
			sayManager.DefaultCallback += NewIngameChatView;
			kibitzManager = new IGSKibitzManager(gameObserver);
			kibitzManager.DefaultCallback += NewKibitzView;
			
			fileManager = new IGSFileManager(client);
			fileManager.FileReceived += delegate(string name, string content)
			{
				EventHandler addFile = delegate
				{
					var fileView = new FileView(ClientSize, content)
					{
						Container = viewContainer,
						Text = name
					};
				};
				Invoke(addFile);
			};
			
			friendManager = new IGSFriendManager(client);
			friendManager.ErrorReceived += error =>
			{
				MessageBox.Show(error,
				                "Error",
				                MessageBoxButtons.OK,
				                MessageBoxIcon.Exclamation,
				                MessageBoxDefaultButton.Button1);
			};
			friendManager.Updated += delegate
			{
				EventHandler update = delegate
				{
					var view = viewContainer.FindView("Friends");
					if (view != null)
					{
						(view as FriendView).Update();
					}
				};
				Invoke(update);
			};
			friendManager.FriendStateChanged += change =>
			{
				EventHandler show = delegate
				{
					var game = "";
					change.GameNumber.Iter(n => game = " <b>Game " + n.ToString() + "</b>");
					try
					{
						if (ConfigManager.Settings.FriendsNotify)
						{
							var notification = new Notification()
							{
								Caption = "Friend info",
								InitialDuration = 5,
								Icon = ConfigManager.GetIcon("game"),
								Text = "<hr/>" + change.Message + game + "<hr/>",
							};
							notification.Visible = true;

							notification.BalloonChanged += (s, a) =>
							{
								if (!a.Visible)
								{
									notification.Dispose();
								}
							};
						}
					}
					catch (Exception)
					{
						MessageBox.Show(change.Message + game);
					}
				};
				Invoke(show);
			};
			
			viewContainer = new FormViewContainer(this);
			
			consoleView = new ConsoleView(ClientSize)
			{
				Container = viewContainer,
				Text = "Console"
			};
			
			KeyPreview = true;
			
			KeyDown += delegate(object sender, KeyEventArgs args)
			{
				viewContainer.ActiveView.KeyDown(args);
			};
			
			KeyUp += delegate(object sender, KeyEventArgs args)
			{
				viewContainer.ActiveView.KeyUp(args);
			};
			
			if (Environment.OSVersion.Platform == PlatformID.WinCE)
			{
				HardwareButtonBindings.AddKeySource(this);
				HardwareButtonHandler hideMenu = delegate
				{
					if (Menu != null)
					{
						Menu = null;
						Controls.Remove(toolBar);
					}
					else
					{
						Menu = mainMenu;
						Controls.Add(toolBar);
					}
				};
				HardwareButtonBindings.Register("Hide menu", hideMenu);
				
				HardwareButtonBindings.SetBindings(ConfigManager.Settings.ButtonBindings);
			}
			
			MouseDown += delegate(object sender, MouseEventArgs args)
			{
				viewContainer.ActiveView.MouseDown(args);
			};
			
			MouseUp += delegate(object sender, MouseEventArgs args)
			{
				viewContainer.ActiveView.MouseUp(args);
			};
			
			MouseMove += delegate(object sender, MouseEventArgs args)
			{
				viewContainer.ActiveView.MouseMove(args);
			};
			
			Paint += delegate(object sender, PaintEventArgs args)
			{
				viewContainer.ActiveView.Paint(args);
			};
			
			toolBar = new CustomToolBar();
			
			var viewsButton = new ToolBarButton()
			{
				Style = ToolBarButtonStyle.DropDownButton,
				DropDownMenu = viewContainer.ViewMenu,
				ToolTipText = "Switch view"
			};
			
			var closeButton = new ToolBarButton()
			{
				ToolTipText = "Close view"
			};
			
			Controls.Add(toolBar);
			
			toolBar.ImageList = new ImageList();
			
			if (Environment.OSVersion.Platform != PlatformID.WinCE)
			{
				Controls.Remove(toolBar);
				var form = new Form()
				{
					FormBorderStyle = FormBorderStyle.FixedSingle,
					MaximizeBox = false,
					MinimizeBox = false,
				};
				form.Controls.Add(toolBar);
				form.ClientSize = new Size(form.ClientSize.Width, toolBar.Height);
				form.Show();
			}
			
			if ((Width == 240) || (Width == 320))
			{
				toolBar.ImageList.ImageSize = new Size(16, 16);
				ConfigManager.HiRes = false;
			}
			else
			{
				toolBar.ImageList.ImageSize = new Size(32, 32);
				ConfigManager.HiRes = true;
			}
			
			ResourceManager resourceManager = new ResourceManager("IGoEnchi.ToolbarIcons", Assembly.GetExecutingAssembly());
			
			toolBar.AddTool(new Tool(viewsButton, ConfigManager.GetIcon("views"),
			                         delegate {viewContainer.HideView();}));
			
			toolBar.AddTool(new Tool(closeButton, ConfigManager.GetIcon("close"),
			                         delegate(object sender, EventArgs args)
			                         {
			                         	string name = viewContainer.ActiveView.Text;
			                         	
			                         	viewContainer.RemoveView(viewContainer.ActiveView);
			                         }));
			
			toolBar.Lock();
			viewContainer.ToolBar = toolBar;
		}
		
		public void ChangeRenderer(RendererType type)
		{
			var changed = false;
			switch (type)
			{
				case RendererType.GDI:
					if (!(renderer is GdiRenderer))
					{
						renderer.Dispose();
						renderer = new GdiRenderer(ClientSize);
						changed = true;
					}
					break;
				case RendererType.Direct3D:
					if (!(renderer is Direct3DRenderer))
					{
						renderer.Dispose();
						renderer = new Direct3DRenderer(ClientSize, this);
						changed = true;
					}
					break;
			}
			if (changed)
			{
				foreach (var view in viewContainer.Views)
				{
					if (view is GameView)
					{
						(view as GameView).Renderer = renderer;
					}
				}
			}
		}
		
		public GameRenderer Renderer
		{
			get {return renderer;}
		}
		
		public void ToolbarButtonClick(object sender, ToolBarButtonClickEventArgs args)
		{
			if (toolBar.Buttons.IndexOf(args.Button) == 0)
			{
				viewContainer.HideView();
			}
			else if (toolBar.Buttons.IndexOf(args.Button) == 1)
			{
				if (viewContainer.ActiveView != null)
				{
					var name = viewContainer.ActiveView.Text;
					
					viewContainer.RemoveView(viewContainer.ActiveView);
				}
			}
		}
		
		public void NewChatView(string userName, string message)
		{
			EventHandler addView = delegate
			{
				var chatView = new ChatView(ClientSize, tellManager, client.CurrentAccount.Name, userName, null)
				{
					Container = viewContainer,
					Text = "Chat with " + userName
				};
				chatView.WriteLine(userName + ": " + message);
			};
			
			Invoke(addView);
		}
		
		public void NewIngameChatView(string userName, string message)
		{
			EventHandler addView = delegate
			{
				GameView view = null;
				
				foreach (var item in viewContainer.Views)
				{
					if (item is PlayedGameView &&
					    (item as PlayedGameView).OpponentName == userName)
					{
						view = item as GameView;
					}
				}
				
				var chatView = new ChatView(ClientSize, sayManager, client.CurrentAccount.Name, userName, view)
				{
					Container = viewContainer,
					Text = "Ingame chat with " + userName
				};
				chatView.WriteLine(userName + ": " + message);
			};
			
			Invoke(addView);
		}
		
		public void NewKibitzView(string userName, string message)
		{
			EventHandler addView = delegate
			{
				GameView view = null;
				foreach (var item in viewContainer.Views)
				{
					if (item is ObservedGameView &&
					    (item as ObservedGameView).GameNumber ==
					    Convert.ToInt32(userName))
					{
						view = item as GameView;
					}
				}
				
				var chatView = new ChatView(ClientSize, kibitzManager, client.CurrentAccount.Name, userName, view)
				{
					Container = viewContainer,
					Text = "Kibitz in game " + userName
				};
				
			};
			
			Invoke(addView);
		}
		
		public void ObserveRequested(int gameNumber)
		{
			var game = gameObserver.Games.Find(item => item.GameNumber == gameNumber);
			if (game != null)
			{
				var view = viewContainer.Views.Find(
					item => item is ObservedGameView &&
					(item as ObservedGameView).GameNumber == game.GameNumber);
				if (view == null)
				{
					CreateObservedGameView(game);
				}
				viewContainer.SwitchTo(view);
			}
			else
			{
				gameObserver.ObserveGame(gameNumber);
			}
		}

		public void IngameChatRequested(object sender, EventArgs args)
		{
			if (viewContainer.FindView("Ingame chat with " + (sender as PlayedGameView).OpponentName) == null)
			{
				var chatView = new ChatView(
					ClientSize, sayManager, client.CurrentAccount.Name,
					(sender as PlayedGameView).OpponentName, sender as PlayedGameView)
				{
					Container = viewContainer,
					Text = "Ingame chat with " + (sender as PlayedGameView).OpponentName
				};
			}
			else
			{
				viewContainer.SwitchTo(viewContainer.FindView("Ingame chat with " + (sender as PlayedGameView).OpponentName));
			}
		}
		
		public void KibitzRequested(object sender, EventArgs args)
		{
			if (viewContainer.FindView("Kibitz in game " + (sender as ObservedGameView).GameNumber) == null)
			{
				var chatView = new ChatView(
					ClientSize, kibitzManager, client.CurrentAccount.Name,
					(sender as ObservedGameView).GameNumber.ToString(),
					sender as ObservedGameView)
				{
					Container = viewContainer,
					Text = "Kibitz in game " + (sender as ObservedGameView).GameNumber
				};
			}
			else
			{
				viewContainer.SwitchTo(viewContainer.FindView("Kibitz in game " + (sender as ObservedGameView).GameNumber));
			}
		}
		
		public void ChatRequested(string name)
		{
			if (viewContainer.FindView("Chat with " + name) == null)
			{
				var chatView = new ChatView(
					ClientSize, tellManager,
					client.CurrentAccount.Name, name,
					null)
				{
					Container = viewContainer,
					Text = "Chat with " + name
				};
			}
			else
			{
				viewContainer.SwitchTo(viewContainer.FindView("Chat with " + name));
			}
		}

		public void MatchRequested(string player)
		{
			var dialog = new MatchDialog(MatchRequest.DefaultRequest(player));
			
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				gamePlayer.Match(dialog.Request);
			}
			else
			{
				gamePlayer.Decline(gamePlayer.IncomingRequest.OpponentName);
			}
		}
		
		public void MatchRequestReceived(object sender, EventArgs args)
		{
			var request = gamePlayer.IncomingRequest;
			EventHandler statsHandler = null;
			statsHandler = delegate
			{
				if (request.OpponentName == serverInfo.PlayerStats.Name)
				{
					MatchDialog dialog = new MatchDialog(request, serverInfo.PlayerStats);
					if (dialog.ShowDialog() == DialogResult.OK)
					{
						gamePlayer.Match(dialog.Request);
					}
					else
					{
						gamePlayer.Decline(gamePlayer.IncomingRequest.OpponentName);
					}
					serverInfo.PlayerStatsUpdated -= statsHandler;
				}
			};
			
			serverInfo.PlayerStatsUpdated += statsHandler;
			serverInfo.RequestPlayerStats(gamePlayer.IncomingRequest.OpponentName);
		}
		
		public void OnSeekStarted(object sender, EventArgs args)
		{
			Invoke(new EventHandler(AddSeekView));
		}

		public void AddSeekView(object sender, EventArgs args)
		{
			if (viewContainer.FindView("Seeking game") != null)
			{
				(viewContainer.FindView("Seeking game") as SeekView).ResetData(gamePlayer.LastSeekRequest);
			}
			else
			{
				var seekView = new SeekView(ClientSize, gamePlayer.LastSeekRequest)
				{
					Container = viewContainer,
					Text = "Seeking game"
				};
				seekView.SeekCanceled += new EventHandler(OnSeekCanceled);
			}
		}
		
		public void OnSeekCanceled(object sender, EventArgs args)
		{
			gamePlayer.CancelSeek();
		}
		
		public void OnSeekEnded(object sender, EventArgs args)
		{
			EventHandler closeView = delegate
			{
				var view = viewContainer.FindView("Seeking game");
				if (view != null)
				{
					viewContainer.RemoveView(view);
				}
			};
			Invoke(closeView);
		}
		
		public void NMatchRequestReceived(object sender, EventArgs args)
		{
			var request = gamePlayer.IncomingNRequest;
			EventHandler statsHandler = null;
			statsHandler = delegate
			{
				if (request.OpponentName == serverInfo.PlayerStats.Name)
				{
					NMatchDialog dialog = new NMatchDialog(request, serverInfo.PlayerStats);
					if (dialog.ShowDialog() == DialogResult.OK)
					{
						gamePlayer.NMatch(dialog.Request);
					}
					else
					{
						gamePlayer.Decline(gamePlayer.IncomingNRequest.OpponentName);
					}
					serverInfo.PlayerStatsUpdated -= statsHandler;
				}
			};
			serverInfo.PlayerStatsUpdated += statsHandler;
			serverInfo.RequestPlayerStats(gamePlayer.IncomingNRequest.OpponentName);
		}
		
		public void CreateObservedGameView(ObservedGame game)
		{
			var gameView = new ObservedGameView(
				ClientSize, game, renderer, gameObserver)
			{
				Container = viewContainer,
				Text = "Game " + game.GameNumber + ": " + game.Info.BlackPlayer + " vs " + game.Info.WhitePlayer
			};
			gameView.KibitzRequested += new EventHandler(KibitzRequested);
		}
		
		public void ObservedGameAdded(object sender, IGSGameEventArgs args)
		{
			var game = args.Game;
			EventHandler addView = delegate
			{
				CreateObservedGameView(game);
			};
			Invoke(addView);
		}
		
		public void PlayedGameAdded(object sender, IGSGameEventArgs args)
		{
			var game = args.Game;
			EventHandler addView = delegate
			{
				var gameView = new PlayedGameView(
					ClientSize, game, renderer, gamePlayer)
				{
					Container = viewContainer,
					Text = "Game " + game.GameNumber + ": " + game.Info.BlackPlayer + " vs " + game.Info.WhitePlayer
				};
				gameView.ChatRequested += new EventHandler(IngameChatRequested);
			};
			Invoke(addView);
		}
		
		public void PrintInfo(List<string> lines)
		{
			foreach (var line in lines)
			{
				var html = new StringBuilder();
				html.Append("<html><body>");
				html.Append("<font color=\"#0000FF\"><b>" + line + "</b></font>");
				html.Append("</body></html>");

				consoleView.WriteLine(line);
			}
		}
		
		public void PrintError(List<string> lines)
		{
			var message = new StringBuilder();
			foreach (var line in lines)
			{
				message.Append(line + "\r\n");
			}
			MessageBox.Show(message.ToString(),
			                "Error",
			                MessageBoxButtons.OK,
			                MessageBoxIcon.None,
			                MessageBoxDefaultButton.Button1);
		}
		
		protected override void Dispose(bool disposing)
		{
			renderer.Dispose();
			base.Dispose(disposing);
		}
		
		private void OnAboutClick(object sender, EventArgs args)
		{
			var view = viewContainer.FindView("About");
			if (view == null)
			{
				try
				{
					var resourceManager =
						new ResourceManager("IGoEnchi.Text", Assembly.GetExecutingAssembly())
					{
						IgnoreCase = true
					};
					
					var stream = new MemoryStream(resourceManager.GetObject("About") as byte[]);
					
					var game = SGFCompiler.Compile(SGFLoader.LoadFromStream(stream));
					game.ToMove(0);
					game.CommentNode("Thank you for using IGoEnchi v0.26. If you have any feedback, " +
					                 "please visit http://igoenchi.sourceforge.net");
					
					var aboutView = new GameBrowseView(ClientSize, game, renderer)
					{
						Text = "About",
						Container = viewContainer
					};
				}
				catch (Exception)
				{
					MessageBox.Show("IGoEnchi v0.26 http://igoenchi.sourceforge.net",
					                "About",
					                MessageBoxButtons.OK,
					                MessageBoxIcon.Asterisk,
					                MessageBoxDefaultButton.Button1);
				}
			}
			else
			{
				viewContainer.SwitchTo(view);
			}
		}

		private void OnExitClick(object sender, EventArgs args)
		{
			Controls.Remove(toolBar);
			
			if (Environment.OSVersion.Platform == PlatformID.WinCE)
			{
				HardwareButtonBindings.RemoveKeySource(this);
				HardwareButtonBindings.RemoveBindings();
			}
			
			ConfigManager.Save();
			
			if (client.Connected)
			{
				client.Disconnect();
			}
			
			Application.Exit();
		}
		
		private void OnConnectClick(object sender, EventArgs args)
		{
			if (!client.Connected)
			{
				var account = ConfigManager.Settings.CurrentAccount;
				var dialog = new AccountDialog(account);
				if (account.Password != "" ||
				    dialog.ShowDialog() == DialogResult.OK)
				{
					account = dialog.Account;
					var index = ConfigManager.Settings.Accounts.FindIndex(
						storedAccount => storedAccount.Name == account.Name);
					if (index >= 0)
					{
						ConfigManager.Settings.Accounts[index] = account;
					}
					else
					{
						ConfigManager.Settings.Accounts.Add(account);
					}
					ConfigManager.Settings.CurrentAccountName = account.Name;
					
					Cursor.Current = Cursors.WaitCursor;
					try
					{
						if (client.Connect(account))
						{
							fileManager.RequestFile("motd", "MOTD");
							if (client.CurrentAccount.Name.ToLower() != "guest")
							{
								EventHandler handler = null;
								handler = delegate
								{
									client.ApplyToggleSettings(serverInfo.ToggleSettings);
									serverInfo.ToggleSettingsUpdated -= handler;
								};
								serverInfo.ToggleSettingsUpdated += handler;
								serverInfo.RequestPlayerStats(client.CurrentAccount.Name);
							}
						}
					}
					catch (Exception) {}
					Cursor.Current = Cursors.Default;
					ConfigManager.Save();
				}
			}
		}
		
		private void OnDisconnectClick(object sender, EventArgs args)
		{
			if (client.Connected)
			{
				client.Disconnect();
			}
		}
		
		private void OnGamesClick(object sender, EventArgs args)
		{
			if (viewContainer.FindView("Games") == null)
			{
				GamesListView gamesListView = new GamesListView(ClientSize, serverInfo, gameObserver)
				{
					Container = viewContainer,
					Text = "Games"
				};
				gamesListView.ObserveRequested += ObserveRequested;
			}
			else
			{
				viewContainer.SwitchTo(viewContainer.FindView("Games"));
			}
		}
		
		private void OnPlayersClick(object sender, EventArgs args)
		{
			if (viewContainer.FindView("Players") == null)
			{
				var playersListView = new PlayersListView(
					ClientSize, serverInfo)
				{
					Container = viewContainer,
					Text = "Players"
				};
				playersListView.ChatRequested += ChatRequested;
				playersListView.MatchRequested += MatchRequested;
			}
			else
			{
				viewContainer.SwitchTo(viewContainer.FindView("Players"));
			}
		}
		
		private void OnToggleClick(object sender, EventArgs args)
		{
			var dialog = new ToggleDialog(client.ToggleSettings);
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				client.ApplyToggleSettings(dialog.Settings);
			}
		}
		
		private void OnMatchClick(object sender, EventArgs args)
		{
			var dialog = new MatchDialog();
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				gamePlayer.Match(dialog.Request);
			}
		}
		
		private void OnNMatchClick(object sender, EventArgs args)
		{
			var dialog = new NMatchDialog();
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				gamePlayer.NMatch(dialog.Request);
			}
		}
		
		private void OnSeekGameClick(object sender, EventArgs args)
		{
			var dialog = new SeekDialog();
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				gamePlayer.Seek(dialog.Request);
			}
		}
		
		private void OnObserveClick(object sender, EventArgs args)
		{
			var input = new IntInput()
			{
				MinValue = 1
			};
			var dialog = new ValueDialog<IntInput>("Game number: ", input);
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				gameObserver.ObserveGame(input.Value);
			}
		}
		
		private void OnStoredGamesClick(object sender, EventArgs args)
		{
			if (viewContainer.FindView("Stored games") == null)
			{
				var view = new StoredGamesView(ClientSize, gamePlayer)
				{
					Container = viewContainer,
					Text = "Stored games"
				};
			}
			else
			{
				viewContainer.SwitchTo(viewContainer.FindView("Stored games"));
			}
		}
		
		private void OnChatClick(object sender, EventArgs args)
		{
			var input = new StringInput();
			var dialog = new ValueDialog<StringInput>("Player name: ", input);
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				if (viewContainer.FindView("Chat with " + input.Text) == null)
				{
					var chatView = new ChatView(ClientSize, tellManager, client.CurrentAccount.Name, input.Text, null)
					{
						Container = viewContainer,
						Text = "Chat with " + input.Text
					};
				}
				else
				{
					viewContainer.SwitchTo(viewContainer.FindView("Chat with " + input.Text));
				}
			}
		}
		
		private void OnSaveClick(object sender, EventArgs args)
		{
			if (viewContainer.ActiveView is GameView)
			{
				var game = (viewContainer.ActiveView as GameView).Game;
				var dialog = new SaveFileDialog();
				dialog.Filter = "Smart Game Format (*.sgf)|*.sgf";
				dialog.FileName = viewContainer.ActiveView.Text;
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					var fileName = dialog.FileName;
					if (fileName == viewContainer.ActiveView.Text)
					{
						SGFWriter.SaveGame(game, fileName);
					}
					else if (viewContainer.ActiveView is GameEditView)
					{
						if (viewContainer.Rename(viewContainer.ActiveView.Text, fileName))
						{
							SGFWriter.SaveGame(game, fileName);
						}
						else
						{
							MessageBox.Show("A view with this name is already opened. Please close it first.",
							                "Save",
							                MessageBoxButtons.OK,
							                MessageBoxIcon.Exclamation,
							                MessageBoxDefaultButton.Button1);
						}
					}
					else
					{
						SGFWriter.SaveGame(game, fileName);
					}
				}
			}
			else
			{
				MessageBox.Show("No game is selected", "Save",
				                MessageBoxButtons.OK,
				                MessageBoxIcon.Exclamation,
				                MessageBoxDefaultButton.Button1);
			}
		}
		
		private void OnGnuGoResumeClick(object sender, EventArgs args)
		{
			var view = viewContainer.FindView("GNU Go") as GnuGoView;
			if (view != null)
			{				
				viewContainer.SwitchTo(view);
			}
			else
			{		
				var openFileDialog = new OpenFileDialog()
				{
					Filter = "Smart Game Format (*.sgf)|*.sgf",
				};
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					var fileName = openFileDialog.FileName;							
				
					try
					{
						Cursor.Current = Cursors.WaitCursor;
						var tree = SGFLoader.LoadAndParseSingle(fileName);
						var game = SGFCompiler.Compile(tree);
						game.ToMove(0);					
						var warning = true;
						Cursor.Current = Cursors.Default;
						while (game.CurrentNode.HasChildren)
						{
							if (warning && game.CurrentNode.ChildNodes.Count > 1)
							{
								warning = false;
								/*MessageBox.Show("This game record contains branches, so it may not resume properly.",
								                "Warning",
								                MessageBoxButtons.OK,
								                MessageBoxIcon.Exclamation,
								                MessageBoxDefaultButton.Button1);*/
							}
							game.ToNextMove(false);							
						}
						game.Update();
												
						var config = ConfigManager.Settings.GnuGoSettings;
						config.BoardSize = game.BoardSize;
						config.Handicap = 0;
						config.Komi = 0.0f;
							
						var dialog = new GnuGoDialog(config, true);
						if (dialog.ShowDialog() == DialogResult.OK)
						{							
							ConfigManager.Settings.GnuGoSettings = dialog.GnuGoSettings;
							ConfigManager.Save();
							try
							{					
								Cursor.Current = Cursors.WaitCursor;
								File.Copy(fileName, GnuGoView.GNUGoFile);
								var gtp = GnuGoLauncher.Run(dialog.GnuGoSettings, GnuGoView.GNUGoFile);
								view = new GnuGoView(
									ClientSize, 
									game,
									renderer, gtp, dialog.GnuGoSettings.Color, 
									file =>
									{
										var newGtp = GnuGoLauncher.Run(dialog.GnuGoSettings, file);
										return newGtp;
									})
								{
									Container = viewContainer,
									Text = "GNU Go"
								};							
							}
							catch (GnuGoException e)
							{								
								GnuGoView.RemoveSGF();
								var message = 
									e.Kind == GnuGoError.ExecutableNotFound ?
										"Could not find GnuGoCE.exe" :
									e.Kind == GnuGoError.CouldNotStart ?
										"Unable to launch GnuGoCE.exe" :
										"Could not connect to GNU Go";
									
								MessageBox.Show(message,
								                "Error",
								                MessageBoxButtons.OK,
								                MessageBoxIcon.Exclamation,
								                MessageBoxDefaultButton.Button1);
							}
							catch (Exception e)
							{	
								GnuGoView.RemoveSGF();								
								MessageBox.Show("Connection to GNU Go failed: " + e.Message,
									        	"Error",
									        	MessageBoxButtons.OK,
									        	MessageBoxIcon.Exclamation,
									        	MessageBoxDefaultButton.Button1);
							}													
						}
					}
					catch (Exception e)
					{	
						MessageBox.Show(e.Message,
							        	"Error",
							        	MessageBoxButtons.OK,
							        	MessageBoxIcon.Exclamation,
							        	MessageBoxDefaultButton.Button1);
					}
					Cursor.Current = Cursors.Default;
				}													
			}
		}
		
		private void OnOpenClick(object sender, EventArgs args)
		{
			var openFileDialog = new OpenFileDialog()
			{
				Filter = "Smart Game Format (*.sgf)|*.sgf",
			};
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				var fileName = openFileDialog.FileName;
				if (viewContainer.FindView(fileName) != null)
				{
					viewContainer.SwitchTo(viewContainer.FindView(fileName));
				}
				else
				{
					Cursor.Current = Cursors.WaitCursor;
					try
					{
						if ((sender as MenuItem).Text == "Edit game" ||
						    (sender as MenuItem).Text == "Browse game")
						{
							var tree = SGFLoader.LoadAndParseSingle(fileName);
							var game = SGFCompiler.Compile(tree);
							game.ToMove(0);
							
							var view =
								(sender as MenuItem).Text == "Edit game" ?
									new GameEditView(ClientSize, game, renderer) :
									new GameBrowseView(ClientSize, game, renderer);
							
							view.Container = viewContainer;
							view.Text = fileName;							
						}						
						else
						{
							var trees = SGFLoader.LoadAndParse(new string[] {fileName});
							var games = SGFCompiler.Compile(trees, TransformType.Random);
							
							var view =
								new TsumegoView(ClientSize,
								                games.GetEnumerator(),
								                renderer)
							{
								Container = viewContainer,
								Text = fileName
							};
						}
					}
					catch (Exception exception)
					{
						MessageBox.Show(exception.Message,
						                "Error",
						                MessageBoxButtons.OK,
						                MessageBoxIcon.Exclamation,
						                MessageBoxDefaultButton.Button1);
					}
					Cursor.Current = Cursors.Default;
				}
			}
			
			this.Invalidate();
		}

		protected override void OnPaintBackground(PaintEventArgs args)
		{
			if (!(viewContainer.ActiveView is GameView))
			{
				base.OnPaintBackground(args);
			}
		}
		
	}
}
