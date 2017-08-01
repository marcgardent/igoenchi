using System;
using System.Linq;
using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.GoGameAnalytic.Maps.Influence.RayTracing
{
    /// <summary>
    /// Raytracing inspiration to compute Influence of stones
    /// </summary>
    public class RayTracer : IMapProcessor
    {
        private readonly BitPlane lights;
        private readonly BitPlane solids;
        
        public RayTracer(BitPlane lights, BitPlane solids)
        {
            this.lights = lights;
            this.solids = solids;
        }
        
        public Map GetMap()
        {
            var ret = new Map(lights.Width, lights.Height);

            foreach (var light in lights.Unabled)
            {
                foreach (var dest in solids.AllCoords)
                {
                    var ray = new Ray(light, dest);
                    var last = ray.Points.TakeWhile(x => !(x.All().Any(c=>solids[c]))).LastOrDefault();
                    if (last != null && CoordsUtils.Equals(dest, last))
                    {
                        ret[dest.X, dest.Y] += 1.0 / (Math.Abs(light.X - dest.X) + Math.Abs(light.Y - dest.Y));
                    }
                }
            }

            return ret;
        }

        public Map GetMapWIP()
        {
            var ret= new Map(lights.Width, lights.Height);

            foreach (var light in lights.Unabled)
            {
                var bag = solids.AllCoords.ToList();
                bag.RemoveAt(bag.FindIndex(c => c.X == light.X && c.Y == light.Y));
                
                while (bag.Any())
                {
                    var dest = bag.First();
                    bag.RemoveAt(0);
                    var path = new Ray(light, dest);
                    
                    foreach (var coordse in path.Points)
                    { 
                        if (solids[coordse])
                        {
                            break;
                        }
                        else
                        {
                            var found = bag.FindIndex(c => c.X == dest.X && c.Y == dest.Y);
                            if (found >= 0)
                            {
                                bag.RemoveAt(found);
                            }
                            if (found >= 0 || dest.X == coordse.X && dest.Y == dest.Y)
                            {
                                ret[dest.X, dest.Y] += 1.0/(Math.Abs(light.X - coordse.X) + Math.Abs(light.Y - coordse.Y));
                            }
                        }
                    } 
                }
            }

            return ret;
        }
    }
}