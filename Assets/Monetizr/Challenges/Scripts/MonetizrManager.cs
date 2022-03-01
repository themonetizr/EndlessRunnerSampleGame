using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monetizr.Challenges;
using UnityEngine.Networking;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Assertions;

namespace Monetizr.Challenges
{
    public enum ErrorType
    {
        NotinitializedSDK,
        SimultaneusAdAssets,
        AdAssetStillShowing,
    };

    public static class MonetizrErrors
    {
        public static readonly Dictionary<ErrorType, string> msg = new Dictionary<ErrorType, string>()
        {
            { ErrorType.NotinitializedSDK, "You're trying to use Monetizer SDK before it's been initialized. Call MonetizerManager.Initalize first." },
            { ErrorType.SimultaneusAdAssets, "Simultaneous display of multiple ads is not supported!" },
            { ErrorType.AdAssetStillShowing, "Some ad asset are still showing." }

        };
    }

    /// <summary>
    /// Predefined asset types for easier access
    /// </summary>
    public enum AssetsType
    {
        Unknown,
        BrandLogoSprite, //icon
        BrandBannerSprite, //banner
        BrandRewardLogoSprite, //logo
        BrandRewardBannerSprite, //reward_banner
        SurveyURLString, //survey
        VideoURLString, //video url
        VideoFilePathString, //video url
        BrandTitleString, //text

    }

    /// <summary>
    /// ChallengeExtention for easier access to Challenge assets
    /// TODO: merge with Challenge
    /// </summary>
    public class ChallengeExtention
    {
        private static readonly Dictionary<AssetsType, System.Type> AssetsSystemTypes = new Dictionary<AssetsType, System.Type>()
        {
            { AssetsType.BrandLogoSprite, typeof(Sprite) },
            { AssetsType.BrandBannerSprite, typeof(Sprite) },
            { AssetsType.BrandRewardLogoSprite, typeof(Sprite) },
            { AssetsType.BrandRewardBannerSprite, typeof(Sprite) },
            { AssetsType.SurveyURLString, typeof(String) },
            { AssetsType.VideoURLString, typeof(String) },
            { AssetsType.VideoFilePathString, typeof(String) },
            { AssetsType.BrandTitleString, typeof(String) },

        };

        public Challenge challenge { get; private set; }
        private Dictionary<AssetsType, object> assets = new Dictionary<AssetsType, object>();

        public ChallengeExtention(Challenge challenge)
        {
            this.challenge = challenge;
        }

        public void SetAsset<T>(AssetsType t, object asset)
        {
            assets.Add(t, asset);
        }

        public T GetAsset<T>(AssetsType t)
        {
            if (AssetsSystemTypes[t] != typeof(T))
                throw new ArgumentException($"AssetsType {t} and {typeof(T)} do not match!");

            if (!assets.ContainsKey(t))
                throw new ArgumentException($"Requested asset {t} doesn't exist in challenge!");

            return (T)Convert.ChangeType(assets[t], typeof(T));
        }

    }

