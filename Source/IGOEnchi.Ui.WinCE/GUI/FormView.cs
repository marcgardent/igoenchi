using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.WindowsCE.Forms;

namespace IGoEnchi
{		
	public abstract class FormView
	{	
		public event EventHandler Select;	
		public event EventHandler Close;	
				
		private Size size;
		public Tool[] Tools {get; protected set;}		
		public bool CanClose {get; protected set;}		
		public List<Control> Controls {get; protected set;}
		public string text = "View";
		private FormViewContainer container;
			
		public Size Size
		{
			get {return size;}
			set 
			{
				size = value;
				OnResize();
			}			
		}
		
		public FormViewContainer Container
		{
			get {return container;}
			set 
			{
				container = value;
				if (container.FindView(Text) == null)
				{
					container.AddView(this);
				}
			}
		}

		public FormView(Size size)
		{	
			Controls = new List<Control>();
			this.size = size;
			OnInit();
			CanClose = true;
		}			
		
		public string Text
		{
			get {return text;}
			set 
			{				
				if (Container != null)
				{
					if (Container.UpdateLink(Text, value))
					{
						text = value;
					}
				}
				else
				{
					text = value;
				}
			}			
		}
		
		protected virtual void OnResize() {}
		
		protected virtual void OnInit() {}
		
		public virtual void OnSelect(EventArgs args)
		{
			if (Select != null)
			{
				Select(this, args);
			}
		}
		
		public virtual void OnClose(EventArgs args)
		{
			if (Close != null)
			{
				Close(this, args);
			}
		}
		
		public virtual void FillForm(Form form)
		{
			if (Controls != null)
			{
				foreach (var control in Controls)
				{
					form.Controls.Add(control);
				}
				if (Controls.Count > 0)
				{
					Controls[0].Focus();
				}
			}
		}
		
		public virtual void ClearForm(Form form)
		{
			if (Controls != null)
			{
				foreach (var control in Controls)
				{
					form.Controls.Remove(control);
				}
			}
		}
		
		public virtual void Paint(PaintEventArgs args) {}
		
		public virtual void KeyDown(KeyEventArgs args) {}
		
		public virtual void KeyUp(KeyEventArgs args) {}
		
		public virtual void MouseDown(MouseEventArgs args) {}
		
		public virtual void MouseUp(MouseEventArgs args) {}
		
		public virtual void MouseMove(MouseEventArgs args) {}
		
		protected void Invalidate()
		{
			if (Container != null)
			{
				Container.BaseForm.Invalidate();
			}
		}
		
		protected Graphics CreateGraphics()
		{
			return ConfigManager.MainForm.CreateGraphics();
		}
	}
		
	public class ChatView: FormView
	{
		private TextBox chatBox;		
		private TextBox inputBox;	
		private Button sendButton;
		private StringBuilder buffer;
		private IGSChatManager chatController;		
		
		private String localUserName;
		private String remoteUserName;
			
		private GameView source;
		
		public ChatView(Size size, 
		                IGSChatManager chatController, 
		                String localUserName,
		                String remoteUserName,
		                GameView source): base(size)
		{									
			using (var graphics = CreateGraphics())
			{
				inputBox = new TextBox();
				SizeF textSize = graphics.MeasureString("Send__", inputBox.Font);
				textSize.Height *= 1.3F;			
				inputBox.Dispose();
				
				inputBox = new TextBox()
				{
					Width = Size.Width - (int) textSize.Width,
					Height = (int) textSize.Height,
					ScrollBars = ScrollBars.Vertical
				};				
			
				sendButton = new Button()
				{				
					Width = (int) textSize.Width,
					Height = inputBox.Height,
					Left = Size.Width - (int) textSize.Width,
					Anchor = AnchorStyles.Right,
					Text = "Send"
				};
				sendButton.Click += delegate {Send();};				
			
				buffer = new StringBuilder(200);			
			}
			
			chatBox = new TextBox()
			{
				ReadOnly = true,
				Multiline = true,
				Dock = DockStyle.Bottom,
				Height = Size.Height - inputBox.Height,
				ScrollBars = ScrollBars.Vertical
			};
			
			this.chatController = chatController;
			chatController.RegisterChat(remoteUserName, new IGSChatHandler(ReceiveMessage));
			
			this.localUserName = localUserName;
			this.remoteUserName = remoteUserName;						
			
			Controls = new List<Control>()
			{
				inputBox, chatBox, sendButton
			};
			
			if (source != null)
			{
				Tools = new Tool[]
				{					
					new Tool(new ToolBarButton() {ToolTipText = "To game"},
				         	ConfigManager.GetIcon("game"),
				         	ToGameButtonClick)
				};
				this.source = source;
			}
		}
		
		public override void OnSelect(EventArgs args)
		{
			base.OnSelect(args);
			chatBox.SelectionStart = chatBox.Text.Length;
			chatBox.SelectionLength = 1;			
			chatBox.ScrollToCaret();
		}
		
		private void ToGameButtonClick(object sender, EventArgs args)
		{
			if (source != null && Container.Views.Contains(source))
			{
				Container.SwitchTo(source);
			}
		}
		
		protected override void OnResize()
		{
			if (inputBox != null) 
			{
				inputBox.Width = Size.Width - sendButton.Width;
			}
			if (chatBox != null)
			{
				chatBox.Height = Size.Height - inputBox.Height;
			}
			if (sendButton != null)
			{
				sendButton.Left = Size.Width - (int) sendButton.Width;
			}
		}
		
