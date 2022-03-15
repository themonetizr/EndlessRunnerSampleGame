//#define TEST_SLOW_LATENCY

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
using System.IO.Compression;

namespace Monetizr.Challenges
{
    public enum ErrorType
    {
        NotinitializedSDK,
        SimultaneusAdAssets,
        AdAssetStillShowing,
        ConnectionError,
    };

    public static class MonetizrErrors
    {
        public static readonly Dictionary<ErrorType, string> msg = new Dictionary<ErrorType, string>()
        {
            { ErrorType.NotinitializedSDK, "You're trying to use Monetizer SDK before it's been initialized. Call MonetizerManager.Initalize first." },
            { ErrorType.SimultaneusAdAssets, "Simultaneous display of multiple ads is not supported!" },
            { ErrorType.AdAssetStillShowing, "Some ad asset are still showing." },
            { ErrorType.ConnectionError, "Connection error while getting list of campaigns!" }

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
        //VideoURLString, //video url
        VideoFilePathString, //video url
        BrandTitleString, //text
        TinyTeaserTexture, //text
        //Html5ZipURLString,
        Html5ZipFilePathString

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
            //{ AssetsType.VideoURLString, typeof(String) },
            { AssetsType.VideoFilePathString, typeof(String) },
            { AssetsType.BrandTitleString, typeof(String) },
            { AssetsType.TinyTeaserTexture, typeof(Texture2D) },
            //{ AssetsType.Html5ZipURLString, typeof(String) },
            { AssetsType.Html5ZipFilePathString, typeof(String) },

        };

        public Challenge challenge { get; private set; }
        private Dictionary<AssetsType, object> assets = new Dictionary<AssetsType, object>();

        public bool isChallengeLoaded;

        public ChallengeExtention(Challenge challenge)
        {
            this.challenge = challenge;
            this.isChallengeLoaded = true;
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

        private string activeChallengeId;

        private Action<bool> soundSwitch;

        private bool isActive = false;

        //Storing ids in separate list to get faster access (the same as Keys in challenges dictionary below)
        private List<string> challengesId = new List<string>();
        private Dictionary<String, ChallengeExtention> challenges = new Dictionary<String, ChallengeExtention>();

        public static MonetizrManager Initialize(string apiKey, Action<bool> onRequestComplete, Action<bool> soundSwitch)
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

            monetizrManager.Initalize(apiKey, onRequestComplete, soundSwitch);

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
            Analytics.OnApplicationQuit();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        private void Initalize(string apiKey, Action<bool> onRequestComplete, Action<bool> soundSwitch)
        {
            this.soundSwitch = soundSwitch;

            _challengesClient = new ChallengesClient(apiKey);

            InitializeUI();

            RequestChallenges(
                (bool isOk) =>
                {
                    if (isOk)
                        isActive = true;
                    
                    onRequestComplete(isOk);
                });
        }

        public void SoundSwitch(bool on)
        {
            soundSwitch?.Invoke(on);
        }

        private void InitializeUI()
        {
            uiController = new UIController();
        }

        internal static void ShowStartupNotification(Action onComplete)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            if (!instance.HasChallengesAndActive())
            {
                onComplete?.Invoke();
                return;
            }

            instance.uiController.ShowPanelFromPrefab("MonetizrNotifyPanel", PanelId.StartNotification, onComplete, true);
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

        internal static void AddSponsoredMission(int id, Sprite defaultRewardIcon, int rewardAmount, Action<int> onSponsoredClaim)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            MissionUIDescription m = new MissionUIDescription()
            {
                sponsoredId = id,
                rewardIcon = defaultRewardIcon,
                reward = rewardAmount,
                isSponsored = true,
                onUserDefinedClaim = onSponsoredClaim
            };
                       
            instance.uiController.AddMission(m);
        }

        internal static void CleanUserDefinedMissions()
        {
            instance.uiController.CleanMissionsList();
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

            if (!instance.isActive)
                return;

            instance.uiController.ShowPanelFromPrefab("MonetizrSurveyPanel", PanelId.Survey, onComplete);
        }

        public static void ShowTinyMenuTeaser(Action onTap)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            if (!instance.HasChallengesAndActive())
                return;

            instance.uiController.ShowTinyMenuTeaser(onTap);
        }

        public static void HideTinyMenuTeaser()
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            if (!instance.isActive)
                return;

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

                if (data == null)
                {
                    ech.isChallengeLoaded = false;
                    return;
                }

