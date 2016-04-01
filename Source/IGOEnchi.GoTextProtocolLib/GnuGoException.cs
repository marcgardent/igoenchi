using System;

namespace IGoEnchi
{
    public class GnuGoException : Exception
    {
        public readonly GnuGoError Kind;

        public GnuGoException(GnuGoError kind)
        {
            Kind = kind;
        }
    }
}