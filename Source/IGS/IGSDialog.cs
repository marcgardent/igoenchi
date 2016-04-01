using System;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.WindowsCE.Forms;

namespace IGoEnchi
{
	public class IGSDialog: Form
	{
		private MainMenu mainMenu;
		
		public IGSDialog(): base()
		{
			mainMenu = new MainMenu();
			
			MenuItem menuItem = new MenuItem() {Text = "OK"};
			menuItem.Click += new EventHandler(OKButtonClick);
			mainMenu.MenuItems.Add(menuItem);
			
			menuItem = new MenuItem() {Text = "Cancel"};
			menuItem.Click += new EventHandler(CancelButtonClick);
			mainMenu.MenuItems.Add(menuItem);

			Menu = mainMenu;

			ControlBox = false;
		}
		
		private void OKButtonClick(object sender, EventArgs args)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
		
		private void CancelButtonClick(object sender, EventArgs args)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
		
		public new void Show()
		{
			throw new NotSupportedException("IGSDialog.Show() isn't supported");
		}
		
		public void ShowInputPanel()
		{
			InputPanel inputPanel = new InputPanel();
			inputPanel.Enabled = true;
		}
		
		public void HideInputPanel()
		{
			InputPanel inputPanel = new InputPanel();
			inputPanel.Enabled = false;
		}
		
		public new DialogResult ShowDialog()
		{
			if (Environment.OSVersion.Platform == PlatformID.WinCE)
			{
				try
				{
					ShowInputPanel();
				}
				catch (Exception) {}
			}
			return (this as Form).ShowDialog();
		}
		
		public new DialogResult OnClosed(EventArgs args)
		{
			if (Environment.OSVersion.Platform == PlatformID.WinCE)
			{
				try
				{
					HideInputPanel();
				}
				catch (Exception) {}
			}
			return (this as Form).ShowDialog();
		}
	}
	
	public class AccountDialog: IGSDialog
	{
		private Func<IGSAccount> getAccount;
		
		public IGSAccount Account
		{
			get {return getAccount();}
		}
		
		public AccountDialog(): this(IGSAccount.DefaultAccount)	{}
		
		public AccountDialog(IGSAccount account): base()
		{
			var hostTextBox = new TextBox()
			{
				Text = account.Server
			};
			var portUpDown = new NumericUpDown()
			{
				Minimum = 0,
				Maximum = UInt16.MaxValue,
				Value = account.Port
			};
			var nameTextBox = new TextBox()
			{
				MaxLength = 10,
				Text = account.Name
			};
			var passwordTextBox = new TextBox()
			{
				PasswordChar = '*',
				Text = account.Password
			};
			var savePasswordCheckBox = new CheckBox()
			{
				Text = "Save Password (plaintext)?",
				Checked = account.PasswordSpecified
			};
			
			getAccount = () =>
				new IGSAccount(hostTextBox.Text,
				               Convert.ToInt32(portUpDown.Value),
				               nameTextBox.Text,
				               passwordTextBox.Text,
				               savePasswordCheckBox.Checked);
			
			foreach (var item in new Control[] {hostTextBox , portUpDown, nameTextBox, passwordTextBox, savePasswordCheckBox})
			{
				Layout.Resize(item);
			}
			
			Layout.Bind(
				Layout.PropertyTable(
					Layout.Label("Host:"), hostTextBox,
					Layout.Label("Port:"), portUpDown,
					Layout.Label("Name:"), nameTextBox,
					Layout.Label("Password:"), passwordTextBox,
					null, savePasswordCheckBox),
				this);
		}
	}
	
	public interface IInputGenerator
	{
		Control GenerateInput();
		void ReadInput();
	}
	
	public class StringInput: IInputGenerator
	{
		public string Text {get; set;}
		private TextBox control;
		
		public Control GenerateInput()
		{
			control = new TextBox() {Text = Text};
			return control;
		}
		
		public void ReadInput()
		{
			Text = control.Text;
		}
	}
	
	public class IntInput: IInputGenerator
	{
		public int Value {get; set;}
		public int MinValue {get; set;}
		public int MaxValue {get; set;}
		private NumericUpDown control;
		
		public IntInput()
		{
			MinValue = int.MinValue;
			MaxValue = int.MaxValue;
		}
		
		public Control GenerateInput()
		{
			control = new NumericUpDown()
			{
				Value = Value,
				Minimum = MinValue,
				Maximum = MaxValue
			};
			return control;
		}
		
		public void ReadInput()
		{
			Value = (int) control.Value;
		}
	}
	
