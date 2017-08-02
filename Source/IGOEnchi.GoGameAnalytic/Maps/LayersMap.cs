using System.Collections.Generic;
using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.GoGameAnalytic.Maps
{
    public class LayersMap<TGroup>
    {
        private readonly Map map;
        private readonly IMapper<TGroup> mapping;
        private readonly Dictionary<TGroup, BitPlane> Layers = new Dictionary<TGroup, BitPlane>();

        public LayersMap(Map map, IMapper<TGroup> mapping)
        {
            this.map = map;
            this.mapping = mapping;
            
            foreach (var coords in map.AllCoords)
            {
                var grp = this.mapping.Map(map[coords]);

                if (!Layers.ContainsKey(grp))
                {
                    Layers[grp] = new BitPlane((byte)map.Width, (byte)map.Height);
                }
                Layers[grp][coords] = true;
            }
        }

        public BitPlane GetLayer(TGroup group) => Layers.ContainsKey(group) ? Layers[group] : new BitPlane(map.Width, map.Height);

    }
}