﻿//
// System.Random.cs
//
// Authors:
//   Bob Smith (bob@thestuff.net)
//   Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2001 Bob Smith.  http://www.thestuff.net
// (C) 2003 Ben Maurer
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace Outfit7.Util.Randomization {

    [Serializable]
    public class MonoRandom : Random {
        const int MBIG = int.MaxValue;
        const int MSEED = 161803398;
        const int MZ = 0;

        int inext, inextp;
        int[] SeedArray = new int [56];

        public MonoRandom()
            : this(Environment.TickCount) {
        }

        public MonoRandom(int seed) {
            int ii;
            int mj, mk;

            // Numerical Recipes in C online @ http://www.library.cornell.edu/nr/bookcpdf/c7-1.pdf
            mj = MSEED - Math.Abs(seed);
            SeedArray[55] = mj;
            mk = 1;
            for (int i = 1; i < 55; i++) {  //  [1, 55] is special (Knuth)
                ii = (21 * i) % 55;
                SeedArray[ii] = mk;
                mk = mj - mk;
                if (mk < 0)
                    mk += MBIG;
                mj = SeedArray[ii];
            }
            for (int k = 1; k < 5; k++) {
                for (int i = 1; i < 56; i++) {
                    SeedArray[i] -= SeedArray[1 + (i + 30) % 55];
                    if (SeedArray[i] < 0)
                        SeedArray[i] += MBIG;
                }
            }
            inext = 0;
            inextp = 31;
        }

        protected override double Sample() {
            int retVal;

            if (++inext >= 56)
                inext = 1;
            if (++inextp >= 56)
                inextp = 1;

            retVal = SeedArray[inext] - SeedArray[inextp];

            if (retVal < 0)
                retVal += MBIG;

            SeedArray[inext] = retVal;

            return retVal * (1.0 / MBIG);
        }

        public override int Next() {
            return (int) (Sample() * int.MaxValue);
        }

        public override int Next(int maxValue) {
            if (maxValue < 0)
                throw new ArgumentOutOfRangeException("Max value is less than min value.");

            return (int) (Sample() * maxValue);
        }

        public override int Next(int minValue, int maxValue) {
            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException("Min value is greater than max value.");

            // special case: a difference of one (or less) will always return the minimum
            // e.g. -1,-1 or -1,0 will always return -1
            uint diff = (uint) (maxValue - minValue);
            if (diff <= 1)
                return minValue;

            return (int) ((uint) (Sample() * diff) + minValue);
        }

        public override void NextBytes(byte[] buffer) {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            for (int i = 0; i < buffer.Length; i++) {
                buffer[i] = (byte) (Sample() * (byte.MaxValue + 1));
            }
        }

        public override double NextDouble() {
            return this.Sample();
        }
    }
}
