//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using Outfit7.Util;
using Outfit7.Util.Io;
using SimpleJSON;
using UnityEngine;
using System.IO;

namespace Outfit7.Social {

    /// <summary>
    /// Social users persister.
    /// </summary>
    public class SocialUserPersister : JsonFileReaderWriter {

        protected const string FileName = "O7SocialUsers.json";

        protected static string CreateFilePath() {
            return Path.Combine(Application.persistentDataPath, FileName);
        }

        public static void ClearPrefs() {
            try {
                O7File.Delete(CreateFilePath());

            } catch (Exception e) {
                O7Log.WarnT(Tag, e, "Cannot delete file: {0}", CreateFilePath());
            }
        }

        protected JSONNode UsersJ;

        public SocialUserPersister() : base(CreateFilePath()) {
        }

        public virtual JSONNode LoadUsers() {
            lock (Lock) {
                if (UsersJ == null) {
                    UsersJ = ReadJson();
                }
                return UsersJ;
            }
        }

        public virtual void SaveUsers(JSONNode usersJ) {
            lock (Lock) {
                UsersJ = usersJ;
                WriteJson(usersJ);
            }
        }
    }
}
