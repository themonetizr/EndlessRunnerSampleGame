using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Monetizr.Challenges;

public class MissionsManager : MonoBehaviour
{
    [Serializable]
    public class Mission
    {
        public string title;
        public int reward;
        public int progress;
        public float duration;
        public bool completed;
        
        public bool isActive;
        public float startTime;

        [NonSerialized] public Image progressBar;
        [NonSerialized] public GameObject root;

        [NonSerialized] public string monetizrID;

        public Mission(string title, int reward, float duration, bool completed, string monetizrID)
        {
            this.title = title;
            this.reward = reward;
            this.duration = duration;
            this.completed = completed;
            this.monetizrID = monetizrID;
        }

        public void UpdateProgress(float time)
        {
            progressBar.fillAmount = time / duration;
        }
    }

    [Serializable]
    public class Missions
    {
        public List<Mission> ml = new List<Mission>();
    }

    public Missions missions = new Missions();

    public GameObject missionUIPrefab;
    public GameObject missionGroupRoot;
    public GameObject missionCompleteUI;

    float gameplayStartTime = 0;
    bool isGamePlayStarted = false;

    float playerStartMoveTime = 0;
    float playerTotalMoveLevelTime = 0;
    bool isPlayerMoving = false;

    public void Initialize()
    {
        //if no missions

        //if (!PlayerPrefs.HasKey("missions_list"))
        {
            CreateDefaultMissions();

            PlayerPrefs.SetString("missions_list", JsonUtility.ToJson(missions));
        }
        /*else
        {
            Debug.Log("key found: " + PlayerPrefs.GetString("missions_list"));

            var json = PlayerPrefs.GetString("missions_list");

            missions = JsonUtility.FromJson<Missions>(json);
        }*/

        CreateMissionsUI();
    }

    private void CreateMissionsUI()
    {
        foreach (var m in missions.ml)
        {
            m.root = GameObject.Instantiate(missionUIPrefab, missionGroupRoot.transform);

            m.root.transform.Find("Text").GetComponent<Text>().text = m.title;

            m.root.transform.Find("RewardText").GetComponent<Text>().text = $"move ball for {m.duration}sec and get {m.reward} coins!";


            m.progressBar = m.root.transform.Find("ProgressBar").GetComponent<Image>();
            var image = m.root.transform.Find("Image").GetComponent<Image>();

            if(m.monetizrID != null)
            {
                var sprite = MonetizrManager.Instance.GetAsset<Sprite>(m.monetizrID, AssetsType.BrandLogoSprite);

                m.progressBar.sprite = sprite;
                image.sprite = sprite;
            }
        }
    }

    private void CreateDefaultMissions()
    {
        missions.ml.Add(new Mission("GREAT START", 1, 1, false, null));
        //missions.ml.Add(new Mission("Play 30 sec", 30, 0, 30, false));
        //missions.ml.Add(new Mission("Play 200 sec", 200, 0, 200, false));

        if(MonetizrManager.Instance.GetAvailableChallenges().Count > 0)
        {
            foreach(var challengeID in MonetizrManager.Instance.GetAvailableChallenges())
            {
                var title = MonetizrManager.Instance.GetAsset<string>(challengeID, AssetsType.BrandTitleString);

                missions.ml.Add(new Mission(title, 2, 2, false, challengeID));
            }
        }


    }

    public void GamePlayStart()
    {
        Debug.Log("GamePlayStart");

        gameplayStartTime = Time.realtimeSinceStartup;
        isGamePlayStarted = true;

        foreach (var m in missions.ml)
        {
            m.startTime = Time.realtimeSinceStartup;
            m.isActive = true;
        }
    }

    public void GamePlayEnd()
    {
        Debug.Log("GamePlayEnd");

        PlayerEndMove();

        isGamePlayStarted = false;

        foreach (var m in missions.ml)
        {
            m.isActive = false;
        }
    }

    public void PlayerStartMove()
    {
        Debug.Log("PlayerStartMove");

        playerStartMoveTime = Time.realtimeSinceStartup;

        isPlayerMoving = true;
    }

    public void PlayerEndMove()
    {
        isPlayerMoving = false;

        Debug.Log("PlayerEndMove " + playerTotalMoveLevelTime);
    }

    private void MissionComplete(Mission m)
    {
        Debug.Log("Mission completed" + m.title);
        m.completed = true;
        m.root.SetActive(false);

        missionCompleteUI.SetActive(true);

        //GameManager.Instance.wallet.Amount += m.reward;

        if (m.monetizrID != null)
        {
            missionCompleteUI.transform.Find("CompleteWindow/Image").GetComponent<Image>().sprite =
                    MonetizrManager.Instance.GetAsset<Sprite>(m.monetizrID, AssetsType.BrandBannerSprite);

            MonetizrManager.Instance.ClaimReward(m.monetizrID);
        }
    }

    public void CloseMissionCompleteUI()
    {
        missionCompleteUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(isPlayerMoving)
        {
            playerTotalMoveLevelTime += (Time.realtimeSinceStartup - playerStartMoveTime);
            playerStartMoveTime = Time.realtimeSinceStartup;
        }


        if (!isGamePlayStarted)
            return;

        float time = playerTotalMoveLevelTime;

        foreach (var m in missions.ml)
        {
            if (m.completed || !m.isActive)
                continue;

            m.UpdateProgress(time);

            if (time > m.duration)
            {
                MissionComplete(m);
            }

        }


    }
}
