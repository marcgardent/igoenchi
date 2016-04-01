using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

namespace IGoEnchi
{
	public static class Layout
	{
		private static readonly int Padding = FontUtils.From96DPI(3);
		
		private static SizeF Measure(string text)
		{
			using (var graphics = ConfigManager.MainForm.CreateGraphics())
			{
				var font = ConfigManager.GUIFont;			
				return graphics.MeasureString(text, font);
			}
		}
		
		public static Label Label(string text)
		{
			var size = Measure(text);
			return new Label() 
			{
				Font = ConfigManager.GUIFont,
				Text = text, 
				Width = (int) size.Width + 4,
				Height = (int) size.Height
			};							
		}
		
		public static Button Button(string caption)
		{
			var size = Measure(caption);
			return new Button() 
			{
				Font = ConfigManager.GUIFont,
				Text = caption, 				
				Height = (int) (size.Height * 1.6f)
			};			
		}
		
		public static void Resize(Control control)
		{
			if (control != null)
			{
				control.Height = 
					(int) (FontUtils.GetHeight(ConfigManager.GUIFont) * 1.6f);
			}
		}
		
		public static LayoutTable PropertyTable(params Control[] controls)
		{
			var height = controls.Length / 2;
			
			var table = new LayoutTable(height, 2);			
			table.Fill(controls);
			table.FixRows();
			table.FixColumns(0);
			
			return table;
		}
		
		private static LayoutTable Table(int rows, int columns, Control[] controls)
		{
			var table = new LayoutTable(rows, columns);
			table.Fill(controls);
			table.FixRows();
			
			return table;
		}
		
		public static LayoutTable Stack(params Control[] controls)
		{
			return Table(controls.Length, 1, controls);			
		}
		
		public static LayoutTable Flow(params Control[] controls)
		{
			return Table(1, controls.Length, controls);			
		}
		
		public static void Bind(ILayout layout, Control control)
		{
			Func<Rectangle> target = () =>
				new Rectangle(control.ClientRectangle.Left + Padding,
							  control.ClientRectangle.Top + Padding,
							  control.ClientRectangle.Width - 2 * Padding,
							  control.ClientRectangle.Height - 2 * Padding);
			layout.Apply(control, target());
			control.Resize += delegate 
			{
				layout.Apply(control, target());
			};
		}
	}
	
	public interface ILayout
	{								
		Size Measure();		
		void Apply(Control control, Rectangle bounds);				
	}
	
	public class EmptyLayout: ILayout
	{
		public static EmptyLayout Value = new EmptyLayout();
		public static ILayout Interface = Value as ILayout;
		
		private EmptyLayout() {}
		
		public Size Measure()
		{
			return Size.Empty;
		}
		
		public void Apply(Control control, Rectangle bounds) {}		
	}
	
	public class ControlWrapper: ILayout
	{
		public Control Control {get; private set;}
		
		public int MinWidth {get; set;}
		public int MinHeight {get; set;}
		
		public int Width {get; set;}
		public int Height {get; set;}
		
		public ControlWrapper(Control control)
		{
			Control = control;
			MinSize = control.Size;
		}
		
		public Size MinSize
		{
			get 
			{
				return new Size(MinWidth, MinHeight);
			}
			set
			{
				MinWidth = value.Width;
				MinHeight = value.Height;
			}
		}
				
		public Size Size
		{
			get 
			{
				return new Size(Width, Height);
			}
			set
			{
				Width = value.Width;
				Height = value.Height;
			}
		}	
		
		public Size Measure()
		{
			return MinSize;		
		}
		
		public void Apply(Control control, Rectangle bounds)
		{
			if (!control.Controls.Contains(Control))
			{
				control.Controls.Add(Control);
			}
			Control.Bounds = bounds;
		}
	}
	
	public class LayoutTable: ILayout
	{				
		private List<List<ILayout>> rows;
		private List<bool> rowFixed;
		private List<bool> columnFixed;
		
		public LayoutTable()
		{
			rows = new List<List<ILayout>>();
			
			rowFixed = new List<bool>();
			columnFixed = new List<bool>();
		}				
		
		public LayoutTable(int rows, int columns): this()
		{
			Stretch(rows, columns);
		}
		
		public int RowsCount
		{
			get {return rows.Count;}
		}
		
		public int ColumnsCount
		{
			get {return rows.Count > 0 ? rows[0].Count : 0;}
		}
		
		public IEnumerable<ILayout> Row(int index)
		{
			return rows[index];
		}
		
		public IEnumerable<ILayout> Column(int index)
		{
			foreach (var item in rows)
			{
				yield return item[index];
			}
		}
		
