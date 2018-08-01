//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;

namespace Outfit7.Web {

    /// <summary>
    /// Common exception if there is an error in the response to the rest call.
    /// </summary>
    public class RestCallException : Exception {

        public RestCallException(string message) : base(message) {
        }
    }
}
