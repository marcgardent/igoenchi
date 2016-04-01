namespace IGoEnchi
{
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
}