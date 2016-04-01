using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Resources;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.Runtime.InteropServices;
 
namespace IGoEnchi
{
	public class GdiRenderer: GameRenderer
	{
		private Pen linesPen;
		private Pen whitePen;
		private Pen blackPen;
		private Pen redPen;
		
		private Brush blackBrush;
		private Brush whiteBrush;
		private Font font;
		private Font messageFont;
				
		private Bitmap offScreenBuffer;
		private Bitmap blackStone;
		private Bitmap whiteStone;
		private Bitmap wood;
		private Bitmap coloredMarkSeed;		
				
		private byte[] blendArray;
		private Dictionary<int, Bitmap> blackStoneCache;
		private Dictionary<int, Bitmap> whiteStoneCache;				
		private Dictionary<int, Bitmap> coloredMarkCache;
		
		public GdiRenderer(Size screenSize)
		{
			linesPen = new Pen(Color.Black);
			whitePen = new Pen(Color.Beige);
			blackPen = new Pen(Color.Black);
			redPen = new Pen(Color.Red);
			
			blackBrush = new SolidBrush(Color.Black);
			whiteBrush = new SolidBrush(Color.White);
			font = new Font(ConfigManager.Settings.DefaultFontName, 8, FontStyle.Bold);
			messageFont = new Font(ConfigManager.Settings.DefaultFontName, 10, FontStyle.Bold);
			
			var brush = new SolidBrush(Color.Beige);
			var transparentBrush = new SolidBrush(Color.Purple);

			var resourceManager = 
				new ResourceManager("IGoEnchi.BoardImages", Assembly.GetExecutingAssembly())
				{IgnoreCase = true};
			
			ChangeSize(screenSize);
			
			wood = resourceManager.GetObject("Wood") as Bitmap;
			
			blackStoneCache = new Dictionary<int, Bitmap>();
			whiteStoneCache = new Dictionary<int, Bitmap>();
			
			coloredMarkSeed = resourceManager.GetObject("Mark") as Bitmap;
			coloredMarkCache = new Dictionary<int, Bitmap>();
		}
		
		public bool Simplified 
		{
			get {return ConfigManager.Settings.NoTextures;}
		}
		
		public override void Dispose()
		{
			if (offScreenBuffer != null)
			{
				offScreenBuffer.Dispose();
			}		
			
			if (blackStone != null &&
			    !blackStoneCache.ContainsValue(blackStone))
			{
				blackStone.Dispose();
			}
			
			if (whiteStone != null &&
			    !whiteStoneCache.ContainsValue(whiteStone))
			{
				whiteStone.Dispose();
			}
			
			if (coloredMarkSeed != null)
			{
				coloredMarkSeed.Dispose();
			}
			
			foreach (var item in blackStoneCache)
			{
				item.Value.Dispose();
			}
									
			foreach (var item in whiteStoneCache)
			{
				item.Value.Dispose();
			}
			
			foreach (var item in coloredMarkCache)
			{
				item.Value.Dispose();
			}
			
			wood.Dispose();			
			redPen.Dispose();
			blackPen.Dispose();
			whitePen.Dispose();
			linesPen.Dispose();		
			blackBrush.Dispose();
			whiteBrush.Dispose();
			font.Dispose();
			messageFont.Dispose();
		}
		
		private void LoadStones(int cellSize)
		{					
			var currentSize = 
				blackStone != null ? blackStone.Width : -1;
		
			var size = 
				cellSize < 10 ? 10 :
				cellSize > 60 ? 60 : 
				cellSize;
			
			if (size != currentSize)
			{			
				if (blackStone != null &&
					blackStone.Width <= 60 &&
					!blackStoneCache.ContainsKey(
						blackStone.Width))
				{
					blackStoneCache[blackStone.Width] = 
						blackStone;					
				}
				
				if (whiteStone != null &&
					whiteStone.Width <= 60 &&
					!whiteStoneCache.ContainsKey(
						whiteStone.Width))
				{
					whiteStoneCache[whiteStone.Width] =
						whiteStone;
				}													

				if (blackStoneCache.ContainsKey(size) &&
				    whiteStoneCache.ContainsKey(size))
				{
					blackStone = blackStoneCache[size];
					whiteStone = whiteStoneCache[size];
				}
				else
				{				
					var resourceManager =
						new ResourceManager("IGoEnchi.StoneImages", Assembly.GetExecutingAssembly())
					{IgnoreCase = true};										
					
					blackStone = resourceManager.GetObject("Black" + size.ToString()) as Bitmap;
					whiteStone = resourceManager.GetObject("White" + size.ToString()) as Bitmap;
				}
			}
		}
		
