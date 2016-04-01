using System.Windows.Forms;

namespace IGoEnchi
{
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