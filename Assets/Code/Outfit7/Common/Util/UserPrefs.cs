//
//  Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Outfit7.Threading;
using SimpleJSON;
using UnityEngine;

namespace Outfit7.Util {

    /// <summary>
    /// User preferences based on <see cref="PlayerPrefs"/>.
    /// </summary>
    public static class UserPrefs {

        private const string Tag = "UserPrefs";
        private const string EmptyCollectionValue = "Marko,Sta,mcarJeLace,nInJ,ePoje,de,lVes,Preda,l";
        private static bool SavePending;

        public static MainExecutor MainExecutor { get; set; }

        public static void Clear() {
            O7Log.WarnT(Tag, "Clearing all user preferences");
            PlayerPrefs.DeleteAll();
        }

        public static void Remove(string key) {
            PlayerPrefs.DeleteKey(key);
        }

        public static void RemoveHash(string key) {
            Remove(CreateHashKey(key));
        }

        public static bool HasKey(string key) {
            return PlayerPrefs.HasKey(key);
        }

        public static void Save() {
            O7Log.VerboseT(Tag, "Saving all user preferences");
            if (MainExecutor != null && SavePending) {
                // Remove pending save
                MainExecutor.RemoveAllSchedules(Save);
            }
            PlayerPrefs.Save();
            SavePending = false;
        }

        public static void SaveDelayed() {
            SaveDelayed(1.2f);
        }

        public static void SaveDelayed(float delaySecs) {
            O7Log.VerboseT(Tag, "Saving delayed for {0} second(s), savePending={1}", delaySecs, SavePending);
            if (MainExecutor != null) {
                if (SavePending) {
                    // Already scheduled
                    return;
                }

                MainExecutor.PostDelayed(Save, delaySecs);
                SavePending = true;

            } else {
                O7Log.WarnT(Tag, "Delayed save cannot be accomplished because MainExecutor is null");
                Save();
            }
        }

        private static string CreateHashKey(string key) {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(key));
        }

        private static string CreateHashValue(string value, string hashSuffix) {
            return StringUtils.HasText(value) ? CryptoUtils.Sha1(value + hashSuffix) : null;
        }

#region Get Data

        public static bool GetBool(string key, bool defaultValue) {
            if (HasKey(key)) {
                return PlayerPrefs.GetInt(key) >= 1;
            }
            return defaultValue;
        }

        public static int GetInt(string key, int defaultValue) {
            if (HasKey(key)) {
                return PlayerPrefs.GetInt(key);
            }
            return defaultValue;
        }

