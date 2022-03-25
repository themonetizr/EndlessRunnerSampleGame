using System;
using System.Collections;
using Monetizr.Campaigns;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MissionUI : MonoBehaviour
{
    public RectTransform missionPlace;
    public AssetReference missionEntryPrefab;
    public AssetReference addMissionButtonPrefab;

    public Sprite defaultMissionIcon;
    public Sprite defaultRewardIcon;

    /*public IEnumerator Open()
    {
        gameObject.SetActive(true);

        foreach (Transform t in missionPlace)
            Addressables.ReleaseInstance(t.gameObject);

        for(int i = 0; i < PlayerData.instance.missions.Count; ++i)
        {
            if (PlayerData.instance.missions.Count > i)
            {
                AsyncOperationHandle op = missionEntryPrefab.InstantiateAsync();
                yield return op;
                if (op.Result == null || !(op.Result is GameObject))
                {
                    Debug.LogWarning(string.Format("Unable to load mission entry {0}.", missionEntryPrefab.Asset.name));
                    yield break;
                }
                MissionEntry entry = (op.Result as GameObject).GetComponent<MissionEntry>();
                entry.transform.SetParent(missionPlace, false);
                entry.FillWithMission(PlayerData.instance.missions[i], this);
            }
            else
            {
                AsyncOperationHandle op = addMissionButtonPrefab.InstantiateAsync();
                yield return op;
                if (op.Result == null || !(op.Result is GameObject))
                {
                    Debug.LogWarning(string.Format("Unable to load button {0}.", addMissionButtonPrefab.Asset.name));
                    yield break;
                }
                AdsForMission obj = (op.Result as GameObject)?.GetComponent<AdsForMission>();
                obj.missionUI = this;
                obj.transform.SetParent(missionPlace, false);
            }
        }
    }*/

    
    public void UpdateGameUI()
    {
        foreach (var m in PlayerData.instance.missions)
        {
            Action onClaimButtonPress = () => { Claim(m); };

            var missionIcon = defaultMissionIcon;
            var rewardIcon = defaultRewardIcon;

            MonetizrManager.RegisterUserDefinedMission(m.GetMissionType().ToString(), m.GetMissionDesc(), missionIcon, rewardIcon, m.reward, m.progress, onClaimButtonPress);
        }
    }

    public void CallOpen()
    {
        //gameObject.SetActive(true);
        //StartCoroutine(Open());

        UpdateGameUI();

        MonetizrManager.ShowRewardCenter();
    }

    public void Claim(MissionBase m)
    {
        PlayerData.instance.ClaimMission(m);

        UpdateGameUI();

        //// Rebuild the UI with the new missions
        //StartCoroutine(Open());
    }

    public void Close()
    {
        //gameObject.SetActive(false);
    }
}
