using Monetizr.Challenges;
using UnityEngine;

public class SponsoredPickupCoinMission : MissionBase
{
    private int _previousCoinAmount;
    public Challenge challenge { get; private set; }

    private float _lastUpdate;

    public override string GetMissionDesc()
    {
        return challenge == null ? string.Empty : challenge.title;
    }

    public override string GetMissionContent()
    {
        return challenge == null ? string.Empty : challenge.content;
    }

    public override void Created()
    {
        
    }
    
    public void Created(Challenge sponsoredChallenge)
    {
        Debug.Log("Sponsored mission was created.");
        
        challenge = sponsoredChallenge;
        
        max = 100; // This is the max value needed to be collected.

        if (challenge.reward == 0) // in-game money
            reward = 5;

        progress = challenge.progress;

        _lastUpdate = Time.time;
    }

    public override MissionType GetMissionType()
    {
        return MissionType.PICKUP;
    }

    public override void RunStart(TrackManager manager)
    {
        _previousCoinAmount = 0;
    }

    public override void Update(TrackManager manager)
    {
        int coins = manager.characterController.coins - _previousCoinAmount;
        progress += coins;
        challenge.progress += coins;
        
        _previousCoinAmount = manager.characterController.coins;
        
        // Here I would need to update the progress to API
        // well I should update the Challenge object here.
        // manager.challengesApi.UpdateChallenge(_challenge);
        // Temporary update progress every 2 seconds.
        if (!(Time.time > _lastUpdate + 2f)) return;

        _lastUpdate = Time.time;

        //SponsoredMissionsManager.instance.UpdateMission(challenge, Mathf.RoundToInt(progress));
    }
}