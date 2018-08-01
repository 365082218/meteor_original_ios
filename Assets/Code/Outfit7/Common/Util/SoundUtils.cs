//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using UnityEngine;
using System.Collections;

namespace Outfit7.Util {

    /// <summary>
    /// (taken from WhiteTileGameMusicController)
    ///
    ///    I will use the following notation:
    ///    a^b means "a to the power of b". So for example
    ///    a^1 = a
    ///    a^2 = a*a
    ///    a^3 = a*a*a
    ///    a^4 = a*a*a*a
    ///    etc.
    ///
    ///    A speed multiple of 2x will put you an ocatve above the original pitch.
    ///    This is equivalent to a rise of 12 semitones.
    ///    As a rise of a semitone always causes a constant multiplier (lets call this x) the following holds true:
    ///
    ///    x^12 = 2
    ///
    ///    Therefore x = 12th root of 2 = 2^(1/12) = 1.0594630943592952645618252949461
    ///    Notice this is pretty close to a 6% increase as stated above.
    ///
    ///    To move down by a semitone you simply use the inverse of this number (i.e. 1/x)
    ///    lets call it y = 0.94387431268169349664191315666784
    ///
    ///    So in summary the equations for your purpose are:
    ///    speed_increase_in_percent = ((x^number_of_semitones) * 100) - 100
    ///    speed_decrease_in_persent = 100 - ((y^number_of_semitones) * 100)
    ///
    ///    You should be able to just plug numbers in and you will get a value in the format you specified.
    ///    You can use windows calculator to do "to the power of" by selecting "view->scientific" and using the x^y button. You don't need to be as precise as I have been and can probably use the following with little impact:
    ///    x = 1.05946
    ///    y = 0.94387
    ///
    /// </summary>
    public static class SoundUtils {

        /// A major scale is a diatonic scale. The sequence of intervals between the notes of a major scale is:
        /// "whole, whole, half, whole, whole, whole, half".
        /// This array represents the increments in half-tones from the base tone from the start of the scale.
        public static readonly int[] MajorScaleTonesInc = new int[] { 0, 2, 4, 5, 7, 9, 11, 12 };

        /// <summary>
        /// The index of the base tone in MIDI notation. 60 is tone C4.
        /// </summary>
        private const int BaseToneIndex = 60;
        private const float SemitoneConstUp = 1.0594630943592952645618252949461f;
        private const float SemitoneConstDown = 0.94387431268169349664191315666784f;

        // TODO: add support for GetPitchForTone in string notation e.g. GetPitchForTone("C4", "D5#");

        /// <summary>
        /// Gets the pitch for the wanted tone.
        /// Tone index increase is a half tone increase.
        /// </summary>
        /// <returns>The pitch for tone.</returns>
        /// <param name="toneIndexDiff">Just the difference in tone from some base tone.</param>
        public static float GetPitchForTone(int toneIndexDiff) {
            return GetPitchForTone(0, toneIndexDiff);
        }

        /// <summary>
        /// Gets the pitch for the wanted tone.
        /// Tone index increase is a half tone increase.
        /// </summary>
        /// <returns>The pitch for tone.</returns>
        /// <param name="baseToneIndex">Base tone index.</param>
        /// <param name="wantedToneIndex">Wanted tone index.</param>
        public static float GetPitchForTone(int baseToneIndex, int wantedToneIndex) {
            int tone = wantedToneIndex - baseToneIndex;

            if (tone == 0) {
                return 1;
            } else if (tone > 0) {
                return Mathf.Pow(SemitoneConstUp, tone);
            } else {
                return Mathf.Pow(SemitoneConstDown, -tone); // tone must always be a positive value!!!
            }
        }

        public static int GetMajorScaleToneInc(int i) {
            return GetScaleToneInc(i, MajorScaleTonesInc);
        }

        /// <summary>
        /// Gets the scale tone increment for some index i.
        /// </summary>
        /// <returns>The major scale tone inc.</returns>
        /// <param name="i">The index.</param>
        /// <param name = "scale"></param>
        public static int GetScaleToneInc(int i, int[] scale) {
            int quotient = i / scale.Length;
            int reminder = i % scale.Length;

            return scale[scale.Length - 1] * quotient + scale[reminder];
        }

        /// <summary>
        /// Gets the scale tone increment for some index i in a ping-pong matter.
        /// It basically converts an array of tones from, for example, [1, 2, 3, 4] to [1, 2, 3, 4, 3, 2].
        /// </summary>
        /// <returns>The scale tone index.</returns>
        /// <param name="i">The index.</param>
        /// <param name = "scale"></param>
        public static int GetScaleToneIncPingPong(int i, int[] scale) {
            int index = i % ((scale.Length * 2) - 2);
            if (index >= scale.Length) {
                index = scale.Length * 2 - 2 - index;
            }
            return scale[index];
        }
    }
}