        public static long GetLong(string key, long defaultValue) {
            if (HasKey(key)) {
                string value = PlayerPrefs.GetString(key);
                try {
                    return long.Parse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);

                } catch (FormatException) {
                    O7Log.WarnT(Tag, "Invalid Int64 format for value '{0}' for key '{1}'", value, key);
                    return defaultValue;

                } catch (OverflowException) {
                    O7Log.WarnT(Tag, "Overflow of Int64 for value '{0}' for key '{1}'", value, key);
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        public static float GetFloat(string key, float defaultValue) {
            if (HasKey(key)) {
                return PlayerPrefs.GetFloat(key);
            }
            return defaultValue;
        }

        public static double GetDouble(string key, double defaultValue) {
            if (HasKey(key)) {
                string value = PlayerPrefs.GetString(key);
                try {
                    return double.Parse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo);

                } catch (FormatException) {
                    O7Log.WarnT(Tag, "Invalid Double format for value '{0}' for key '{1}'", value, key);
                    return defaultValue;

                } catch (OverflowException) {
                    O7Log.WarnT(Tag, "Overflow of Double for value '{0}' for key '{1}'", value, key);
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        public static string GetString(string key, string defaultValue) {
            if (HasKey(key)) {
                return PlayerPrefs.GetString(key);
            }
            return defaultValue;
        }

        public static DateTime GetDateTime(string key, DateTime defaultValue) {
            return DateTime.FromBinary(GetLong(key, defaultValue.ToBinary()));
        }

        public static JSONNode GetJson(string key, JSONNode defaultValue) {
            if (HasKey(key)) {
                string value = PlayerPrefs.GetString(key);

                // Do not crash if value is corrupted - user can manually change values
                try {
                    return JSON.Parse(value);

                } catch {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        public static List<string> GetCollectionAsList(string key, List<string> defaultCollection) {
            if (HasKey(key)) {
                string value = PlayerPrefs.GetString(key);
                if (value == EmptyCollectionValue) {
                    return new List<string>(0);
                }
                List<string> list = StringUtils.CommaDelimitedListToStringList(value);
                return list;
            }
            return defaultCollection;
        }

        public static LinkedList<string> GetCollectionAsLinkedList(string key, LinkedList<string> defaultCollection) {
            if (HasKey(key)) {
                string value = PlayerPrefs.GetString(key);
                if (value == EmptyCollectionValue) {
                    return new LinkedList<string>();
                }
                List<string> list = StringUtils.CommaDelimitedListToStringList(value);
                return new LinkedList<string>(list);
            }
            return defaultCollection;
        }

        public static HashSet<string> GetCollectionAsHashSet(string key, HashSet<string> defaultCollection) {
            if (HasKey(key)) {
                string value = PlayerPrefs.GetString(key);
                if (value == EmptyCollectionValue) {
                    return new HashSet<string>();
                }
                List<string> list = StringUtils.CommaDelimitedListToStringList(value);
                return new HashSet<string>(list);
            }
            return defaultCollection;
        }

        public static Stack<string> GetCollectionAsStack(string key, Stack<string> defaultCollection) {
            if (HasKey(key)) {
                string value = PlayerPrefs.GetString(key);
                if (value == EmptyCollectionValue) {
                    return new Stack<string>(0);
                }
                List<string> list = StringUtils.CommaDelimitedListToStringList(value);
                return new Stack<string>(list);
            }
            return defaultCollection;
        }

        public static Queue<string> GetCollectionAsQueue(string key, Queue<string> defaultCollection) {
            if (HasKey(key)) {
                string value = PlayerPrefs.GetString(key);
                if (value == EmptyCollectionValue) {
                    return new Queue<string>(0);
                }
                List<string> list = StringUtils.CommaDelimitedListToStringList(value);
                return new Queue<string>(list);
            }
            return defaultCollection;
        }

        public static bool CheckHash(string originalKey, string value, string hashSuffix) {
            if (!StringUtils.HasText(value)) {
                // If value is empty or null, hash is null and irrelevant
                return true;
            }

            // Check hash
            string hash = GetString(CreateHashKey(originalKey), null);
            string valueH = CreateHashValue(value, hashSuffix);
            return (valueH == hash);
        }

#endregion

#region Set Data

        public static void SetBool(string key, bool value) {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        public static bool? SetBoolAndReturnPrevious(string key, bool value) {
            bool? prevValue = null;
            if (HasKey(key)) {
                prevValue = PlayerPrefs.GetInt(key) >= 1;
            }
            SetBool(key, value);
            return prevValue;
        }

        public static void SetInt(string key, int value) {
            PlayerPrefs.SetInt(key, value);
        }

        public static int? SetIntAndReturnPrevious(string key, int value) {
            int? prevValue = null;
            if (HasKey(key)) {
                prevValue = PlayerPrefs.GetInt(key);
            }
            SetInt(key, value);
            return prevValue;
        }

        public static void SetLong(string key, long value) {
            PlayerPrefs.SetString(key, StringUtils.ToUniString(value));
        }

        public static long? SetLongAndReturnPrevious(string key, long value) {
            long? prevValue = null;
            if (HasKey(key)) {
                string prevValueS = PlayerPrefs.GetString(key);
                try {
                    prevValue = long.Parse(prevValueS, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);

                } catch (FormatException) {
                    O7Log.WarnT(Tag, "Invalid Int64 format for value '{0}' for key '{1}'", prevValueS, key);

                } catch (OverflowException) {
                    O7Log.WarnT(Tag, "Overflow of Int64 for value '{0}' for key '{1}'", prevValueS, key);
                }
            }
            SetLong(key, value);
            return prevValue;
        }

        public static void SetFloat(string key, float value) {
            PlayerPrefs.SetFloat(key, value);
        }

        public static float? SetFloatAndReturnPrevious(string key, float value) {
            float? prevValue = null;
            if (HasKey(key)) {
                prevValue = PlayerPrefs.GetFloat(key);
            }
            SetFloat(key, value);
            return prevValue;
        }

        public static void SetDouble(string key, double value) {
            PlayerPrefs.SetString(key, StringUtils.ToUniString(value));
        }

        public static double? SetDoubleAndReturnPrevious(string key, double value) {
            double? prevValue = null;
            if (HasKey(key)) {
                string prevValueS = PlayerPrefs.GetString(key);
                try {
                    prevValue = double.Parse(prevValueS, NumberStyles.Float, NumberFormatInfo.InvariantInfo);

                } catch (FormatException) {
                    O7Log.WarnT(Tag, "Invalid Double format for value '{0}' for key '{1}'", prevValueS, key);

                } catch (OverflowException) {
                    O7Log.WarnT(Tag, "Overflow of Double for value '{0}' for key '{1}'", prevValueS, key);
                }
            }
            SetDouble(key, value);
            return prevValue;
        }

        public static void SetString(string key, string value) {
            PlayerPrefs.SetString(key, value);
        }

        public static string SetStringAndReturnPrevious(string key, string value) {
            string prevValue = null;
            if (HasKey(key)) {
                prevValue = PlayerPrefs.GetString(key);
            }
            SetString(key, value);
            return prevValue;
        }

        public static void SetDateTime(string key, DateTime value) {
            // Convert to UTC to remain consistent through time zones
            // But don't touch DateTimeKind.Unspecified, which is used by MinValue & MaxValue!
            if (value.Kind == DateTimeKind.Local) {
                value = value.ToUniversalTime();
            }
            SetLong(key, value.ToBinary());
        }

        public static void SetJson(string key, JSONNode value) {
            SetString(key, (value != null) ? value.ToString() : null);
        }

        public static void SetCollection(string key, ICollection<string> collection) {
            string value;
            if (collection == null || collection.Count == 0) {
                value = EmptyCollectionValue;
            } else {
                value = StringUtils.CollectionToCommaDelimitedString(collection);
            }
            SetString(key, value);
        }

        public static void SetHash(string originalKey, string value, string hashSuffix) {
            string hash = CreateHashValue(value, hashSuffix);
            SetString(CreateHashKey(originalKey), hash);
        }

#endregion
    }
}
