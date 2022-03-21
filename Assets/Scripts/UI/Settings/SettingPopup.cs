using Monetizr.Challenges;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingPopup : MonoBehaviour
{
    public AudioMixer mixer;

    public Slider masterSlider;
    public Slider musicSlider;
    public Slider masterSFXSlider;

    public InputField apiKeyInput;

    public Toggle monetizrToggle;

    public LoadoutState loadoutState;
    public DataDeleteConfirmation confirmationPopup;

    protected float m_MasterVolume;
    protected float m_MusicVolume;
    protected float m_MasterSFXVolume;

    protected const float k_MinVolume = -80f;
    protected const string k_MasterVolumeFloatName = "MasterVolume";
    protected const string k_MusicVolumeFloatName = "MusicVolume";
    protected const string k_MasterSFXVolumeFloatName = "MasterSFXVolume";

    public Dropdown monetizrCampaigns;

    public void Open()
    {
        gameObject.SetActive(true);
        UpdateUI();

        MonetizrManager.HideTinyMenuTeaser();
    }

    public void Close()
    {
		PlayerData.instance.Save ();
        gameObject.SetActive(false);

        MonetizrManager.ShowTinyMenuTeaser(null);
    }

    void UpdateUI()
    {
        mixer.GetFloat(k_MasterVolumeFloatName, out m_MasterVolume);
        mixer.GetFloat(k_MusicVolumeFloatName, out m_MusicVolume);
        mixer.GetFloat(k_MasterSFXVolumeFloatName, out m_MasterSFXVolume);

        masterSlider.value = 1.0f - (m_MasterVolume / k_MinVolume);
        musicSlider.value = 1.0f - (m_MusicVolume / k_MinVolume);
        masterSFXSlider.value = 1.0f - (m_MasterSFXVolume / k_MinVolume);

        MonetzirUpdateList();
    }

    public void DeleteData()
    {
        confirmationPopup.Open(loadoutState);
    }


    public void MasterVolumeChangeValue(float value)
    {
        m_MasterVolume = k_MinVolume * (1.0f - value);
        mixer.SetFloat(k_MasterVolumeFloatName, m_MasterVolume);
		PlayerData.instance.masterVolume = m_MasterVolume;
    }

    public void MusicVolumeChangeValue(float value)
    {
        m_MusicVolume = k_MinVolume * (1.0f - value);
        mixer.SetFloat(k_MusicVolumeFloatName, m_MusicVolume);
		PlayerData.instance.musicVolume = m_MusicVolume;
    }

    public void MasterSFXVolumeChangeValue(float value)
    {
        m_MasterSFXVolume = k_MinVolume * (1.0f - value);
        mixer.SetFloat(k_MasterSFXVolumeFloatName, m_MasterSFXVolume);
		PlayerData.instance.masterSFXVolume = m_MasterSFXVolume;
    }

    public void MonetzirUpdateList()
    {
        List<string> dropOptions = new List<string> ();

        monetizrCampaigns.ClearOptions();

        int selection = 0;

        for(int i = 0; i < MonetizrManager.Instance.GetAvailableChallenges().Count; i++)
        {
            string id = MonetizrManager.Instance.GetAvailableChallenges()[i];

            if (id == MonetizrManager.Instance.GetActiveChallenge())
                selection = i;

            dropOptions.Add(MonetizrManager.Instance.GetAsset<string>(id, AssetsType.BrandTitleString));            
        }

        monetizrCampaigns.AddOptions(dropOptions);

        monetizrCampaigns.value = selection;

    }

    public void MonetizrDropdownValueChanged(int change)
    {
        Debug.Log("MonetizrDropdownValueChanged: " + monetizrCampaigns.value);
                
        MonetizrManager.Instance.SetActiveChallengeId(MonetizrManager.Instance.GetAvailableChallenges()[monetizrCampaigns.value]);
    }

    // Invoked when the value of the text field changes.
    public void ValueChangeCheck()
    {
        //SponsoredMissionsManager.instance.changeAPIKey(apiKeyInput.text);
    }

    public void MonetizrToggleChange()
    {
        MonetizrManager.Instance.Enable(monetizrToggle.isOn);
    }
}
