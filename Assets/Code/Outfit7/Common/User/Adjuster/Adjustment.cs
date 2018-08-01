//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using Outfit7.Json;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.User.Adjuster {

    /// <summary>
    /// Adjuster adjustment.
    /// </summary>
    public class Adjustment {

        private const string JsonId = "id";
        private const string JsonIntegerValue = "integerValue";
        private const string JsonLongValue = "longValue";
        private const string JsonFloatValue = "floatValue";
        private const string JsonStringValue = "stringValue";
        private const string JsonIntegerListValue = "integerListValue";
        private const string JsonLongListValue = "longListValue";
        private const string JsonFloatListValue = "floatListValue";
        private const string JsonStringListValue = "stringListValue";

        public string Id { get; private set; }

        public int IntegerValue { get; private set; }

        public long LongValue { get; private set; }

        public float FloatValue { get; private set; }

        public string StringValue { get; private set; }

        public List<int> IntegerListValue { get; private set; }

        public List<long> LongListValue { get; private set; }

        public List<float> FloatListValue { get; private set; }

        public List<string> StringListValue { get; private set; }

        public Adjustment(JSONNode adjustmentJ) {
            Id = adjustmentJ[JsonId];
            Assert.HasText(Id, "id");
            IntegerValue = adjustmentJ[JsonIntegerValue].AsInt;
            Assert.IsTrue(IntegerValue >= 0, "integerValue must be >= 0");
            LongValue = adjustmentJ[JsonLongValue].AsLong;
            Assert.IsTrue(LongValue >= 0, "longValue must be >= 0");
            FloatValue = adjustmentJ[JsonFloatValue].AsFloat;
            Assert.IsTrue(FloatValue >= 0, "floatValue must be >= 0");
            StringValue = adjustmentJ[JsonStringValue];
            Assert.IsTrue(StringValue == null || StringUtils.HasText(StringValue), "stringValue must be == null or not empty");

            JSONArray ilvA = SimpleJsonUtils.EnsureJsonArray(adjustmentJ[JsonIntegerListValue]);
            if (ilvA != null) {
                IntegerListValue = new List<int>(ilvA.Count);
                foreach (JSONNode n in ilvA) {
                    IntegerListValue.Add(n.AsInt);
                }
            }

            JSONArray llvA = SimpleJsonUtils.EnsureJsonArray(adjustmentJ[JsonLongListValue]);
            if (llvA != null) {
                LongListValue = new List<long>(llvA.Count);
                foreach (JSONNode n in llvA) {
                    LongListValue.Add(n.AsLong);
                }
            }

            JSONArray flvA = SimpleJsonUtils.EnsureJsonArray(adjustmentJ[JsonFloatListValue]);
            if (flvA != null) {
                FloatListValue = new List<float>(flvA.Count);
                foreach (JSONNode n in flvA) {
                    FloatListValue.Add(n.AsFloat);
                }
            }

            StringListValue = SimpleJsonUtils.CreateList(adjustmentJ[JsonStringListValue]);
        }

        public JSONClass ToJson() {
            JSONClass adjustmentJ = new JSONClass();

            adjustmentJ[JsonId] = Id;

            if (IntegerValue > 0) {
                adjustmentJ[JsonIntegerValue].AsInt = IntegerValue;
            }
            if (LongValue > 0) {
                adjustmentJ[JsonLongValue].AsLong = LongValue;
            }
            if (FloatValue > 0) {
                adjustmentJ[JsonFloatValue].AsFloat = FloatValue;
            }
            if (StringValue != null) {
                adjustmentJ[JsonStringValue] = StringValue;
            }

            if (IntegerListValue != null) {
                JSONArray array = new JSONArray();
                foreach (int item in IntegerListValue) {
                    array.Add(new JSONData(item));
                }
                adjustmentJ[JsonIntegerListValue] = array;
            }

            if (LongListValue != null) {
                JSONArray array = new JSONArray();
                foreach (long item in LongListValue) {
                    array.Add(new JSONData(item));
                }
                adjustmentJ[JsonLongListValue] = array;
            }

            if (FloatListValue != null) {
                JSONArray array = new JSONArray();
                foreach (float item in FloatListValue) {
                    array.Add(new JSONData(item));
                }
                adjustmentJ[JsonFloatListValue] = array;
            }

            adjustmentJ[JsonStringListValue] = SimpleJsonUtils.CreateJsonArray(StringListValue);

            return adjustmentJ;
        }

        public override string ToString() {
            return string.Format("[Adjustment: Id={0}, IntegerValue={1}, LongValue={2}, FloatValue={3}, StringValue={4}, IntegerListValue={5}, LongListValue={6}, FloatListValue={7}, StringListValue={8}]",
                                 Id, IntegerValue, LongValue, FloatValue, StringValue, StringUtils.CollectionToCommaDelimitedString(IntegerListValue),
                                 StringUtils.CollectionToCommaDelimitedString(LongListValue), StringUtils.CollectionToCommaDelimitedString(FloatListValue),
                                 StringUtils.CollectionToCommaDelimitedString(StringListValue));
        }
    }
}
