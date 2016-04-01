using System.Collections.Generic;

namespace IGOEnchi.BruGoClientLib
{
    public class BruGoCacheEntry
    {
        public string Description { get; set; }
        public BruGoNode[] Nodes { get; set; }
    }

    public class BruGoCache
    {
        private readonly List<BruGoCacheEntry> cache;

        public BruGoCache()
        {
            cache = new List<BruGoCacheEntry>();
        }

        public int Size
        {
            get { return cache.Count; }
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

        public BruGoCacheEntry Get()
        {
            if (cache.Count > 0)
            {
                var item = cache[cache.Count - 1];
                cache.RemoveAt(cache.Count - 1);
                return item;
            }
            return null;
        }
    }
}