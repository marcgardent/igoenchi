using System.Windows.Forms;

namespace IGoEnchi
{
    public interface IInputGenerator
    {
        Control GenerateInput();
        void ReadInput();
    }
}