	public class FloatInput: IInputGenerator
	{
		public float Value {get; set;}
		public float MinValue {get; set;}
		public float MaxValue {get; set;}
		public float Step {get; set;}
		private NumericUpDown control;
		
		public FloatInput()
		{
			MinValue = float.MinValue;
			MaxValue = float.MaxValue;
			Step = 1;
		}
		
		public Control GenerateInput()
		{
			control = new NumericUpDown()
			{
				Value = (decimal) Value,
				Minimum = (decimal) MinValue,
				Maximum = (decimal) MaxValue,
				Increment = (decimal) 0.5f
			};
			return control;
		}
		
		public void ReadInput()
		{
			Value = (float) control.Value;
		}
	}
	
	public class ValueDialog<TValue>: IGSDialog where TValue: IInputGenerator
	{
		public ValueDialog(string caption, TValue generator)
		{
			var input = generator.GenerateInput();
			Layout.Resize(input);
			Layout.Bind(Layout.PropertyTable(Layout.Label(caption), input),
			            this);			             
			
			this.Closing += delegate {generator.ReadInput();};
		}
	}
	
	public class MatchDialog: IGSDialog
	{
		private TextBox userNameTextBox;
		private RadioButton blackButton;
		private RadioButton whiteButton;
		private NumericUpDown boardSizeUpDown;
		private NumericUpDown timeUpDown;
		private NumericUpDown byouyomiUpDown;
		private Notification notification;
		private LayoutTable table;
				
		public MatchRequest Request
		{
			get
			{
				return new MatchRequest(userNameTextBox.Text,
				                        Color,
				                        (int) boardSizeUpDown.Value,
				                        (int) timeUpDown.Value,
				                        (int) byouyomiUpDown.Value);
			}
		}
		
		private string Color
		{
			get {return blackButton.Checked ? "B" : "W";}
		}
		
		public MatchDialog(): base()
		{
			userNameTextBox = new TextBox()
			{
				MaxLength = 10
			};			
			blackButton = new RadioButton()
			{
				Text = "Black",
				Checked = true,
				Height = userNameTextBox.Height
			};
			whiteButton = new RadioButton()
			{
				Text = "White",
				Height = userNameTextBox.Height
			};
			boardSizeUpDown = new NumericUpDown()
			{
				Minimum = 2,
				Maximum = 19,
				Value = 19
			};
			timeUpDown = new NumericUpDown()
			{
				Minimum = 0,
				Maximum = 1000,
				Value = 30
			};
			byouyomiUpDown = new NumericUpDown()
			{
				Minimum = 0,
				Maximum = 1000
			};
			
			foreach (var item in new Control[] 
			         {
			         	userNameTextBox, blackButton, whiteButton, 
			         	boardSizeUpDown, timeUpDown, byouyomiUpDown
			         })
			{
				Layout.Resize(item);
			}
			
			table = Layout.PropertyTable(
				Layout.Label("Player name:"), userNameTextBox,
				Layout.Label("Your color:"), blackButton,
				null, whiteButton,
				Layout.Label("Board size:"), boardSizeUpDown,
				Layout.Label("Base time:"), timeUpDown,
				Layout.Label("Byouyomi:"), byouyomiUpDown);
						
			Layout.Bind(table, this);
		}
		
		public MatchDialog(MatchRequest request): this()
		{
			userNameTextBox.Text = request.OpponentName;
			userNameTextBox.Enabled = false;
			if (request.Color == "W")
			{
				whiteButton.Checked = true;
			}
			boardSizeUpDown.Value = request.BoardSize;
			timeUpDown.Value = request.BaseTime;
			byouyomiUpDown.Value = request.Byouyomi;
		}
		
		public MatchDialog(MatchRequest request,
		                   IGSPlayerInfo stats): this(request)
		{
			if (Environment.OSVersion.Platform == PlatformID.WinCE)
			{	
				userNameTextBox.Width = 0;
				var statsButton = new Button() 
				{
					Text = "?", 
					Height = userNameTextBox.Height,
					Width = userNameTextBox.Height
				};
				
				var innerTable = new LayoutTable(1, 2);
				innerTable.Fill(userNameTextBox, statsButton);
				innerTable.FixRows();
				innerTable.FixColumns(1);
				
				table.PutLayout(innerTable, 0, 1);
				table.Apply(this, this.ClientRectangle);
				
				statsButton.Click += delegate {notification.Visible = true;};
				
				notification = new Notification()
				{
					Text = "<html><body><form method='GET' action='notify'><hr/>" +
						"Name: " + stats.Name +
						"<br/>Rank: " + stats.Rank.ToString() +
						"<br/>Wins/Losses: " + stats.GamesWon + "/" +
						stats.GamesLost +
						"<hr/></form></body></html>",
					Caption = "Stats of " + stats.Name,
					InitialDuration = 10,
					Icon = ConfigManager.GetIcon("game")
				};
			}
		}
		
