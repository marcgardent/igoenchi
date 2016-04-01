using System;
using System.Windows.Forms;


namespace IGoEnchi
{
	public class IGSDialog: Form
	{
		private MainMenu mainMenu;
		
		public IGSDialog(): base()
		{
			mainMenu = new MainMenu();
			
			MenuItem menuItem = new MenuItem() {Text = "OK"};
			menuItem.Click += new EventHandler(OKButtonClick);
			mainMenu.MenuItems.Add(menuItem);
			
			menuItem = new MenuItem() {Text = "Cancel"};
			menuItem.Click += new EventHandler(CancelButtonClick);
			mainMenu.MenuItems.Add(menuItem);

			Menu = mainMenu;

			ControlBox = false;
		}
		
		private void OKButtonClick(object sender, EventArgs args)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
		
		private void CancelButtonClick(object sender, EventArgs args)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
		
		public new void Show()
		{
			throw new NotSupportedException("IGSDialog.Show() isn't supported");
		}
		
		public void ShowInputPanel()
		{
			InputPanel inputPanel = new InputPanel();
			inputPanel.Enabled = true;
		}
		
		public void HideInputPanel()
		{
			InputPanel inputPanel = new InputPanel();
			inputPanel.Enabled = false;
		}
		
		public new DialogResult ShowDialog()
		{
			if (Environment.OSVersion.Platform == PlatformID.WinCE)
			{
				try
				{
					ShowInputPanel();
				}
				catch (Exception) {}
			}
			return (this as Form).ShowDialog();
		}
		
		public new DialogResult OnClosed(EventArgs args)
		{
			if (Environment.OSVersion.Platform == PlatformID.WinCE)
			{
				try
				{
					HideInputPanel();
				}
				catch (Exception) {}
			}
			return (this as Form).ShowDialog();
		}
	}
}