                File.WriteAllBytes(fpath, data);

                Debug.Log("saving: " + fpath);
            }
            else
            {
                data = File.ReadAllBytes(fpath);

                if (data == null)
                {
                    ech.isChallengeLoaded = false;
                    return;
                }

                Debug.Log("reading: " + fpath);
            }

#if TEST_SLOW_LATENCY
            await Task.Delay(1000);
#endif

            Texture2D tex = new Texture2D(0, 0);
            tex.LoadImage(data);

            if (texture != AssetsType.Unknown)
                ech.SetAsset<Texture2D>(texture, tex);

            if (sprite != AssetsType.Unknown)
            {
                Sprite s = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

                ech.SetAsset<Sprite>(sprite, s);
            }
        }

        private async Task PreloadAssetToCache(ChallengeExtention ech, Challenge.Asset asset, /*AssetsType urlString,*/ AssetsType fileString, bool required = true)
        {
            string path = Application.persistentDataPath + "/" + ech.challenge.id;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fname = Path.GetFileName(asset.url);
            string fpath = path + "/" + fname;
            string zipFolder = null;
            string fileToCheck = fpath;

            Debug.Log(fname);

            if (fname.Contains("zip"))
            {
                zipFolder = path + "/" + fname.Replace(".zip", "");
                fileToCheck = zipFolder + "/index.html";

                Debug.Log($"archive: {zipFolder} {fileToCheck} {File.Exists(fileToCheck)}");
            }

            byte[] data = null;

            if (!File.Exists(fileToCheck))
            {
                data = await DownloadHelper.DownloadAssetData(asset.url);

                if (data == null)
                {
                    if(required)
                        ech.isChallengeLoaded = false;

                    return;
                }

                File.WriteAllBytes(fpath, data);

                if (zipFolder != null)
                {
                    Debug.Log("extracting to: " + zipFolder);

                    if (!Directory.Exists(zipFolder))
                        Directory.CreateDirectory(zipFolder);

                    ZipFile.ExtractToDirectory(fpath, zipFolder, true);
                    
                    File.Delete(fpath);
                }
                            
                               
                Debug.Log("saving: " + fpath);
            }

            if(zipFolder != null)
                fpath = fileToCheck;

            Debug.Log("resource: " + fpath);

            //ech.SetAsset<string>(urlString, asset.url);
            ech.SetAsset<string>(fileString, fpath);
        }
              

        /// <summary>
        /// Request challenges from the server
        /// </summary>
        public async void RequestChallenges(Action<bool> onRequestComplete)
        {
            List<Challenge> challenges = new List<Challenge>();

            try {

                challenges = await _challengesClient.GetList();
            }
            catch (Exception)
            {
               Debug.Log(MonetizrErrors.msg[ErrorType.ConnectionError]);
               onRequestComplete?.Invoke(false);
            }

#if TEST_SLOW_LATENCY
            await Task.Delay(10000);
            Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
#endif

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

                        case "tiny_teaser":
                            await AssignAssetTextures(ech, asset, AssetsType.TinyTeaserTexture, AssetsType.Unknown);

                            break;

                        case "survey":
                            ech.SetAsset<string>(AssetsType.SurveyURLString, asset.url);

                            break;
                        case "video":
                            await PreloadAssetToCache(ech, asset, AssetsType.VideoFilePathString, true);

                            break;
                        case "text":
                            ech.SetAsset<string>(AssetsType.BrandTitleString, asset.title);

                            break;

                        case "html":
                            await PreloadAssetToCache(ech, asset, AssetsType.Html5ZipFilePathString,false);

                            break;


                    }

                }

                if (ech.isChallengeLoaded)
                {
                    this.challenges.Add(ch.id, ech);
                    challengesId.Add(ch.id);
                }
            }

            if (challengesId.Count > 0)
                activeChallengeId = challengesId[0];

#if TEST_SLOW_LATENCY
            Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
#endif

            Debug.Log($"RequestChallenges completed with count: {challenges.Count} active: {activeChallengeId}");

            //TODO: Check error state
            onRequestComplete?.Invoke(challengesId.Count > 0);
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

        public bool HasChallengesAndActive()
        {
            return isActive && challengesId.Count > 0;
        }

        public string GetActiveChallenge()
        {
            return activeChallengeId;
        }

        public void SetActiveChallengeId(string id)
        {
            activeChallengeId = id;
        }

        public void Enable(bool enable)
        {
            isActive = enable;
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