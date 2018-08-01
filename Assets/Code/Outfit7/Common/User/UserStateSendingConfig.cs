//
//   Copyright (c) 2017 Outfit7. All rights reserved.
//

namespace Outfit7.User {

    /// <summary>
    /// User state sending configuration data.
    /// </summary>
    public class UserStateSendingConfig {

        public enum ESendingMode {
            Normal,
            NormalWithRemote,
            ForcingOverwriteAsNew,
            ForcingOverwriteAsNewWithRemote,
            ForcingOverwriteAsRestored,
            ForcingOverwriteAsRestoredWithRemote
        }

        public ESendingMode SendingMode { get; protected set; }

        public string ServerBaseUrl { get; protected set; }

        public bool ShouldDownloadStrangers { get; protected set; }

        public UserStateSendingConfig(ESendingMode sendingMode, string serverBaseUrl, bool downloadStrangers = false) {
            SendingMode = sendingMode;
            ServerBaseUrl = serverBaseUrl;
            ShouldDownloadStrangers = downloadStrangers;
        }

        public override string ToString() {
            return string.Format("[UserStateSendingConfig: SendingMode={0}, ServerBaseUrl={1}, ShouldDownloadStrangers={2}]",
                SendingMode, ServerBaseUrl, ShouldDownloadStrangers);
        }
    }
}
