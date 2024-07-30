using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class Selector
{
    public Vector2 selectorPos;
    public Vector2 prevSelectorPos;
    public Vector2 selection;
    public Vector2[] selections;
    public Vector2 prevSelection;
    public Vector2[] prevSelections;
    public TMP_Text[][] textArray;
    public int width;
    public int height;
    public bool noYellow;
    public Selector(int x, int y)
    {
        selectorPos = Vector2.zero;
        selection = Vector2.zero;
        prevSelectorPos = Vector2.zero;
        prevSelection = Vector2.zero;
        textArray = new TMP_Text[y][];
        selections = new Vector2[0];
        prevSelections = new Vector2[0];
        width = x;
        height = y;
        for (int i = 0; i < textArray.Length; i++)
        {
            textArray[i] = new TMP_Text[x];
        }
    }
    public Selector(int[] x, int y)
    {
        selectorPos = Vector2.zero;
        prevSelectorPos = Vector2.zero;
        textArray = new TMP_Text[y][];
        selections = new Vector2[y];
        prevSelections = new Vector2[y];
        height = y;
        for (int i = 0; i < textArray.Length; i++)
        {
            textArray[i] = new TMP_Text[x[i]];
            selections[i] = new Vector2(0, i);
            prevSelections[i] = new Vector2(0, i);
        }
        width = textArray[0].Length;
    }
    public void SetData(Vector2[] data)
    {
        selections = new Vector2[data.Length];
        prevSelections = new Vector2[data.Length];
        for (int i = 0; i < textArray.Length; i++)
        {
            for (int j = 0; j < textArray[i].Length; j++)
            {
                textArray[i][j].color = Color.white;
            }
            selections[i] = data[i];
            prevSelections[i] = data[i];
            textArray[i][Mathf.RoundToInt(data[i].x)].color = Color.yellow;
        }
        UpdateSelector();
    }
    public void UpdateSelector()
    {
        textArray[Mathf.RoundToInt(selectorPos.y)][Mathf.RoundToInt(selectorPos.x)].text = "<mark color=#FFFFFF50 padding=15,15,15,15>" + textArray[Mathf.RoundToInt(selectorPos.y)][Mathf.RoundToInt(selectorPos.x)].text;
        if (prevSelectorPos != selectorPos)
        {
            textArray[Mathf.RoundToInt(prevSelectorPos.y)][Mathf.RoundToInt(prevSelectorPos.x)].text = textArray[Mathf.RoundToInt(prevSelectorPos.y)][Mathf.RoundToInt(prevSelectorPos.x)].text.Replace("<mark color=#FFFFFF50 padding=15,15,15,15>", "");
        }
        if (selections.Length == 0 && !noYellow)
        {
            textArray[Mathf.RoundToInt(selection.y)][Mathf.RoundToInt(selection.x)].color = Color.yellow;
            textArray[Mathf.RoundToInt(selection.y)][Mathf.RoundToInt(selection.x)].fontStyle = FontStyles.Bold;
            if (selection != prevSelection)
            {
                textArray[Mathf.RoundToInt(prevSelection.y)][Mathf.RoundToInt(prevSelection.x)].color = Color.white;
                textArray[Mathf.RoundToInt(prevSelection.y)][Mathf.RoundToInt(prevSelection.x)].fontStyle = FontStyles.Normal;
            }
        }
        else if (selections.Length > 0)
        {
            textArray[Mathf.RoundToInt(selections[Mathf.RoundToInt(selectorPos.y)].y)][Mathf.RoundToInt(selections[Mathf.RoundToInt(selectorPos.y)].x)].color = Color.yellow;
            if (selections[Mathf.RoundToInt(selectorPos.y)] != prevSelections[Mathf.RoundToInt(selectorPos.y)])
            {
                textArray[Mathf.RoundToInt(prevSelections[Mathf.RoundToInt(selectorPos.y)].y)][Mathf.RoundToInt(prevSelections[Mathf.RoundToInt(selectorPos.y)].x)].color = Color.white;
            }
            width = textArray[Mathf.RoundToInt(selectorPos.y)].Length;
        }

    }
}


public class MenuMapManager : MonoBehaviour
{
    public Selector menuSelector = new(1,7);
    public Selector optionSelector = new(new int[] {1,3,5,2,5,5,3},7);
    public Selector upgradeSelector = new(3, 3);
    public string[] menuChoices;
    readonly string[] upgChoices = new string[] { "Health", "Attack", "Defense", "Regen", "Crit", "Upg 6", "Upg 7", "Upg 8", "Upg 9" };
    public Canvas menu;
    public Canvas input;
    public Canvas options;
    public Canvas upgrade;
    public TMP_InputField typeBox;
    public TMP_Text saveText;
    
    public GameObject menuTextArray;
    public GameObject optionsTextArray;
    public GameObject upgradeTextArray;


