
using System;
using System.Collections.Generic;
using Outfit7.Json;
using Outfit7.Util;
using Outfit7.Util.Io;
using SimpleJSON;
using UnityEngine;

namespace Outfit7.Social.Network {

    /// <summary>
    /// Social friend persister.
    /// </summary>
    public static class SocialFriendPersister {

        private const string Tag = "SocialFriendPersister";
        public static string PersistentPath = Application.persistentDataPath;

        private static string FilePath(string fileName) {
            return PersistentPath + "/" + fileName;
        }

        static SocialFriendPersister() {
            PersistentPath = Application.persistentDataPath;
        }

        public static Dictionary<string,SocialFriend> LoadFriendsData(string fileName) {
            string usersData = ReadDataFromFile(fileName);
            if (usersData == null) return null;

            return ParseSocialFriends(usersData);
        }

        public static SocialFriend LoadUserData(string key) {
            string userData = UserPrefs.GetString(key, null);
            if (userData == null) return null;

            return ParseUserData(userData);
        }

        public static void SaveUserData(string data, string key) {
            UserPrefs.SetString(key, data);
            UserPrefs.SaveDelayed();
        }

        private static SocialFriend ParseUserData(JSONNode dataJ) {
            if (dataJ == null) return null;

            SocialFriend user = new SocialFriend(
                                    dataJ["id"],
                                    dataJ["first_name"],
                                    dataJ["middle_name"],
                                    dataJ["last_name"],
                                    dataJ["img_url"],
                                    dataJ["installed"].AsBool);

            if (user.Id != null) return user;

            O7Log.WarnT(Tag, "Trouble parsing {0}", dataJ.ToString());
            return null;
        }

        public static Dictionary<string,SocialFriend> ParseSocialFriends(string friendList) {
            Dictionary<string,SocialFriend> friends;
            try {
                JSONNode json = JSON.Parse(friendList);
                JSONArray friendsJson = SimpleJsonUtils.EnsureJsonArray(json);
                friends = new Dictionary<string,SocialFriend>(friendsJson.Count);

                foreach (JSONNode child in friendsJson.Childs) {
                    SocialFriend friend = ParseUserData(child);
                    if (friend != null && friend.Id != null) {
                        friends.Add(friend.Id, friend);
                    } else {
                        O7Log.WarnT(Tag, "Cannot parse friend data '{0}'", child);
                    }
                }

                O7Log.DebugT(Tag, "Parsed friends count {0}", friends.Count);

            } catch (Exception e) { // currupted json
                O7Log.ErrorT(Tag, "Cannot parse social friends json {0}", e);
                friends = null;
            }

            return friends;
        }

        public static void WriteFriendsData(string data, string fileName) {
            WriteDataToFile(data, fileName);
        }

        private static void WriteDataToFile(string data, string fileName) {
            O7Log.DebugT(Tag, "WriteData {0}", data);

            if (StringUtils.IsNullOrEmpty(data)) return;

            string filePath = FilePath(fileName);

            O7Log.DebugT(Tag, "Writing social data to file {0}", filePath);

            SimpleFileReaderWriter sfrw = new SimpleFileReaderWriter(filePath);
            try {
                sfrw.WriteContent(data);

            } catch (Exception e) {
                O7Log.WarnT(Tag, e, "Cannot save social data to {0}", filePath);
            }
        }

        private static string ReadDataFromFile(string fileName) {
            O7Log.DebugT(Tag, "ReadData from {0}", fileName);

            string filePath = FilePath(fileName);

            if (!O7File.Exists(filePath)) {
                O7Log.DebugT(Tag, "File does not exist");
                return null;
            }

            SimpleFileReaderWriter sfrw = new SimpleFileReaderWriter(filePath);
            try {
                return sfrw.ReadContent();

            } catch (Exception e) {
                O7Log.WarnT(Tag, e, "Cannot read from file {0}", filePath);
                return null;
            }
        }

        public static void DeleteFile(string fileName) {
            try {
                O7File.Delete(FilePath(fileName));

            } catch (Exception e) {
                O7Log.WarnT(Tag, e, "Cannot delete file: {0}", FilePath(fileName));
            }
        }

        public static SocialFriend ParseUserData(string jsonString) {
            if (StringUtils.IsNullOrEmpty(jsonString)) {
                O7Log.DebugT(Tag, "No user data");
                return null;
            }

            try {
                JSONNode json = JSON.Parse(jsonString);

                SocialFriend user = ParseUserData(json);
                O7Log.DebugT(Tag, "Parsed user data {0}", user);
                return user;

            } catch (Exception e) { // currupted json
                O7Log.ErrorT(Tag, "Cannot parse social friends json {0}", e);
                return null;
            }
        }
    }
}
