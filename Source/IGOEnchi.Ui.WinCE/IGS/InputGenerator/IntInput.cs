using System.Windows.Forms;

namespace IGoEnchi
{
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
}