		private void ChangeSize(Size targetSize)
		{						
			if (offScreenBuffer != null)
			{
				offScreenBuffer.Dispose();
			}
			
			offScreenBuffer = new Bitmap(targetSize.Width, targetSize.Height, PixelFormat.Format24bppRgb);
			blendArray = new byte[offScreenBuffer.Width * offScreenBuffer.Height * 3];
		}
		
		public override void StartRendering(Rectangle position)
		{
			var oldSize = TargetSize;
			base.StartRendering(position);						
			if (TargetSize != oldSize)
			{
				ChangeSize(TargetSize);
			}			
		}
		
		public override void FinishRendering(Graphics graphics)
		{			
			if (TruncatedPosition.Width < Position.Width)
			{
				using (var brush = new SolidBrush(Color.Black))
				{
					graphics.FillRectangle(brush, Position.X, Position.Y,
					                       TruncatedPosition.X - Position.X,
					                       Position.Height);
					graphics.FillRectangle(brush, Position.X, Position.Y,
					                       Position.Width,
					                       TruncatedPosition.Y - Position.Y);
				}			
				graphics.DrawImage(offScreenBuffer, 
			    	           	   TruncatedPosition.X, 
			        	       	   TruncatedPosition.Y);
			}
			else
			{
				var size = Math.Min(Position.Width, Position.Height);
				using (var brush = new SolidBrush(Color.Black))
				{
					graphics.FillRectangle(brush, Position.X, 
					                       TruncatedPosition.Bottom,
					                       Position.Width,
					                       Position.Height);					
				}	
								
				graphics.DrawImage(
					offScreenBuffer, Position.X, Position.Y,
					ClipPosition,
					GraphicsUnit.Pixel);
			}
			
		}
		
		public override void DrawCursor(byte x, byte y)
		{
			var target = CoordsToRectangle(x, y);
						
			using (var graphics = Graphics.FromImage(offScreenBuffer))
			{
				DrawCursor(graphics, target);
			}
		}
				
		public override void ClearBoard(byte boardSize)
		{			
			var oldCellSize = CellSize;
			base.ClearBoard(boardSize);			
			LoadStones(CellSize);
			if (CellSize != oldCellSize)
			{
				ChangeSize(TargetSize);
			}
			
			if (font != null)
			{
				font.Dispose();
			}
						
			font = new Font(ConfigManager.Settings.DefaultFontName, RequiredFontSize, FontStyle.Bold);
			
			var penSize = BoardSize > 13 ? 1 : 2;
			whitePen.Width = penSize;
			blackPen.Width = penSize;
			redPen.Width = penSize * 2;
			
			using (var graphics = Graphics.FromImage(offScreenBuffer))
			{
				graphics.Clear(Color.Black);
				
				var width = CellSize * BoardSize;
				var height = CellSize * BoardSize;
				
				if (Simplified)
				{
					var boardColor = Color.Gray;
					using (var brush = new SolidBrush(boardColor))
					{
						graphics.FillRectangle(brush, 0, 0, width, height);
					}
					DrawGrid(graphics, boardColor);
				}
				else
				{
					var xTiles = width / wood.Width;
					var yTiles = height / wood.Height;
					
					var xEdge = width % wood.Width;
					var yEdge = height % wood.Height;
					
					for (var x = 0; x < xTiles; x++)
					{
						for (var y = 0; y < yTiles; y++)
						{
							graphics.DrawImage(wood, x * wood.Width, y * wood.Height);
						}
					}
					
					if (xEdge > 0)
					{
						for (var y = 0; y < yTiles; y++)
						{
							graphics.DrawImage(
								wood, xTiles * wood.Width, y * wood.Height,
								new Rectangle(0, 0, xEdge, wood.Height),
								GraphicsUnit.Pixel);
						}
					}
					
					if (yEdge > 0)
					{
						for (var x = 0; x < xTiles; x++)
						{
							graphics.DrawImage(
								wood, x * wood.Width, yTiles * wood.Height,
								new Rectangle(0, 0, wood.Width, yEdge),
								GraphicsUnit.Pixel);
						}
					}
					
					if (xEdge * yEdge > 0)
					{
						graphics.DrawImage(
							wood, xTiles * wood.Width, yTiles * wood.Height,
							new Rectangle(0, 0, xEdge, yEdge),
							GraphicsUnit.Pixel);
					}
					
					var boardColor = wood.GetPixel(wood.Width / 2, wood.Height / 2);
					DrawGrid(graphics, boardColor);
				}
			}
		}
		
