using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.WindowsCE.Forms;
using CE = Microsoft.WindowsCE.Forms;

namespace IGoEnchi
{
	public delegate void HardwareButtonHandler();
					
	public enum HardwareButton {B1, B2, B3, B4, B5, B6}
		
	public class HardwareButtonBindings
	{
		private static HardwareButton[] buttonValues =
			new HardwareButton[] 
			{
				HardwareButton.B1,
				HardwareButton.B2,
				HardwareButton.B3,
				HardwareButton.B4
			};
		private static Dictionary<string, HardwareButtonHandler> actions =
			new Dictionary<string, HardwareButtonHandler>();
		private static Dictionary<HardwareKeys, string> bindings =
			new Dictionary<HardwareKeys, string>();	
		private static List<CE.HardwareButton> buttons =
			new List<CE.HardwareButton>();
		
		public static HardwareButton[] Buttons
		{
			get {return buttonValues;}
		}
		
		private static HardwareKeys ButtonConvert(HardwareButton button)
		{
			switch (button)
			{
					case HardwareButton.B1:
						return HardwareKeys.ApplicationKey1;
					case HardwareButton.B2:
						return HardwareKeys.ApplicationKey2;
					case HardwareButton.B3:
						return HardwareKeys.ApplicationKey3;
					case HardwareButton.B4:
						return HardwareKeys.ApplicationKey4;
					case HardwareButton.B5:
						return HardwareKeys.ApplicationKey5;
					case HardwareButton.B6:
						return HardwareKeys.ApplicationKey6;
					default:
						throw new ArgumentException("Invalid button value");
			}
			//return (HardwareKeys) button;
		}
		
		public static void Register(string name, HardwareButtonHandler handler)
		{
			actions[name] = handler;
		}
					
		public static void Unbind(HardwareButton key)
		{						
			var boundButton = buttons.Find(
				delegate (CE.HardwareButton button)
			    {
					return button.HardwareKey == ButtonConvert(key);
				});			
			if (boundButton != null)
			{				
				buttons.Remove(boundButton);
				boundButton.AssociatedControl = null;
				boundButton.Dispose();
				bindings.Remove(ButtonConvert(key));
			}
		}
		
		public static void Bind(HardwareButton button, string actionName)
		{
			var key = ButtonConvert(button);

			Unbind(button);			
			
			bindings[key] = actionName;
			buttons.Add(
				new CE.HardwareButton()
				{
					HardwareKey = key,
					AssociatedControl = ConfigManager.MainForm,
				});
		}
		
		public static string GetBinding(HardwareButton button)
		{
			var key = ButtonConvert(button);
			return bindings.ContainsKey(key) ? bindings[key] : String.Empty;
		}
		
		public static string[] GetBindings()
		{
			var result = new string[Buttons.Length];
			for (var i = 0; i < result.Length; i++)
			{
				result[i] = GetBinding(Buttons[i]);
			}
			return result;
		}
		
		public static void SetBindings(string[] actionNames)
		{			
			for (var i = 0; i < Math.Min(actionNames.Length, Buttons.Length); i++)
			{
				Bind(Buttons[i], actionNames[i]);
			}			
		}
		
		public static List<string> GetActions()
		{
			var result = new List<string>();
			foreach (var name in actions.Keys)
			{
				result.Add(name);
			}
			return result;
		}
		
		private static void KeyDown(object sender, KeyEventArgs args)
		{
			foreach (var button in buttons)
			{
				if ((HardwareKeys) args.KeyCode == button.HardwareKey)
				{
					if (actions.ContainsKey(bindings[button.HardwareKey]))
					{
						var action = actions[bindings[button.HardwareKey]];
						if (action != null)
						{
							action();
						}
					}
				}
			}		
		}
		
		public static void AddKeySource(Control control)
		{
			control.KeyDown += KeyDown;
		}
		
		public static void RemoveKeySource(Control control)
		{
			control.KeyDown -= KeyDown;
		}
		
		public static void RemoveBindings()
		{
			foreach (var button in buttons)
			{
				button.AssociatedControl = null;
				button.Dispose ();
			}
			buttons.Clear();
			bindings.Clear();
		}
	}
}

