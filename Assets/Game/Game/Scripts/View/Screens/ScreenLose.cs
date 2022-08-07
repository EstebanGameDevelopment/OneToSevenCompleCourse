using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{

    public class ScreenLose : MonoBehaviour
    {
        void Start()
        {
            this.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.lose");

            this.transform.Find("GoToNextLevel").GetComponent<Button>().onClick.AddListener(PressedReloadCurrentLevel);
            this.transform.Find("GoToMainMenu").GetComponent<Button>().onClick.AddListener(PressedGoToMainMenu);
        }

        private void PressedGoToMainMenu()
        {
            GameController.Instance.UserHasPressedReloadGame();
        }

        private void PressedReloadCurrentLevel()
        {
            GameController.Instance.PressedGoToNextLevel();
        }
    }
}