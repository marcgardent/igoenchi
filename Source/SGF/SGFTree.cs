/*  Copyright (C) 2006-2008 Valentin Kraevskiy
 * 	This file is part of IGoEnchi project
 *	Please read ..\license.txt for conditions of distribution and use
 */ 

using System;
using System.Collections.Generic;
 
namespace IGoEnchi
{
	public struct SGFProperty
	{
		private string name;
		private string value;
		
		public SGFProperty(string name, string value)
		{
			this.name = name;
			this.value = value;			
		}
		
		public string Name
		{
			get {return name;}
		}
		
		public string Value
		{
			get {return value;}
		}
	}
	
	public class SGFTree
	{
		private List<SGFTree> nodesList;		
		private List<SGFProperty> propertiesList;
		private SGFTree parentNode;
	
		public SGFTree()
		{
			nodesList = new List<SGFTree>();
			propertiesList = new List<SGFProperty>();			
		}
		
		public SGFTree Parent
		{
			get 
			{
				return this.parentNode;
			}
		}
		
		public List<SGFTree> ChildNodes
		{
			get 
			{
				return this.nodesList;
			}
		}
		
		public List<SGFProperty> Properties
		{
			get 
			{
				return this.propertiesList;
			}
		}
		
		public SGFTree NewNode()
		{
			SGFTree node = new SGFTree();
			AddNode(node);
			return node;							
		}
		
		public void AddNode(SGFTree node)
		{			
			nodesList.Add(node);
			node.parentNode = this;
		}
		
		public void RemoveNode(SGFTree node)
		{
			nodesList.Remove(node);
		}
		
		public void AddAttribute(SGFProperty property)
		{
			propertiesList.Add(property);
		}
		
		public void RemoveAttribute(SGFProperty property)
		{
			propertiesList.Remove(property);
		}
	}
}
