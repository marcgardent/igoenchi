using System;

namespace GoART.NAudioIntegration
{
    /// <summary>
    /// Additionnal class 
    /// </summary>
    public class EqualTemperamentTonesProvider
    {
        private readonly float _frequency;
        private static double R = Math.Pow(2, 1.0 / 12.0);

        public EqualTemperamentTonesProvider(float frequency = 55)
        {
            _frequency = frequency;
        }
        
        public float Generic(int octave, double ton)
        {
            return (float)(_frequency * Math.Pow(R, ton*2 + octave * 12));
        }

        public byte MainOctave = 4;

        public float Do(int octave) => Generic(octave, -4.5);
        public float DoDiese(int octave) => Generic(octave, -4);
        public float Re(int octave) => Generic(octave, -3.5);
        public float MiBemol(int octave) => Generic(octave, -3);
        public float Mi(int octave) => Generic(octave, -2.5);
        public float Fa(int octave) => Generic(octave, -2);
        public float FaDiese(int octave) => Generic(octave, -1.5);
        public float Sol(int octave) => Generic(octave, -1);
        public float SolDiese(int octave) => Generic(octave, -0.5);
        public float La(int octave) => Generic(octave, 0);
        public float SiBemol(int octave) => Generic(octave, 0.5);
        public float Si(int octave) => Generic(octave, 1);
        
        public float Do() => Do(MainOctave);
        public float DoDiese() => DoDiese(MainOctave);
        public float Re() => Re(MainOctave);
        public float MiBemol() => MiBemol(MainOctave);
        public float Mi() => Mi(MainOctave);
        public float Fa() => Fa(MainOctave);
        public float FaDiese() => FaDiese(MainOctave);
        public float Sol() => Sol(MainOctave);
        public float SolDiese() => SolDiese(MainOctave);
        public float La() => La(MainOctave);
        public float SiBemol() => SiBemol(MainOctave);
        public float Si() => Si(MainOctave);
    }
}