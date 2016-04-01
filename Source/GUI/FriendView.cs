using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
 
namespace IGoEnchi
{
 	public class CustomListBox: Control
 	{	 	
 		public int SelectedIndex {get; private set;}
 		
 		private List<object> items;
 		private Icon playIcon;
 		private Icon observeIcon; 		
 		private Bitmap buffer; 		
 		
 		public event EventHandler SelectedIndexChanged;
 		
 		public CustomListBox()
 		{
 			SelectedIndex = -1;
 			items = new List<object>();
 			
 			playIcon = ConfigManager.GetIcon("game");
 			observeIcon = ConfigManager.GetIcon("observe");
 		}
 		
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			playIcon.Dispose();
			observeIcon.Dispose();
		}			
 		
		private void SetSelectedIndex(int index)
		{
			if (SelectedIndex != index)
			{
				SelectedIndex = index;
				if (SelectedIndexChanged != null)
				{
					SelectedIndexChanged(this, EventArgs.Empty);
				}
			}
		}			
		
		public object this[int index]
		{
			get 
			{
				if (index >= 0 && index < items.Count)
				{
					return items[index];
				}
				else
				{
					throw new ArgumentOutOfRangeException("Index is out of range");
				}
			}
		}
		
		public object SelectedItem
		{
			get
			{
				if (SelectedIndex >= 0)
				{
					return items[SelectedIndex];
				}
				else
				{
					throw new Exception("No item is selected");
				}
			}
		}
		
		public void Add(object item)
		{
			if (item != null && !items.Contains(item))
			{
				items.Add(item);
				Invalidate();
			}
		}
		
		public void Remove(object item)
		{
			items.Remove(item);
			if (SelectedIndex >= items.Count)
			{
				SetSelectedIndex(items.Count - 1);
			}
			Invalidate();
		}
		
		public void RemoveAt(int index)
		{
			if (index >= 0 && index < items.Count)
			{
				Remove(items[index]);				
			}
		}
		
		public void RemoveSelected()
		{
			RemoveAt(SelectedIndex);
		}
		
		public void Clear()
		{
			items.Clear();
			SetSelectedIndex(-1);
			Invalidate();
		}

 		private void Select(int x, int y)
 		{
 			var lastIndex = SelectedIndex;
 			SetSelectedIndex(GetIndex(x, y));
			if (lastIndex != SelectedIndex)
			{
				Invalidate();
			}
 		}
 		
 		private int GetIndex(int x, int y)
 		{
			var top = 0;
			for (var i = 0; i < items.Count; i++)
			{
				if (items[i] is FriendState)
				{
					var item = items[i] as FriendState;
					var height = 0;
					using (var graphics = ConfigManager.MainForm.CreateGraphics())
					{
						var size = (int) Math.Ceiling(graphics.MeasureString(
							item.Name, Font).Height);
						height = Math.Max(size, playIcon.Height);
					}
					if (y - Top >= top &&
					    y - Top < top + height)
					{
						return i;
					}
					
					top += height;
				}
			}		
			return -1;	
 		} 		 		
 		
