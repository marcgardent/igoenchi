using System.Windows.Forms;

namespace IGoEnchi
{
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
}