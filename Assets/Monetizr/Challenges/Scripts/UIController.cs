using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Monetizr.Challenges
{

    public enum PanelId
    {
        Unknown = -1,
        StartNotification,
        RewardCenter,
        CongratsNotification,
        Survey,
        TinyMenuTeaser
    }

    public class MissionUIDescription
    {
        public bool isSponsored;

        public Sprite brandBanner;
        public string missionTitle;
        public string missionDescription;
        public Sprite missionIcon;

        public Sprite rewardIcon;
        public int reward;
        public float progress;
        public Action onClaimButtonPress;

        public MissionUIDescription()
        {
        }

        public MissionUIDescription(string missionTitle,
                                    string missionDescription,
                                    Sprite missionIcon,
                                    Sprite rewardIcon,
                                    int reward,
                                    float progress,
                                    Action onClaimButtonPress)
        {
            /*if (string.IsNullOrEmpty(missionTitle))
            {
                throw new ArgumentException($"'{nameof(missionTitle)}' cannot be null or empty", nameof(missionTitle));
            }

            if (string.IsNullOrEmpty(missionDescription))
            {
                throw new ArgumentException($"'{nameof(missionDescription)}' cannot be null or empty", nameof(missionDescription));
            }*/

            this.isSponsored = false;

            this.brandBanner = null;
            this.missionTitle = missionTitle;
            this.missionDescription = missionDescription;
            this.missionIcon = missionIcon;// ?? throw new ArgumentNullException(nameof(missionIcon));
            this.rewardIcon = rewardIcon;// ?? throw new ArgumentNullException(nameof(rewardIcon));
            this.reward = reward;
            this.progress = progress;
            this.onClaimButtonPress = onClaimButtonPress;
        }
    }

    public class UIController
    {
        private List<MissionUIDescription> missionsDescriptions = new List<MissionUIDescription>();

        private GameObject mainCanvas;
        private PanelId previousPanel;


        public Dictionary<PanelId, PanelController> panels = null;

        public UIController()
        {
            var resCanvas = Resources.Load("MonetizrCanvas");

            Assert.IsNotNull(resCanvas);

            mainCanvas = GameObject.Instantiate<GameObject>(resCanvas as GameObject);

            GameObject.DontDestroyOnLoad(mainCanvas);

            Assert.IsNotNull(mainCanvas);

            /*var notifyPanel = GameObject.Instantiate<GameObject>(Resources.Load("MonetizrNotifyPanel") as GameObject, mainCanvas.transform);
            var rewardPanel = GameObject.Instantiate<GameObject>(Resources.Load("MonetizrRewardCenterPanel") as GameObject, mainCanvas.transform);

            Assert.IsNotNull(notifyPanel);
            Assert.IsNotNull(rewardPanel);*/

            previousPanel = PanelId.Unknown;

            panels = new Dictionary<PanelId, PanelController>();
            /*panels.Add(PanelId.RewardCenter, rewardPanel.GetComponent<PanelController>());
            panels.Add(PanelId.StartNotification, notifyPanel.GetComponent<PanelController>());
            panels.Add(PanelId.CongratsNotification, notifyPanel.GetComponent<PanelController>());
            */

            //foreach (var p in panels)
            //    p.Value.SetActive(false, true);
        }

        internal void CleanMissionsList()
        {
            //if (missionsDescriptions == null)
            //    missionsDescriptions = new List<MissionUIDescription>();

            missionsDescriptions.Clear();
        }

        public void AddMission(MissionUIDescription m)
        {
            missionsDescriptions.Add(m);
        }


        public void PlayVideo(String path, Action<bool> onComplete)
        {
            MonetizrManager.HideRewardCenter();

            var prefab = GameObject.Instantiate<GameObject>(Resources.Load("MonetizrVideoPlayer") as GameObject, mainCanvas.transform);

            var player = prefab.GetComponent<MonetizrVideoPlayer>();

            player.Play(path, (bool isSkip) => {

                    MonetizrManager.ShowRewardCenter(null);

                    onComplete?.Invoke(isSkip);
                    GameObject.Destroy(prefab);
            } );
        }

        public GameObject GetActiveElement(PanelId id, string name)
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
        }

        public void ShowPanel(PanelId id = PanelId.Unknown, Action onComplete = null, bool rememberPrevious = false)
        {
            Debug.Log("ShowPanel: " + id);

            if (previousPanel != PanelId.Unknown)
                panels[previousPanel].SetActive(false);

            if(id == PanelId.RewardCenter)
            {
                //CleanMissionsList();
            }

            Action _onComplete = () => {
                onComplete?.Invoke();
                missionsDescriptions.Clear();

                Debug.Log("missionsDescriptions clear");
            };

            panels[id].PreparePanel(id, _onComplete, missionsDescriptions);

            //moving to the top
            panels[id].gameObject.transform.SetSiblingIndex(panels[id].gameObject.transform.parent.childCount-1);

            panels[id].SetActive(true);

            if (rememberPrevious)
                previousPanel = id;
        }

        public void ShowPanelFromPrefab(String prefab, PanelId id = PanelId.Unknown, Action onComplete = null, bool rememberPrevious = false)
        {
            Debug.Log("ShowPanel: " + id);

            if (panels.ContainsKey(previousPanel) && previousPanel != PanelId.Unknown)
                panels[previousPanel].SetActive(false);

            var panel = GameObject.Instantiate<GameObject>(Resources.Load(prefab) as GameObject, mainCanvas.transform);

            Action complete = () =>
                {
                    onComplete?.Invoke();
                    GameObject.Destroy(panel);

                    panels.Remove(id);
                };


            var ctrlPanel = panel.GetComponent<PanelController>();

            ctrlPanel.PreparePanel(id, complete, missionsDescriptions);

            ctrlPanel.SetActive(true);

            if (rememberPrevious)
               previousPanel = id;

            panels.Add(id, ctrlPanel);
        }

        public void ShowTinyMenuTeaser(Action onTap)
        {
             MonetizrMenuTeaser teaser;

            if (!panels.ContainsKey(PanelId.TinyMenuTeaser))
            {
                var obj = GameObject.Instantiate<GameObject>(Resources.Load("MonetizrMenuTeaser") as GameObject, mainCanvas.transform);
                teaser = obj.GetComponent<MonetizrMenuTeaser>();
                panels.Add(PanelId.TinyMenuTeaser, teaser);
                teaser.button.onClick.AddListener(() => { onTap?.Invoke(); });
            }
            else
            {
                teaser = panels[PanelId.TinyMenuTeaser] as MonetizrMenuTeaser;
            }

            teaser.PreparePanel(PanelId.TinyMenuTeaser, null, null);

            //previousPanel = PanelId.TinyMenuTeaser;

            teaser.SetActive(true);
        }

        public void HideTinyMenuTeaser()
        {
            if (panels.ContainsKey(PanelId.TinyMenuTeaser))
            {
                panels[PanelId.TinyMenuTeaser].SetActive(false);
            }
        }


        public void HidePanel(PanelId id = PanelId.Unknown)
        {
            if (id == PanelId.Unknown && previousPanel != PanelId.Unknown)
                panels[previousPanel].SetActive(false);
            else
                panels[id].SetActive(false);

        }

      
    }


}