using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Resources;
using System.Reflection;
using System.Windows.Forms;

namespace IGoEnchi
{
	public class Direct3DRenderer: GameRenderer, IDisposable
	{
		private Device device;
		private Sprite sprite;
		private Sprite messageSprite;
		private Surface background;
		private Texture whiteStone;
		private Texture blackStone;
		private Texture coloredMark;
		private Texture[] markup;		
		private Bitmap wood;
		private Microsoft.WindowsMobile.DirectX.Direct3D.Font font;
		private Microsoft.WindowsMobile.DirectX.Direct3D.Font messageFont;
		private int stoneSize;
		
		public Direct3DRenderer(Size screenSize, Control target): base()
		{
			device = new Device(0, DeviceType.Default, 
				target, CreateFlags.None,
				new PresentParameters() 
					{Windowed = true,
					 SwapEffect = SwapEffect.Discard});
			device.RenderState.AlphaBlendEnable = true;			
			device.RenderState.SourceBlend = Blend.SourceAlpha;
			device.RenderState.DestinationBlend = Blend.InvSourceAlpha;			
			
			device.DeviceReset += delegate {CreateResources();};
			
			CreateResources();
			
			messageFont = new Microsoft.WindowsMobile.DirectX.Direct3D.Font(device,
				new System.Drawing.Font(ConfigManager.Settings.DefaultFontName, 10, FontStyle.Bold));
						
		}				
		
		private void CreateResources()
		{			
			if (sprite == null) sprite = new Sprite(device);
			if (messageSprite == null) messageSprite = new Sprite(device);
			
			ResourceManager resourceManager = 
				new ResourceManager("IGoEnchi.BoardImages", Assembly.GetExecutingAssembly())
				{IgnoreCase = true};
				
			if (wood == null) wood = resourceManager.GetObject("Wood") as Bitmap;

			blackStone = CreateTexture(resourceManager, "BlackStone");
			whiteStone = CreateTexture(resourceManager, "WhiteStone");			
			stoneSize = blackStone.GetSurfaceLevel(0).Description.Width;
			coloredMark = CreateTexture(resourceManager, "Mark", stoneSize / 2);
			
			if (markup == null) markup = new Texture[8];
			for (var i = 0; i < markup.Length; i++)
			{
				markup[i] = CreateMarkup(stoneSize, i);
			}			
			
			background = null;
		}

		private void CreateFont()
		{
			if (font != null)
			{
				font.Dispose();
			}
			var width = CellSize / (float) stoneSize;
			var gdiFont = new System.Drawing.Font(ConfigManager.Settings.DefaultFontName, 
			                                      RequiredFontSize / width, 
			                                      FontStyle.Bold);
			font = new Microsoft.WindowsMobile.DirectX.Direct3D.Font(device, gdiFont);
		}
		
		private Texture CreateMarkup(int size, int index)
		{
			using (var bitmap = new Bitmap(size, size))
			{
				using (var pen = new Pen(Color.White) {Width = size / 10})
				using (var stoneMarkPen = new Pen(Color.Red) {Width = size / 10})
				using (var brush = new SolidBrush(Color.White))
				using (var cursorBrush = new SolidBrush(Color.Red))				
				{
					using (var graphics = Graphics.FromImage(bitmap))
					{		
						var target = new Rectangle(0, 0, size, size);
						switch (index)
						{														
							case 0: DrawCircle(graphics, pen, target);
									break;
							case 1: DrawSquare(graphics, pen, target);
									break;
							case 2: DrawSquare(graphics, brush, target);
									break;
							case 3: DrawTriangle(graphics, pen, target);
									break;
							case 4: DrawMark(graphics, pen, target);									
									break;
							case 5: DrawCursor(graphics, target);
									break;
							case 6: DrawColoredCross(graphics, stoneMarkPen, target);
									break;							
							case 7: graphics.Clear(Color.Black);
									break;							
						}
					}
					var stream = new MemoryStream();
					bitmap.Save(stream, ImageFormat.Bmp);
					stream.Position = 0;
				
					var colorKey = index < 7 ? bitmap.GetPixel(0, 0).ToArgb() : Int32.MaxValue;
					var texture = TextureLoader.FromStream(
						device, stream, (int) stream.Length, 0, 0, 0,
						Usage.Texture | Usage.Lockable, Format.A8R8G8B8, Pool.VideoMemory,
						Filter.Linear, Filter.Linear, colorKey);
					
					stream.Close();
					return texture;
				}
			}
		}
		
