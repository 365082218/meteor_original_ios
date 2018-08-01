using System;
using System.Globalization;

namespace Starlite.Raven {

    public static class RavenAssert {

        public static void IsTrue(bool expression) {
            IsTrue(expression, "[Assertion failed] - this expression must be true");
        }

        public static void IsTrue(bool expression, string message, params object[] args) {
            if (!expression) {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, message, args));
            }
        }
    }
}
