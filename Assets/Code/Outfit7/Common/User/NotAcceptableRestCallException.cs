//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using Outfit7.Web;
using SimpleJSON;

namespace Outfit7.User {

    /// <summary>
    /// Not acceptable (406/409) REST call exception.
    /// </summary>
    public class NotAcceptableRestCallException : RestCallException {

        public bool IsPlayerIdConflict { get; private set; }

        public JSONNode ResponseBody { get; private set; }

        public NotAcceptableRestCallException(bool playedIdConflict, string status, JSONNode responseBody)
            : base(status) {
            IsPlayerIdConflict = playedIdConflict;
            ResponseBody = responseBody;
        }
    }
}
