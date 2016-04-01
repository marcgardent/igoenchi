using System;
using System.Collections.Generic;
using System.Text;

namespace IGoEnchi
{		
	public delegate void IGSFileHandler(string fileName, string content);
	
	public class IGSFileReceivedEventArgs: EventArgs
	{
		public string Name {get; private set;}
		public string Content {get; private set;}
		
		public IGSFileReceivedEventArgs(string name, string content)
		{
			Name = name;
			Content = content;
		}
	}
	
	public class IGSFileManager
	{						
		private IGSClient client;
		private List<string> expectedFiles;
		private int unknownFiles = 0;
		
		public event IGSFileHandler FileReceived;
		
		public IGSFileManager(IGSClient client)
		{
			if (client == null)
			{
				throw new Exception("Argument cannot be null");
			}
			client.AddHandler(IGSMessages.File, ReadFile);
			
			this.client = client;
			expectedFiles = new List<string>();
		}
		
		public void RequestFile(string name)
		{			
			RequestFile(name, "Help on " + name);
		}
		
		public void RequestFile(string name, string caption)
		{
			client.WriteLine("help " + name);			
			expectedFiles.Add(caption);
		}

		private void ReadFile(List<string> lines)
		{
			var name = String.Empty;
			if (expectedFiles.Count > 0)
			{
				name = expectedFiles[0];
				expectedFiles.RemoveAt(0);
			}
			else
			{
				unknownFiles += 1;					
				name = "Unknown File " + unknownFiles;
			}
			var text = new StringBuilder();
			for (var i = 1; i < lines.Count - 1; i++)
			{				
				text.Append(lines[i]);				
				text.Append("\r\n");
			}
			OnFileReceived(name, text.ToString());
		}
		
		private void OnFileReceived(string fileName, string content)
		{
			if (FileReceived != null)
			{
				FileReceived(fileName, content);
			}
		}
	}
}

