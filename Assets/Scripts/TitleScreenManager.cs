using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text;
using System;

public class TileScreenButtons : MonoBehaviour
{
    public Button startButton;
    public Button loadButton;
    public TMP_InputField input;


    void Start()
    {
        loadButton.interactable = false;
        string saveFilePath = Application.persistentDataPath + "/PlayerData.json";
        if (File.Exists(saveFilePath))
        {
            SaveData save = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveFilePath));
            if (save != null)
            {
                loadButton.interactable = true;
                loadButton.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = "Load " + save.playerName;
            }
            else
            {
                loadButton.interactable = false;
            }
        }
    }

    public void NextScene()
    {
        SceneManager.LoadScene("SampleScene");
    }


    public void LoadButtonPressed()
    {
        PlayerPrefs.SetString("name", "LoadNameThatsLong");
        PlayerPrefs.Save();
        SceneManager.LoadScene("Tutorial");

    }

    public void SkipIntro()
    {
        PlayerPrefs.SetString("name", "Skipper");
        PlayerPrefs.Save();
        SceneManager.LoadScene("Tutorial");
    }
}
    