		public IEnumerable<IEnumerable<ILayout>> Rows
		{
			get
			{
				for (var i = 0; i < RowsCount; i++)
				{
					yield return Row(i);
				}
			}
		}
		
		public IEnumerable<IEnumerable<ILayout>> Columns
		{
			get
			{
				for (var i = 0; i < ColumnsCount; i++)
				{
					yield return Column(i);
				}			
			}
		}
		
		public void Stretch(int row, int column)
		{
			var newSize = new Size(Math.Max(ColumnsCount, column),
			                       Math.Max(RowsCount, row));
			
			if (newSize.Width > ColumnsCount)
			{				
				columnFixed.AddRange(Collection.Range(newSize.Width - ColumnsCount, false));
				var rowItems = Collection.Range(newSize.Width - ColumnsCount, EmptyLayout.Interface);
				for (var i = 0; i < RowsCount; i++)
				{
					rows[i].AddRange(rowItems);
				}				
			}		
			
			if (newSize.Height > RowsCount) 
			{	
				rowFixed.AddRange(Collection.Range(newSize.Height - RowsCount, false));
				var rowItems = Collection.Range(newSize.Width, EmptyLayout.Interface);				
				Func<List<ILayout>> generator = () => new List<ILayout>(rowItems);				
				rows.AddRange(Collection.Range(newSize.Height - RowsCount, generator));
			}
			
		}
		
		public void PutControl(Control control, int row, int column)
		{
			PutLayout(control != null ? new ControlWrapper(control) : EmptyLayout.Interface,
			          row, column);
		}		
		
		public void Fill(params Control[] controls)
		{
			var count = Math.Min(controls.Length, RowsCount * ColumnsCount);
			for (var i = 0; i < count; i++)
			{								
				var row = i / ColumnsCount;
				var column = i % ColumnsCount;				
				PutControl(controls[i] , row, column);
			}
		}
		
		public void PutLayout(ILayout layout, int row, int column)
		{
			if (row >= 0 && column >= 0)
			{
				Stretch(row + 1, column + 1);
				rows[row][column] = layout ?? EmptyLayout.Interface;
			}
		}				
		
		public void FixRows(params int[] indices)
		{
			if (indices.Length == 0)
			{
				for (var i = 0; i < rowFixed.Count; i++)
				{
					rowFixed[i] = true;
				}
			}
			else
			{
				foreach (var index in indices)
				{
					rowFixed[index] = true;
				}
			}
		}
		
		public void FixColumns(params int[] indices)
		{
			if (indices.Length == 0)
			{
				for (var i = 0; i < columnFixed.Count; i++)
				{
					columnFixed[i] = true;
				}
			}
			else
			{
				foreach (var index in indices)
				{
					columnFixed[index] = true;
				}
			}
		}
		
		public Size Measure()
		{
			var minWidth = Collection.SumMeasure(
				Columns, column => Collection.MaxMeasure(column, item => item.Measure().Width));
			var minHeight = Collection.SumMeasure(
				Rows, row => Collection.MaxMeasure(row, item => item.Measure().Height));
			
			return new Size(minWidth, minHeight);			
		}
		
		public void Apply(Control control, Rectangle bounds)
		{										
			var size = Measure();
			
			var freeColumns = Collection.SumMeasure(columnFixed, f => f ? 0 : 1);
			var freeRows = Collection.SumMeasure(rowFixed, f => f ? 0 : 1);
			var widthBonus = freeColumns == 0 ? 0 : 
				Math.Max(0, bounds.Width - size.Width) / freeColumns;
			var heightBonus = freeRows == 0 ? 0 : 
				Math.Max(0, bounds.Height - size.Height) / freeRows;
			
			var rowSizes = Collection.Map(Rows, row =>
				Collection.MaxMeasure(row, item => item.Measure().Height));
			var columnSizes = Collection.Map(Columns, column =>
				Collection.MaxMeasure(column, item => item.Measure().Width));
									
			var y = 0;			
			var yShift = bounds.Top;
			foreach (var ySize in rowSizes)
			{
				var rowSize = ySize + (rowFixed[y] ? 0 : heightBonus);
				var x = 0;
				var xShift = bounds.Left;			
				foreach (var xSize in columnSizes)
				{
					var columnSize = xSize + (columnFixed[x] ? 0 : widthBonus);
					var cellBounds = new Rectangle(
						xShift, yShift, columnSize, rowSize);
					var cell = rows[y][x];
					cell.Apply(control, cellBounds);
				
					xShift += columnSize;
					x += 1;
				}
				yShift += rowSize;				
				y += 1;
			}						
		}
	}
}