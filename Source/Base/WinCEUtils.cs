using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
 
namespace IGoEnchi
{
	public static class WinCEUtils
	{
		private enum Flags {
        	SHFS_SHOWTASKBAR = 0x0001,  
        	SHFS_HIDETASKBAR = 0x0002        	
    	}
		
		public static void HideTaskbar(Form form)
		{
			if (Environment.OSVersion.Platform == PlatformID.WinCE)
			{
				try 
				{
					SHFullScreen(form.Handle, Flags.SHFS_HIDETASKBAR);
				}
				catch (Exception) {}
			}
		}
		
		public static void ShowTaskbar(Form form)
		{
			if (Environment.OSVersion.Platform == PlatformID.WinCE)
			{
				try 
				{
					SHFullScreen(form.Handle, Flags.SHFS_SHOWTASKBAR);
				}
				catch (Exception) {}
			}
		}
		
		public static string PathTo(string fileName)
		{
			return GetCurrentDirectory() + @"\" + fileName;				
		}
		
		public static string GetCurrentDirectory()
		{
			return 
				Environment.OSVersion.Platform == PlatformID.WinCE ?
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) :
				Directory.GetCurrentDirectory();
		}
		
		[DllImport("aygshell.dll", EntryPoint="SHFullScreen")]
		private static extern bool SHFullScreen(IntPtr handle, Flags state); 
	}
}

