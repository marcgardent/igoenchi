using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Resources;
using System.Collections;
using System.Reflection;
using IGOEnchi.GoGameLib.Models;
using IGOEnchi.GoGameLogic.Models;

namespace IGoEnchi
{
	public abstract class GameRenderer: IDisposable
	{
		private readonly int MaxOverlap = 2;
		
		public byte BoardSize {get; private set;}
		public int CellSize {get; private set;}
		public Rectangle Position {get; private set;}
				
		protected GameRenderer()
		{
			BoardSize = 19;
		}
		
		public abstract void Dispose();
		
		public Size TargetSize
		{
			get 
			{
				return 
					Position.Width > TruncatedPosition.Width ?
						Position.Size : TruncatedPosition.Size;
			}
		}
		
		public Rectangle TruncatedPosition
		{
			get
			{
				var size = Math.Min(Position.Width, Position.Height);
				var actualSize = BoardSize * CellSize;
				
				var shift = (size - actualSize) / 2;
				return new Rectangle(
					Position.X + shift, 
					Position.Y + shift,
					actualSize, actualSize);
			}
		}
		
		public Rectangle ClipPosition
		{
			get 
			{
				return new Rectangle(Position.X - TruncatedPosition.X,
					              	 Position.Y - TruncatedPosition.Y,
					              	 Position.Width,
					              	 Position.Height);
			}
			
		}
		
		public virtual void StartRendering(Rectangle position)
		{			
			Position = position;
		}		
		
		public abstract void FinishRendering(Graphics graphics);
		
		public virtual void ClearBoard(byte boardSize)
		{
			BoardSize = boardSize;
			
			var size = Math.Min(Position.Width, Position.Height);
			CellSize = size / BoardSize;
			
			if (BoardSize * (CellSize + 1) - size <= MaxOverlap * 2)
			{
				CellSize += 1;
			}
		}
		
		public abstract void DrawCursor(byte x, byte y);		
		
		protected void DrawCursor(Graphics graphics, Rectangle target)
		{
			target = new Rectangle(
				target.X + target.Width / 4,
				target.Y + target.Height / 4,
				target.Width / 2,
				target.Height / 2);
			
			using (var brush = new SolidBrush(Color.Red))			
			{
				graphics.FillRectangle(brush, target);
			}
		}
		
		public abstract void DrawMessage(string message);		
		
		public abstract void DrawString(byte x, byte y, string text, bool black);
		
		public void DrawString(byte x, byte y, string text)
		{
			DrawString(x, y, text, false);
		}
		
		public abstract void DrawStone(byte x, byte y, bool black);
		
		public abstract void DrawShadow(byte x, byte y, bool black);
		
		public abstract void DrawMark(byte x, byte y, bool black);
				
		public void DrawMark(byte x, byte y)
		{
			DrawMark(x, y, true);
		}
		
		public void DrawMark(Graphics graphics, Pen pen, Rectangle target)
		{
			graphics.DrawLine(pen,
			                  target.X + 3 * target.Width / 4,
			                  target.Y + target.Height / 4,
			                  target.X + target.Width / 4,
			                  target.Y + 3 * target.Height / 4);
			graphics.DrawLine(pen,
			                  target.X + target.Width / 4,
			                  target.Y + target.Height / 4,
			                  target.X + 3 * target.Width / 4,
			                  target.Y + 3 * target.Height / 4);
		}
		
		public abstract void DrawTriangle(byte x, byte y, bool black);
		
		public void DrawTriangle(byte x, byte y)
		{
			DrawTriangle(x, y, true);
		}
		
		protected void DrawTriangle(Graphics graphics, Pen pen, Rectangle target)
		{		
			graphics.DrawPolygon(pen,
			                     new Point[]
			                     {new Point(target.X + target.Width / 4,
			                                target.Y + 3 * target.Width / 4),
			                      new Point(target.X + 3 * target.Width / 4,
			                     	        target.Y + 3 * target.Width / 4),
			                      new Point(target.X + target.Width / 2,
			                     	        target.Y + target.Width / 4)});
		}
		
		public abstract void DrawSquare(byte x, byte y, bool black, bool filled);
				
		public void DrawSquare(byte x, byte y, bool black)
		{
			DrawSquare(x, y, black, false);
		}
		
		public void DrawSquare(byte x, byte y)
		{
			DrawSquare(x, y, true, false);
		}
		
		public void DrawSelected(byte x, byte y, bool black)
		{
			DrawSquare(x, y, black, true);
		}
		
		public void DrawSelected(byte x, byte y)
		{
			DrawSquare(x, y, true, true);
		}
		
		public void DrawTerritory(byte x, byte y, bool black)
		{
			DrawSquare(x, y, black);
		}
		
		protected void DrawSquare(Graphics graphics, Pen pen, Rectangle target)
		{
			var rect = new Rectangle(target.X + target.Width / 4,
				                     target.Y + target.Height / 4,
				                     target.Width / 2,
				                     target.Height / 2);
			graphics.DrawRectangle(pen, rect);
		}
		
		protected void DrawSquare(Graphics graphics, Brush brush, Rectangle target)
		{
			var rect = new Rectangle(target.X + target.Width / 4,
				                     target.Y + target.Height / 4,
				                     target.Width / 2,
				                     target.Height / 2);
			graphics.FillRectangle(brush, rect);
		}
		
