using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{

    public class ScreenWin : MonoBehaviour
    {
        public TextMeshProUGUI EnemiesKilled;
        public TextMeshProUGUI CoinsCollected;

        void Start()
        {
            this.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.win");

            this.transform.Find("GoToNextLevel").GetComponent<Button>().onClick.AddListener(PressedGoToNextLevel);
            this.transform.Find("GoToMainMenu").GetComponent<Button>().onClick.AddListener(PressedGoToMainMenu);

            EnemiesKilled.text = "Enemies Killed = " + GameController.Instance.CounterDeadEnemies;
            CoinsCollected.text = "Collected Coins = " + GameController.Instance.CounterCollectedCoins;
        }

        private void PressedGoToMainMenu()
        {
            GameController.Instance.UserHasPressedReloadGame();
        }

        private void PressedGoToNextLevel()
        {
            GameController.Instance.PressedGoToNextLevel();
        }
    }
}