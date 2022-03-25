//#define USING_FACEBOOK
//#define USING_AMPLITUDE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using mixpanel;
using System;
using UnityEngine.Assertions;

#if USING_FACEBOOK
    using Facebook.Unity;   
#endif

namespace Monetizr.Campaigns
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

    internal class VisibleAdAsset
    {
        public AdType adType;
        public string challengeId;
        public DateTime activateTime;

    }

    internal class MonetizrAnalytics
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

//        private Dictionary<string, ChallengeTimes> challengesWithTimes = new Dictionary<string, ChallengeTimes>();

//        private const int SECONDS_IN_DAY = 24 * 60 * 60;


        //private Dictionary<AdType,VisibleAdAsset> visibleAdAsset = new Dictionary<AdType, VisibleAdAsset>();

        //AdType and ChallengeId
        private Dictionary<KeyValuePair<AdType, string>, VisibleAdAsset> visibleAdAsset = new Dictionary<KeyValuePair<AdType, string>, VisibleAdAsset>();

#if USING_AMPLITUDE
        private Amplitude amplitude;
#endif

        public MonetizrAnalytics()
        {
            Debug.Log($"MonetizrAnalytics initialized with user id: {GetUserId()}");

#if USING_AMPLITUDE
            amplitude = Amplitude.Instance;
            amplitude.logging = true;
            amplitude.init("16e0224a663d67c293ee5b0f0ff926ad");
#endif

            Mixpanel.Init();
            Mixpanel.SetToken("d4de97058730720b3b8080881c6ba2e0");

#if USING_FACEBOOK
            if (FB.IsInitialized)
            {
                FB.ActivateApp();
            }
            else
            {
                FB.Init(() =>
                {
                    FB.ActivateApp();

                    Debug.Log("[FB] Activated!");
                });
            }
#endif
        }

        public void BeginShowAdAsset(AdType type, string challengeId = null)
        {
            Debug.Log($"MonetizrAnalytics BeginShowAdAsset: {type} {challengeId}");

            //Key value pair for duplicated types with different challenge ids
            KeyValuePair<AdType, string> adElement = new KeyValuePair<AdType, string>(type, challengeId);

            if (visibleAdAsset.ContainsKey(adElement))
            {
                Debug.Log(MonetizrErrors.msg[ErrorType.AdAssetStillShowing]);
            }

            //Assert.IsFalse(visibleAdAsset.ContainsKey(type), MonetizrErrors.msg[ErrorType.AdAssetStillShowing]);

            var ch = (challengeId == null) ? MonetizrManager.Instance.GetActiveChallenge() : challengeId;

            var adAsset = new VisibleAdAsset() {
                adType = type,
                challengeId = ch,
                activateTime = DateTime.Now
            };

            visibleAdAsset[adElement] = adAsset;

            Mixpanel.StartTimedEvent($"[UNITY_SDK] [AD] {adTypeNames[type]}");

        }

        //if challengeId is define, end only specified ad types, if not - end all
        public void EndShowAdAsset(AdType type, string challengeId = null, bool removeElement = true)
        {     
            //Assert.IsNotNull(visibleAdAsset);
            //Assert.AreEqual(type, visibleAdAsset.adType, MonetizrErrors.msg[ErrorType.SimultaneusAdAssets]);

            //if challenge id isn't null, sent analytics event with this exact id
            if (challengeId != null)
            {
                KeyValuePair<AdType, string> key = new KeyValuePair<AdType, string>(type, challengeId);


                _EndShowAdAsset(key);

                if(removeElement)
                    visibleAdAsset.Remove(key);
            }
            //if challenge id is null, send events for all active ad assets with the same type
            else
            {
                List< KeyValuePair<AdType, string>> toRemove = new List<KeyValuePair<AdType, string>>();

                foreach(var adAssetElement in visibleAdAsset)
                {
                    if(adAssetElement.Key.Key == type)
                    {
                        _EndShowAdAsset(adAssetElement.Key);

                        if (removeElement)
                            toRemove.Add(adAssetElement.Key);
                    }
                }

                foreach (var i in toRemove)
                    visibleAdAsset.Remove(i);

            }

           
        }

        private void _EndShowAdAsset(KeyValuePair<AdType, string> adAsset)
        {
            Debug.Log($"MonetizrAnalytics EndShowAdAsset: {adAsset.Key} {adAsset.Value}");

            string brandName = MonetizrManager.Instance.GetAsset<string>(visibleAdAsset[adAsset].challengeId, AssetsType.BrandTitleString);

            var challenge = MonetizrManager.Instance.GetChallenge(visibleAdAsset[adAsset].challengeId);

            var props = new Value();
            props["application_id"] = challenge.application_id;
            props["bundle_id"] = Application.identifier;
            props["player_id"] = GetUserId();
            props["application_name"] = Application.productName;
            props["application_version"] = Application.version;
            props["impressions"] = "1";
            //props["campaign_id"] = visibleAdAsset[adAsset].challengeId;
            props["camp_id"] = visibleAdAsset[adAsset].challengeId;
            props["brand_id"] = challenge.brand_id;
            props["brand_name"] = brandName;
            props["type"] = adTypeNames[adAsset.Key];
            //props["duration"] = (DateTime.Now - visibleAdAsset[type].activateTime).TotalSeconds;

            string eventName = $"[UNITY_SDK] [AD] {adTypeNames[adAsset.Key]}";

            Mixpanel.Track(eventName, props);

#if USING_AMPLITUDE
            Dictionary<string, object> eventProps = new Dictionary<string, object>();
            eventProps.Add("camp_id", visibleAdAsset[adAsset].challengeId);
            eventProps.Add("brand_id", challenge.brand_id);
            eventProps.Add("brand_name", brandName);
            eventProps.Add("type", adTypeNames[adAsset.Key]);
            eventProps.Add("duration", (DateTime.Now - visibleAdAsset[adAsset].activateTime).TotalSeconds);

            amplitude.logEvent(eventName, eventProps);
#endif

            //if (removeElement)
            //    visibleAdAsset.Remove(adAsset);
        }

        public string GetUserId()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        public void TrackEvent(string name, MissionUIDescription currentMissionDesc)
        {
            var eventName = $"[UNITY_SDK] {name}";

            string campaign_id = "none";
            string brand_id = "none";
            string app_id = "none";
            string brand_name = "none";
                
            if (currentMissionDesc != null)
            {
                var ch = currentMissionDesc.campaignId;
                brand_id = MonetizrManager.Instance.GetChallenge(ch).brand_id;
                app_id = MonetizrManager.Instance.GetChallenge(ch).application_id;
                brand_name = MonetizrManager.Instance.GetAsset<string>(ch, AssetsType.BrandTitleString);
                campaign_id = ch;
            }

            var props = new Value();
            props["application_id"] = app_id;
            props["bundle_id"] = Application.identifier;
            props["player_id"] = SystemInfo.deviceUniqueIdentifier;
            props["application_name"] = Application.productName;
            props["application_version"] = Application.version;
            //props["campaign_id"] = campaign_id;
            props["camp_id"] = campaign_id;
            props["brand_id"] = brand_id;
            props["brand_name"] = brand_name;

            Mixpanel.Track(eventName, props);

#if USING_AMPLITUDE
            Dictionary<string, object> eventProps = new Dictionary<string, object>();
            eventProps.Add("camp_id", campaign_id);
            eventProps.Add("brand_id", brand_id);
            eventProps.Add("brand_name", brand_name);
     
            amplitude.logEvent(eventName, eventProps);
#endif
        }

        public void OnApplicationQuit()
        {
            foreach (var ad in visibleAdAsset)
            {
                _EndShowAdAsset(ad.Key);
            }

            Mixpanel.Flush();
        }

        /// <summary>
        /// Updates <see cref="challengesWithTimes"/> with <paramref name="challenges"/>. 
        /// If a challenge has already been registered in a previous Update, its times will remain unchanged.
        /// If a challenge no longer exists in <paramref name="challenges"/> it will be removed.
        /// </summary>
        //public void Update(List<Challenge> challenges)
        //{
        //    Dictionary<string, ChallengeTimes> updatedChallengesWithTimes = new Dictionary<string, ChallengeTimes>();

        //    foreach (Challenge challenge in challenges)
        //    {
        //        if (challengesWithTimes.ContainsKey(challenge.id))
        //        {
        //            updatedChallengesWithTimes.Add(challenge.id, challengesWithTimes[challenge.id]);
        //        }
        //        else
        //        {
        //            int currentTime = Mathf.FloorToInt(Time.realtimeSinceStartup);
        //            updatedChallengesWithTimes.Add(challenge.id, new ChallengeTimes(currentTime, currentTime));
        //        }
        //    }

        //    challengesWithTimes = new Dictionary<string, ChallengeTimes>(updatedChallengesWithTimes);
        //}

        /// <summary>
        /// Returns time in seconds between now and time when <paramref name="challenge"/> first appeared in <see cref="Update(List{Challenge})"/>
        /// </summary>
        //public int GetElapsedTime(Challenge challenge)
        //{
        //    if (!challengesWithTimes.ContainsKey(challenge.id))
        //    {
        //        Debug.LogError("Challenge: " + challenge.title + " was not added to analytics");
        //        return -1;
        //    }

        //    return Mathf.FloorToInt(Time.realtimeSinceStartup) - challengesWithTimes[challenge.id].firstSeenTime;
        //}

        /// <summary>
        /// Returns time in seconds between now and last time <paramref name="challenge"/> status update was marked.
        /// </summary>
        //public int GetTimeSinceLastUpdate(Challenge challenge)
        //{
        //    if (!challengesWithTimes.ContainsKey(challenge.id))
        //    {
        //        Debug.LogError("Challenge: " + challenge.title + " was not added to analytics");
        //        return -1;
        //    }

        //    return Mathf.FloorToInt(Time.realtimeSinceStartup) - challengesWithTimes[challenge.id].lastUpdateTime;
        //}

        /// <summary>
        /// Sets last update time for <paramref name="challenge"/> to current time.
        /// If time since last update exceeds 24h, resets first time seen to 0.
        /// </summary>
        //public void MarkChallengeStatusUpdate(Challenge challenge)
        //{
        //    if (!challengesWithTimes.ContainsKey(challenge.id))
        //    {
        //        Debug.LogError("Challenge: " + challenge.title + " was not added to analytics");
        //        return;
        //    }

        //    if (GetTimeSinceLastUpdate(challenge) > SECONDS_IN_DAY)
        //    {
        //        challengesWithTimes[challenge.id].firstSeenTime = Mathf.FloorToInt(Time.realtimeSinceStartup);
        //    }

        //    challengesWithTimes[challenge.id].lastUpdateTime = Mathf.FloorToInt(Time.realtimeSinceStartup);
        //}

        //private class ChallengeTimes
        //{
        //    public ChallengeTimes(int firstSeenTime, int lastUpdateTime)
        //    {
        //        this.firstSeenTime = firstSeenTime;
        //        this.lastUpdateTime = lastUpdateTime;
        //    }

        //    public int firstSeenTime;
        //    public int lastUpdateTime;
        //}
    }



}
