using System;
using System.Windows.Forms;

namespace IGoEnchi
{
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
}