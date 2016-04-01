using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

namespace IGoEnchi
{
	public static class SoundUtils
	{
		private enum Flags {
        	SND_SYNC = 0x0000,  
        	SND_ASYNC = 0x0001,  
        	SND_NODEFAULT = 0x0002, 
        	SND_MEMORY = 0x0004,  
        	SND_LOOP = 0x0008,  
        	SND_NOSTOP = 0x0010,  
        	SND_NOWAIT = 0x00002000, 
        	SND_ALIAS = 0x00010000,
        	SND_ALIAS_ID = 0x00110000,
        	SND_FILENAME = 0x00020000,
        	SND_RESOURCE = 0x00040004
    	}
		
		private static Dictionary<string, byte[]> sounds = 
			new Dictionary<string, byte[]>();
		
		public static void Play(string sound)
		{
			if (ConfigManager.Settings.Sound &&
			    Environment.OSVersion.Platform == PlatformID.WinCE)
			{
				var data = null as byte[];
				if (!sounds.ContainsKey(sound))
				{	
					var resourceManager = 
						new ResourceManager("IGoEnchi.Sounds", Assembly.GetExecutingAssembly())
						{IgnoreCase = true};				
					data = resourceManager.GetObject(sound) as byte[];
					sounds[sound] = data;
				}			
				else
				{
					data = sounds[sound];
				}
				
				if (data != null)
				{
					try
					{
						PlaySound(data, Flags.SND_ASYNC | Flags.SND_MEMORY);
					}
					catch (Exception) {}
				}
			}
		}
				
		[DllImport("CoreDll.DLL", EntryPoint="sndPlaySound", SetLastError=true)]
    	private extern static int PlaySound(byte[] sound, Flags flags);
	}
}