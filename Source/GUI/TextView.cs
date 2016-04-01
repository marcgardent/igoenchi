using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace IGoEnchi
{
	public class TextView: FormView
	{
		private TextBox textBox;
		private StringBuilder buffer;
		private bool threadSafe;
		
		public TextView(Size size): this(size, true){}
		
		public TextView(Size size, bool threadSafe): base(size)
		{
			textBox = new TextBox()
			{
				Multiline = true,
				ReadOnly = true,
				BackColor = Color.White,
				Dock = DockStyle.Fill,
				ScrollBars = ScrollBars.Vertical				
			};			
			Controls.Add(textBox);
			buffer = new StringBuilder(200);
			this.threadSafe = threadSafe;
		}
				
		public override void OnSelect(EventArgs args)
		{
			base.OnSelect(args);
			textBox.SelectionStart = textBox.Text.Length;
			textBox.SelectionLength = 1;			
			textBox.ScrollToCaret();
		}
		
		public void WriteLine(String line)
		{					
			if (threadSafe)
			{
				buffer.Append(line + "\r\n");
				Container.BaseForm.Invoke(new EventHandler(this.FlushBuffer));
			}
			else
			{
				WriteText(line + "\r\n");
			}
		}
		
		private void WriteText(String text)
		{
			textBox.Text += text;
			textBox.SelectionStart = textBox.Text.Length;
			textBox.SelectionLength = 1;			
			textBox.ScrollToCaret();
		}
						
		private void FlushBuffer(object sender, EventArgs args)
		{	
			WriteText(buffer.ToString());
			buffer.Remove(0, buffer.Length);			
		}
	}
	
	public class ConsoleView: TextView
	{
		public ConsoleView(Size size): base(size)
		{						
			CanClose = false;			
		}				
	}
	
	public class FileView: TextView
	{		
		public FileView(Size size, 		                       
		                string content): base(size, false)
		{
			WriteLine(content);
		}					
	}
	
	public class PlayerStatsView: TextView
	{
		private IGSServerInfo serverInfo;
		private string name;
				
		public PlayerStatsView(Size size, 
		                       IGSServerInfo serverInfo, 
		                       string name): base(size)
		{						
			this.serverInfo = serverInfo;
			serverInfo.RequestPlayerStats(name);
			serverInfo.PlayerStatsUpdated += 
				delegate {Container.BaseForm.Invoke(new EventHandler(LoadStats));};
			
			this.name = name;
		}
		
		private void LoadStats(object sender, EventArgs args)
		{
			IGSPlayerInfo stats = serverInfo.PlayerStats;
			if (stats.Name == name)
			{
				WriteLine("Name:\r\n\t" + stats.Name);
				WriteLine("Rank:\r\n\t" + stats.Rank.ToString());
				WriteLine("Wins:\r\n\t" + stats.GamesWon.ToString());
				WriteLine("Losses:\r\n\t" + stats.GamesLost.ToString());
				WriteLine("Language:\r\n\t" + stats.Language);
				WriteLine("County:\r\n\t" + stats.Country);
				WriteLine("Info:\r\n\t" + stats.Info);
			}
		}
	}
}