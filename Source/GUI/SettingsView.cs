using System;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.Collections.Generic;
using System.Windows.Forms;

namespace IGoEnchi
{
	public class SettingsView: FormView
	{
		private ListBox accountsBox;
		private Button addAccountButton;
		private Button editAccountButton;
		private Button removeAccountButton;
		private ComboBox fontBox;
		private ComboBox encodingBox;
		private RadioButton gdiButton;
		private RadioButton direct3DButton;
		private CheckBox textureBox;
		private CheckBox cursorBox;
		private CheckBox moveMarkBox;
		private CheckBox friendsBox;
		private CheckBox chatBox;
		private CheckBox soundBox;
		private ComboBox[] bindings;		
		private TabControl tabs;
		
		public SettingsView(Size size): base(size)
		{		
			tabs = new TabControl()
			{
				Dock = DockStyle.Fill
			};																													
			
			tabs.TabPages.Add(CreateAccountPage());
			tabs.TabPages.Add(CreateDisplayPage());			
			tabs.TabPages.Add(CreateNotificationPage());
			tabs.TabPages.Add(CreateButtonsPage());
			
			Controls = new List<Control>()
			{				
				tabs
			};
		}				
		
		private TabPage CreateAccountPage()
		{
			accountsBox = new ListBox();
			GetAccounts(accountsBox);
			
			addAccountButton = Layout.Button("Add");
			editAccountButton = Layout.Button("Edit");
			removeAccountButton = Layout.Button("Remove");
			
			addAccountButton.Click += delegate 
			{
				var dialog = new AccountDialog();
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					var account = dialog.Account;
					if (ConfigManager.Settings.Accounts.Find(
						a => a.Name == account.Name) == null)
					{
						ConfigManager.Settings.Accounts.Add(account);
						accountsBox.Items.Add(account.Name);
						accountsBox.SelectedItem = account.Name;
					}					
				}
			};
			
			editAccountButton.Click += delegate 
			{
				var index = accountsBox.SelectedIndex;
				if (index >= 0)
				{
					var name = accountsBox.SelectedItem as string;
					var dialog = new AccountDialog(
						ConfigManager.Settings.Accounts.Find(
							a => a.Name == name) ?? IGSAccount.DefaultAccount);
					if (dialog.ShowDialog() == DialogResult.OK)
					{
						var account = dialog.Account;
						
						ConfigManager.Settings.Accounts.RemoveAll(						
							a => a.Name == name);
						ConfigManager.Settings.Accounts.Insert(index, account);
						accountsBox.Items[index] = account.Name;
					}
				}
			};
			
			removeAccountButton.Click += delegate 
			{
				var index = accountsBox.SelectedIndex;
				if (index >= 0)
				{
					ConfigManager.Settings.Accounts.RemoveAt(index);
					accountsBox.Items.RemoveAt(index);
					if (ConfigManager.Settings.Accounts.Count == 0)
					{
						ConfigManager.Settings.Accounts.Add(IGSAccount.DefaultAccount);
						accountsBox.Items.Add(IGSAccount.DefaultAccount.Name);
					}
					if (accountsBox.SelectedIndex < 0)
					{
						accountsBox.SelectedIndex = accountsBox.Items.Count - 1;
					}
				}
			};
			
			var accountTable = new LayoutTable(2, 1);
			accountTable.PutControl(accountsBox, 0, 0);
			accountTable.PutLayout(
				Layout.Flow(addAccountButton, 
				            editAccountButton,
				            removeAccountButton),
				1, 0);
			accountTable.FixRows(1);
			
			var page = new TabPage()
			{
				Text = "Account",
				Dock = DockStyle.Fill
			};
			Layout.Bind(accountTable, page);
			return page;
		}
		
