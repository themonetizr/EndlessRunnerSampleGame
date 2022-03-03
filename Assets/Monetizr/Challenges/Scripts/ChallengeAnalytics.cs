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
            { AdType.IntroBanner, "Intro banner" },
            { AdType.BrandLogo, "Banner logo" },
            { AdType.TinyTeaser, "Tiny teaser" },
            { AdType.RewardLogo, "Reward logo" },
            { AdType.Video, "Video" },
            { AdType.RewardBanner, "Reward banner" },
        };

        private Dictionary<string, ChallengeTimes> challengesWithTimes = new Dictionary<string, ChallengeTimes>();

        private const int SECONDS_IN_DAY = 24 * 60 * 60;

        private Dictionary<AdType,VisibleAdAsset> visibleAdAsset = new Dictionary<AdType, VisibleAdAsset>();

        public MonetizrAnalytics()
        {
            Debug.Log($"MonetizrAnalytics initialized with user id: {GetUserId()}");

            Mixpanel.Init();
        }

        public void BeginShowAdAsset(AdType type)
        {
            Debug.Log($"MonetizrAnalytics BeginShowAdAsset: {type}");


            if(visibleAdAsset.ContainsKey(type))
            {
                Debug.Log(MonetizrErrors.msg[ErrorType.AdAssetStillShowing]);
            }

            //Assert.IsFalse(visibleAdAsset.ContainsKey(type), MonetizrErrors.msg[ErrorType.AdAssetStillShowing]);

           /* if (visibleAdAsset.ContainsKey(type))
            {
                Debug.Log($"MonetizrAnalytics Some asset {type} is still visible!");

                
            //    EndShowAdAsset(visibleAdAsset.adType);
            }*/

            
         
            var ch = MonetizrManager.Instance.GetActiveChallenge();

            var adAsset = new VisibleAdAsset() {
                adType = type,
                challengeId = ch,
                activateTime = DateTime.Now
            };

            visibleAdAsset[type] = adAsset;

            Mixpanel.StartTimedEvent($"[UNITY_SDK] [AD] {adTypeNames[type]}");

        }

        public void EndShowAdAsset(AdType type)
        {     
            
            Debug.Log($"MonetizrAnalytics EndShowAdAsset: {type}");

            //Mixpanel.Track("Sent Message");

            //Assert.IsNotNull(visibleAdAsset);
            //Assert.AreEqual(type, visibleAdAsset.adType, MonetizrErrors.msg[ErrorType.SimultaneusAdAssets]);

            var challenge = MonetizrManager.Instance.GetChallenge(visibleAdAsset[type].challengeId);
            
            var props = new Value();
            props["application_id"] = Application.identifier;
            props["player_id"] = GetUserId();
            props["application_name"] = Application.productName;
            props["application_version"] = Application.version;
            props["impressions"] = "1";
            props["campaign_id"] = visibleAdAsset[type].challengeId;
            props["camp_id"] = visibleAdAsset[type].challengeId;
            props["brand_id"] = challenge.brand_id;
            props["type"] = adTypeNames[type];
            //props["duration"] = (DateTime.Now - visibleAdAsset[type].activateTime).TotalSeconds;

            Mixpanel.Track($"[UNITY_SDK] [AD] {adTypeNames[type]}", props);

            visibleAdAsset.Remove(type);
        }

        public string GetUserId()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        public void TrackEvent(string name)
        {
            string campaign_id = "none";
            string brand_id = "none";

            if (MonetizrManager.Instance.HasChallengesAndActive())
            {
                var ch = MonetizrManager.Instance.GetActiveChallenge();

                brand_id = MonetizrManager.Instance.GetChallenge(ch).brand_id;
                campaign_id = ch;
            }

            var props = new Value();
            props["application_id"] = Application.identifier;
            props["player_id"] = SystemInfo.deviceUniqueIdentifier;
            props["application_name"] = Application.productName;
            props["application_version"] = Application.version;
            props["campaign_id"] = campaign_id;
            props["camp_id"] = campaign_id;
            props["brand_id"] = brand_id;

            Mixpanel.Track($"[UNITY_SDK] {name}", props);
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
