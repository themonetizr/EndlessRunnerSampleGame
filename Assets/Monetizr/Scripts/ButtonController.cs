using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{
    public enum ButtonType
    {

    }

    [RequireComponent(typeof(Button))]
    public class ButtonController : MonoBehaviour
    {
        public RewardCenterPanel clickReceiver;
        public MissionUIDescription missionDescription;
        public ButtonType buttonType;
        Button button;
        public int id = 0;

        void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClicked);
        }

        void OnButtonClicked()
        {
            Debug.Log($"Clicked: {buttonType} id: {id}");

            clickReceiver.ButtonPressed(this, missionDescription);
            //GameState.GetInstance().ButtonPress(buttonType,id);       
        }
    }

}