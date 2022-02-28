using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using mixpanel;

namespace Monetizr.Challenges
{
    public enum AdType
    {
        IntroBanner,
        BrandLogo,
        TinyTeaser,
        ChallengeLogo,
        Video,
        RewardBanner
    }

    public class ChallengeAnalytics
    {
        private Dictionary<string, ChallengeTimes> challengesWithTimes = new Dictionary<string, ChallengeTimes>();

        private const int SECONDS_IN_DAY = 24 * 60 * 60;


        ChallengeAnalytics()
        {
            Mixpanel.Init();
        }

        public void StartShowAdAsset(Challenge challenge, AdType type)
        {

        }

        public void EndShowAdAsset(Challenge challenge, AdType type)
        {

        }

        /// <summary>
        /// Updates <see cref="challengesWithTimes"/> with <paramref name="challenges"/>. 
        /// If a challenge has already been registered in a previous Update, its times will remain unchanged.
        /// If a challenge no longer exists in <paramref name="challenges"/> it will be removed.
        /// </summary>
        public void Update(List<Challenge> challenges)
        {
            Dictionary<string, ChallengeTimes> updatedChallengesWithTimes = new Dictionary<string, ChallengeTimes>();

            foreach (Challenge challenge in challenges)
            {
                if (challengesWithTimes.ContainsKey(challenge.id))
                {
                    updatedChallengesWithTimes.Add(challenge.id, challengesWithTimes[challenge.id]);
                }
                else
                {
                    int currentTime = Mathf.FloorToInt(Time.realtimeSinceStartup);
                    updatedChallengesWithTimes.Add(challenge.id, new ChallengeTimes(currentTime, currentTime));
                }
            }

            challengesWithTimes = new Dictionary<string, ChallengeTimes>(updatedChallengesWithTimes);
        }

        /// <summary>
        /// Returns time in seconds between now and time when <paramref name="challenge"/> first appeared in <see cref="Update(List{Challenge})"/>
        /// </summary>
        public int GetElapsedTime(Challenge challenge)
        {
            if (!challengesWithTimes.ContainsKey(challenge.id))
            {
                Debug.LogError("Challenge: " + challenge.title + " was not added to analytics");
                return -1;
            }

            return Mathf.FloorToInt(Time.realtimeSinceStartup) - challengesWithTimes[challenge.id].firstSeenTime;
        }

        /// <summary>
        /// Returns time in seconds between now and last time <paramref name="challenge"/> status update was marked.
        /// </summary>
        public int GetTimeSinceLastUpdate(Challenge challenge)
        {
            if (!challengesWithTimes.ContainsKey(challenge.id))
            {
                Debug.LogError("Challenge: " + challenge.title + " was not added to analytics");
                return -1;
            }

            return Mathf.FloorToInt(Time.realtimeSinceStartup) - challengesWithTimes[challenge.id].lastUpdateTime;
        }

        /// <summary>
        /// Sets last update time for <paramref name="challenge"/> to current time.
        /// If time since last update exceeds 24h, resets first time seen to 0.
        /// </summary>
        public void MarkChallengeStatusUpdate(Challenge challenge)
        {
            if (!challengesWithTimes.ContainsKey(challenge.id))
            {
                Debug.LogError("Challenge: " + challenge.title + " was not added to analytics");
                return;
            }

            if (GetTimeSinceLastUpdate(challenge) > SECONDS_IN_DAY)
            {
                challengesWithTimes[challenge.id].firstSeenTime = Mathf.FloorToInt(Time.realtimeSinceStartup);
            }

            challengesWithTimes[challenge.id].lastUpdateTime = Mathf.FloorToInt(Time.realtimeSinceStartup);
        }

        private class ChallengeTimes
        {
            public ChallengeTimes(int firstSeenTime, int lastUpdateTime)
            {
                this.firstSeenTime = firstSeenTime;
                this.lastUpdateTime = lastUpdateTime;
            }

            public int firstSeenTime;
            public int lastUpdateTime;
        }
    }



}
