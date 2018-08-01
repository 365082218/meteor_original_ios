//
//   Copyright (c) 2017 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using Outfit7.Social.Network;
using Outfit7.Threading;
using Outfit7.Threading.Task;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Social {

    /// <summary>
    /// Social users worker.
    /// </summary>
    public abstract class SocialUserWorker<U> where U : BasicSocialUser {

        protected const string Tag = "SocialUserWorker";
        private const string ExecutorName = "SocialUserWorker";
        protected ThreadExecutor Executor;

        public SocialUserPersister SocialUserPersister { get; set; }

        public AbstractSocialUserUnmarshaller<U> SocialUserUnmarshaller { get; set; }

        public virtual void Init() {
            O7Log.DebugT(Tag, "Init");

            Executor = new ThreadExecutor(ExecutorName);
            Executor.SleepMillis = 500;
        }

        public virtual Pair<List<U>, List<U>> LoadUsers(IDictionary<string, SocialFriend> friendMap) {
            O7Log.DebugT(Tag, "Loading social users...");

            JSONNode usersJ = SocialUserPersister.LoadUsers();
            if (usersJ == null) {
                O7Log.DebugT(Tag, "Canceled loading social users: no data");
                return null;
            }

            Pair<List<U>, List<U>> pair = SocialUserUnmarshaller.Unmarshal(usersJ, friendMap);

            O7Log.DebugT(Tag, "Loaded {0} social friends and {1} strangers", pair.First.Count, pair.Second.Count);

            return pair;
        }

        public virtual void PostUpdateUsers(JSONNode usersJ, IDictionary<string, SocialFriend> friendMap,
            TaskFeedback<Pair<List<U>, List<U>>> feedback) {
            TaskFeedback<Pair<List<U>, List<U>>> executorFeedback = new ExecutorTaskFeedbackWrapper<Pair<List<U>, List<U>>>(feedback);

            Action loader = delegate {
                executorFeedback.OnStart();

                Pair<List<U>, List<U>> pair = UpdateUsers(usersJ, friendMap);
                executorFeedback.OnFinish(pair);
            };
            Executor.Post(loader);
        }

        protected virtual Pair<List<U>, List<U>> UpdateUsers(JSONNode usersJ, IDictionary<string, SocialFriend> friendMap) {
            O7Log.DebugT(Tag, "Updating social users...");

            Pair<List<U>, List<U>> pair = SocialUserUnmarshaller.Unmarshal(usersJ, friendMap);

            SocialUserPersister.SaveUsers(usersJ);

            O7Log.DebugT(Tag, "Updated {0} social friends and {1} strangers", pair.First.Count, pair.Second.Count);

            return pair;
        }
    }
}