		public abstract void DrawCircle(byte x, byte y, bool black);
		
		public void DrawCircle(byte x, byte y)
		{
			DrawCircle(x, y, true);
		}
		
		protected void DrawCircle(Graphics graphics, Pen pen, Rectangle target)
		{
			var rect = new Rectangle(target.X + target.Width / 4,
				                     target.Y + target.Height / 4,
				                     target.Width / 2,
				                     target.Height / 2);
			graphics.DrawEllipse(pen, rect);
		}
		
		protected abstract void DrawColoredCross(byte x, byte y);
		
		protected void DrawColoredCross(Graphics graphics, Pen pen, Rectangle target)
		{
			DrawMark(graphics, pen, target);
		}
		
		public abstract void DrawColoredMark(Color color, byte x, byte y);		
		
		public void DrawMoveMark(byte x, byte y, bool isBlack)
		{
			if (ConfigManager.Settings.OldMark)
			{
				DrawCircle(x, y, isBlack);
			}
			else
			{
				DrawColoredCross(x, y);
			}
		}
		
		public Stone GetCoords(int x, int y, byte boardSize)
		{			
			Stone result = new Stone();
			result.X = (byte) ((x - TruncatedPosition.X) / CellSize);
			result.Y = (byte) ((y - TruncatedPosition.Y) / CellSize);
			return result;
		}
		
		protected int RequiredFontSize
		{
			get
			{
				var fontSize = 0;
				if (BoardSize <= 2) fontSize = 26;
				else if (BoardSize <= 4) fontSize = 18;
				else if (BoardSize <= 5) fontSize = 14;
				else if (BoardSize <= 9) fontSize = 12;
				else if (BoardSize <= 13) fontSize = 10;
				else fontSize = 8;
				return fontSize;
			}
		}
		
		protected void DrawGrid(Graphics graphics, Color boardColor)
		{			
			Func<int, int> toDark = x => x / 5;
			Func<int, int> toLight = x => x * 3 / 4;						
			
			var darkColor = Color.FromArgb(
				toDark(boardColor.R), toDark(boardColor.G), toDark(boardColor.B));
			var lightColor = Color.FromArgb(
				toLight(boardColor.R), toLight(boardColor.G), toLight(boardColor.B));
			
			foreach (var j in new int[] {1, 0})
			{
				var color = j == 0 ? darkColor : lightColor;
				
				using (var pen = new Pen(color))			
				{								
					for (var i = 0; i < BoardSize; i++)
					{						
						graphics.DrawLine(pen,
					    	              (int) (i * CellSize + CellSize / 2) + j,
					        	          (int) (CellSize / 2),
					            	      (int) (i * CellSize + CellSize / 2) + j,
				    	            	  (int) ((BoardSize - 1)* CellSize + CellSize / 2));
				
						graphics.DrawLine(pen,
						                  (int) (CellSize / 2),
						                  (int) (i * CellSize + CellSize / 2) + j,
						                  (int) ((BoardSize - 1) * CellSize + CellSize / 2),
						                  (int) (i * CellSize + CellSize / 2) + j);
					}
				}
			

				using (var brush = new SolidBrush(color))			
				{				
					if (BoardSize > 2)
					{
						var radius = (CellSize) / 6 + j;
						var distance = 
						BoardSize >= 12 ? 3 :
						BoardSize >= 5 ?  2 :
						BoardSize == 4 ?  1 : 0;
						
						if (BoardSize % 2 == 1)
						{
							
							graphics.FillEllipse(brush,
							                     BoardSize * CellSize / 2 - radius,
							                     BoardSize * CellSize / 2 - radius,
							                     radius * 2,
							                     radius * 2);
							for (int i = 0; i < 2; i++)
							{
								graphics.FillEllipse(brush,
								                     CellSize * (distance +	i * (BoardSize - distance * 2 - 1)) +
								                     CellSize / 2 - radius,
								                     BoardSize * CellSize / 2 - radius,
								                     radius * 2,
								                     radius * 2);
								
								graphics.FillEllipse(brush,
								                     BoardSize * CellSize / 2 - radius,
								                     CellSize * (distance +	i * (BoardSize - distance * 2 - 1)) +
								                     CellSize / 2 - radius,
								                     radius * 2,
								                     radius * 2);
							}
						}
						
						for (int i = 0; i < 2; i++)
						{
							graphics.FillEllipse(brush,
							                     CellSize * (distance +	i * (BoardSize - distance * 2 - 1)) +
							                     CellSize / 2 - radius,
							                     CellSize * (distance +	i * (BoardSize - distance * 2 - 1)) +
							                     CellSize / 2 - radius,
							                     radius * 2,
							                     radius * 2);
							
							graphics.FillEllipse(brush,
							                     CellSize * (BoardSize - distance -	i * (BoardSize - distance * 2 - 1)) -
							                     CellSize / 2 - radius - 1,
							                     CellSize * (distance +	i * (BoardSize - distance * 2 - 1)) +
							                     CellSize / 2 - radius,
							                     radius * 2,
							                     radius * 2);
						}
					}
				}
			}
		}
	}
}