    /// <summary>
    /// Extention to support async/await in the DownloadAssetData
    /// </summary>
    public static class ExtensionMethods
    {
        public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<object>();
            asyncOp.completed += obj => { tcs.SetResult(null); };
            return ((Task)tcs.Task).GetAwaiter();
        }
    }

    class DownloadHelper
    {
        /// <summary>
        /// Downloads any type of asset and returns its data as an array of bytes
        /// </summary>
        public static async Task<byte[]> DownloadAssetData(string url, Action onDownloadFailed = null)
        {
            UnityWebRequest uwr = UnityWebRequest.Get(url);

            await uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                Debug.LogError(uwr.error);
                onDownloadFailed?.Invoke();
                return null;
            }

            return uwr.downloadHandler.data;
        }
    }

    /// <summary>
    /// Main manager for Monetizr
    /// </summary>
    public class MonetizrManager : MonoBehaviour
    {
        public ChallengesClient _challengesClient { get; private set; }

        private static MonetizrManager instance = null;

        private UIController uiController = null;

        //Storing ids in separate list to get faster access (the same as Keys in challenges dictionary below)
        private List<string> challengesId = new List<string>();
        private Dictionary<String, ChallengeExtention> challenges = new Dictionary<String, ChallengeExtention>();

        public static MonetizrManager Initialize(string apiKey, Action<bool> onRequestComplete)
        {
            if (instance != null)
            {
                //instance.RequestChallenges(onRequestComplete);
                return instance;
            }

            Debug.Log("MonetizrManager Initialize");

            var monetizrObject = new GameObject("MonetizrManager");
            var monetizrManager = monetizrObject.AddComponent<MonetizrManager>();

            DontDestroyOnLoad(monetizrObject);
            instance = monetizrManager;

            monetizrManager.Initalize(apiKey, onRequestComplete);

            return instance;
        }

        public static MonetizrManager Instance
        {
            get
            {   
                return instance;
            }
        }

        public static MonetizrAnalytics Analytics
        {
            get
            {
                return instance._challengesClient.analytics;
            }
        }

        void OnApplicationQuit()
        {
            Analytics.Flush();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        private void Initalize(string apiKey, Action<bool> onRequestComplete)
        {
            _challengesClient = new ChallengesClient(apiKey)
            {
                playerInfo = new PlayerInfo("monetizr_mj")
            };

            RequestChallenges(
                (bool isOk) =>
                {
                    InitializeUI();

                    onRequestComplete(isOk);
                });
        }

        private void InitializeUI()
        {
            uiController = new UIController();
        }

        internal static void ShowStartupNotification(Action onComplete)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            instance.uiController.ShowPanelFromPrefab("MonetizrNotifyPanel",PanelId.StartNotification, onComplete, true);
        }

        internal static void ShowCongratsNotification(Action onComplete)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            instance.uiController.ShowPanelFromPrefab("MonetizrNotifyPanel", PanelId.CongratsNotification, onComplete, true);
        }

        internal static void AddUserDefinedMission(string missionTitle, string missionDescription, Sprite missionIcon, Sprite rewardIcon, int reward, float progress, Action onClaimButtonPress)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            MissionUIDescription m = new MissionUIDescription(missionTitle, missionDescription, missionIcon, rewardIcon, reward, progress, onClaimButtonPress);
            instance.uiController.AddMission(m);
        }

        internal static void ShowRewardCenter(Action onComplete)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            instance.uiController.ShowPanelFromPrefab("MonetizrRewardCenterPanel", PanelId.RewardCenter, onComplete, true);
        }

        internal static void HideRewardCenter()
        {
            instance.uiController.HidePanel(PanelId.RewardCenter);
        }

        internal static void ShowSurvey(Action onComplete)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            instance.uiController.ShowPanelFromPrefab("MonetizrSurveyPanel", PanelId.Survey, onComplete);
        }

        public static void ShowTinyMenuTeaser(Action onTap)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            instance.uiController.ShowTinyMenuTeaser(onTap);
        }

        public static void HideTinyMenuTeaser()
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            instance.uiController.HidePanel(PanelId.TinyMenuTeaser);
        }


        //TODO: shouldn't have possibility to show video directly by game
        internal static void _PlayVideo(Action<bool> onComplete)
        {
            instance.uiController.PlayVideo(null, onComplete);
        }

        /// <summary>
        /// Helper function to download and assign graphics assets
        /// </summary>
        private async Task AssignAssetTextures(ChallengeExtention ech, Challenge.Asset asset, AssetsType texture, AssetsType sprite)
        {
            string path = Application.persistentDataPath + "/" + ech.challenge.id;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fname = Path.GetFileName(asset.url);
            string fpath = path + "/" + fname;

            Debug.Log(fname);

            byte[] data = null;

            if (!File.Exists(fpath))
            {
                data = await DownloadHelper.DownloadAssetData(asset.url);

                File.WriteAllBytes(fpath, data);

                Debug.Log("saving: " + fpath);
            }
            else
            {
                data = File.ReadAllBytes(fpath);

                Debug.Log("reading: " + fpath);
            }

            Texture2D tex = new Texture2D(0, 0);
            tex.LoadImage(data);

            Sprite s = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

            if (texture != AssetsType.Unknown)
                ech.SetAsset<Texture2D>(texture, tex);

            if (sprite != AssetsType.Unknown)
                ech.SetAsset<Sprite>(sprite, s);
        }

        private async Task PreloadAssetToCache(ChallengeExtention ech, Challenge.Asset asset)
        {
            string path = Application.persistentDataPath + "/" + ech.challenge.id;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fname = Path.GetFileName(asset.url);
            string fpath = path + "/" + fname;

            Debug.Log(fname);

            byte[] data = null;

            if (!File.Exists(fpath))
            {
                data = await DownloadHelper.DownloadAssetData(asset.url);

                File.WriteAllBytes(fpath, data);



                Debug.Log("saving: " + fpath);
            }

            ech.SetAsset<string>(AssetsType.VideoURLString, asset.url);
            ech.SetAsset<string>(AssetsType.VideoFilePathString, fpath);
        }

        /// <summary>
        /// Request challenges from the server
        /// </summary>
        public async void RequestChallenges(Action<bool> onRequestComplete)
        {
            //TODO: add connection error tracking
            var challenges = await _challengesClient.GetList();

            foreach (var ch in challenges)
            {
                var ech = new ChallengeExtention(ch);

                if (this.challenges.ContainsKey(ch.id))
                    continue;

                foreach (var asset in ch.assets)
                {
                    switch (asset.type)
                    {
                        case "icon":
                            await AssignAssetTextures(ech, asset, AssetsType.Unknown, AssetsType.BrandLogoSprite);

                            break;
                        case "banner":
                            await AssignAssetTextures(ech, asset, AssetsType.Unknown, AssetsType.BrandBannerSprite);

                            break;
                        case "logo":
                            await AssignAssetTextures(ech, asset, AssetsType.Unknown, AssetsType.BrandRewardLogoSprite);

                            break;
                        case "reward_banner":
                            await AssignAssetTextures(ech, asset, AssetsType.Unknown, AssetsType.BrandRewardBannerSprite);

                            break;
                        case "survey":
                            ech.SetAsset<string>(AssetsType.SurveyURLString, asset.url);

                            break;
                        case "video":
                            await PreloadAssetToCache(ech, asset);

                            break;
                        case "text":
                            ech.SetAsset<string>(AssetsType.BrandTitleString, asset.title);

                            break;


                    }

                }

                this.challenges.Add(ch.id, ech);


                challengesId.Add(ch.id);
            }

            Debug.Log("RequestChallenges completed with count: " + challenges.Count);

            //TODO: Check error state
            onRequestComplete?.Invoke(true);
        }

        /// <summary>
        /// Get Challenge by Id
        /// TODO: Don't give access to challenge itself, update progress internally
        /// </summary>
        /// <returns></returns>
        [Obsolete("This Method is obsolete and don't recommended for use")]
        public Challenge GetChallenge(String chId)
        {
            return challenges[chId].challenge;
        }

        /// <summary>
        /// Get list of the available challenges
        /// </summary>
        /// <returns></returns>
        public List<string> GetAvailableChallenges()
        {
            return challengesId;
        }

        /// <summary>
        /// Get Asset from the challenge
        /// </summary>
        public T GetAsset<T>(String challengeId, AssetsType t)
        {
            return challenges[challengeId].GetAsset<T>(t);
        }

        /// <summary>
        /// Single update for reward and claim
        /// </summary>
        public async void ClaimReward(String challengeId)
        {
            var challenge = challenges[challengeId].challenge;

            try
            {
                /*if (progress < 100)
                {
                    await _challengesClient.UpdateStatus(challenge, progress);
                }
                else
                {
                    await _challengesClient.Claim(challenge);
                }*/

                await _challengesClient.Claim(challenge);
            }
            catch (Exception e)
            {
                Debug.Log($"An error occured: {e.Message}");
            }
        }


    }

}