using System;
using System.Collections.Generic;
using System.Text;

namespace Ristysoft
{
    public class IntegerRange
    {
        public IntegerRange(int from, int to)
        {
            if (from <= to)
            {
                From = from;
                To = to;
            }
            else
            {
                To = from;
                From = to;
            }
        }

        public int From { get; private set; }

        public int To { get; private set; }

        public override string ToString()
        {
            if (From == To)
                return From.ToString();
            else
                return string.Format("{0}-{1}", From, To);
        }
    }
}
