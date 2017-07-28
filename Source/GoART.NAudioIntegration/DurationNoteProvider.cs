using System;

namespace GoART.NAudioIntegration
{
    /// <summary>
    /// the relative duration of a note, 
    /// https://en.wikipedia.org/wiki/Note_value
    /// </summary>
    public class DurationNoteProvider
    {
        public TimeSpan Fraction(double a) => TimeSpan.FromMilliseconds(Whole.TotalMilliseconds * a);

        public TimeSpan Whole;
        public TimeSpan octupleWhole => Fraction(8);
        public TimeSpan quadrupleWhole => Fraction(4);
        public TimeSpan doubleWhole => Fraction(2);

        public TimeSpan Half => Fraction(0.5);
        public TimeSpan Quarter => Fraction(0.25);
        public TimeSpan Eighth => Fraction(0.125);

        public DurationNoteProvider(TimeSpan whole)
        {
            this.Whole = whole;
        }
    }
}