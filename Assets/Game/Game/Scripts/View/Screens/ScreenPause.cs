using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{

    public class ScreenPause : MonoBehaviour
    {
        void Start()
        {
            this.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.pause");

            this.transform.Find("ButtonResume").GetComponent<Button>().onClick.AddListener(PressedResumeGame);
            this.transform.Find("ButtonReload").GetComponent<Button>().onClick.AddListener(PressedGoToMainMenu);
        }

        private void PressedGoToMainMenu()
        {
            GameController.Instance.UserHasPressedReloadGame();
        }

        private void PressedResumeGame()
        {
            GameController.Instance.UserHasPressedReturnGame();
        }
    }
}