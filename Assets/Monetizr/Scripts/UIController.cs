using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Monetizr.Campaigns
{

    internal enum PanelId
    {
        Unknown = -1,
        StartNotification,
        RewardCenter,
        CongratsNotification,
        SurveyWebView,
        VideoWebView,
        Html5WebView,
        TinyMenuTeaser,
        SurveyNotification,
    }

    internal class MissionUIDescription
    {
        internal string campaignId;
        internal int sponsoredId;
        internal bool isSponsored;
        internal string brandName;

        internal Sprite brandBanner;
        internal string missionTitle;
        internal string missionDescription;
        internal Sprite missionIcon;

        internal Sprite rewardIcon;
        internal int reward;
        internal float progress;
        internal Action onClaimButtonPress;
        internal Action<int> onUserDefinedClaim;
        internal Sprite brandLogo;
        internal Sprite brandRewardBanner;
        internal string rewardTitle;
        internal string surveyUrl;
    }

    internal class UIController
    {
        public List<MissionUIDescription> missionsDescriptions = new List<MissionUIDescription>();

        private GameObject mainCanvas;
        private PanelId previousPanel;


        public Dictionary<PanelId, PanelController> panels = null;
        public bool isVideoPlaying;

        public UIController()
        {
            var resCanvas = Resources.Load("MonetizrCanvas");

            Assert.IsNotNull(resCanvas);

            mainCanvas = GameObject.Instantiate<GameObject>(resCanvas as GameObject);

            GameObject.DontDestroyOnLoad(mainCanvas);

            Assert.IsNotNull(mainCanvas);

            previousPanel = PanelId.Unknown;

            panels = new Dictionary<PanelId, PanelController>();
        }

        internal void CleanUserDefinedMissions()
        {
            //if (missionsDescriptions == null)
            //    missionsDescriptions = new List<MissionUIDescription>();
                        
            missionsDescriptions.RemoveAll((e) => { return e.isSponsored == false;  });
        }

        /*public int HasDuplicateSponsoredMissionWithId(MissionUIDescription m2)
        {
            if (!m2.isSponsored)
                return -1;
                    
            for (int i = 0; i < missionsDescriptions.Count; i++)
            {
                var m = missionsDescriptions[i];
                                
                if (m.isSponsored && m.sponsoredId == m2.sponsoredId)
                {
                    return i;
                }
            }

            return -1;
        }*/

        public void AddMission(MissionUIDescription m)
        {
            /*int i = HasDuplicateSponsoredMissionWithId(m);

            if(i >= 0)
            {
                missionsDescriptions.RemoveAt(i);
            }*/

            missionsDescriptions.Add(m);
        }

        internal void AddMissionAndBindToCampaign(MissionUIDescription sponsoredMission)
        {
            //bind to server campagns
            var challenges = MonetizrManager.Instance.GetAvailableChallenges();

            //check already binded campaigns
            HashSet<string> bindedCampaigns = new HashSet<string>();
            missionsDescriptions.ForEach((MissionUIDescription _m) => { if (_m.campaignId != null) bindedCampaigns.Add(_m.campaignId); });

            var activeChallenge = MonetizrManager.Instance.GetActiveChallenge();

            //bind to active challenge first
            challenges.Remove(activeChallenge);
            challenges.Insert(0, activeChallenge);

            //search unbinded campaign
            foreach (string ch in challenges)
            {
                if (bindedCampaigns.Contains(ch))
                    continue;
                
                sponsoredMission.campaignId = ch;
                sponsoredMission.surveyUrl = MonetizrManager.Instance.GetAsset<string>(ch, AssetsType.SurveyURLString);

                break;
            }

            missionsDescriptions.Add(sponsoredMission);
        }

        public void PlayVideo(String path, Action<bool> onComplete)
        {
            isVideoPlaying = true;

            MonetizrManager.HideRewardCenter();

            var prefab = GameObject.Instantiate<GameObject>(Resources.Load("MonetizrVideoPlayer") as GameObject, mainCanvas.transform);

            var player = prefab.GetComponent<MonetizrVideoPlayer>();

            player.Play(path, (bool isSkip) => {
                
                    onComplete?.Invoke(isSkip);

                    GameObject.Destroy(prefab);
                    isVideoPlaying = false;
            } );
        }

        /*public GameObject GetActiveElement(PanelId id, string name)
        {
            return panels[id].GetActiveElement(name);
        }

        public GameObject GetElement(PanelId id, string name)
        {
            return panels[id].GetElement(name);
        }

        public GameObject GetElement(string name)
        {
            return GetActiveElement(previousPanel, name);
        }

        public GameObject GetPanel(PanelId id)
        {
            return panels[id].gameObject;
        }

        public void EnableInput(PanelId id, bool enable)
        {
            panels[id].EnableInput(enable);
        }*/

        /*public void ShowPanel(PanelId id = PanelId.Unknown, Action onComplete = null, bool rememberPrevious = false)
        {
            Debug.Log("ShowPanel: " + id);

            if (previousPanel != PanelId.Unknown)
                panels[previousPanel].SetActive(false);
                        
            Action _onComplete = () => {
                onComplete?.Invoke();
            };

            panels[id].PreparePanel(id, _onComplete);

            //moving to the top
            panels[id].gameObject.transform.SetSiblingIndex(panels[id].gameObject.transform.parent.childCount-1);

            panels[id].SetActive(true);

            if (rememberPrevious)
                previousPanel = id;
        }*/

        public void ShowPanelFromPrefab(String prefab, PanelId id = PanelId.Unknown, Action<bool> onComplete = null, bool rememberPrevious = false, MissionUIDescription m = null)
        {
            Debug.Log($"ShowPanel: {id} Mission: {m==null}");

            if (panels.ContainsKey(previousPanel) && previousPanel != PanelId.Unknown)
                panels[previousPanel].SetActive(false);

            var panel = GameObject.Instantiate<GameObject>(Resources.Load(prefab) as GameObject, mainCanvas.transform);

            Action<bool> complete = (bool isSkipped) =>
                {
                    onComplete?.Invoke(isSkipped);
                    GameObject.Destroy(panel);

                    panels.Remove(id);
                };


            var ctrlPanel = panel.GetComponent<PanelController>();

            ctrlPanel.uiController = this;

            ctrlPanel.PreparePanel(id, complete, m);

            ctrlPanel.SetActive(true);

            if (rememberPrevious)
               previousPanel = id;

            panels.Add(id, ctrlPanel);
        }

        public void ShowTinyMenuTeaser(Vector2 screenPos, Action UpdateGameUI)
        {
             MonetizrMenuTeaser teaser;

            if (!panels.ContainsKey(PanelId.TinyMenuTeaser))
            {
                var obj = GameObject.Instantiate<GameObject>(Resources.Load("MonetizrMenuTeaser") as GameObject, mainCanvas.transform);
                teaser = obj.GetComponent<MonetizrMenuTeaser>();
                panels.Add(PanelId.TinyMenuTeaser, teaser);
                teaser.button.onClick.AddListener(() => { MonetizrManager.ShowRewardCenter(UpdateGameUI); });

                if (screenPos != null)
                {
                    teaser.rectTransform.anchoredPosition = screenPos;
                }
            }
            else
            {
                teaser = panels[PanelId.TinyMenuTeaser] as MonetizrMenuTeaser;
            }

            if (teaser.IsVisible())
                return;

            teaser.PreparePanel(PanelId.TinyMenuTeaser, null, null);

            //previousPanel = PanelId.TinyMenuTeaser;

            teaser.SetActive(true);
        }

        public void HidePanel(PanelId id = PanelId.Unknown)
        {
            if (id == PanelId.Unknown && previousPanel != PanelId.Unknown)
                id = previousPanel;

            if(panels.ContainsKey(id))
                panels[id].SetActive(false);

        }

      
    }


}