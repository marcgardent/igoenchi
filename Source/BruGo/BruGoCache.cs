using System;
using System.Collections.Generic;

namespace IGoEnchi
{
	public class BruGoCacheEntry
	{
		public string Description {get; set;}
		public BruGoNode[] Nodes {get; set;}
	}
	
	public class BruGoCache
	{
		private List<BruGoCacheEntry> cache;
		
		public BruGoCache()
		{
			cache = new List<BruGoCacheEntry>();
		}
		
		public void Clear()
		{
			cache.Clear();
		}
		
		public bool IsEmpty()
		{
			return cache.Count == 0;
		}
		
		public void Add(string description, BruGoNode[] nodes)
		{
			cache.Add(new BruGoCacheEntry 
			          { 
			          	Description = description,
			          	Nodes = nodes
			          });
		}
		
		public int Size
		{
			get {return cache.Count;}
		}
		
		public BruGoCacheEntry Get()
		{
			if (cache.Count > 0)
			{
				var item = cache[cache.Count - 1];
				cache.RemoveAt(cache.Count - 1);
				return item;
			}
			else
			{
				return null;
			}
		}
	}
}