		private void DimRegion(Bitmap bitmap, Rectangle target)
		{			
			var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), 
			                           ImageLockMode.ReadWrite, 
			                           PixelFormat.Format24bppRgb);
			
			Marshal.Copy(data.Scan0, blendArray, 0, blendArray.Length);			
			for (var i = target.Top; i < target.Top + target.Height; i++)
			{				
				for (var j = target.Left; j < target.Left + target.Width; j++)
				{
					blendArray[i * data.Stride + 3 * j] >>= 1;
					blendArray[i * data.Stride + 3 * j + 1] >>= 1;
					blendArray[i * data.Stride + 3 * j + 2] >>= 1;
				}
			}			
			Marshal.Copy(blendArray, 0, data.Scan0, blendArray.Length);
			bitmap.UnlockBits(data);
		}
		
		public override void DrawMessage(string message)
		{			
			using (var graphics = Graphics.FromImage(offScreenBuffer))
			{
				var margin = 10;
				var size = graphics.MeasureString(message, messageFont);
				Rectangle destination = new Rectangle((BoardSize * CellSize - (int) size.Width) / 2 - 2 * margin,
				                                      (BoardSize * CellSize - (int) size.Height * 2) / 2 - margin,
			        	                              (int) size.Width + 4 * margin,
			            	                          (int) size.Height + 2 * margin);	
				StringFormat format = new StringFormat();
				format.Alignment = StringAlignment.Center;
				format.FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
				format.LineAlignment = StringAlignment.Center;				
				using (var pen = new Pen(Color.Black, 2))
				{										
					DimRegion(offScreenBuffer, destination);
					graphics.DrawRectangle(pen, destination);	
				}
				using (var brush = new SolidBrush(Color.White))
				{
					graphics.DrawString(message, messageFont, brush,
			    		                (RectangleF) destination, format);	
				}
			}
		}
		
		public override void DrawString(byte x, byte y, string text, bool black)
		{
			using (var graphics = Graphics.FromImage(offScreenBuffer))
			{			
				StringFormat format = new StringFormat();
				format.Alignment = StringAlignment.Center;
				format.FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
				format.LineAlignment = StringAlignment.Center;
				Rectangle destination = new Rectangle((int) (x * CellSize) - 10,
			    	                                  (int) (y * CellSize) - 10,
			        	                              (int) CellSize + 20,
			            	                          (int) CellSize + 20);
			
				graphics.DrawString(text, font, SelectBrush(black),
			    	                (RectangleF) destination, format);				
			}
		}
		
		public override void DrawStone(byte x, byte y, bool black)
		{
			using (var graphics = Graphics.FromImage(offScreenBuffer))
			{							
				if (Simplified)
				{
					graphics.FillEllipse(SelectBrush(black),
					                     (int) (x * CellSize),
					                     (int) (y * CellSize),
					                     (int) CellSize,
					                     (int) CellSize);					
					var quarterColor = 
						black ? Color.FromArgb(32, 32, 32) 
							  : Color.FromArgb(224, 224, 224);					
					using (var quarterPen = new Pen(quarterColor))
					{												
						graphics.DrawEllipse(quarterPen,
					    	                 (int) (x * CellSize),
					        	             (int) (y * CellSize),
					            	         (int) CellSize,
					                	     (int) CellSize);						
					}
				}
				else
				{
					var destination = CoordsToRectangle(x, y);
					var stone = black ? blackStone : whiteStone;
					
					var imageAttributes = new ImageAttributes();
					imageAttributes.SetColorKey(stone.GetPixel(0, 0), stone.GetPixel(0, 0));
				
					graphics.DrawImage(stone,
			        	           	   destination,
			            	       	   0, 0,
			                   	   	   stone.Width,
				                   	   stone.Height,
  			                   	   	   GraphicsUnit.Pixel,
			                           imageAttributes);
				}
			}
		}
		
		public override void DrawShadow(byte x, byte y, bool black)
		{			
			using (var graphics = Graphics.FromImage(offScreenBuffer))
			{
				if (!Simplified && GDIExtensions.AlphaBlendSupported)
				{
					var stone = black ? blackStone : whiteStone;
					var destination = CoordsToRectangle(x, y);
					
					GDIExtensions.AlphaBlend(
						graphics, stone, destination.X, destination.Y);					
				}
				else
				{
					graphics.DrawEllipse(SelectPen(black),
					                     (int) (x * CellSize),
					                     (int) (y * CellSize),
					                     (int) CellSize,
					                     (int) CellSize);
				}							
			}			
		}
		
		private Pen SelectPen(bool black)
		{
			return black ? blackPen : whitePen;
		}
		
		private Brush SelectBrush(bool black)
		{
			return black ? blackBrush : whiteBrush;
		}
		
		public override void DrawMark(byte x, byte y, bool black)
		{
			using (var graphics = Graphics.FromImage(offScreenBuffer))
			{
				DrawMark(graphics, SelectPen(black), CoordsToRectangle(x, y));
			}
		}
		
		public override void DrawTriangle(byte x, byte y, bool black)
		{
			using (var graphics = Graphics.FromImage(offScreenBuffer))
			{
				DrawTriangle(graphics, SelectPen(black), CoordsToRectangle(x, y));
			}
		}
				
		public override void DrawSquare(byte x, byte y, bool black, bool filled)
		{
			using (var graphics = Graphics.FromImage(offScreenBuffer))
			{			
				if (filled)
				{
					DrawSquare(graphics, SelectBrush(black), CoordsToRectangle(x, y));				
				}
				else
				{
					DrawSquare(graphics, SelectPen(black), CoordsToRectangle(x, y));
				}
			}
		}
						
		public override void DrawCircle(byte x, byte y, bool black)
		{
			using (var graphics = Graphics.FromImage(offScreenBuffer))
			{			
				DrawCircle(graphics, SelectPen(black),
				           CoordsToRectangle(x, y));
			}			
		}
		
		protected override void DrawColoredCross(byte x, byte y)
		{
			using (var graphics = Graphics.FromImage(offScreenBuffer))
			{
				DrawColoredCross(graphics, redPen, CoordsToRectangle(x, y));
			}
		}
		
		public override void DrawColoredMark(Color color, byte x, byte y)
		{
			var bitmap = null as Bitmap;
			var key = color.ToArgb();
			if (!coloredMarkCache.TryGetValue(key, out bitmap))
			{
				bitmap = new Bitmap(coloredMarkSeed);
				coloredMarkCache[key] = bitmap;
				
				var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), 
				                           ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
				var imageArray = new byte[data.Stride * data.Height];
				Marshal.Copy(data.Scan0, imageArray, 0, imageArray.Length);			
				for (var i = 0; i < bitmap.Height; i++)
				{				
					for (var j = 0; j < bitmap.Width; j++)
					{						
						var b = imageArray[i * data.Stride + 3 * j];
						var g = imageArray[i * data.Stride + 3 * j + 1];
						var r = imageArray[i * data.Stride + 3 * j + 2];
						
						imageArray[i * data.Stride + 3 * j] = (byte) (b * color.B / 256);
						imageArray[i * data.Stride + 3 * j + 1] = (byte) (g * color.G / 256);
						imageArray[i * data.Stride + 3 * j + 2] = (byte) (r * color.R / 256);
					}
				}			
				Marshal.Copy(imageArray, 0, data.Scan0, imageArray.Length);
				
				bitmap.UnlockBits(data);
			}
			
			var destination = CoordsToRectangle(x, y);			
			destination.X += destination.Width / 4;
			destination.Y += destination.Height / 4;
			destination.Width /= 2;
			destination.Height /= 2;
						
			var imageAttributes = new ImageAttributes();
			imageAttributes.SetColorKey(bitmap.GetPixel(0, 0), bitmap.GetPixel(0, 0));
		
			using (var graphics = Graphics.FromImage(offScreenBuffer))
			{
				graphics.DrawImage(bitmap,
		        	           	   destination,
		            	       	   0, 0,
		                   	   	   bitmap.Width,
			                   	   bitmap.Height,
		                   	   	   GraphicsUnit.Pixel,
		                           imageAttributes);
			}
		}
		
		private Rectangle CoordsToRectangle(byte x, byte y)
		{
			return new Rectangle(x * CellSize, y * CellSize,
			                     CellSize, CellSize);
		}
	}
}