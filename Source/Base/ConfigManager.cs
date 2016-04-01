using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace IGoEnchi
{	
	public enum RendererType {GDI, Direct3D};
	
	public class IGSClientSettings
	{
		private bool Open {get; set;}
		private bool Looking {get; set;}
		private bool Kibitz {get; set;}
		private bool Shout {get; set;}
		
		public IGSClientSettings(bool open, bool looking, bool kibitz, bool shout)
		{
			Open = open;
			Looking = looking;
			Kibitz = kibitz;
			Shout = shout;
		}
		
		public static IGSClientSettings Default
		{
			get {return new IGSClientSettings(true, false, true, true);}
		}
	}
		
	public class SortModes
	{
		[XmlElement("games")]
		public string Games {get; set;}
		[XmlElement("players")]
		public string Players {get; set;}
		
		public static string ActualName(string column)
		{
			return column.EndsWith(" DESC") ?
					column.Substring(0, column.Length - 5) :
					column;			
		}
	}
	
	[XmlRoot("settings")]
	public class Settings
	{
		[XmlElement("font")]
		public string DefaultFontName {get; set;}
		[XmlElement("encoding")]
		public string DefaultEncodingName {get; set;}
		[XmlElement("renderer")]
		public RendererType Renderer {get; set;}
		[XmlElement("keepCursor")]
		public bool KeepCursor {get; set;}
		[XmlElement("oldMark")]
		public bool OldMark {get; set;}
		[XmlElement("noTextures")]
		public bool NoTextures {get; set;}
		[XmlElement("friendsNotify")] 
		public bool FriendsNotify {get; set;}
		[XmlElement("chatNotify")]
		public bool ChatNotify {get; set;}		
		[XmlElement("sound")]
		public bool Sound {get; set;}
		[XmlElement("account")]
		public List<IGSAccount> Accounts {get; set;}
		[XmlElement("currentAccount")]
		public string CurrentAccountName {get; set;}		
		[XmlElement("sortModes")]
		public SortModes SortModes {get; set;}
		[XmlElement("buttonBinding")]
		public string[] ButtonBindings {get; set;}
		[XmlElement("gnuGoSettings")]
		public GnuGoSettings GnuGoSettings {get; set;}
		
		public Settings()
		{
			DefaultFontName = "Tahoma";
			DefaultEncodingName = "utf-8";
			CurrentAccountName = IGSAccount.DefaultAccount.Name;
			Accounts = new List<IGSAccount>();
			Renderer = RendererType.GDI;
			KeepCursor = false;
			FriendsNotify = true;
			ChatNotify = true;
			SortModes = new SortModes()
			{
				Games = "",
				Players = ""
			};
			ButtonBindings = new string[0];
			GnuGoSettings = GnuGoSettings.Default;
		}
		
		[XmlIgnore]
		public IGSAccount CurrentAccount 
		{
			get
			{
				return Accounts.Find(
					account => account.Name == CurrentAccountName)
					?? IGSAccount.DefaultAccount;
			}
		}
	}
	
	public class ConfigManager
	{	
		private static ResourceManager resourceManager = 
			new ResourceManager("IGoEnchi.ToolbarIcons", Assembly.GetExecutingAssembly())
			{IgnoreCase = true};		
		private static IFormatProvider singleFormatProvider = 
			CultureInfo.InvariantCulture.NumberFormat;
		
		public static Font GUIFont {get; private set;}		
		public static bool HiRes {get; set;}
		public static int BoardStorageStep {get; set;}
		public static Settings Settings {get; private set;}				

		private static MainForm mainForm;
		
		public static MainForm MainForm 
		{
			set
			{
				mainForm = value;
				GUIFont = value.Font;
			}
			get
			{
				return mainForm;
			}
		}
		
		public static float StringToFloat(string text)
		{
			return Single.Parse(text, singleFormatProvider);
		}
		
		public static string FloatToString(float value)
		{
			return value.ToString(singleFormatProvider);
		}
		
		public static void Reset()
		{
			HiRes = true;
			BoardStorageStep = 20;
			Settings = new Settings();
		}
		
		public static void Save()
		{						
			try
			{
				using (var textWriter = new StreamWriter(GetFilename()))
				{
					var xmlSerializer = new XmlSerializer(typeof(Settings));				
					xmlSerializer.Serialize(textWriter, Settings);					
				}
			}
			catch (Exception)
			{
				MessageBox.Show("Couldn't write to config.xml");
			}
		}
		
		public static void Load()
		{			
			try
			{				
				var filename = GetFilename();
				if (File.Exists(filename))
				{					
					using (var textReader = new StreamReader(filename))
					{
						var xmlSerializer = new XmlSerializer(typeof(Settings));					
						Settings = (Settings) xmlSerializer.Deserialize(textReader);						
					}
				}
			}
			catch (Exception) 
			{				
				MessageBox.Show("Couldn't read from config.xml");
			}
			
			if (Settings.Accounts.Count == 0)
			{
				Settings.Accounts.Add(IGSAccount.DefaultAccount);
			}
			if (Settings.Accounts.Find(a => a.Name == Settings.CurrentAccountName) == null)
			{
				Settings.CurrentAccountName = Settings.Accounts[0].Name;
			}
		}
		
		private static string GetFilename()
		{			
			return WinCEUtils.PathTo("config.xml");
		}
		
		public static Icon GetIcon(string name)
		{
			var iconName = HiRes ? name + "_hires" : name;			
			return resourceManager.GetObject(iconName) as Icon;	
		}			
				
		public static void ChangeRenderer(RendererType renderer)
		{			
			try
			{
				MainForm.ChangeRenderer(renderer);				
				Settings.Renderer = renderer;
			}
			catch (Exception)
			{						
				MainForm.ChangeRenderer(RendererType.GDI);
				MessageBox.Show("Direct3D doesn't seem to be supported on this device.");
				Settings.Renderer = RendererType.GDI;
			}							
		}
	}
}
