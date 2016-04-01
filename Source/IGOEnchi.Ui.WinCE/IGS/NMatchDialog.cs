using System;
using System.Windows.Forms;

namespace IGoEnchi
{
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
            IgsPlayerInfo stats): this(request)
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
}