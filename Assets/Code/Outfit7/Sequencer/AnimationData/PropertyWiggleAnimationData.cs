using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    public static class Perlin {
        
        public static float Noise(float x) {
            var X = Mathf.FloorToInt(x) & 0xff;
            x -= Mathf.Floor(x);
            var u = Fade(x);
            return Lerp(u, Grad(perm[X], x), Grad(perm[X + 1], x - 1)) * 2;
        }

        /*public static float Fbm(float x, int octave) {
            var f = 0.0f;
            var w = 0.5f;
            for (var i = 0; i < octave; i++) {
                f += w * Noise(x);
                x *= 2.0f;
                w *= 0.5f;
            }
            return f;
        }*/

        static float Fade(float t) {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        static float Lerp(float t, float a, float b) {
            return a + t * (b - a);
        }

        static float Grad(int hash, float x) {
            return (hash & 1) == 0 ? x : -x;
        }

        static int[] perm = {
            151, 160, 137, 91, 90, 15,
            131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23,
            190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33,
            88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166,
            77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244,
            102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196,
            135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123,
            5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42,
            223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
            129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228,
            251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107,
            49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254,
            138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180,
            151
        };
    }

    public class PropertyWiggleAnimationData : PropertyAnimationData {
        public AnimationCurve CustomCurve = new AnimationCurve(new Keyframe[]{ new Keyframe(0f, 0f), new Keyframe(0.2f, 1f), new Keyframe(0.8f, 1f), new Keyframe(1f, 0f) });
        public ParameterFloatField Frequency = new ParameterFloatField(5f);
        public ParameterVectorField StartValue = new ParameterVectorField(Vector4.zero);
        public ParameterVectorField EndValue = new ParameterVectorField(Vector4.zero);


        public override void Init(BaseProperty property) {
            StartValue.Init(property);
            EndValue.Init(property);
        }

        public override void SetStartingValues(Vector4 current) {
            StartValue.Value = current;
            EndValue.Value = Vector4.one;
        }

        public override Vector4 Evaluate(float absoluteTime, float duration) {
            //Debug.LogError(Perlin.Noise((absoluteTime / duration) * 3));
            float normalizedTime = absoluteTime / duration;
            float valueX = StartValue.Value.x + (Perlin.Noise(normalizedTime * Frequency.Value) * EndValue.Value.x);
            float valueY = StartValue.Value.y + (Perlin.Noise(500 + (normalizedTime * Frequency.Value)) * EndValue.Value.y);
            float valueZ = StartValue.Value.z + (Perlin.Noise(1000 + (normalizedTime * Frequency.Value)) * EndValue.Value.z);
            return Vector4.Lerp(StartValue.Value, new Vector4(valueX, valueY, valueZ, StartValue.ValueW), CustomCurve.Evaluate(normalizedTime));
            //return Vector4.Lerp(StartValue, EndValue, normalizedTime);
        }

        public override Vector2 GetMinMax(BaseProperty property) {
            Vector4 minVector = Vector4.Min(StartValue.Value, EndValue.Value);
            float min = Mathf.Min(Mathf.Min(Mathf.Min(minVector.x, minVector.y), minVector.z), minVector.w);
            Vector4 maxVector = Vector4.Max(StartValue.Value, EndValue.Value);
            float max = Mathf.Max(Mathf.Max(Mathf.Max(maxVector.x, maxVector.y), maxVector.z), maxVector.w);
            return new Vector2(min, max);
        }
    }

}