		public new DialogResult ShowDialog()
		{
			userNameTextBox.Focus();
			DialogResult result = (this as IGSDialog).ShowDialog();
			if (notification != null)
			{
				notification.Visible = false;
				notification.Dispose();
			}
			return result;
		}
	}

	public class NMatchDialog: IGSDialog
	{
		private TextBox userNameTextBox;
		private RadioButton blackButton;
		private RadioButton whiteButton;
		private RadioButton nigiriButton;
		private NumericUpDown boardSizeUpDown;
		private NumericUpDown handicapUpDown;
		private NumericUpDown timeUpDown;
		private NumericUpDown byouyomiTimeUpDown;
		private NumericUpDown byouyomiStonesUpDown;		
		private Notification notification;
		private LayoutTable table;
		
		public NMatchRequest Request
		{
			get
			{
				return new NMatchRequest(userNameTextBox.Text,
				                         Color,
				                         (int) handicapUpDown.Value,
				                         (int) boardSizeUpDown.Value,
				                         (int) timeUpDown.Value * 60,
				                         (int) byouyomiTimeUpDown.Value * 60,
				                         (int) byouyomiStonesUpDown.Value);
			}
		}
		
		private string Color
		{
			get
			{
				if (blackButton.Checked)
				{
					return "B";
				}
				else if (whiteButton.Checked)
				{
					return "W";
				}
				else
				{
					return "N";
				}
			}
		}
		
		public NMatchDialog(): base()
		{
			userNameTextBox = new TextBox()
			{				
				MaxLength = 10
			};		
			blackButton = new RadioButton()
			{
				Text = "Black",				
				Checked = true,
				Height = userNameTextBox.Height
			};
			whiteButton = new RadioButton()
			{
				Text = "White",
				Height = userNameTextBox.Height				
			};
			nigiriButton = new RadioButton()
			{
				Text = "Nigiri",				
				Height = userNameTextBox.Height				
			};			
			handicapUpDown = new NumericUpDown()
			{
				Minimum = 0,
				Maximum = 9,
				Value = 0				
			};			
			boardSizeUpDown = new NumericUpDown()
			{
				Minimum = 2,
				Maximum = 19,
				Value = 19			
			};						
			timeUpDown = new NumericUpDown()
			{
				Minimum = 0,
				Maximum = 15,
				Value = 10
			};					
			byouyomiTimeUpDown = new NumericUpDown()
			{
				Minimum = 0,
				Maximum = 60			
			};
			byouyomiStonesUpDown = new NumericUpDown()
			{
				Minimum = 0,
				Maximum = 25,
				Value = 25				
			};
			
			foreach (var item in new Control[] 
			         {
			         	userNameTextBox, blackButton, whiteButton, nigiriButton,
			         	handicapUpDown, boardSizeUpDown, 
			         	timeUpDown, byouyomiTimeUpDown, byouyomiStonesUpDown
			         })
			{
				Layout.Resize(item);
			}
			
			table = Layout.PropertyTable(
				Layout.Label("Player name:"), userNameTextBox,
				Layout.Label("Your color:"), blackButton,
				null, whiteButton,				
				null, nigiriButton,
				Layout.Label("Handicap:"), handicapUpDown,
				Layout.Label("Board size:"), boardSizeUpDown,
				Layout.Label("Base time:"), timeUpDown,
				Layout.Label("Byouyomi:"), byouyomiTimeUpDown,
				Layout.Label("Stones:"), byouyomiStonesUpDown);
			
			Layout.Bind(table, this);
		}
		
		public NMatchDialog(NMatchRequest request): this()
		{
			userNameTextBox.Text = request.OpponentName;
			userNameTextBox.Enabled = false;
			(request.Color == "B" ? blackButton :
			 request.Color == "W" ? whiteButton :
			 nigiriButton).Checked = true;
			handicapUpDown.Value = request.Handicap;
			boardSizeUpDown.Value = request.BoardSize;
			timeUpDown.Value = request.BaseTime / 60;
			byouyomiTimeUpDown.Value = request.ByouyomiTime / 60;
			byouyomiStonesUpDown.Value = request.ByouyomiStones;
		}
		
