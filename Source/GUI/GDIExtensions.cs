using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace IGoEnchi
{	
	public enum GradientDirection
	{
		Horizontal,
		Vertical
	}
	
	public sealed class GDIExtensions
	{
		private struct TriVertex
		{
			public int X;
			public int Y;
			public ushort Red;
			public ushort Green;
			public ushort Blue;
			public ushort Alpha;
			
			public TriVertex(int x, int y, Color color)
			{
				X = x;
				Y = y;
				Red = (ushort) (color.R << 8);
				Green = (ushort) (color.G << 8);
				Blue = (ushort) (color.B << 8);
				Alpha = (ushort) (color.A << 8);
			}
		}
		
		private struct GradientRect
		{
			public uint UpperLeft;
			public uint LowerRight;
		}
		
		private struct BlendFunction
		{
			public byte BlendOp;
			public byte BlendFlags;
			public byte SourceConstantAlpha;
			public byte AlphaFormat;
		}
		
		private static readonly byte AC_SRC_OVER = 0x0;		
							
		private static readonly uint GRADIENT_FILL_RECT_H = 0x0;
    	private static readonly uint GRADIENT_FILL_RECT_V = 0x1;
		
    	private static readonly int ERROR_NOT_SUPPORTED = 0x32;
    	
		private static BlendFunction function = 
			new BlendFunction()
			{
				BlendOp = AC_SRC_OVER,
				BlendFlags = 0,
				SourceConstantAlpha = 127,
				AlphaFormat = 0
			};
		
		private static bool? alphaBlendSupported = null;
		private static bool? gradientFillSupported = null;
		
		public static bool AlphaBlendSupported
		{
			get 
			{
				if (!alphaBlendSupported.HasValue)
				{
					alphaBlendSupported = true;
					try
					{
						AlphaBlend(IntPtr.Zero, 0, 0, 0, 0,
			           			   IntPtr.Zero, 0, 0, 0, 0,
			           			   function);			
						if (Marshal.GetLastWin32Error() == ERROR_NOT_SUPPORTED)
						{
							alphaBlendSupported = false;
						}
					}
					catch (Exception)
					{
						alphaBlendSupported = false;
					}
				}
				return alphaBlendSupported.Value;			
			}
		}
		
		public static bool GradientFillSupported
		{
			get 
			{
				if (!gradientFillSupported.HasValue)
				{
					gradientFillSupported = true;
					try
					{
						GradientFill(IntPtr.Zero, 
						             new TriVertex[0], 0,
						             new GradientRect[0], 0,
						             0);
						if (Marshal.GetLastWin32Error() == ERROR_NOT_SUPPORTED)
						{
							gradientFillSupported = false;
						}
					}
					catch (Exception)
					{
						gradientFillSupported = false;
					}
				}
				return gradientFillSupported.Value;			
			}
		}
		
		private GDIExtensions() {}
		
		public static void AlphaBlend(Graphics target, Bitmap source,
		                	  		  int x, int y)
		{
			if (!AlphaBlendSupported)
			{
				return;
			}
			
			using (var graphics = Graphics.FromImage(source))
			{				
				var destHdc = target.GetHdc();			
				var sourceHdc = graphics.GetHdc();
				
				try 
				{
					AlphaBlend(
						destHdc,
						x, y, source.Width, source.Height,
						sourceHdc,
						0, 0, source.Width, source.Height,
						function);
				}
				finally
				{				
					graphics.ReleaseHdc(sourceHdc);
					target.ReleaseHdc(destHdc);
				}
			}			
		}
		
		public static void GradientFill(Graphics graphics, Rectangle target,
		                         		Color startColor, Color endColor,
		                         		GradientDirection direction)
		{
			if (!GradientFillSupported)
			{
				return;
			}
			
			var hdc = graphics.GetHdc();
			try
			{
				var vertices = new TriVertex[]
				{
					new TriVertex(target.Left, target.Top, startColor),
					new TriVertex(target.Right, target.Bottom, endColor)
				};
				var rectangles = new GradientRect[]
				{
					new GradientRect()
					{
						UpperLeft = 0,
						LowerRight = 1
					}				
				};
								
				GradientFill(hdc, vertices, 2, rectangles, 1,
				             direction == GradientDirection.Horizontal ?
				             	GRADIENT_FILL_RECT_H : GRADIENT_FILL_RECT_V);
			}
			finally
			{
				graphics.ReleaseHdc(hdc);
			}
		}
		
   		[DllImport("coredll.dll", SetLastError=true)]
   		private extern static Int32 AlphaBlend(
   			IntPtr hdcDest, 
   			Int32 xDest, Int32 yDest, Int32 cxDest, Int32 cyDest, 
   			IntPtr hdcSrc, 
   			Int32 xSrc, Int32 ySrc, Int32 cxSrc, Int32 cySrc, 
   			BlendFunction blendFunction);
   		
   		[DllImport("coredll.dll", SetLastError=true)]
    	private extern static bool GradientFill(
        	IntPtr hdc,
        	TriVertex[] vertices, UInt32 verticesCount,
        	GradientRect[] rectangles, UInt32 rectanglesCount,
        	UInt32 mode);   
	}
}