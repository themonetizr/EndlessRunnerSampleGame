using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ButtonType
{
    MainMenu_StartButton,
    RewardScreen_ClaimX2Button,
    RewardScreen_NoThanksButton,
    RewardScreen_GalleryButton,
    GamePlay_Restart,
    RewardScreen_TryAgain,
    GamePlay_GoToLevelsSelector,
    LevelsSelector_Level,
    PauseMenu_Close,
    PauseMenu_GoToLevelsSelector,
    Gallery_PlayButton,
    GamePlay_Hint,
}

[RequireComponent(typeof(Button))]
public class ButtonController : MonoBehaviour
{
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

        //GameState.GetInstance().ButtonPress(buttonType,id);       
    }
}