		private Texture CreateTexture(ResourceManager manager, string resource)
		{
			return CreateTexture(manager, resource, 0);
		}
		
		private Texture CreateTexture(ResourceManager manager, string resource, int size)
		{
			using (var bitmap = manager.GetObject(resource) as Bitmap)
			{				
				var stream = new MemoryStream();
				bitmap.Save(stream, ImageFormat.Bmp);
				stream.Position = 0;
				
				var texture = TextureLoader.FromStream(
					device, stream, (int) stream.Length, size, size, 0,
					Usage.Texture | Usage.Lockable, Format.A8R8G8B8, Pool.VideoMemory,
					Filter.Linear, Filter.Linear, bitmap.GetPixel(0, 0).ToArgb());
									
				stream.Close();
				return texture;
			}
		}
		
		private void CreateBoard()
		{
			var size = (int)(BoardSize * CellSize); 
			using (var bitmap = new Bitmap(size, size))
			{
				using (var graphics = Graphics.FromImage(bitmap))
				{					
					graphics.DrawImage(wood,
			    	        	       new Rectangle(0, 0, size, size),
			        	           	   new Rectangle(0, 0, wood.Width, wood.Height),
			            	       	   GraphicsUnit.Pixel);

					var boardColor = wood.GetPixel(wood.Width / 2, wood.Height / 2);
					DrawGrid(graphics, boardColor);

					background = new Surface(device, bitmap, Pool.VideoMemory);
				}
			}
		}
		
		public override void Dispose()
		{
			foreach (var item in markup)
			{			
				item.Dispose();
			}
			blackStone.Dispose();
			whiteStone.Dispose();
			coloredMark.Dispose();
			wood.Dispose();
			if (background != null)
			{
				background.Dispose();
			}
			if (font != null)
			{
				font.Dispose();
			}
			messageFont.Dispose();
			sprite.Dispose();
			device.Dispose();
		}
						
		public override void StartRendering(Rectangle position)
		{
			base.StartRendering(position);			
			device.BeginScene();
			device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
			
			messageSprite.Begin(SpriteFlags.AlphaBlend);
			sprite.Begin(SpriteFlags.AlphaBlend | SpriteFlags.SortDepthFrontToBack);
		}
		
		public override void FinishRendering(Graphics graphics)
		{			
			sprite.End();
			messageSprite.End();
			device.EndScene();						
			device.Present();			
		}
		
		public override void DrawCursor(byte x, byte y)
		{
			DrawMarkup(x, y, false, 5);
		}
		
		public override void ClearBoard(byte boardSize)
		{
			var oldSize = BoardSize;
			base.ClearBoard(boardSize);
			var width = CellSize / (float) stoneSize;
			var matrix = Matrix.Scaling(width, width, 1.0f);
			matrix.Multiply(Matrix.Translation(TruncatedPosition.X, 
			                                   TruncatedPosition.Y,
			                                   0));
			sprite.Transform = matrix;
			
			if (background == null || oldSize != boardSize)
			{
				CreateBoard();
			}
			CreateFont();
				
			
			if (TruncatedPosition.Width < Position.Width)
			{
				var point = new Point(Math.Max(TruncatedPosition.Left, 0),
				                      Math.Max(TruncatedPosition.Top, 0));
				var start = new Point(point.X - TruncatedPosition.Left,
				                      point.Y - TruncatedPosition.Top);
				var size = new Size(
					Math.Min(BoardSize * CellSize, Position.Width - point.X),
					Math.Min(BoardSize * CellSize, Position.Height - point.Y));
				device.CopyRects(background,
					             new Rectangle(start.X, start.Y, size.Width, size.Height),				                               
				                 device.GetBackBuffer(0, BackBufferType.Mono),
				                 new Point(point.X, point.Y));
			}
			else
			{
				device.CopyRects(background, ClipPosition,
				                 device.GetBackBuffer(0, BackBufferType.Mono),
				                 Point.Empty);
			}					
		}
		
