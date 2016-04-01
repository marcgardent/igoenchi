using System.Windows.Forms;

namespace IGoEnchi
{
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
}