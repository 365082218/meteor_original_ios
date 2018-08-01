//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

#if NETFX_CORE
namespace System {

    /// <summary>
    /// Invalid program exception.
    /// </summary>
    public sealed class InvalidProgramException : InvalidOperationException {

        public InvalidProgramException() : base() {
        }

        public InvalidProgramException(string message) : base(message) {
        }

        public InvalidProgramException(string message, Exception inner) : base(message, inner) {
        }
    }
}
#endif
