//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;

namespace Outfit7.Util {

    /// <summary>
    /// Provides thread-safe static methods for getting random data by using one Random object.
    /// </summary>
    public static class StaticRandom {

        private static readonly Random random = new Random();

        public static int Next() {
            lock (random) { // Random is not thread safe
                return random.Next();
            }
        }

        public static int Next(int maxValue) {
            lock (random) { // Random is not thread safe
                return random.Next(maxValue);
            }
        }

        public static int Next(int minValue, int maxValue) {
            lock (random) { // Random is not thread safe
                return random.Next(minValue, maxValue);
            }
        }

        public static void NextUnique(ref int lastValue, int minValue, int maxValue) {
            lock (random) { // Random is not thread safe
                // Get next value but must not be the same as last value (max 5 iterations)
                for (int i = 0; i < 5; i++) {
                    int value = random.Next(minValue, maxValue);
                    if (value != lastValue) {
                        lastValue = value;
                        return;
                    }
                }
                // If the new value was not found move the value up or down
                if (random.Next(100) >= 50) {
                    lastValue++;
                    if (lastValue >= maxValue) {
                        lastValue = minValue;
                    }
                } else {
                    lastValue--;
                    if (lastValue < minValue) {
                        lastValue = maxValue - 1;
                    }
                }
            }
        }

        public static bool NextBool() {
            return Next(2) == 0;
        }

        public static void NextBytes(byte[] buffer) {
            lock (random) { // Random is not thread safe
                random.NextBytes(buffer);
            }
        }

        public static float NextFloat() {
            return (float) NextDouble();
        }

        public static float NextFloat(float maxValue) {
            return (float) NextDouble() * maxValue;
        }

        public static float NextFloat(float minValue, float maxValue) {
            return (float) NextDouble() * (maxValue - minValue) + minValue;
        }

        public static double NextDouble() {
            lock (random) { // Random is not thread safe
                return random.NextDouble();
            }
        }

        public static double NextDouble(double maxValue) {
            return NextDouble() * maxValue;
        }

        public static double NextDouble(double minValue, double maxValue) {
            return NextDouble() * (maxValue - minValue) + minValue;
        }

        public static T NextElement<T>(IList<T> collection) {
            return collection[Next(collection.Count)];
        }
    }
}
