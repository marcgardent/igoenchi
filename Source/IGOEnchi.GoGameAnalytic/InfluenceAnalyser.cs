using System.Collections.Generic;
using System.Linq;
using IGOEnchi.GoGameLogic;
using IGOPhoenix.GoGameAnalytic.BitPlaneParsing;
using IGOPhoenix.GoGameAnalytic.Maps;
using IGOPhoenix.GoGameAnalytic.Maps.Influence.RayTracing;

namespace IGOPhoenix.GoGameAnalytic
{
    public class InfluenceAnalyser
    {
        public readonly Map WhiteSupport;
        public readonly Map BlackSupport;
        
        public Map MoveInfluence;

        public readonly LocalInfluence LocalInfluence;

        public readonly List<TerritoryParser.TerritoryCoords> BlackTerritories;
        public readonly List<TerritoryParser.TerritoryCoords> WhiteTerritories;
        public readonly List<TerritoryParser.TerritoryCoords> ContestTerritories;

        public enum TerritoryState
        {
            Black,
            White,
            Contest,
        }

        public InfluenceAnalyser(Board board, GoMoveNode lastmove)
        {
            this.LocalInfluence= new LocalInfluence(board);

            
            var solidsBlack = board.BlackAndWhite.Or(this.LocalInfluence.White);
            var solidsWhite = board.BlackAndWhite.Or(this.LocalInfluence.Black);

            this.WhiteSupport = new RayTracer(board.Black, solidsWhite).GetMap();
            this.BlackSupport = new RayTracer(board.Black, solidsBlack).GetMap();

            var single = new BitPlane(board.Size);
            single.Add(lastmove.Stone);
            this.MoveInfluence = new RayTracer(single, lastmove.Stone.IsBlack? solidsBlack : solidsWhite).GetMap();

            var influence = this.WhiteSupport - BlackSupport;
             

            var contestThreshold = 4d;
            IMapper<TerritoryState> mapper = new ThresholdMapper<TerritoryState>()
                .Lt(-1 / contestThreshold, TerritoryState.Black)
                .Le(1 / contestThreshold, TerritoryState.Contest)
                .Gt(1 / contestThreshold, TerritoryState.White);

            var layers = new LayersMap<TerritoryState>(influence, mapper);

            var tparser = new TerritoryParser();
            var black = layers.GetLayer(TerritoryState.Black);
            this.BlackTerritories = tparser.Parse(black);
            this.WhiteTerritories = tparser.Parse(layers.GetLayer(TerritoryState.White));
            this.ContestTerritories = tparser.Parse(layers.GetLayer(TerritoryState.Contest));
             


        }

        
    }
}