		public override void DrawMessage(string message)
		{			
			var margin = 10;
			var target = new Rectangle(0, 0, BoardSize * CellSize, BoardSize * CellSize);
			var size = messageFont.MeasureString(messageSprite, message, ref target,			                                      
			                                     DrawTextFormat.VerticalCenter | DrawTextFormat.Center);
			var destination = new Rectangle((int) ((BoardSize * CellSize - size.Width) / 2 - 2 * margin),
			                                (int) ((BoardSize * CellSize / 2 - size.Height) - margin),
			    	                        (int) (size.Width + 4 * margin),
			    	                        (int) (size.Height + 2 * margin));			
			sprite.Flush();
			
			var factor = new SizeF(destination.Width / (float) stoneSize, 
			                       destination.Height / (float) stoneSize);
			messageSprite.Transform = Matrix.Scaling(factor.Width, factor.Height, 1);
			messageSprite.Draw(markup[7], Vector3.Empty,
			                   new Vector3(destination.Left / factor.Width,
			                               destination.Top / factor.Height,
			                               0.0f), 0x78FFFFFF);
			messageSprite.Transform = Matrix.Identity;			
			messageFont.DrawText(messageSprite, message, destination, 
			              DrawTextFormat.VerticalCenter | DrawTextFormat.Center,
			              unchecked((int) 0xFFFFFFFF));
			messageSprite.Flush();			
			var surface = device.GetBackBuffer(0, BackBufferType.Mono);
			device.ColorFill(surface,
			                 new Rectangle(destination.Left - 2, destination.Top - 2,
			                               2, destination.Height + 3),
			                 Color.Black);
			device.ColorFill(surface,
			                 new Rectangle(destination.Left + destination.Width - 1, destination.Top - 2,
			                               2, destination.Height + 3),
			                 Color.Black);
			device.ColorFill(surface,
			                 new Rectangle(destination.Left - 2, destination.Top - 2,
			                               destination.Width + 3, 2),
			                 Color.Black);
			device.ColorFill(surface,
			                 new Rectangle(destination.Left - 2, destination.Top + destination.Height - 1,
			                               destination.Width + 3, 2),
			                 Color.Black);
			
		}
		
		public override void DrawString(byte x, byte y, string text, bool black)
		{
			var width = CellSize / (float) stoneSize;
			Rectangle destination = new Rectangle((int) (((x * CellSize) - 10) / width),
			                                      (int) (((y * CellSize) - 10) / width),
			                                      (int) ((CellSize + 20) / width),
			                                      (int) ((CellSize + 20) / width));
			font.DrawText(sprite, text, destination, 
			              DrawTextFormat.VerticalCenter | DrawTextFormat.Center,
			              unchecked((int) (black ? 0xFF000000 : 0xFFFFFFFF)));
		}
		
		public override void DrawStone(byte x, byte y, bool black)
		{			
			sprite.Draw(black ? blackStone : whiteStone, Vector3.Empty,
			            new Vector3(x * stoneSize, y * stoneSize, 0),
			            Color.White.ToArgb());
		}
		
		public override void DrawColoredMark(Color color, byte x, byte y)
		{			
			sprite.Draw(coloredMark, Vector3.Empty,
			            new Vector3((x + 0.25f) * stoneSize, (y + 0.25f) * stoneSize, 0),
			            color.ToArgb());			
		}
		
		public override void DrawShadow(byte x, byte y, bool black)
		{									
			sprite.Draw(black ? blackStone : whiteStone, Vector3.Empty,
			            new Vector3(x * stoneSize, y * stoneSize, 0),
			            0x78FFFFFF);
		}
		
		protected override void DrawColoredCross(byte x, byte y)
		{
			DrawMarkup(x, y, false, 6);
		}
						
		public override void DrawMark(byte x, byte y, bool black)
		{
			DrawMarkup(x, y, black, 4);
		}
		
		public override void DrawTriangle(byte x, byte y, bool black)
		{
			DrawMarkup(x, y, black, 3);
		}
		
		public override void DrawSquare(byte x, byte y, bool black, bool filled)
		{
			DrawMarkup(x, y, black, filled ? 2 : 1);
		}
		
		public override void DrawCircle(byte x, byte y, bool black)
		{			
			DrawMarkup(x, y, black, 0);
		}
		
		private void DrawMarkup(byte x, byte y, bool black, int index)
		{
			var width = CellSize / (float) stoneSize;
			sprite.Draw(markup[index],
			            Vector3.Empty,
			            new Vector3(x * CellSize / width, y * CellSize / width, 0),
			            unchecked((int) (black ? 0xC0000000 : 0xC0FFFFFF)));			
		}	
	}
}
