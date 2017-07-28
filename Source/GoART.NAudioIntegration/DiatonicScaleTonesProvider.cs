namespace GoART.NAudioIntegration
{
    
    /// <summary>
    /// Tonal music
    /// </summary>
    public class DiatonicScaleTonesProvider
    {

        public Mode CurrentMode
        {
            get; set;
        }

        public enum Mode
        {
            Major,
            Minor
        }

        public readonly int octave;

        private readonly EqualTemperamentTonesProvider tones;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tones"></param>
        /// <param name="octave"></param>
        public DiatonicScaleTonesProvider(EqualTemperamentTonesProvider tones, int octave )
        {
            this.tones = tones;
            this.octave = octave; 
        }


        private static double[] majorIntervals = new []{0, 1, 2,   2.5, 3.5, 4.5, 5.5, 6};
        private static double[] minorIntervals = new []{0, 1, 1.5, 2.5, 3.5, 4,   5.5, 6};
        
        private double[] CurrentIntervals => CurrentMode == Mode.Major ? majorIntervals : minorIntervals;
        private double TonalInterval(int degree) => CurrentIntervals[(degree - 1)%8] ;
        
        public float TonalDegree(int degree) => tones.Generic(octave, TonalInterval(degree));

        public float Tonique => TonalDegree(1);
        public float SusTonique => TonalDegree(2);
        public float Mediante => TonalDegree(3);
        public float SousDominante => TonalDegree(4);
        public float Dominante => TonalDegree(5);
        public float SusDominante => TonalDegree(6);
        public float Sensible => TonalDegree(7);
        public float Octave => TonalDegree(8);
    }
}