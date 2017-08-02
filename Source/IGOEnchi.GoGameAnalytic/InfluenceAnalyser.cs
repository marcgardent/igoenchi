using System.Collections.Generic;
using IGOEnchi.GoGameLogic;
using IGOPhoenix.GoGameAnalytic.BitPlaneParsing;
using IGOPhoenix.GoGameAnalytic.Maps;
using IGOPhoenix.GoGameAnalytic.Maps.Influence.RayTracing;

namespace IGOPhoenix.GoGameAnalytic
{
    public class InfluenceAnalyser
    {
        public readonly Map whiteInfluence;
        public readonly Map blackInfluence;
        public Map MoveInfluence;
        
        public readonly List<TerritoryParser.TerritoryCoords> BlackTerritories;
        public readonly List<TerritoryParser.TerritoryCoords> WhiteTerritories;
        public readonly List<TerritoryParser.TerritoryCoords> CommonBorders;
        public readonly List<TerritoryParser.TerritoryCoords> ContestTerritories;

        public enum TerritoryState
        {
            Black,
            White,
            Contest,
        }

        public InfluenceAnalyser(Board board, GoMoveNode lastmove)
        {
            var solids = board.BlackAndWhite;

            this.whiteInfluence = new RayTracer(board.White, solids).GetMap();
            this.blackInfluence = new RayTracer(board.Black, solids).GetMap();
            var influence = this.whiteInfluence - blackInfluence;

            IMapper<TerritoryState> mapper = new ThresholdMapper<TerritoryState>()
                .Lt(-1 / 32d, TerritoryState.Black)
                .Le(1 / 32d, TerritoryState.Contest)
                .Gt(1 / 32d, TerritoryState.White);

            var layers = new LayersMap<TerritoryState>(influence, mapper);

            var tparser = new TerritoryParser();
            var black = layers.GetLayer(TerritoryState.Black);
            this.BlackTerritories = tparser.Parse(black);
            this.WhiteTerritories = tparser.Parse(layers.GetLayer(TerritoryState.White));
            this.ContestTerritories = tparser.Parse(layers.GetLayer(TerritoryState.Contest));
             

            var single = new BitPlane(board.Size);
            single.Add(lastmove.Stone);
            this.MoveInfluence = new RayTracer(single, solids).GetMap();
        }

        
    }
}