		private TabPage CreateDisplayPage()
		{
			fontBox = new ComboBox();
			GetFonts(fontBox);	
			
			encodingBox = new ComboBox();			
			GetEncodings(encodingBox);									
			
			gdiButton = new RadioButton()
			{
				Text = "GDI",				
				Checked = ConfigManager.Settings.Renderer == RendererType.GDI,				
			};
			direct3DButton = new RadioButton()
			{
				Text = "Direct3D",				
				Checked = ConfigManager.Settings.Renderer == RendererType.Direct3D,				
			};		
			
			textureBox = new CheckBox()
			{
				Text = "No textures (GDI only)",
				Checked = ConfigManager.Settings.NoTextures
			};
			
			cursorBox = new CheckBox()
			{
				Text = "Keep cursor on board",				
				Checked = ConfigManager.Settings.KeepCursor
			};
			
			moveMarkBox = new CheckBox()
			{
				Text = "Use old last move mark",
				Checked = ConfigManager.Settings.OldMark
			};
				
			foreach (var item in new Control[] {
			         	fontBox, encodingBox, gdiButton, direct3DButton, 
			         	cursorBox, moveMarkBox, textureBox})
			{
				Layout.Resize(item);
			}
			
			var displayTable = Layout.Stack(null, cursorBox, moveMarkBox, textureBox);
			displayTable.PutLayout(
				Layout.PropertyTable(
					Layout.Label("Font:"), fontBox,
					Layout.Label("Encoding:"), encodingBox,
					Layout.Label("Renderer:"), gdiButton,
					null, direct3DButton), 
				0, 0);
						
			var page = new TabPage() 
			{
				Text = "Display",
				Dock = DockStyle.Fill
			};
			Layout.Bind(displayTable, page);			
			return page;
		}
		
		private TabPage CreateNotificationPage()
		{
			friendsBox = new CheckBox()
			{
				Text = "Friends' state changes",				
				Checked = ConfigManager.Settings.FriendsNotify,
			};			
			chatBox = new CheckBox()
			{				
				Text = "In-game chat messages arrive",								
				Checked = ConfigManager.Settings.ChatNotify,
			};
			soundBox = new CheckBox()
			{
				Text = "Enable",
				Checked = ConfigManager.Settings.Sound
			};
			
			foreach (var item in new [] {friendsBox, chatBox, soundBox})
			{
				Layout.Resize(item);
			}
			
			var notificationTable = Layout.Stack(
				Layout.Label("Notify when:"),
				friendsBox,
				chatBox,
				Layout.Label("Sound:"),
				soundBox);

			var page = new TabPage() 
			{
				Text = "Notifications",
				Dock = DockStyle.Fill
			};
			Layout.Bind(notificationTable, page);			
			return page;
		}
		
		private TabPage CreateButtonsPage()
		{
			bindings = Collection.ToArray(
				Collection.Range(4, () => new ComboBox()));
			var actions = HardwareButtonBindings.GetActions();
			var index = 0;
			foreach (var item in bindings)
			{
				Layout.Resize(item);
				item.Items.Add("None");
				foreach (var name in actions)
				{
					item.Items.Add(name);
				}
				var selected = HardwareButtonBindings.GetBinding(HardwareButtonBindings.Buttons[index]);
				item.SelectedItem = 
					(selected != string.Empty && item.Items.Contains(selected)) ? 
						selected : "None";
				index += 1;
			}
			
			var buttonsTable = Layout.Stack(
				Layout.Label("Hardware button 1"), bindings[0],
				Layout.Label("Hardware button 2"), bindings[1],
				Layout.Label("Hardware button 3"), bindings[2],
				Layout.Label("Hardware button 4"), bindings[3]);			
			
			var page = new TabPage() 
			{
				Text = "Buttons",
				Dock = DockStyle.Fill
			};
			Layout.Bind(buttonsTable, page);						
			return page;
		}
		
