//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Idevgame.Util {

    /// <summary>
    /// String utils.
    /// Partly copied from Spring.Core.Util.StringUtils.
    /// </summary>
    public static class StringUtils {

        /// <summary>
        /// An empty array of <see cref="System.String"/> instances.
        /// </summary>
        public static readonly string[] EmptyStrings = { };

        /// <summary>
        /// Tokenize the given <see cref="System.String"/> into a
        /// list of strings.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If <paramref name="s"/> is <see langword="null"/>, returns an empty
        /// list of strings.
        /// </p>
        /// <p>
        /// If <paramref name="delimiters"/> is <see langword="null"/> or the empty
        /// <see cref="System.String"/>, returns a list of strings with one
        /// element: <paramref name="s"/> itself.
        /// </p>
        /// </remarks>
        /// <param name="s">The <see cref="System.String"/> to tokenize.</param>
        /// <param name="delimiters">
        /// The delimiter characters, assembled as a <see cref="System.String"/>.
        /// </param>
        /// <param name="trimTokens">
        /// Trim the tokens via <see cref="System.String.Trim()"/>.
        /// </param>
        /// <param name="ignoreEmptyTokens">
        /// Omit empty tokens from the result list.</param>
        /// <returns>A list of the tokens.</returns>
        public static List<string> Split(string s, string delimiters, bool trimTokens, bool ignoreEmptyTokens) {
            return Split(s, delimiters, trimTokens, ignoreEmptyTokens, null);
        }

        /// <summary>
        /// Tokenize the given <see cref="System.String"/> into a
        /// list of strings.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If <paramref name="s"/> is <see langword="null"/>, returns an empty
        /// list of strings.
        /// </p>
        /// <p>
        /// If <paramref name="delimiters"/> is <see langword="null"/> or the empty
        /// <see cref="System.String"/>, returns a list of strings with one
        /// element: <paramref name="s"/> itself.
        /// </p>
        /// </remarks>
        /// <param name="s">The <see cref="System.String"/> to tokenize.</param>
        /// <param name="delimiters">
        /// The delimiter characters, assembled as a <see cref="System.String"/>.
        /// </param>
        /// <param name="trimTokens">
        /// Trim the tokens via <see cref="System.String.Trim()"/>.
        /// </param>
        /// <param name="ignoreEmptyTokens">
        /// Omit empty tokens from the result list.
        /// </param>
        /// <param name="quoteChars">
        /// Pairs of quote characters. <paramref name="delimiters"/> within a pair of quotes are ignored
        /// </param>
        /// <returns>A list of the tokens.</returns>
        public static List<string> Split(string s, string delimiters, bool trimTokens, bool ignoreEmptyTokens, string quoteChars) {
            if (s == null) {
                return new List<string>(0);
            }
            if (string.IsNullOrEmpty(delimiters)) {
                return new List<string>{ s };
            }
            if (quoteChars == null) {
                quoteChars = string.Empty;
            }
            Assert.IsTrue(quoteChars.Length % 2 == 0, "the number of quote characters must be even");

            char[] delimiterChars = delimiters.ToCharArray();

            // scan separator positions
            int[] delimiterPositions = new int[s.Length];
            int count = MakeDelimiterPositionList(s, delimiterChars, quoteChars, delimiterPositions);

            List<string> tokens = new List<string>(count + 1);
            int startIndex = 0;
            for (int ixSep = 0; ixSep < count; ixSep++) {
                string token = s.Substring(startIndex, delimiterPositions[ixSep] - startIndex);
                if (trimTokens) {
                    token = token.Trim();
                }
                if (!(ignoreEmptyTokens && token.Length == 0)) {
                    tokens.Add(token);
                }
                startIndex = delimiterPositions[ixSep] + 1;
            }
            // add remainder
            if (startIndex < s.Length) {
                string token = s.Substring(startIndex);
                if (trimTokens) {
                    token = token.Trim();
                }
                if (!(ignoreEmptyTokens && token.Length == 0)) {
                    tokens.Add(token);
                }
            } else if (startIndex == s.Length) {
                if (!(ignoreEmptyTokens)) {
                    tokens.Add(string.Empty);
                }
            }

            return tokens;
        }

        private static int MakeDelimiterPositionList(string s, char[] delimiters, string quoteChars, int[] delimiterPositions) {
            int count = 0;
            int quoteNestingDepth = 0;
            char expectedQuoteOpenChar = '\0';
            char expectedQuoteCloseChar = '\0';

            for (int ixCurChar = 0; ixCurChar < s.Length; ixCurChar++) {
                char curChar = s[ixCurChar];

                for (int ixCurDelim = 0; ixCurDelim < delimiters.Length; ixCurDelim++) {
                    if (delimiters[ixCurDelim] == curChar) {
                        if (quoteNestingDepth == 0) {
                            delimiterPositions[count] = ixCurChar;
                            count++;
                            break;
                        }
                    }

                    if (quoteNestingDepth == 0) {
                        // check, if we're facing an opening char
                        for (int ixCurQuoteChar = 0; ixCurQuoteChar < quoteChars.Length; ixCurQuoteChar += 2) {
                            if (quoteChars[ixCurQuoteChar] == curChar) {
                                quoteNestingDepth++;
                                expectedQuoteOpenChar = curChar;
                                expectedQuoteCloseChar = quoteChars[ixCurQuoteChar + 1];
                                break;
                            }
                        }
                    } else {
                        // check if we're facing an expected open or close char
                        if (curChar == expectedQuoteOpenChar) {
                            quoteNestingDepth++;
                        } else if (curChar == expectedQuoteCloseChar) {
                            quoteNestingDepth--;
                        }
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Convert a CSV list into a list of strings.
        /// </summary>
        /// <remarks>
        /// Values may also be quoted using doublequotes.
        /// </remarks>
        /// <param name="s">A CSV list.</param>
        /// <returns>
        /// A list of strings, or the empty list of strings
        /// if <paramref name="s"/> is <see langword="null"/>.
        /// </returns>
        public static List<string> CommaDelimitedListToStringList(string s) {
            return Split(s, ",", false, false, "\"\"");
        }

        /// <summary>
        /// Take a <see cref="System.String"/> which is a delimited list
        /// and convert it to a list of strings.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the supplied <paramref name="delimiter"/> is a
        /// <cref lang="null"/> or zero-length string, then a single element
        /// list of strings composed of the supplied
        /// <paramref name="input"/> <see cref="System.String"/> will be
        /// returned. If the supplied <paramref name="input"/>
        /// <see cref="System.String"/> is <cref lang="null"/>, then an empty,
        /// zero-length list of strings will be returned.
        /// </p>
        /// </remarks>
        /// <param name="input">
        /// The <see cref="System.String"/> to be parsed.
        /// </param>
        /// <param name="delimiter">
        /// The delimeter (this will not be returned). Note that only the first
        /// character of the supplied <paramref name="delimiter"/> is used.
        /// </param>
        /// <returns>
        /// A list of the tokens in the list.
        /// </returns>
        public static List<string> DelimitedListToStringList(string input, string delimiter) {
            if (input == null) {
                return new List<string>(0);
            }
            if (!HasLength(delimiter)) {
                return new List<string>(1) { input };
            }
            return Split(input, delimiter, false, false, null);
        }

        /// <summary>
        /// Convenience method to return an
        /// <see cref="System.Collections.ICollection"/> as a delimited
        /// (e.g. CSV) <see cref="System.String"/>.
        /// </summary>
        /// <param name="c">
        /// The <see cref="System.Collections.ICollection"/> to parse.
        /// </param>
        /// <param name="delimiter">
        /// The delimiter to use (probably a ',').
        /// </param>
        /// <returns>The delimited string representation.</returns>
        public static string CollectionToDelimitedString<T>(
            ICollection<T> c, string delimiter) {
            if (c == null) {
                return "null";
            }
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (T obj in c) {
                if (i++ > 0) {
                    sb.Append(delimiter);
                }
                sb.Append(obj);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Convenience method to return an
        /// <see cref="System.Collections.ICollection"/> as a CSV
        /// <see cref="System.String"/>.
        /// </summary>
        /// <param name="collection">
        /// The <see cref="System.Collections.ICollection"/> to display.
        /// </param>
        /// <returns>The delimited string representation.</returns>
        public static string CollectionToCommaDelimitedString<T>(
            ICollection<T> collection) {
            return CollectionToDelimitedString(collection, ",");
        }

        /// <summary>
        /// Convenience method to return an array as a CSV
        /// <see cref="System.String"/>.
        /// </summary>
        /// <param name="source">
        /// The array to parse. Elements may be of any type (
        /// <see cref="System.Object.ToString"/> will be called on each
        /// element).
        /// </param>
        public static string ArrayToCommaDelimitedString(object[] source) {
            return ArrayToDelimitedString(source, ",");
        }

        /// <summary>
        /// Convenience method to return a <see cref="System.String"/>
        /// array as a delimited (e.g. CSV) <see cref="System.String"/>.
        /// </summary>
        /// <param name="source">
        /// The array to parse. Elements may be of any type (
        /// <see cref="System.Object.ToString"/> will be called on each
        /// element).
        /// </param>
        /// <param name="delimiter">
        /// The delimiter to use (probably a ',').
        /// </param>
        public static string ArrayToDelimitedString(
            object[] source, string delimiter) {
            if (source == null) {
                return "null";
            } else {
                return StringUtils.CollectionToDelimitedString(source, delimiter);
            }
        }

        public static List<string> QuoteDelimitedListToStringList(string s, char quoteChar, char delimiter, bool trim) {
            if (string.IsNullOrEmpty(s))
                return new List<string>(0);

            int startI = 0;
            bool insideQuote = false;
            int startTokenI = 0;
            List<string> tokens = new List<string>();

            while (true) {
                int nI = s.IndexOf(delimiter, startI);
                if (nI == -1) {
                    // EOF. Get last token if any.
                    if (s.Length - startTokenI > 0) {
                        string lastToken = s.Substring(startTokenI);
                        if (trim) {
                            lastToken = lastToken.Trim();
                        }
                        tokens.Add(lastToken);
                    }
                    break;
                }

                int quoteI = s.IndexOf(quoteChar, startI);
                if (quoteI != -1 && quoteI < nI) {
                    // Found quote start or end before delimiter
                    insideQuote = !insideQuote;
                    startI = quoteI + 1;
                    continue;
                }

                startI = nI + 1;

                if (insideQuote) continue; // Skip delimiters inside quote

                string token = s.Substring(startTokenI, nI - startTokenI);
                if (trim) {
                    token = token.Trim();
                }
                tokens.Add(token);
                startTokenI = startI;
            }
            return tokens;
        }

        /// <summary>Checks if a string has length.</summary>
        /// <param name="target">
        /// The string to check, may be <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the string has length and is not
        /// <see langword="null"/>.
        /// </returns>
        /// <example>
        /// <code lang="C#">
        /// StringUtils.HasLength(null) = false
        /// StringUtils.HasLength("") = false
        /// StringUtils.HasLength(" ") = true
        /// StringUtils.HasLength("Hello") = true
        /// </code>
        /// </example>
        public static bool HasLength(string target) {
            return (target != null && target.Length > 0);
        }

        /// <summary>
        /// Checks if a <see cref="System.String"/> has text.
        /// </summary>
        /// <remarks>
        /// <p>
        /// More specifically, returns <see langword="true"/> if the string is
        /// not <see langword="null"/>, it's <see cref="String.Length"/> is >
        /// zero <c>(0)</c>, and it has at least one non-whitespace character.
        /// </p>
        /// </remarks>
        /// <param name="target">
        /// The string to check, may be <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="target"/> is not
        /// <see langword="null"/>,
        /// <see cref="String.Length"/> > zero <c>(0)</c>, and does not consist
        /// solely of whitespace.
        /// </returns>
        /// <example>
        /// <code language="C#">
        /// StringUtils.HasText(null) = false
        /// StringUtils.HasText("") = false
        /// StringUtils.HasText(" ") = false
        /// StringUtils.HasText("12345") = true
        /// StringUtils.HasText(" 12345 ") = true
        /// </code>
        /// </example>
        public static bool HasText(string target) {
            if (target == null) {
                return false;
            } else {
                return HasLength(target.Trim());
            }
        }

        /// <summary>
        /// Checks if a <see cref="System.String"/> is <see langword="null"/>
        /// or an empty string.
        /// </summary>
        /// <remarks>
        /// <p>
        /// More specifically, returns <see langword="false"/> if the string is
        /// <see langword="null"/>, it's <see cref="String.Length"/> is equal
        /// to zero <c>(0)</c>, or it is composed entirely of whitespace
        /// characters.
        /// </p>
        /// </remarks>
        /// <param name="target">
        /// The string to check, may (obviously) be <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="target"/> is
        /// <see langword="null"/>, has a length equal to zero <c>(0)</c>, or
        /// is composed entirely of whitespace characters.
        /// </returns>
        /// <example>
        /// <code language="C#">
        /// StringUtils.IsNullOrEmpty(null) = true
        /// StringUtils.IsNullOrEmpty("") = true
        /// StringUtils.IsNullOrEmpty(" ") = true
        /// StringUtils.IsNullOrEmpty("12345") = false
        /// StringUtils.IsNullOrEmpty(" 12345 ") = false
        /// </code>
        /// </example>
        public static bool IsNullOrEmpty(string target) {
            return !HasText(target);
        }

        /// <summary>
        /// Returns <paramref name="value"/>, if it contains non-whitespaces. <c>null</c> otherwise.
        /// </summary>
        public static string GetTextOrNull(string value) {
            if (!HasText(value)) {
                return null;
            }
            return value;
        }

        /// <summary>
        /// Strips first and last character off the string.
        /// </summary>
        /// <param name="text">The string to strip.</param>
        /// <returns>The stripped string.</returns>
        public static string StripFirstAndLastCharacter(string text) {
            if (text != null
                && text.Length > 2) {
                return text.Substring(1, text.Length - 2);
            } else {
                return String.Empty;
            }
        }

        /// <summary>
        /// Surrounds (prepends and appends) the string value of the supplied
        /// <paramref name="fix"/> to the supplied <paramref name="target"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The return value of this method call is always guaranteed to be non
        /// <see langword="null"/>. If every value passed as a parameter to this method is
        /// <see langword="null"/>, the <see cref="System.String.Empty"/> string will be returned.
        /// </p>
        /// </remarks>
        /// <param name="fix">
        /// The pre<b>fix</b> and suf<b>fix</b> that respectively will be prepended and
        /// appended to the target <paramref name="target"/>. If this value
        /// is not a <see cref="System.String"/> value, it's attendant
        /// <see cref="System.Object.ToString()"/> value will be used.
        /// </param>
        /// <param name="target">
        /// The target that is to be surrounded. If this value is not a
        /// <see cref="System.String"/> value, it's attendant
        /// <see cref="System.Object.ToString()"/> value will be used.
        /// </param>
        /// <returns>The surrounded string.</returns>
        public static string Surround(object fix, object target) {
            return StringUtils.Surround(fix, target, fix);
        }

        /// <summary>
        /// Surrounds (prepends and appends) the string values of the supplied
        /// <paramref name="prefix"/> and <paramref name="suffix"/> to the supplied
        /// <paramref name="target"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The return value of this method call is always guaranteed to be non
        /// <see langword="null"/>. If every value passed as a parameter to this method is
        /// <see langword="null"/>, the <see cref="System.String.Empty"/> string will be returned.
        /// </p>
        /// </remarks>
        /// <param name="prefix">
        /// The value that will be prepended to the <paramref name="target"/>. If this value
        /// is not a <see cref="System.String"/> value, it's attendant
        /// <see cref="System.Object.ToString()"/> value will be used.
        /// </param>
        /// <param name="target">
        /// The target that is to be surrounded. If this value is not a
        /// <see cref="System.String"/> value, it's attendant
        /// <see cref="System.Object.ToString()"/> value will be used.
        /// </param>
        /// <param name="suffix">
        /// The value that will be appended to the <paramref name="target"/>. If this value
        /// is not a <see cref="System.String"/> value, it's attendant
        /// <see cref="System.Object.ToString()"/> value will be used.
        /// </param>
        /// <returns>The surrounded string.</returns>
        public static string Surround(object prefix, object target, object suffix) {
            return string.Format(
                CultureInfo.InvariantCulture, "{0}{1}{2}", prefix, target, suffix);
        }

        /// <summary>
        /// Converts escaped characters (for example "\t") within a string
        /// to their real character.
        /// </summary>
        /// <param name="inputString">The string to convert.</param>
        /// <returns>The converted string.</returns>
        public static string ConvertEscapedCharacters(string inputString) {
            if (inputString == null)
                return null;
            StringBuilder sb = new StringBuilder(inputString.Length);
            for (int i = 0; i < inputString.Length; i++) {
                if (inputString[i].Equals('\\')) {
                    i++;
                    if (inputString[i].Equals('t')) {
                        sb.Append('\t');
                    } else if (inputString[i].Equals('r')) {
                        sb.Append('\r');
                    } else if (inputString[i].Equals('n')) {
                        sb.Append('\n');
                    } else if (inputString[i].Equals('\\')) {
                        sb.Append('\\');
                    } else {
                        sb.Append("\\" + inputString[i]);
                    }
                } else {
                    sb.Append(inputString[i]);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Unquotes the specified string out of the double quotation marks '"'. Specially designed for localization
        /// table conversions.
        /// </summary>
        /// <param name="value">The string to unquote (null permitted).</param>
        /// <returns>The unquoted string or empty if null or whitespace only.</returns>
        public static string UnQuote(string value) {
            if (!HasText(value)) return "";
            value = value.Trim();
            // Remove quotes
            if (value[0] == '\"' && value[value.Length - 1] == '\"') {
                value = StripFirstAndLastCharacter(value);
            }
            // Replace "" with "
            value = value.Replace("\"\"", "\"");
            // Replace \n with real new-lines
            value = ConvertEscapedCharacters(value);
            return value.Trim();
        }

        public static string CombineStringWithCount(string value, int count) {
            Assert.IsTrue(count >= 1, "count must be >= 1");
            Assert.IsTrue(value.IndexOf('=') == -1, "value must not contain '='");
            if (count == 1)
                return value;
            return string.Format("{0}={1}", value, ToUniString(count));
        }

        public static bool TryParsingCombinedStringWithCount(string combined, out string value, out int count) {
            if (string.IsNullOrEmpty(combined)) {
                value = combined;
                count = 0;
                return false;
            }
            string[] pair = combined.Split('=');
            value = pair[0];
            if (pair.Length == 1) {
                // No count data - only one
                count = 1;
                return true;
            }
            return int.TryParse(pair[1], out count);
        }

        public static string ToUniString(bool value) {
            return value ? "true" : "false";
        }

        public static string ToUniString(short value) {
            return value.ToString("D", NumberFormatInfo.InvariantInfo);
        }

        public static string ToUniString(int value) {
            return value.ToString("D", NumberFormatInfo.InvariantInfo);
        }

        public static string ToUniString(long value) {
            return value.ToString("D", NumberFormatInfo.InvariantInfo);
        }

        public static string ToUniString(float value) {
            return value.ToString("R", NumberFormatInfo.InvariantInfo);
        }

        public static string ToUniString(double value) {
            return value.ToString("R", NumberFormatInfo.InvariantInfo);
        }
    }
}
