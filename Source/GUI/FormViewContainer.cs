using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
 
namespace IGoEnchi
{
	public class FormViewContainer
	{		
		private List<FormView> viewList;
		
		public FormView ActiveView {get; private set;}
		public ContextMenu ViewMenu  {get; private set;}
		public Form BaseForm  {get; private set;}
		public CustomToolBar ToolBar  {get; set;}		
				
		public FormViewContainer(Form baseForm)
		{
			viewList = new List<FormView>();
			ViewMenu = new ContextMenu();
			BaseForm = baseForm;
		}
		
		public List<FormView> Views
		{
			get {return viewList;}
		}
		
		public void AddView(FormView view)
		{
			if (view == null)
			{
				throw new Exception("Argument cannot be null");
			}
			
			if (!viewList.Contains(view))
			{
				viewList.Add(view);				
								
				MenuItem menuItem = new MenuItem();				
				menuItem.Text = view.Text;			
				menuItem.Click += new EventHandler(OnSwitchClick);
				ViewMenu.MenuItems.Add(menuItem);
				
				view.Container = this;
			}
			SwitchTo(view);						
		}
		
		private void OnSwitchClick(object sender, EventArgs args)
		{						
			if (sender is MenuItem)
			{				
				MenuItem menuItem = sender as MenuItem;
				FormView view = FindView(menuItem.Text);				
				if (view != null)
				{					
					SwitchTo(view);
				}
			}
		}
		
		public void HideView()
		{
			if (viewList.Count > 1)
			{
				FormView view = ActiveView;
				viewList.Remove(view);
				viewList.Add(view);
				SwitchTo(viewList[0] as FormView);
			}
		}
		
		public bool RemoveView(FormView view)
		{
			if (view == null)
			{
				throw new Exception("Argument cannot be null");
			}

			if (view.CanClose)
			{
				viewList.Remove(view);			
				ViewMenu.MenuItems.Remove(FindMenuItem(view.Text));

				view.OnClose(new EventArgs());
							
				if (viewList.Count > 0)
				{
					if (ActiveView == view) 			    
					{	
						SwitchTo(viewList[0] as FormView);
					}	
				}

				return true;
			}
			else
			{
				HideView();
			}
			return false;						
		}
		
		public bool Rename(string oldName, string newName)
		{
			if (FindView(newName) == null)
			{
				var view = FindView(oldName);
				if (view != null)
				{
					view.Text = newName;
					return true;
				}				
			}
			return false;			
		}
		
		public bool UpdateLink(string oldName, string newName)
		{			
			if (FindView(newName) == null)
			{
				MenuItem menuItem = FindMenuItem(oldName);
				if (menuItem != null)
				{
					menuItem.Text = newName;
					return true;
				}
			}			
			return false;			
		}
		
		public void SwitchTo(FormView view)
		{		
			if (view == null)
			{
				throw new Exception("Argument cannot be null");
			}
			else if (!viewList.Contains(view))
			{
				throw new Exception("Control doesn't contain the view specified");
			}
			
			viewList.Remove(view);
			viewList.Insert(0, view);
						
			if (ActiveView != null)
			{
				ActiveView.ClearForm(BaseForm);
			}
			
			ActiveView = view;						
			
			view.FillForm(BaseForm);
			view.OnSelect(new EventArgs());
			
			foreach (MenuItem item in ViewMenu.MenuItems)
			{
				item.Checked = false;
			}
				
			var menuItem = FindMenuItem(view.Text);
			if (menuItem != null)
			{
				menuItem.Checked = true;
			}
			
			if (ToolBar != null)
			{
				ToolBar.SetViewTools(view.Tools);
			}
			BaseForm.Invalidate();
			BaseForm.Activate();
		}
		
		public FormView FindView(String name)
		{
			foreach (FormView view in viewList)
			{
				if (view.Text == name)
				{
					return view;
				}
			}
			return null;
		}
		
		private MenuItem FindMenuItem(String name)
		{
			foreach (MenuItem menuItem in ViewMenu.MenuItems)
			{
				if (menuItem.Text == name)
				{
					return menuItem;
				}
			}
			return null;
		}
	}
}

