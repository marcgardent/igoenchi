using System;
using System.Collections.Generic;

namespace IGoEnchi
{
	public class BruGoNode
	{
		public int X {get; set;}
		public int Y {get; set;}
		public string ID {get; set;}
		public string Description {get; set;}		
		
		public BruGoNode()
		{
			ID = "";
			Description = "";
		}
	}
	
	public static class BruGoAPIParser
	{
		private static BruGoNode GetNode(Dictionary<char, BruGoNode> dictionary,
		                                 char key)
		{
			BruGoNode node = null;
			if (!dictionary.TryGetValue(key, out node))
			{
				node = new BruGoNode();
				dictionary[key] = node;
			}
			return node;			
		}
		
		public static BruGoNode[] Parse(string position)
		{
			var dictionary = new Dictionary<char, BruGoNode>();
			var lines = position.Split('\n');
			
			var y = 0;
			foreach (var line in lines)
			{				
				if (line.StartsWith("|"))
				{
					for (var x = 0; x < line.Length - 2; x++)
					{
						var symbol = line[x + 1];
						if (symbol != '_' && symbol != '|' &&
						   	symbol != 'W' && symbol != 'B')
						{
							var node = GetNode(dictionary, symbol);
							node.X = x;
							node.Y = y;
						}
					}
					y += 1;
				}
				else
				{
					var properties = line.Split(':', '|');					
					
					var symbol = properties.Length > 0 ? properties[0].Trim() : "";
					var id = properties.Length > 1 ? properties[1].Trim() : "";
					var description = properties.Length > 2 ? properties[2].Trim() : "";
					
					if (symbol.Length == 1 && id.Length > 0)
					{
						var node = GetNode(dictionary, symbol[0]);
						node.ID = id;
						node.Description = description;
					}
				}
			}
			
			var nodes = new BruGoNode[dictionary.Count];
			var index = 0;
			foreach (var node in dictionary.Values)
			{
				nodes[index] = node;
				index += 1;
			}
			
			return nodes;
		}
	}
}