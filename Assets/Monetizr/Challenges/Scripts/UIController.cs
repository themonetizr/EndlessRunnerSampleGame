using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Monetizr.Challenges
{

    public enum PanelId
    {
        Unknown = -1,
        Notification,
        RewardCenter,
    }

    public class UIController
    {
        private PanelId previousPanel;

        public Dictionary<PanelId, PanelController> panels = null;

        public UIController()
        {
            var canvas = GameObject.Instantiate<GameObject>(Resources.Load("MonetizrCanvas") as GameObject);

            Assert.IsNotNull(canvas);

            var notifyPanel = GameObject.Instantiate<GameObject>(Resources.Load("MonetizrNotifyPanel") as GameObject, canvas.transform);
            var rewardPanel = GameObject.Instantiate<GameObject>(Resources.Load("MonetizrRewardCenterPanel") as GameObject, canvas.transform);

            Assert.IsNotNull(notifyPanel);
            Assert.IsNotNull(rewardPanel);

            previousPanel = PanelId.Unknown;

            panels = new Dictionary<PanelId, PanelController>();
            panels.Add(PanelId.Notification, notifyPanel.GetComponent<PanelController>());
            panels.Add(PanelId.RewardCenter, rewardPanel.GetComponent<PanelController>());

            foreach (var p in panels)
                p.Value.SetActive(false, true);
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

        public void ShowPanel(PanelId id = PanelId.Unknown, bool rememberPrevious = false)
        {
            Debug.Log("ShowPanel: " + id);

            if (previousPanel != PanelId.Unknown)
                panels[previousPanel].SetActive(false);

            panels[id].PreparePanel();

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