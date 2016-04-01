using System;
using System.Windows.Forms;

namespace IGoEnchi
{
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
            IgsPlayerInfo stats): this(request)
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
}