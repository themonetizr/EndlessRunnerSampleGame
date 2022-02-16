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
    }

    public class UIController
    {
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

            var notifyPanel = GameObject.Instantiate<GameObject>(Resources.Load("MonetizrNotifyPanel") as GameObject, mainCanvas.transform);
            var rewardPanel = GameObject.Instantiate<GameObject>(Resources.Load("MonetizrRewardCenterPanel") as GameObject, mainCanvas.transform);

            Assert.IsNotNull(notifyPanel);
            Assert.IsNotNull(rewardPanel);

            previousPanel = PanelId.Unknown;

            panels = new Dictionary<PanelId, PanelController>();
            panels.Add(PanelId.RewardCenter, rewardPanel.GetComponent<PanelController>());
            panels.Add(PanelId.StartNotification, notifyPanel.GetComponent<PanelController>());
            panels.Add(PanelId.CongratsNotification, notifyPanel.GetComponent<PanelController>());


            foreach (var p in panels)
                p.Value.SetActive(false, true);
        }

        public void PlayVideo(String path, Action<bool> onComplete)
        {
            var prefab = GameObject.Instantiate<GameObject>(Resources.Load("MonetizrVideoPlayer") as GameObject, mainCanvas.transform);

            var player = prefab.GetComponent<MonetizrVideoPlayer>();

            player.Play(path, (bool isSkip) => {
                    onComplete.Invoke(isSkip);
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

            panels[id].PreparePanel(id,onComplete);

            panels[id].SetActive(true);

            if (rememberPrevious)
                previousPanel = id;
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