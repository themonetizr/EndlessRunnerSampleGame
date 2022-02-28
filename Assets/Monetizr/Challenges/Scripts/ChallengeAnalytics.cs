using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using mixpanel;
using System;
using UnityEngine.Assertions;

namespace Monetizr.Challenges
{
    public enum AdType
    {
        IntroBanner,
        BrandLogo,
        TinyTeaser,
        RewardLogo,
        Video,
        RewardBanner

    }

    public class VisibleAdAsset
    {
        public AdType adType;
        public string challengeId;
        public DateTime activateTime;

    }

    public class MonetizrAnalytics
    {
        public static readonly Dictionary<AdType, string> adTypeNames = new Dictionary<AdType, string>()
        {
            { AdType.IntroBanner, "intro_banner" },
            { AdType.BrandLogo, "banner_logo" },
            { AdType.TinyTeaser, "tiny_teaser" },
            { AdType.RewardLogo, "reward_logo" },
            { AdType.Video, "video" },
            { AdType.RewardBanner, "reward_banner" },
        };

        private Dictionary<string, ChallengeTimes> challengesWithTimes = new Dictionary<string, ChallengeTimes>();

        private const int SECONDS_IN_DAY = 24 * 60 * 60;

        private VisibleAdAsset visibleAdAsset = null;

        public MonetizrAnalytics()
        {
            Mixpanel.Init();
        }

        public void BeginShowAdAsset(AdType type)
        {
            Debug.Log("MonetizrAnalytics BeginShowAdAsset!");

            Assert.IsNull(visibleAdAsset, MonetizrErrors.msg[ErrorType.AdAssetStillShowing]);

            var ch = MonetizrManager.Instance.GetAvailableChallenges();

            visibleAdAsset = new VisibleAdAsset() {
                adType = type,
                challengeId = ch[0],
                activateTime = DateTime.Now
            };

        }

        public void EndShowAdAsset(AdType type)
        {
            Debug.Log("MonetizrAnalytics EndShowAdAsset!");

            //Mixpanel.Track("Sent Message");

            Assert.IsNotNull(visibleAdAsset);
            Assert.AreEqual(type, visibleAdAsset.adType, MonetizrErrors.msg[ErrorType.SimultaneusAdAssets]);

            var challenge = MonetizrManager.Instance.GetChallenge(visibleAdAsset.challengeId);
            
            var props = new Value();
            props["application_id"] = Application.identifier.ToLower();
            props["player_id"] = SystemInfo.deviceUniqueIdentifier;
            props["application_name"] = Application.productName;
            props["application_version"] = Application.version;
            props["ad_impressions"] = "1";
            props["ad_campaign_id"] = visibleAdAsset.challengeId;
            props["ad_brand_id"] = challenge.brand_id;
            props["ad_type"] = adTypeNames[type];
            props["ad_duration"] = (DateTime.Now - visibleAdAsset.activateTime).TotalSeconds;

            Mixpanel.Track("[UNITY_SDK] ad_asset", props);

            

            visibleAdAsset = null;

            //quest?

            //"id" vs "campaign_id"?

            //not necessary in asset
            //"campaign_id":"8ff82e4b-0d13-46a4-a91c-684c3e0d0e70",
            //"brand_id":"d250d29e-8488-4a2f-b0b3-a1e2953ac2c4",
            //"application_id":"d10d793a-e937-4622-a79f-68cbc01a97ad",

        }

        public void Flush()
        {
            Mixpanel.Flush();
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
