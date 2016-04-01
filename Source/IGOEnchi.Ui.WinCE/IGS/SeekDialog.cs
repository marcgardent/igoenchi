using System.Windows.Forms;

namespace IGoEnchi
{
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
}