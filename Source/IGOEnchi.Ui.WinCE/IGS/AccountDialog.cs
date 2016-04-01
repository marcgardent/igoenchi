using System;
using System.Windows.Forms;

namespace IGoEnchi
{
    public class AccountDialog: IGSDialog
    {
        private Func<IGSAccount> getAccount;
		
        public IGSAccount Account
        {
            get {return getAccount();}
        }
		
        public AccountDialog(): this(IGSAccount.DefaultAccount)	{}
		
        public AccountDialog(IGSAccount account): base()
        {
            var hostTextBox = new TextBox()
            {
                Text = account.Server
            };
            var portUpDown = new NumericUpDown()
            {
                Minimum = 0,
                Maximum = UInt16.MaxValue,
                Value = account.Port
            };
            var nameTextBox = new TextBox()
            {
                MaxLength = 10,
                Text = account.Name
            };
            var passwordTextBox = new TextBox()
            {
                PasswordChar = '*',
                Text = account.Password
            };
            var savePasswordCheckBox = new CheckBox()
            {
                Text = "Save Password (plaintext)?",
                Checked = account.PasswordSpecified
            };
			
            getAccount = () =>
                new IGSAccount(hostTextBox.Text,
                    Convert.ToInt32(portUpDown.Value),
                    nameTextBox.Text,
                    passwordTextBox.Text,
                    savePasswordCheckBox.Checked);
			
            foreach (var item in new Control[] {hostTextBox , portUpDown, nameTextBox, passwordTextBox, savePasswordCheckBox})
            {
                Layout.Resize(item);
            }
			
            Layout.Bind(
                Layout.PropertyTable(
                    Layout.Label("Host:"), hostTextBox,
                    Layout.Label("Port:"), portUpDown,
                    Layout.Label("Name:"), nameTextBox,
                    Layout.Label("Password:"), passwordTextBox,
                    null, savePasswordCheckBox),
                this);
        }
    }
}