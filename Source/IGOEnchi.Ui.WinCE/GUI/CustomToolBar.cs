using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
 
namespace IGoEnchi
{
	public class CustomToolBar: ToolBar
	{
		private List<EventHandler> handlers;
		private int lockedButtons;
		
		public CustomToolBar(): base()
		{
			handlers = new List<EventHandler>();
			this.ButtonClick += new ToolBarButtonClickEventHandler(Click);
		}				
		
		private new void Click(object sender, ToolBarButtonClickEventArgs args)
		{
			handlers[Buttons.IndexOf(args.Button)].Invoke(args.Button, EventArgs.Empty);			
		}
		
		public void AddTool(Tool tool)
		{			
			Buttons.Add(tool.Button);
			ImageList.Images.Add(tool.Icon);
			handlers.Add(tool.OnClick);
			Buttons[Buttons.Count - 1].ImageIndex = 
				ImageList.Images.Count - 1;
		}
		
		private void Clear()
		{
			for (int i = Buttons.Count - 1; 
				     i >= lockedButtons; i--)
			{
				Buttons.RemoveAt(i);
				ImageList.Images.RemoveAt(i);
				handlers.RemoveAt(i);
			}
		}
		
		public void SetViewTools(Tool[] tools)
		{
			Clear();
			if (tools != null)
			{
				foreach (Tool tool in tools)
				{	
					AddTool(tool);
				}						
			}
		}
		
		public void Lock()
		{
			lockedButtons = Buttons.Count;
		}
		
		public void Unlock()
		{
			lockedButtons = 0;
		}
	}
	
	public struct Tool
	{
		private ToolBarButton button;
		private Icon icon;
		private EventHandler onClick;
		
		public Tool(ToolBarButton button, Icon icon, EventHandler onClick)
		{
			this.button = button;
			this.icon = icon;
			this.onClick = onClick;
		}
		
		public ToolBarButton Button
		{
			get {return button;}		
		}
		
		public Icon Icon
		{
			get {return icon;}
		}
		
		public EventHandler OnClick 
		{
			get {return onClick;}
		}
	}
}
