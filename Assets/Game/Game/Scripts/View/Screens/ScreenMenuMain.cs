using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{

    public class ScreenMenuMain : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            this.transform.Find("Language/Text").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.CodeLanguage;
            this.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.main.menu");

            this.transform.Find("PlaySinglePlayer").GetComponent<Button>().onClick.AddListener(PressedPlaySinglePlayer);
            this.transform.Find("PlayMultiplayer").GetComponent<Button>().onClick.AddListener(PressedPlayMultiplayer);
            this.transform.Find("Language").GetComponent<Button>().onClick.AddListener(SwitchLanguage);
        }

        private void SwitchLanguage()
        {
            if (LanguageController.Instance.CodeLanguage == "es")
            {
                LanguageController.Instance.CodeLanguage = "en";
            }
            else
            {
                LanguageController.Instance.CodeLanguage = "es";
            }
            this.transform.Find("Language/Text").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.CodeLanguage;
            this.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.main.menu");
        }

        private void PressedPlaySinglePlayer()
        {
            GameController.Instance.PlaySinglePlayer();
        }

        private void PressedPlayMultiplayer()
        {
            GameController.Instance.PlayMultiplayer();
        }


        void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S))
            {
                PressedPlaySinglePlayer();
            }
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.M))
            {
                PressedPlaySinglePlayer();
            }
        }
    }
}