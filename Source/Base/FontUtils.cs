using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace IGoEnchi
{	
	public static class FontUtils
	{		
		public static float GetHeight(Font font)
		{			
			return font.Size * getDPI() / 72;
		}
		
		public static int From96DPI(int dots)
		{
			return (int) (dots * getDPI() / 96);
		}

		private static Func<float> getDPI = () =>
		{
			Func<float> f1 = GetDPICF;
			Func<float> f2 = GetDPI;
			getDPI = Environment.OSVersion.Platform == PlatformID.WinCE ?
				f1 : f2;
				
			return getDPI();
		};
		
		private static float GetDPI()
		{			
			var ptr = GetDC(IntPtr.Zero);
			var res = 0.0f;
			using (var graphics = Graphics.FromHdc(ptr))
			{
				res = graphics.DpiY;
			}
			ReleaseDC(IntPtr.Zero, ptr);
			return res;		
		}				

		private static float GetDPICF()
		{			
			var ptr = GetDCCF(IntPtr.Zero);
			var res = 0.0f;
			using (var graphics = Graphics.FromHdc(ptr))
			{
				res = graphics.DpiY;
			}
			ReleaseDCCF(IntPtr.Zero, ptr);
			return res;		
		}
		
		[DllImport("user32.dll", EntryPoint="GetDC")]
    	 private extern static IntPtr GetDC(IntPtr hwnd);
	    
     	 [DllImport("user32.dll", EntryPoint="ReleaseDC")]
    	 private extern static int ReleaseDC(IntPtr hwnd, IntPtr hdc);
		
		[DllImport("coredll.dll", EntryPoint="GetDC")]
    	 private extern static IntPtr GetDCCF(IntPtr hwnd);
	    
     	 [DllImport("coredll.dll", EntryPoint="ReleaseDC")]
    	 private extern static int ReleaseDCCF(IntPtr hwnd, IntPtr hdc);
	}
}