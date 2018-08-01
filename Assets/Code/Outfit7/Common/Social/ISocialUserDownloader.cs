//
//   Copyright (c) 2017 Outfit7. All rights reserved.
//

namespace Outfit7.Social {

    /// <summary>
    /// Social users downloader interface should be implemented by the class that is responsible for downloading social users.
    /// Usually they are downloaded from backend via the user state communication.
    /// </summary>
    public interface ISocialUserDownloader {

        bool IsDownloadInProgress { get; }

        void StartDownload();
    }
}