		private void OnApplyClick(object sender, EventArgs args)
		{
			if (accountsBox.SelectedIndex >= 0)
			{
				ConfigManager.Settings.CurrentAccountName = 
					accountsBox.SelectedItem as string;
			}
			
			if (fontBox.SelectedIndex >= 0)
			{
				var name = fontBox.Items[fontBox.SelectedIndex] as string;					
				if (name != ConfigManager.Settings.DefaultFontName)
				{
					ConfigManager.Settings.DefaultFontName = name;					
				}
			}
			
			if (encodingBox.SelectedIndex >= 0)
			{
				var name = encodingBox.Items[encodingBox.SelectedIndex] as string;					
				if (name != ConfigManager.Settings.DefaultEncodingName)
				{
					ConfigManager.Settings.DefaultEncodingName = name;					
				}
			}
			
			if (gdiButton.Checked)
			{
				ConfigManager.ChangeRenderer(RendererType.GDI);
			}
			else if (direct3DButton.Checked)
			{
				ConfigManager.ChangeRenderer(RendererType.Direct3D);
			}
						
			ConfigManager.Settings.NoTextures = textureBox.Checked;
			ConfigManager.Settings.KeepCursor = cursorBox.Checked;
			ConfigManager.Settings.OldMark = moveMarkBox.Checked;
			ConfigManager.Settings.FriendsNotify = friendsBox.Checked;
			ConfigManager.Settings.ChatNotify = chatBox.Checked;	
			ConfigManager.Settings.Sound = soundBox.Checked;
			
			for (var i = 0; i < bindings.Length; i++)
			{
				if ((string) bindings[i].SelectedItem != "None")
				{
					HardwareButtonBindings.Bind(
						HardwareButtonBindings.Buttons[i],
						(string) bindings[i].SelectedItem);
				}
				else
				{
					HardwareButtonBindings.Unbind(
						HardwareButtonBindings.Buttons[i]);
				}
			}
			
			ConfigManager.Settings.ButtonBindings = 
				HardwareButtonBindings.GetBindings();
			
			if (Container != null)
			{
				Container.RemoveView(this);
			}
			
			ConfigManager.Save();
		}
		
		protected override void OnInit()
		{						
			Tools = new Tool[] 
			{
				new Tool(new ToolBarButton() {ToolTipText = "Apply"}, 
				         ConfigManager.GetIcon("ok"), 
				         OnApplyClick)				
			};
		}	

		private void GetAccounts(ListBox list)
		{
			foreach (var account in ConfigManager.Settings.Accounts)
			{
				list.Items.Add(account.Name);
			}
			list.SelectedItem = ConfigManager.Settings.CurrentAccountName;
			if (list.SelectedIndex < 0 && list.Items.Count > 0)
			{
				list.SelectedIndex = 0;
			}
		}
		
		private void GetFonts(ComboBox list)
		{
			using (var collection = new InstalledFontCollection())
			{
				foreach (var family in collection.Families)
				{								
					list.Items.Add(family.Name);
					if (family.Name == ConfigManager.Settings.DefaultFontName)
					{
						list.SelectedIndex = list.Items.Count - 1;
					}
				}
			}
		}
		
		private void GetEncodings(ComboBox list)
		{
			var encodings = new List<string>
			{
				/*"IBM437", "ibm737",	"ibm775", "ibm850",
				"ibm852", "IBM855",	"ibm857", "cp866",*/
				
				"windows-1250",	"windows-1251",	"Windows-1252",
				"windows-1253",	"windows-1254", "windows-1257",
				
				"us-ascii",
				
				"koi8-r", "koi8-u",
				
				"iso-8859-2", "iso-8859-3",	"iso-8859-4", 
				"iso-8859-5", "iso-8859-7",
				
				"utf-7", "utf-8"
			};
			foreach (var encoding in encodings)
			{						
				try 
				{
					Encoding.GetEncoding(encoding);
					list.Items.Add(encoding);
					if (encoding == ConfigManager.Settings.DefaultEncodingName)
					{
						list.SelectedIndex = list.Items.Count - 1;
					}
				}
				catch (Exception) {}				
			}
		}		
	}
}