		public NMatchDialog(NMatchRequest request,
		                    IGSPlayerInfo stats): this(request)
		{
			if (Environment.OSVersion.Platform == PlatformID.WinCE)
			{
				userNameTextBox.Width = 0;
				
				var statsButton = new Button()
				{					
					Width = userNameTextBox.Height,
					Height = userNameTextBox.Height,
					Text = "?"
				};
			
				var innerTable = new LayoutTable(1, 2);
				innerTable.Fill(userNameTextBox, statsButton);
				innerTable.FixRows();
				innerTable.FixColumns(1);
				
				table.PutLayout(innerTable, 0, 1);
				table.Apply(this, this.ClientRectangle);
				
				statsButton.Click += delegate {notification.Visible = true;};
				notification = new Notification()
				{
					Text = "<html><body><form method='GET' action='notify'><hr/>" +
						"Name: " + stats.Name +
						"<br/>Rank: " + stats.Rank.ToString() +
						"<br/>Wins/Losses: " + stats.GamesWon + "/" +
						stats.GamesLost +
						"<hr/></form></body></html>",
					Caption = "Stats of " + stats.Name,
					InitialDuration = 10,
					Icon = ConfigManager.GetIcon("game")
				};
			}
		}
		
		public new DialogResult ShowDialog()
		{
			userNameTextBox.Focus();
			DialogResult result = (this as IGSDialog).ShowDialog();
			if (notification != null)
			{
				notification.Visible = false;
				notification.Dispose();
			}
			return result;
		}
	}
	
	public class SeekDialog: IGSDialog
	{		
		private RadioButton[] timeRadioButtons;
		private NumericUpDown boardSizeUpDown;
		private NumericUpDown handicapUpDown;
				
		public SeekRequest Request
		{
			get {return new SeekRequest(TimeSettings,
			                            (int) boardSizeUpDown.Value,
			                            (int) handicapUpDown.Value);}
		}
		
		private int TimeSettings
		{
			get
			{
				for (int i = 0; i < timeRadioButtons.Length; i++)
				{
					if (timeRadioButtons[i].Checked)
					{
						return i;
					}
				}
				return 0;
			}
		}
		
		public SeekDialog(): base()
		{
			boardSizeUpDown = new NumericUpDown()
			{
				Minimum = 2,
				Maximum = 19,
				Value = 19		
			};	
			handicapUpDown = new NumericUpDown()
			{
				Minimum = 0,
				Maximum = 9,
				Value = 0				
			};			
			foreach (var item in new Control[] {boardSizeUpDown, handicapUpDown})
			{
				Layout.Resize(item);
			}
			
			timeRadioButtons = Collection.ToArray(Collection.Map(
				SeekRequest.TimeLabels,
				s => new RadioButton() 
				{
					Text = s,
					Height = boardSizeUpDown.Height
				}));
			
			
			
			var table = new LayoutTable(timeRadioButtons.Length + 1, 1);			
			table.Fill(timeRadioButtons);
			
			table.PutLayout(Layout.PropertyTable(
				Layout.Label("Board size:"), boardSizeUpDown,
				Layout.Label("Handicap:"), handicapUpDown),
				timeRadioButtons.Length, 0);
			table.FixRows();			
			
			Layout.Bind(table, this);
		}
		
		public new DialogResult ShowDialog()
		{
			return (this as IGSDialog).ShowDialog();
		}
	}
	
	public class ToggleDialog: IGSDialog
	{
		private static string[] toggleLabels = {"Open", "Looking", "Kibitz"};
		
		private CheckBox[] toggleBoxes;
		
		public IGSToggleSettings Settings
		{
			get {return new IGSToggleSettings(toggleBoxes[0].Checked,
			                                  toggleBoxes[1].Checked,
			                                  toggleBoxes[2].Checked);}
		}
		
		public ToggleDialog(IGSToggleSettings settings): base()
		{			
			toggleBoxes = Collection.ToArray(Collection.Map(
				toggleLabels, s => new CheckBox() {Text = s}));
			foreach (var item in toggleBoxes)
			{
				Layout.Resize(item);
			}
			
			toggleBoxes[0].Checked = settings.Open;
			toggleBoxes[1].Checked = settings.Looking;
			toggleBoxes[2].Checked = settings.Kibitz;	
						
			Layout.Bind(Layout.Stack(toggleBoxes), this);
		}
		
		public new DialogResult ShowDialog()
		{
			return (this as IGSDialog).ShowDialog();
		}
	}
}