		protected override void OnMouseDown(MouseEventArgs args)
		{
			base.OnMouseDown(args);			
			Select(args.X, args.Y);
		}
		
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
		}
 		
		protected override void OnMouseMove(MouseEventArgs args)
		{			
			base.OnMouseMove(args);
			Select(args.X, args.Y);			
		}
		
		protected override void OnPaintBackground(PaintEventArgs args)
		{					
			
		}
 		
 		protected override void OnPaint(PaintEventArgs args)
		{
 			base.OnPaint(args); 			 			 			
 			 			
 			var padding = 3;
 			var target = ClientRectangle;
 			target.Inflate(- padding, - padding);
 			
 			var selectionColor = Color.FromArgb(24, 224, 133);
 			var selectionColor2 = Color.LightBlue;
 			var blendColor = Color.FromArgb(
 				(selectionColor2.R + 2 * BackColor.R) / 3,
 				(selectionColor2.G + 2 * BackColor.G) / 3,
 				(selectionColor2.B + 2 * BackColor.B) / 3);
 			
 			if (buffer == null || buffer.Size != ClientSize)
 			{
 				if (buffer != null)
 				{
 					buffer.Dispose();
 				}
 				buffer = new Bitmap(ClientSize.Width, ClientSize.Height);
 			}
 			
 			var graphics = Graphics.FromImage(buffer); 			
 			
 			graphics.Clear(BackColor);						
 			using (var pen = new Pen(SystemColors.WindowFrame))
			{
				var border = ClientRectangle;
				border.Width -= 1;
				border.Height -= 1;
				graphics.DrawRectangle(pen, border);
			}
 			
			using (var boldFont = new Font(Font.Name, Font.Size, FontStyle.Bold))
			using (var selectionBrush = new SolidBrush(selectionColor))			
			using (var onlineBrush = new SolidBrush(Color.Goldenrod))
			using (var offlineBrush = new SolidBrush(Color.DarkGray))
			using (var statusBrush = new SolidBrush(Color.DarkSlateGray))
			{			
				var top = target.Top;
				for (var i = 0; i < items.Count; i++)
				{
					if (items[i] is FriendState)
					{
						var item = items[i] as FriendState;
						var observed = 
							item.ObservedGames.Count > 0 ? item.ObservedGames[0] : 0;
						var played = 
							item.PlayedGames.Count > 0 ? item.PlayedGames[0] : 0;
						
						var status = 
							played > 0 	 ? ("Game " + played.ToString()) :
							observed > 0 ? ("Game " + observed.ToString()) :
										   "";
						var icon =
							played > 0   ? playIcon :
							observed > 0 ? observeIcon :
										   null;
						
						var nameSize = graphics.MeasureString(item.Name, boldFont);
						var statusSize = graphics.MeasureString(status, Font);
						
						var height = Math.Max((int) Math.Ceiling(nameSize.Height),
						                      playIcon.Height);
						
						if (top + height > target.Top && 
						    top < target.Bottom)
						{							
							if (SelectedIndex == i)
							{					
								if (GDIExtensions.GradientFillSupported)
								{
									GDIExtensions.GradientFill(
										graphics, 
										new Rectangle(target.Left, top, 
										              target.Width, height),
										blendColor, selectionColor, GradientDirection.Horizontal);
								}
								else
								{
									graphics.FillRectangle(
										selectionBrush, target.Left, top,
										target.Width, height);
								}
							}			
							
							graphics.DrawString(
								item.Name,
								boldFont,
								item.Online ? onlineBrush : offlineBrush,
								target.Left, top);
							
							if (status != "")
							{
								graphics.DrawString(
									status,
									Font, statusBrush,
									target.Right - statusSize.Width -
										2 * icon.Width, 
									top);
								
								graphics.DrawIcon(
									icon,
									target.Right - icon.Width,
									top);
							}													
						}
						top += height;
					}
				}
			}
			args.Graphics.DrawImage(buffer, 0, 0);
		}
 	}
 	
 	public class FriendView: FormView
 	{
 		private CustomListBox friendList;
 		private IGSFriendManager friendManager;
 		 		
 		private ToolBarButton chatButton;
 		private ToolBarButton matchButton;
 		private ToolBarButton observeButton;
 		private ToolBarButton removeButton;
 		
 		public event Action<string> ChatRequested;
 		public event Action<string> MatchRequested;
 		public event Action<int> ObserveRequested;
 		
 		public FriendView(Size size, 
 		                  IGSFriendManager friendManager): base(size)
 		{ 		
 			this.friendManager = friendManager;
 			
 			
 			friendList = new CustomListBox()
 			{
 				Dock = DockStyle.Fill
 			}; 			
 			Controls.Add(friendList); 			 			
 			Update(); 			 	
 			
 			var buttons = new List<ToolBarButton>()
 			{
 				chatButton, observeButton,
 				matchButton, removeButton
 			};
 			
 			friendList.SelectedIndexChanged += delegate 
 			{
 				if (friendList.SelectedIndex < 0)
 				{
 					buttons.ForEach(button => button.Enabled = false);
 				}
 				else
 				{
 					var friend = friendList.SelectedItem as FriendState;
 					removeButton.Enabled = true;
 					chatButton.Enabled = friend.Online;
 					matchButton.Enabled = friend.Online;
 					observeButton.Enabled = 
 						friend.ObservedGames.Count > 0 ||
 						friend.PlayedGames.Count > 0;
 				}
 			};
 		}
 		
 		protected override void OnInit()
		{			
 			removeButton = new ToolBarButton() 
 			{
 				ToolTipText = "Remove friend",
 				Enabled = false
 			};
 			
 			observeButton = new ToolBarButton()
 			{
 				ToolTipText = "Observe game",
 				Enabled = false
 			};
 			
 			chatButton = new ToolBarButton()
 			{
 				ToolTipText = "Chat",
 				Enabled = false
 			};
 			
 			matchButton = new ToolBarButton()
 			{
 				ToolTipText = "Match",
 				Enabled = false
 			};
 			
			Tools = new Tool[] 
			{
				new Tool(new ToolBarButton() {ToolTipText = "Add friend"}, 
				         ConfigManager.GetIcon("add"), 
				         OnAddButtonClick),
				new Tool(removeButton,
				         ConfigManager.GetIcon("remove"), 
				         OnRemoveButtonClick),
				new Tool(observeButton, 
				         ConfigManager.GetIcon("observe"), 
				         OnObserveButtonClick),
				new Tool(chatButton, 
				         ConfigManager.GetIcon("talk"), 
				         OnChatButtonClick),
				new Tool(matchButton, 
				         ConfigManager.GetIcon("match"), 
				         OnMatchButtonClick)
			};
		}
 		
 		private void OnAddButtonClick(object sender, EventArgs args)
 		{
 			var input = new StringInput();
			var dialog = new ValueDialog<StringInput>("Player name: ", input);
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				friendManager.Add(input.Text);
			}
 		}
 		
 		private void OnRemoveButtonClick(object sender, EventArgs args)
 		{
 			if (friendList.SelectedIndex >= 0)
 			{ 				
 				friendManager.Remove(
 					(friendList.SelectedItem as FriendState).Name); 				
 			}
 		}
 		
 		private void OnObserveButtonClick(object sender, EventArgs args)
 		{
 			if (friendList.SelectedIndex >= 0)
 			{
 				var friend = friendList[friendList.SelectedIndex] as FriendState;
 				var game = 
 					friend.PlayedGames.Count > 0 ? 
 						friend.PlayedGames[0] :
 					friend.ObservedGames.Count > 0 ?
 						friend.ObservedGames[0] :
 						0;
 				
 				if (game > 0)
 				{
 					OnObserveRequested(game);
 				} 				
 			}
 		}
 		
 		public void OnObserveRequested(int gameNumber)
 		{
 			if (ObserveRequested != null)
 			{
 				ObserveRequested(gameNumber);
 			}
 		}
 		
 		public void OnChatButtonClick(object sender, EventArgs args)
 		{
 			if (friendList.SelectedIndex >= 0)
 			{
 				var friend = friendList.SelectedItem as FriendState;
 				if (friend.Online)
 				{
 					OnChatRequested(friend.Name);
 				} 			
 			}
 		}
 		
 		private void OnChatRequested(string name)
 		{
 			if (ChatRequested != null)
 			{
 				ChatRequested(name);
 			}
 		}
 		
 		public void OnMatchButtonClick(object sender, EventArgs args) 			
 		{
 			if (friendList.SelectedIndex >= 0)
 			{
 				var friend = friendList.SelectedItem as FriendState;
 				if (friend.Online)
 				{
 					OnMatchRequested(friend.Name);
 				} 			
 			}
 		}
 		
 		public void OnMatchRequested(string name)
 		{
 			if (MatchRequested != null)
 			{
 				MatchRequested(name);
 			}
 		}
 		
 		public void Update()
 		{
 			friendList.Clear();
 			foreach (var friend in friendManager.FriendStates)
 			{
 				friendList.Add(friend);
 			} 	 			 			
 		}
 	}
 }