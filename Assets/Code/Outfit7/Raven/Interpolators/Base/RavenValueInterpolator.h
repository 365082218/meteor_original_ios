#ifdef STARLITE
#pragma once

#include <limits>
#include <Utility/RavenLog.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
#define NOTIMPLEMENTED(_name_)                                                                                                                                                                         \
    pRavenLog->Error("%s not implemented!", #_name_);                                                                                                                                                  \
    return T();

        template <class T> class RavenValueInterpolator {
        public:
            RavenValueInterpolator() = delete;

            static double MaxValue(const T& value1, const T& value2);
            static double MinValue(const T& value1, const T& value2);
            static T Add(const T& value, const T& addValue);
            static T Interpolate(const T& start, const T& end, double t);
            static T Multiply(const T& value, const T& multiplyValue);
            static T MultiplyScalar(const T& value, double scalar);
            static T Random(const T& min, const T& max);
        };

        template <> class RavenValueInterpolator<int> {
        public:
            static double MaxValue(const int& value1, const int& value2) {
                return Math::Max(value1, value2);
            }

            static double MinValue(const int& value1, const int& value2) {
                return Math::Min(value1, value2);
            }

            static int Add(const int& value, const int& addValue) {
                return value + addValue;
            }

            static int Interpolate(const int& start, const int& end, double t) {
                return (int)(start + (end - start) * t);
            }

            static int Multiply(const int& value, const int& multiplyValue) {
                return value * multiplyValue;
            }

            static int MultiplyScalar(const int& value, double scalar) {
                return (int)(value * scalar);
            }

            static int Random(const int& min, const int& max) {
                return (int)pRandomManager->GetStaticRandom().Next(min, max);
            }
        };

        template <> class RavenValueInterpolator<float> {
        public:
            static double MaxValue(const float& value1, const float& value2) {
                return Math::Max(value1, value2);
            }

            static double MinValue(const float& value1, const float& value2) {
                return Math::Max(value1, value2);
            }

            static float Add(const float& value, const float& addValue) {
                return value + addValue;
            }

            static float Interpolate(const float& start, const float& end, double t) {
                return Math::Lerp(start, end, (float)t);
            }

            static float Multiply(const float& value, const float& multiplyValue) {
                return value * multiplyValue;
            }

            static float MultiplyScalar(const float& value, double scalar) {
                return value * (float)scalar;
            }

            static float Random(const float& min, const float& max) {
                return (float)pRandomManager->GetStaticRandom().NextDouble(min, max);
            }
        };

        template <> class RavenValueInterpolator<double> {
        public:
            static double MaxValue(const double& value1, const double& value2) {
                return Math::Max(value1, value2);
            }

            static double MinValue(const double& value1, const double& value2) {
                return Math::Min(value1, value2);
            }

            static double Add(const double& value, const double& addValue) {
                return value + addValue;
            }

            static double Interpolate(const double& start, const double& end, double t) {
                return Math::Lerp(start, end, t);
            }

            static double Multiply(const double& value, const double& multiplyValue) {
                return value * multiplyValue;
            }

            static double MultiplyScalar(const double& value, double scalar) {
                return value * scalar;
            }

            static double Random(const double& min, const double& max) {
                return pRandomManager->GetStaticRandom().NextDouble(min, max);
            }
        };

        template <> class RavenValueInterpolator<bool> {
        public:
            static double MaxValue(const bool& value1, const bool& value2) {
                return value1 || value2 ? 1 : 0;
            }

            static double MinValue(const bool& value1, const bool& value2) {
                return !value1 || !value2 ? 0 : 1;
            }

            static bool Add(const bool& value, const bool& addValue) {
                return value | addValue;
            }

            static bool Interpolate(const bool& start, const bool& end, double t) {
                return t < 0.5 ? start : end;
            }

            static bool Multiply(const bool& value, const bool& multiplyValue) {
                return value & multiplyValue;
            }

            static bool MultiplyScalar(const bool& value, double scalar) {
                return value;
            }

            static bool Random(const bool& min, const bool& max) {
                return pRandomManager->GetStaticRandom().Next(2) == 0;
            }
        };

        template <> class RavenValueInterpolator<Vector2> {
        public:
            static double MaxValue(const Vector2& value1, const Vector2& value2) {
                double maxValue = std::numeric_limits<double>::lowest();

                for (Size_T i = 0; i < c_ValueCount; i++) {
                    if (value1.data[i] > maxValue) {
                        maxValue = value1.data[i];
                    }
                    if (value2.data[i] > maxValue) {
                        maxValue = value2.data[i];
                    }
                }

                return maxValue;
            }

            static double MinValue(const Vector2& value1, const Vector2& value2) {
                double minValue = std::numeric_limits<double>::max();
                for (int i = 0; i < c_ValueCount; ++i) {
                    if (value1.data[i] < minValue) {
                        minValue = value1.data[i];
                    }
                    if (value2.data[i] < minValue) {
                        minValue = value2.data[i];
                    }
                }

                return minValue;
            }

            static Vector2 Add(const Vector2& value, const Vector2& addValue) {
                return value + addValue;
            }

            static Vector2 Interpolate(const Vector2& start, const Vector2& end, double t) {
                return start + (end - start) * (float)t;
            }

            static Vector2 Multiply(const Vector2& value, const Vector2& multiplyValue) {
                return value * multiplyValue;
            }

            static Vector2 MultiplyScalar(const Vector2& value, double scalar) {
                return value * (float)scalar;
            }

            static Vector2 Random(const Vector2& min, const Vector2& max) {
                Vector2 v;
                for (int i = 0; i < c_ValueCount; ++i) {
                    v.data[i] = (float)pRandomManager->GetStaticRandom().NextDouble(min.data[i], max.data[i]);
                }
                return v;
            }

        private:
            static const int c_ValueCount = 2;
        };

        template <> class RavenValueInterpolator<Vector3> {
        public:
            static double MaxValue(const Vector3& value1, const Vector3& value2) {
                double maxValue = std::numeric_limits<double>::lowest();

                for (Size_T i = 0; i < c_ValueCount; i++) {
                    if (value1.data[i] > maxValue) {
                        maxValue = value1.data[i];
                    }
                    if (value2.data[i] > maxValue) {
                        maxValue = value2.data[i];
                    }
                }

                return maxValue;
            }

            static double MinValue(const Vector3& value1, const Vector3& value2) {
                double minValue = std::numeric_limits<double>::max();
                for (int i = 0; i < c_ValueCount; ++i) {
                    if (value1.data[i] < minValue) {
                        minValue = value1.data[i];
                    }
                    if (value2.data[i] < minValue) {
                        minValue = value2.data[i];
                    }
                }

                return minValue;
            }

            static Vector3 Add(const Vector3& value, const Vector3& addValue) {
                return value + addValue;
            }

            static Vector3 Interpolate(const Vector3& start, const Vector3& end, double t) {
                return start + (end - start) * (float)t;
            }

            static Vector3 Multiply(const Vector3& value, const Vector3& multiplyValue) {
                return value * multiplyValue;
            }

            static Vector3 MultiplyScalar(const Vector3& value, double scalar) {
                return value * (float)scalar;
            }

            static Vector3 Random(const Vector3& min, const Vector3& max) {
                Vector3 v;
                for (int i = 0; i < c_ValueCount; ++i) {
                    v.data[i] = (float)pRandomManager->GetStaticRandom().NextDouble(min.data[i], max.data[i]);
                }
                return v;
            }

        private:
            static const int c_ValueCount = 3;
        };

        template <> class RavenValueInterpolator<Color> {
        public:
            static double MaxValue(const Color& value1, const Color& value2) {
                double maxValue = std::numeric_limits<double>::lowest();

                for (Size_T i = 0; i < c_ValueCount; i++) {
                    if (value1.data[i] > maxValue) {
                        maxValue = value1.data[i];
                    }
                    if (value2.data[i] > maxValue) {
                        maxValue = value2.data[i];
                    }
                }

                return maxValue;
            }

            static double MinValue(const Color& value1, const Color& value2) {
                double minValue = std::numeric_limits<double>::max();
                for (int i = 0; i < c_ValueCount; ++i) {
                    if (value1.data[i] < minValue) {
                        minValue = value1.data[i];
                    }
                    if (value2.data[i] < minValue) {
                        minValue = value2.data[i];
                    }
                }

                return minValue;
            }

            static Color Add(const Color& value, const Color& addValue) {
                return value + addValue;
            }

            static Color Interpolate(const Color& start, const Color& end, double t) {
                return start + (end - start) * (float)t;
            }

            static Color Multiply(const Color& value, const Color& multiplyValue) {
                return value * multiplyValue;
            }

            static Color MultiplyScalar(const Color& value, double scalar) {
                return value * (float)scalar;
            }

            static Color Random(const Color& min, const Color& max) {
                Color v;
                for (int i = 0; i < c_ValueCount; ++i) {
                    v.data[i] = (float)pRandomManager->GetStaticRandom().NextDouble(min.data[i], max.data[i]);
                }
                return v;
            }

        private:
            static const int c_ValueCount = 4;
        };

        template <> class RavenValueInterpolator<Vector4> {
        public:
            static double MaxValue(const Vector4& value1, const Vector4& value2) {
                double maxValue = std::numeric_limits<double>::lowest();

                for (Size_T i = 0; i < c_ValueCount; i++) {
                    if (value1.data[i] > maxValue) {
                        maxValue = value1.data[i];
                    }
                    if (value2.data[i] > maxValue) {
                        maxValue = value2.data[i];
                    }
                }

                return maxValue;
            }

            static double MinValue(const Vector4& value1, const Vector4& value2) {
                double minValue = std::numeric_limits<double>::max();
                for (int i = 0; i < c_ValueCount; ++i) {
                    if (value1.data[i] < minValue) {
                        minValue = value1.data[i];
                    }
                    if (value2.data[i] < minValue) {
                        minValue = value2.data[i];
                    }
                }

                return minValue;
            }

            static Vector4 Add(const Vector4& value, const Vector4& addValue) {
                return value + addValue;
            }

            static Vector4 Interpolate(const Vector4& start, const Vector4& end, double t) {
                return start + (end - start) * (float)t;
            }

            static Vector4 Multiply(const Vector4& value, const Vector4& multiplyValue) {
                return value * multiplyValue;
            }

            static Vector4 MultiplyScalar(const Vector4& value, double scalar) {
                return value * (float)scalar;
            }

            static Vector4 Random(const Vector4& min, const Vector4& max) {
                Vector4 v;
                for (int i = 0; i < c_ValueCount; ++i) {
                    v.data[i] = (float)pRandomManager->GetStaticRandom().NextDouble(min.data[i], max.data[i]);
                }
                return v;
            }

        private:
            static const int c_ValueCount = 4;
        };

        template <> class RavenValueInterpolator<Quaternion> {
        public:
            static double MaxValue(const Quaternion& value1, const Quaternion& value2) {
                double maxValue = std::numeric_limits<double>::lowest();

                for (Size_T i = 0; i < c_ValueCount; i++) {
                    if (value1.vector.data[i] > maxValue) {
                        maxValue = value1.vector.data[i];
                    }
                    if (value2.vector.data[i] > maxValue) {
                        maxValue = value2.vector.data[i];
                    }
                }

                return maxValue;
            }

            static double MinValue(const Quaternion& value1, const Quaternion& value2) {
                double minValue = std::numeric_limits<double>::max();
                for (int i = 0; i < c_ValueCount; ++i) {
                    if (value1.vector.data[i] < minValue) {
                        minValue = value1.vector.data[i];
                    }
                    if (value2.vector.data[i] < minValue) {
                        minValue = value2.vector.data[i];
                    }
                }

                return minValue;
            }

            static Quaternion Add(const Quaternion& value, const Quaternion& addValue) {
                return value * addValue;
            }

            static Quaternion Interpolate(const Quaternion& start, const Quaternion& end, double t) {
                return Quaternion::Slerp(start, end, (float)t);
            }

            static Quaternion Multiply(const Quaternion& value, const Quaternion& multiplyValue) {
                Quaternion q;
                q.FromEuler(RavenValueInterpolator<Vector3>::Multiply(value.ToEuler(), multiplyValue.ToEuler()));
                return q;
            }

            static Quaternion MultiplyScalar(const Quaternion& value, double scalar) {
                Quaternion q;
                q.FromEuler(value.ToEuler() * (float)scalar);
                return q;
            }

            static Quaternion Random(const Quaternion& min, const Quaternion& max) {
                Quaternion q;
                q.FromEuler(RavenValueInterpolator<Vector3>::Random(Vector3(min.vector), Vector3(max.vector)));
                return q;
            }

        private:
            static const int c_ValueCount = 4;
        };

        template <> class RavenValueInterpolator<Rectangle> {
        public:
            static double MaxValue(const Rectangle& value1, const Rectangle& value2) {
                return RavenValueInterpolator<Vector4>::MaxValue((Vector4)value1, (Vector4)value2);
            }

            static double MinValue(const Rectangle& value1, const Rectangle& value2) {
                return RavenValueInterpolator<Vector4>::MinValue((Vector4)value1, (Vector4)value2);
            }

            static Rectangle Add(const Rectangle& value, const Rectangle& addValue) {
                return (Rectangle)RavenValueInterpolator<Vector4>::Add((Vector4)value, (Vector4)addValue);
            }

            static Rectangle Interpolate(const Rectangle& start, const Rectangle& end, double t) {
                return (Rectangle)RavenValueInterpolator<Vector4>::Interpolate((Vector4)start, (Vector4)end, t);
            }

            static Rectangle Multiply(const Rectangle& value, const Rectangle& multiplyValue) {
                return (Rectangle)RavenValueInterpolator<Vector4>::Multiply((Vector4)value, (Vector4)multiplyValue);
            }

            static Rectangle MultiplyScalar(const Rectangle& value, double scalar) {
                return (Rectangle)RavenValueInterpolator<Vector4>::MultiplyScalar((Vector4)value, scalar);
            }

            static Rectangle Random(const Rectangle& min, const Rectangle& max) {
                return (Rectangle)RavenValueInterpolator<Vector4>::Random((Vector4)min, (Vector4)max);
            }
        };

        template <> class RavenValueInterpolator<Ref<Material>> {
        public:
            static double MaxValue(const Ref<Material>& value1, const Ref<Material>& value2) {
                return 1;
            }

            static double MinValue(const Ref<Material>& value1, const Ref<Material>& value2) {
                return 0;
            }

            static Ref<Material> Add(const Ref<Material>& value, const Ref<Material>& addValue) {
                return value;
            }

            static Ref<Material> Interpolate(const Ref<Material>& start, const Ref<Material>& end, double t) {
                return start;
            }

            static Ref<Material> Multiply(const Ref<Material>& value, const Ref<Material>& multiplyValue) {
                return value;
            }

            static Ref<Material> MultiplyScalar(const Ref<Material>& value, double scalar) {
                return value;
            }

            static Ref<Material> Random(const Ref<Material>& min, const Ref<Material>& max) {
                return pRandomManager->GetStaticRandom().NextDouble() < 0.5 ? min : max;
            }
        };

        template <> class RavenValueInterpolator<Ref<ColorGradient>> {
        public:
            static double MaxValue(const Ref<ColorGradient>& value1, const Ref<ColorGradient>& value2) {
                DebugAssert(value1->GetColorKeys().Count() == value2->GetColorKeys().Count(), "Color keys length mismatch!");
                double maxValue = std::numeric_limits<double>::lowest();
                auto& keys1 = value1->GetColorKeys();
                auto& keys2 = value2->GetColorKeys();
                for (Size_T i = 0; i < keys1.Count(); ++i) {
                    auto value = RavenValueInterpolator<Color>::MaxValue(keys1[i].value, keys2[i].value);
                    if (value > maxValue) {
                        maxValue = value;
                    }
                }

                return maxValue;
            }

            static double MinValue(const Ref<ColorGradient>& value1, const Ref<ColorGradient>& value2) {
                DebugAssert(value1->GetColorKeys().Count() == value2->GetColorKeys().Count(), "Color keys length mismatch!");
                double minValue = std::numeric_limits<double>::max();
                auto& keys1 = value1->GetColorKeys();
                auto& keys2 = value2->GetColorKeys();
                for (Size_T i = 0; i < keys1.Count(); ++i) {
                    auto value = RavenValueInterpolator<Color>::MinValue(keys1[i].value, keys2[i].value);
                    if (value < minValue) {
                        minValue = value;
                    }
                }

                return minValue;
            }

            static Ref<ColorGradient> Add(const Ref<ColorGradient>& value, const Ref<ColorGradient>& addValue) {
                DebugAssert(value->GetAlphaKeys().Count() == addValue->GetAlphaKeys().Count(), "Alpha keys length mismatch!");
                DebugAssert(value->GetColorKeys().Count() == addValue->GetColorKeys().Count(), "Color keys length mismatch!");
                DebugAssert(value->GetMode() == addValue->GetMode(), "Gradient mode mismatch!");

                auto& alphaKeys1 = value->GetAlphaKeys();
                auto& alphaKeys2 = addValue->GetAlphaKeys();
                auto& colorKeys1 = value->GetColorKeys();
                auto& colorKeys2 = addValue->GetColorKeys();

                Ref<ColorGradient> g = Object::New<ColorGradient>();
                auto alphaKeys = List<ColorGradient::AlphaKey>(alphaKeys1.Count());
                auto colorKeys = List<ColorGradient::ColorKey>(colorKeys1.Count());

                for (Size_T i = 0; i < alphaKeys1.Count(); ++i) {
                    auto& key1 = alphaKeys1[i];
                    auto& key2 = alphaKeys2[i];
                    auto keyValue = RavenValueInterpolator<float>::Add(key1.value, key2.value);
                    alphaKeys.Add(ColorGradient::AlphaKey(key1.time, keyValue));
                }
                for (Size_T i = 0; i < colorKeys1.Count(); ++i) {
                    auto& key1 = colorKeys1[i];
                    auto& key2 = colorKeys2[i];
                    auto keyValue = RavenValueInterpolator<Color>::Add(key1.value, key2.value);
                    colorKeys.Add(ColorGradient::ColorKey(key1.time, keyValue));
                }
                g->SetKeys(alphaKeys, colorKeys);
                g->SetMode(value->GetMode());

                return g;
            }

            static Ref<ColorGradient> Interpolate(const Ref<ColorGradient>& start, const Ref<ColorGradient>& end, double t) {
                DebugAssert(start->GetAlphaKeys().Count() == end->GetAlphaKeys().Count(), "Alpha keys length mismatch!");
                DebugAssert(start->GetColorKeys().Count() == end->GetColorKeys().Count(), "Color keys length mismatch!");
                DebugAssert(start->GetMode() == end->GetMode(), "Gradient mode mismatch!");

                auto& alphaKeys1 = start->GetAlphaKeys();
                auto& alphaKeys2 = end->GetAlphaKeys();
                auto& colorKeys1 = start->GetColorKeys();
                auto& colorKeys2 = end->GetColorKeys();

                Ref<ColorGradient> g = Object::New<ColorGradient>();
                auto alphaKeys = List<ColorGradient::AlphaKey>(alphaKeys1.Count());
                auto colorKeys = List<ColorGradient::ColorKey>(colorKeys1.Count());

                for (Size_T i = 0; i < alphaKeys1.Count(); ++i) {
                    auto& key1 = alphaKeys1[i];
                    auto& key2 = alphaKeys2[i];
                    auto keyValue = RavenValueInterpolator<float>::Interpolate(key1.value, key2.value, t);
                    alphaKeys.Add(ColorGradient::AlphaKey(key1.time, keyValue));
                }
                for (Size_T i = 0; i < colorKeys1.Count(); ++i) {
                    auto& key1 = colorKeys1[i];
                    auto& key2 = colorKeys2[i];
                    auto keyValue = RavenValueInterpolator<Color>::Interpolate(key1.value, key2.value, t);
                    colorKeys.Add(ColorGradient::ColorKey(key1.time, keyValue));
                }
                g->SetKeys(alphaKeys, colorKeys);
                g->SetMode(start->GetMode());

                return g;
            }

            static Ref<ColorGradient> Multiply(const Ref<ColorGradient>& value, const Ref<ColorGradient>& multiplyValue) {
                DebugAssert(value->GetAlphaKeys().Count() == multiplyValue->GetAlphaKeys().Count(), "Alpha keys length mismatch!");
                DebugAssert(value->GetColorKeys().Count() == multiplyValue->GetColorKeys().Count(), "Color keys length mismatch!");
                DebugAssert(value->GetMode() == multiplyValue->GetMode(), "Gradient mode mismatch!");

                auto& alphaKeys1 = value->GetAlphaKeys();
                auto& alphaKeys2 = multiplyValue->GetAlphaKeys();
                auto& colorKeys1 = value->GetColorKeys();
                auto& colorKeys2 = multiplyValue->GetColorKeys();

                Ref<ColorGradient> g = Object::New<ColorGradient>();
                auto alphaKeys = List<ColorGradient::AlphaKey>(alphaKeys1.Count());
                auto colorKeys = List<ColorGradient::ColorKey>(colorKeys1.Count());

                for (Size_T i = 0; i < alphaKeys1.Count(); ++i) {
                    auto& key1 = alphaKeys1[i];
                    auto& key2 = alphaKeys2[i];
                    auto keyValue = RavenValueInterpolator<float>::Multiply(key1.value, key2.value);
                    alphaKeys.Add(ColorGradient::AlphaKey(key1.time, keyValue));
                }
                for (Size_T i = 0; i < colorKeys1.Count(); ++i) {
                    auto& key1 = colorKeys1[i];
                    auto& key2 = colorKeys2[i];
                    auto keyValue = RavenValueInterpolator<Color>::Multiply(key1.value, key2.value);
                    colorKeys.Add(ColorGradient::ColorKey(key1.time, keyValue));
                }
                g->SetKeys(alphaKeys, colorKeys);
                g->SetMode(value->GetMode());

                return g;
            }

            static Ref<ColorGradient> MultiplyScalar(const Ref<ColorGradient>& value, double scalar) {
                auto& alphaKeys1 = value->GetAlphaKeys();
                auto& colorKeys1 = value->GetColorKeys();

                Ref<ColorGradient> g = Object::New<ColorGradient>();
                auto alphaKeys = List<ColorGradient::AlphaKey>(alphaKeys1.Count());
                auto colorKeys = List<ColorGradient::ColorKey>(colorKeys1.Count());

                for (Size_T i = 0; i < alphaKeys1.Count(); ++i) {
                    auto& key1 = alphaKeys1[i];
                    auto keyValue = RavenValueInterpolator<float>::MultiplyScalar(key1.value, scalar);
                    alphaKeys.Add(ColorGradient::AlphaKey(key1.time, keyValue));
                }
                for (Size_T i = 0; i < colorKeys1.Count(); ++i) {
                    auto& key1 = colorKeys1[i];
                    auto keyValue = RavenValueInterpolator<Color>::MultiplyScalar(key1.value, scalar);
                    colorKeys.Add(ColorGradient::ColorKey(key1.time, keyValue));
                }
                g->SetKeys(alphaKeys, colorKeys);
                g->SetMode(value->GetMode());

                return g;
            }

            static Ref<ColorGradient> Random(const Ref<ColorGradient>& min, const Ref<ColorGradient>& max) {
                DebugAssert(min->GetAlphaKeys().Count() == max->GetAlphaKeys().Count(), "Alpha keys length mismatch!");
                DebugAssert(min->GetColorKeys().Count() == max->GetColorKeys().Count(), "Color keys length mismatch!");
                DebugAssert(min->GetMode() == max->GetMode(), "Gradient mode mismatch!");

                auto& alphaKeys1 = min->GetAlphaKeys();
                auto& alphaKeys2 = max->GetAlphaKeys();
                auto& colorKeys1 = min->GetColorKeys();
                auto& colorKeys2 = max->GetColorKeys();

                Ref<ColorGradient> g = Object::New<ColorGradient>();
                auto alphaKeys = List<ColorGradient::AlphaKey>(alphaKeys1.Count());
                auto colorKeys = List<ColorGradient::ColorKey>(colorKeys1.Count());

                for (Size_T i = 0; i < alphaKeys1.Count(); ++i) {
                    auto& key1 = alphaKeys1[i];
                    auto& key2 = alphaKeys2[i];
                    auto keyValue = RavenValueInterpolator<float>::Random(key1.value, key2.value);
                    alphaKeys.Add(ColorGradient::AlphaKey(key1.time, keyValue));
                }
                for (Size_T i = 0; i < colorKeys1.Count(); ++i) {
                    auto& key1 = colorKeys1[i];
                    auto& key2 = colorKeys2[i];
                    auto keyValue = RavenValueInterpolator<Color>::Random(key1.value, key2.value);
                    colorKeys.Add(ColorGradient::ColorKey(key1.time, keyValue));
                }
                g->SetKeys(alphaKeys, colorKeys);
                g->SetMode(min->GetMode());

                return g;
            }
        };

        template <> class RavenValueInterpolator<Ref<Sprite>> {
        public:
            static double MaxValue(const Ref<Sprite>& value1, const Ref<Sprite>& value2) {
                return 1;
            }

            static double MinValue(const Ref<Sprite>& value1, const Ref<Sprite>& value2) {
                return 0;
            }

            static Ref<Sprite> Add(const Ref<Sprite>& value, const Ref<Sprite>& addValue) {
                return value;
            }

            static Ref<Sprite> Interpolate(const Ref<Sprite>& start, const Ref<Sprite>& end, double t) {
                return start;
            }

            static Ref<Sprite> Multiply(const Ref<Sprite>& value, const Ref<Sprite>& multiplyValue) {
                return value;
            }

            static Ref<Sprite> MultiplyScalar(const Ref<Sprite>& value, double scalar) {
                return value;
            }

            static Ref<Sprite> Random(const Ref<Sprite>& min, const Ref<Sprite>& max) {
                return pRandomManager->GetStaticRandom().NextDouble() < 0.5 ? min : max;
            }
        };
    }
} // namespace Starlite
#endif