    public TMP_Text defOpt;
    public TMP_Text defMenu;
    public Image defImg;
    public PlayerScript2D playerScript;
    void Start()
    {
        Canvas.ForceUpdateCanvases();   
        menuChoices = new string[] {"Completion Tracker", "Puzzle Records", "Options", "Leave Dungeon", "Save", "Load", "Save and Quit" };
        menu.GetComponent<Canvas>().enabled = false;
        options.GetComponent<Canvas>().enabled = false;
        upgrade.GetComponent<Canvas>().enabled = false;
        input.GetComponent<Canvas>().enabled = false;
        typeBox.DeactivateInputField(true);
        int count = 0;
        menuSelector.noYellow = true;
        upgradeSelector.noYellow = true;
        for (int i = 0; i < menuSelector.textArray.Length; i++)
        {
            for (int j = 0; j < menuSelector.textArray[i].Length; j++)
            {
                count += 1;
                TMP_Text text = Instantiate(defMenu, menuTextArray.transform);
                text.rectTransform.localPosition = new Vector2(0 * j, -90 * i + 175);
                text.name = count.ToString();
                text.text = menuChoices[count - 1];
                menuSelector.textArray[i][j] = text;
            }
        }
        count = 0;
        menuChoices = new string[] { "Back", "Hold to Run", "Toggle Between", "Hold to Walk", "Snail", "Slow", "Normal", "Fast", "Cheetah", "Spam Space", "Disabled", "Very In", "In", "Default", "Out", "Very Out", "0%", "25%", "50%", "75%", "100%", "Show Location", "Show Pieces Left", "None" };
        for (int i = 0; i < optionSelector.textArray.Length; i++)
        {
            for (int j = 0; j < optionSelector.textArray[i].Length; j++)
            {
                count += 1;
                TMP_Text text = Instantiate(defOpt, optionsTextArray.transform);
                if (j == 0)
                {
                    text.rectTransform.localPosition = new Vector2(0, -110 * i);
                    if (i != 4 && i != 2 && i != 5 && i != 3 && i != 6)
                    {
                        text.color = Color.yellow;
                    }
                }
                else
                {
                    text.rectTransform.localPosition = new Vector2(optionSelector.textArray[i][j-1].rectTransform.localPosition.x + optionSelector.textArray[i][j - 1].preferredWidth + 30, -110 * i);
                    if ((i == 2 && j == 3) || (i == 4 && j == 2) || (i == 5 && j == 2) || (i == 3 && j == 1) || (i == 6 && j == 1))
                    {
                        optionSelector.selections[i] = new Vector2(j,i);
                        text.color = Color.yellow;

                    }
                }
                text.name = count.ToString();
                text.text = menuChoices[count - 1];
                optionSelector.textArray[i][j] = text;
                
            }
        }
        count = 0;
        for (int i = 0; i < upgradeSelector.textArray.Length; i++)
        {
            for (int j = 0; j < upgradeSelector.textArray[i].Length; j++)
            {
                count++;
                TMP_Text text = Instantiate(defOpt, upgradeTextArray.transform);
                text.rectTransform.localPosition = new Vector2(350 * j - 500, -250 * i + 50);
                text.name = "Text" + (i * upgradeSelector.textArray[i].Length + j + 1).ToString();
                text.text = upgChoices[count - 1];
                upgradeSelector.textArray[i][j] = text;
            }
        }
        Destroy(defOpt);
        defMenu.gameObject.SetActive(false);
        playerScript.doneee.Item1 = true;
    }
    public void OpenMenu()
    {
        menu.GetComponent<Canvas>().enabled = true;
        playerScript.inMenu = true;
        saveText.text = "Last save: " + playerScript.save.playerName +  "\n" +  playerScript.save.saveTime;
        menuSelector.UpdateSelector();
    }
    public void CloseMenu()
    {
        playerScript.invManager.UpdateInfo();
        menu.GetComponent<Canvas>().enabled = false;    
        playerScript.inMenu = false;
    }
    public void OpenInput()
    {
        input.GetComponent<Canvas>().enabled = true;
        typeBox.enabled = true;
        typeBox.text = "";
        typeBox.ActivateInputField();
        playerScript.typingStuff = true;
    }
    public void CloseInput()
    {
        playerScript.invManager.UpdateInfo();
        typeBox.enabled = false;
        input.GetComponent<Canvas>().enabled = false;
    }
    public void OpenOptions()
    {
        options.GetComponent<Canvas>().enabled = true;
        playerScript.inOptions = true;
        optionSelector.UpdateSelector();
    }
    public void CloseOptions()
    {
        playerScript.invManager.UpdateInfo();
        options.GetComponent<Canvas>().enabled = false;
        playerScript.inOptions = false;
    }

    public void OpenUpgrade()
    {
        upgrade.GetComponent<Canvas>().enabled = true;
        int[] powerList = playerScript.invManager.PowerList();
        for (int i = 0; i < upgradeTextArray.transform.childCount; i++)
        {
            upgradeTextArray.transform.GetChild(i).GetComponent<TMP_Text>().text = upgChoices[i] + "\nCurrent: " + powerList[i];
        }
        playerScript.inUpgrade = true;
        upgradeSelector.UpdateSelector();
    }
    public void CloseUpgrade()
    {
        playerScript.invManager.UpdateInfo();
        upgrade.GetComponent<Canvas>().enabled = false;
        playerScript.inUpgrade = false;
    }
}
