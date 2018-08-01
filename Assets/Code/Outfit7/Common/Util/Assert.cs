//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Outfit7.Util {

    /// <summary>
    /// Assertion utils.
    /// Partly copied from Spring.Core.Util.AssertUtils.
    /// </summary>
    public static class Assert {

        /// <summary>
        /// Checks the value of the supplied <paramref name="argument"/> and throws an
        /// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/>.
        /// </summary>
        /// <param name="argument">The object to check.</param>
        /// <param name="name">The argument name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/>.
        /// </exception>
        public static void NotNull(object argument, string name) {
            if (argument == null) {
                throw new ArgumentNullException(name, string.Format(CultureInfo.InvariantCulture, "Argument '{0}' cannot be null", name));
            }
        }

        /// <summary>
        /// Checks the value of the supplied <paramref name="unityObject"/> and throws an
        /// <see cref="System.ArgumentNullException"/> if the Unity object is destroyed.
        /// </summary>
        /// <param name="unityObject">The object to check.</param>
        /// <param name="name">The object name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="unityObject"/> is destroyed.
        /// </exception>
        public static void NotNull(UnityEngine.Object unityObject, string name) {
            if (unityObject == null) {
                throw new ArgumentNullException(name, string.Format(CultureInfo.InvariantCulture, "Unity object '{0}' cannot be destroyed", name));
            }
        }

        /// <summary>
        /// Checks the value of the supplied <paramref name="argument"/> and throws an
        /// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/>.
        /// </summary>
        /// <param name="argument">The object to check.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">
        /// An arbitrary message that will be passed to any thrown
        /// <see cref="System.ArgumentNullException"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/>.
        /// </exception>
        public static void NotNull(object argument, string name, string message, params object[] args) {
            if (argument == null) {
                throw new ArgumentNullException(name, string.Format(CultureInfo.InvariantCulture, message, args));
            }
        }

        /// <summary>
        /// Checks the value of the supplied <paramref name="unityObject"/> and throws an
        /// <see cref="System.ArgumentNullException"/> if the Unity object is destroyed.
        /// </summary>
        /// <param name="unityObject">The object to check.</param>
        /// <param name="name">The object name.</param>
        /// <param name="message">
        /// An arbitrary message that will be passed to any thrown
        /// <see cref="System.ArgumentNullException"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="unityObject"/> is destroyed.
        /// </exception>
        public static void NotNull(UnityEngine.Object unityObject, string name, string message, params object[] args) {
            if (unityObject == null) {
                throw new ArgumentNullException(name, string.Format(CultureInfo.InvariantCulture, message, args));
            }
        }

        /// <summary>
        /// Checks the value of the supplied string <paramref name="argument"/> and throws an
        /// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </summary>
        /// <param name="argument">The string to check.</param>
        /// <param name="name">The argument name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </exception>
        public static void HasText(string argument, string name) {
            if (StringUtils.IsNullOrEmpty(argument)) {
                throw new ArgumentNullException(name,
                 string.Format(CultureInfo.InvariantCulture, "Argument '{0}' cannot be null or resolve to an empty string : '{1}'", name, argument));
            }
        }

        /// <summary>
        /// Checks the value of the supplied string <paramref name="argument"/> and throws an
        /// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </summary>
        /// <param name="argument">The string to check.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">
        /// An arbitrary message that will be passed to any thrown
        /// <see cref="System.ArgumentNullException"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </exception>
        public static void HasText(string argument, string name, string message, params object[] args) {
            if (StringUtils.IsNullOrEmpty(argument)) {
                throw new ArgumentNullException(name, string.Format(CultureInfo.InvariantCulture, message, args));
            }
        }

        /// <summary>
        /// Checks the value of the supplied <see cref="System.Collections.ICollection"/> <paramref name="argument"/> and throws
        /// an <see cref="ArgumentNullException"/> if it is <see langword="null"/> or contains no elements.
        /// </summary>
        /// <param name="argument">The array or collection to check.</param>
        /// <param name="name">The argument name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/> or
        /// contains no elements.
        /// </exception>
        public static void HasLength<T>(ICollection<T> argument, string name) {
            if (CollectionUtils.IsEmpty(argument)) {
                throw new ArgumentNullException(name, string.Format(CultureInfo.InvariantCulture, "Argument '{0}' cannot be null or resolve to an empty array", name));
            }
        }

        /// <summary>
        /// Checks the value of the supplied <see cref="System.Collections.ICollection"/> <paramref name="argument"/> and throws
        /// an <see cref="ArgumentNullException"/> if it is <see langword="null"/> or contains no elements.
        /// </summary>
        /// <param name="argument">The array or collection to check.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">An arbitrary message that will be passed to any thrown <see cref="System.ArgumentNullException"/>.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/> or
        /// contains no elements.
        /// </exception>
        public static void HasLength<T>(ICollection<T> argument, string name, string message, params object[] args) {
            if (CollectionUtils.IsEmpty(argument)) {
                throw new ArgumentNullException(name, string.Format(CultureInfo.InvariantCulture, message, args));
            }
        }

        /// <summary>
        /// Checks the value of the supplied <see cref="System.Collections.ICollection"/> <paramref name="argument"/> and throws
        /// an <see cref="ArgumentException"/> if it is <see langword="null"/>, contains no elements or only null elements.
        /// </summary>
        /// <param name="argument">The array or collection to check.</param>
        /// <param name="name">The argument name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/>,
        /// contains no elements or only null elements.
        /// </exception>
        public static void HasElements<T>(ICollection<T> argument, string name) {
            if (!CollectionUtils.HasElements(argument)) {
                throw new ArgumentException(name,
                    string.Format(CultureInfo.InvariantCulture, "Argument '{0}' must not be null or resolve to an empty collection and must contain non-null elements", name));
            }
        }


        /// <summary>
        /// Checks whether the specified <paramref name="argument"/> can be cast
        /// into the <paramref name="requiredType"/>.
        /// </summary>
        /// <param name="argument">
        /// The argument to check.
        /// </param>
        /// <param name="argumentName">
        /// The name of the argument to check.
        /// </param>
        /// <param name="requiredType">
        /// The required type for the argument.
        /// </param>
        /// <param name="message">
        /// An arbitrary message that will be passed to any thrown
        /// <see cref="System.ArgumentException"/>.
        /// </param>
        public static void Type(object argument, string argumentName, Type requiredType, string message, params object[] args) {
            if (argument != null && requiredType != null) {
#if NETFX_CORE
                bool assignable = MarkerMetro.Unity.WinLegacy.Reflection.ReflectionExtensions.IsAssignableFrom(requiredType, argument.GetType());
#else
                bool assignable = requiredType.IsAssignableFrom(argument.GetType());
#endif
                if (!assignable) {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, message, args), argumentName);
                }
            }
        }

        /// <summary>
        ///  Assert a boolean expression, throwing <code>ArgumentException</code>
        ///  if the test result is <code>false</code>.
        /// </summary>
        /// <param name="expression">a boolean expression.</param>
        /// <exception cref="ArgumentException">
        /// if expression is <code>false</code>
        /// </exception>
        public static void IsTrue(bool expression) {
            IsTrue(expression, "[Assertion failed] - this expression must be true");
        }

        /// <summary>
        ///  Assert a boolean expression, throwing <code>ArgumentException</code>
        ///  if the test result is <code>false</code>.
        /// </summary>
        /// <param name="expression">a boolean expression.</param>
        /// <param name="message">The exception message to use if the assertion fails.</param>
        /// <exception cref="ArgumentException">
        /// if expression is <code>false</code>
        /// </exception>
        public static void IsTrue(bool expression, string message, params object[] args) {
            if (!expression) {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, message, args));
            }
        }

        /// <summary>
        /// Assert a bool expression, throwing <code>InvalidOperationException</code>
        /// if the expression is <code>false</code>.
        /// </summary>
        /// <param name="expression">a boolean expression.</param>
        /// <param name="message">The exception message to use if the assertion fails</param>
        /// <exception cref="InvalidOperationException">if expression is <code>false</code></exception>
        public static void State(bool expression, string message, params object[] args) {
            if (!expression) {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, message, args));
            }
        }

        /// <summary>
        /// Assert an action, throwing <code>InvalidOperationException</code>
        /// if the action does not throw a specified exception.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// if the action or exceptionType is <code>null</code> or exception thrown is not assignable from the exceptionType
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// if the action does not fail - does not throw an exception at all
        /// </exception>
        public static void Fail(Action action, Type exceptionType) {
            Fail(action, exceptionType, "Action must throw " + exceptionType);
        }

        /// <summary>
        /// Assert an action, throwing <code>InvalidOperationException</code>
        /// if the action does not throw a specified exception.
        /// </summary>
        /// <param name="action">The action to check</param>
        /// <param name="exceptionType">The type of exception that the action must throw</param>
        /// <param name="message">The exception message to use if the assertion fails</param>
        /// <exception cref="ArgumentException">
        /// if the action or exceptionType is <code>null</code> or exception thrown is not assignable from the exceptionType
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// if the action does not fail - does not throw an exception at all
        /// </exception>
        public static void Fail(Action action, Type exceptionType, string message, params object[] args) {
            NotNull(action, "action");
            NotNull(exceptionType, "exceptionType");
            try {
                action();

            } catch (Exception e) {
                if (e.GetType() != exceptionType) {
                    throw new ArgumentException("Expected " + exceptionType + " but got " + e.GetType());
                }
                return;
            }

            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, message, args));
        }
    }
}