		private void ReceiveMessage(string userName, string message)
		{	
			WriteLine(userName + ": " + message);	
			if (Container.ActiveView == source &&
			    ConfigManager.Settings.ChatNotify)
			{
				EventHandler notify = delegate
				{					
					var notification = new Notification()
					{
						Caption = Text,
						InitialDuration = 6,
						Icon = ConfigManager.GetIcon("talk"),
						Text = "<hr/><b>" + userName + ":</b> " + 
							message + " <hr/>",
					};
					notification.Visible = true;

					notification.BalloonChanged += (s, a) =>
					{
						if (!a.Visible)
						{
							notification.Dispose();
						}
					};
				};
				Container.BaseForm.Invoke(notify);
			}
		}
		
		public void WriteLine(String line)
		{
			buffer.Append(line + "\r\n");
			Container.BaseForm.Invoke(new EventHandler(FlushBuffer));
		}
		
		private void FlushBuffer(object sender, EventArgs args)
		{
			chatBox.Text += buffer.ToString();
			buffer.Remove(0, buffer.Length);
			chatBox.SelectionStart = chatBox.Text.Length;
			chatBox.SelectionLength = 1;			
			chatBox.ScrollToCaret();
		}
		
		private void Send()
		{			
			WriteLine(localUserName + ": " + inputBox.Text);
			chatController.SendMessage(remoteUserName, inputBox.Text);				
			inputBox.Text = "";
		}
		
		public void AddInput(string text)
		{
			if (inputBox.Text.Length > 0 &&
			    !Char.IsWhiteSpace(inputBox.Text[inputBox.Text.Length - 1]))
			{
			    inputBox.Text += " ";
			}
			inputBox.Text += text;			
		}
		
		public override void KeyDown(KeyEventArgs args)
		{
			if (args.KeyCode == Keys.Return || args.KeyCode == Keys.Enter)
			{
				Send();
			}
		}
		
		public override void OnClose(EventArgs args)
		{
			chatController.UnregisterChat(remoteUserName);
			base.OnClose(args);
		}
	}
			
	public class StoredGamesView: FormView
	{
		private ListBox list;
		private IGSPlayer gamePlayer;
		
		private ToolBarButton refreshButton;
		private ToolBarButton loadButton;
		
		public StoredGamesView(Size size, IGSPlayer gamePlayer): base(size)
		{
			if (gamePlayer == null)
			{
				throw new ArgumentException("Argument cannot be null");
			}
			
			this.gamePlayer = gamePlayer;
			gamePlayer.StoredGamesReceived += new EventHandler(GamesListReceived);			
												
			list = new ListBox()
			{
				Dock = DockStyle.Fill
			};
						
			Controls = new List<Control>()
			{
				list
			};
			
			RefreshList();
		}
		
		protected override void OnInit()
		{
			refreshButton = new ToolBarButton() 
			{
				ToolTipText = "Refresh",				
			};
			loadButton = new ToolBarButton()
			{
				ToolTipText = "Load"				
			};
			
			Tools = new Tool[]
			{
				new Tool(refreshButton,
				         ConfigManager.GetIcon("refresh"),
				         RefreshButtonClick),
				new Tool(loadButton,
				         ConfigManager.GetIcon("game"),
				         LoadButtonClick)
			};
		}
		
		
		private void RefreshButtonClick(object sender, EventArgs args)
		{
			RefreshList();
		}
		
		private void LoadButtonClick(object sender, EventArgs args)
		{
			if (list.SelectedIndex >= 0)
			{
				gamePlayer.LoadGame(list.SelectedItem.ToString());
			}
		}
		
		private void GamesListReceived(object sender, EventArgs args)
		{			
			list.Invoke(new EventHandler(LoadGamesList));
		}
		
		private void LoadGamesList(object sender, EventArgs args)
		{
			foreach (string name in gamePlayer.StoredGames)
			{
				list.Items.Add(name);
			}
			refreshButton.Enabled = true;
			loadButton.Enabled = list.Items.Count > 0;
		}
		
		private void RefreshList()
		{
			list.Items.Clear();
			gamePlayer.RequestStoredGames();
			refreshButton.Enabled = false;
			loadButton.Enabled = false;
		}		
	}
		
	public class SeekView: FormView
	{
		private TextBox infoBox;		
		
		public event EventHandler SeekCanceled;
		
		public SeekView(Size size, SeekRequest request): base(size)
		{						
			infoBox = new TextBox()
			{
				Multiline = true,
				ReadOnly = true,
				BackColor = Color.White,
				Dock = DockStyle.Fill					
			};
			ResetData(request);				

			Controls = new List<Control>()
			{
				infoBox
			};
		}						
				
		public void ResetData(SeekRequest request)
		{
			infoBox.Text = "Seeking game\r\n" +
				        "Board size: " + request.BoardSize + "\r\n" +
				 		"Max handicap: " + request.Handicap + "\r\n" +
						"in " + SeekRequest.TimeLabels[request.TimeEntry];
		}
		
		public override void OnClose(EventArgs args)
		{
			base.OnClose(args);
			if (SeekCanceled != null)
			{
				SeekCanceled(this, EventArgs.Empty);
			}
		}
	}
}
