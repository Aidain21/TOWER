using System.Collections;
using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[Serializable]
public class Storage
{
    [SerializeField]
    public string[] arr;
    public Storage(string[] arr)
    {
        this.arr = arr;
    }
}
public class DialogueEvents : MoveableObject
{
    public List<Sprite> talkerStatImgs;
    public bool dontAdd;
    public GameObject player;
    public PlayerScript2D playerScript;
    public string[] dialogueData = new string[3];
    public List<Storage> storedEvents = new();
    public List<Storage> storedTalks = new();
    public List<int> talkerStats = new();
    public string tempData;
    public string data2;
    public Texture2D tempImage;
    public IEnumerator curLoop;
    public void RunPastEvents()
    {
        dontAdd = true;
        for (int i = 0; i < storedEvents.Count; i++)
        {
            playerScript.currentTarget = GameObject.Find(storedEvents[i].arr[0]);
            if (playerScript.currentTarget != null)
            {
                storedEvents[i].arr.CopyTo(dialogueData, 0);
                EventTrigger();
                EndEventTrigger();
            }
        }
        for (int i = 0; i < storedTalks.Count; i++)
        {
            playerScript.currentTarget = GameObject.Find(storedTalks[i].arr[0]);
            if (playerScript.currentTarget != null)
            {
                playerScript.currentTarget.GetComponent<SignTextScript>().talkCounter = Int32.Parse(storedTalks[i].arr[1]);
                if (talkerStatImgs[talkerStats[i]] != playerScript.dialogueManager.noMoreText && talkerStatImgs[talkerStats[i]] != playerScript.dialogueManager.hasHiddenText)
                {
                    playerScript.currentTarget.GetComponent<SignTextScript>().talkCounter += 1;
                }
                playerScript.currentTarget.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = talkerStatImgs[talkerStats[i]];
            }
        }
        dontAdd = false;
        playerScript.currentTarget = null;
    }
    public void EventTrigger() //use:   else if (Enumerable.SequenceEqual(dialogueData, new string[] { "NPC Object's name", "Talk Counter", "Current Line" }))
    {

    }
    public void EndEventTrigger(int question = 0)
    {
        if (Enumerable.SequenceEqual(dialogueData, new string[] { "SafeExit", "0", "0" }))
        {
            if (question == 1)
            {
                playerScript.LeaveDungeon("safe");
            }
            else if (question == 2)
            {
                StartCoroutine(GridMove(player, playerScript.transform.position - playerScript.direction, 0.2f, 1));
            }
        }
        if (Enumerable.SequenceEqual(dialogueData, new string[] { "SadExit", "0", "0" }))
        {
            if (question == 1)
            {
                playerScript.LeaveDungeon("abandon");
            }
            else if (question == 2)
            {
                playerScript.menuManager.OpenMenu();
            }
        }
    }
    public (bool,Texture2D) SelectItem(string wantedItem, int successDialogueCounter, int failDialogueCounter, bool dontRemoveItem = false, bool takeAnyItem = false) 
    {
        if (playerScript.selection == null)
        {
            playerScript.dialogueManager.ChangeDialogue(failDialogueCounter, false);
            playerScript.selectingItem = true;
            playerScript.invManager.OpenInventory();
        }
        else if (playerScript.selection.CompareTag("Item") && (playerScript.selection.GetComponent<ItemScript>().item.itemName == wantedItem || takeAnyItem))
        {
            playerScript.currentTarget.GetComponent<SignTextScript>().talkCounter = successDialogueCounter;
            playerScript.dialogueManager.StartDialogue(playerScript.currentTarget.name, playerScript.currentTarget.GetComponent<SignTextScript>().dialogue, successDialogueCounter, playerScript.currentTarget.GetComponent<SignTextScript>().talkerImage);
            if (!dontRemoveItem)
            {
                playerScript.GetItem(wantedItem).item.used = true;
                playerScript.selection = null;
                return (true, null);
            }
            else
            {
                if (playerScript.currentTarget.name == "#PuzzleMaker")
                {
                    Sprite sprite = playerScript.selection.GetComponent<SpriteRenderer>().sprite;
                    Texture2D croppedTexture = new((int)sprite.rect.width, (int)sprite.rect.height);
                    croppedTexture.SetPixels32(sprite.texture.GetPixels32());
                    croppedTexture.Apply();
                    playerScript.selection = null;
                    return (true, croppedTexture);
                }
                playerScript.selection = null;
                return (true, null);
            }
            
            
        }
        else if (playerScript.selection.CompareTag("Item") && playerScript.selection.GetComponent<ItemScript>().item.itemName != wantedItem)
        {
            
            playerScript.selection = null;
            playerScript.dialogueManager.ChangeDialogue(failDialogueCounter, true, dialogueData[2]);
        }
        else if (playerScript.selection == playerScript.currentTarget)
        {
            playerScript.selection = null;
            playerScript.dialogueManager.ChangeDialogue(failDialogueCounter, true, dialogueData[2]);
        }
        return (false, null);
    }

    public string GetPlayerText(int failDialogueCounter)
    {
        if (playerScript.input == "gQprk73vInHt51GHQNA8rTtilfRaNiNTxjm00IUBFd3yeplTPJ")
        {
            playerScript.dialogueManager.ChangeDialogue(failDialogueCounter, false);
            playerScript.menuManager.OpenInput();
        }
        else
        {
            string temp = playerScript.input;
            playerScript.input = "gQprk73vInHt51GHQNA8rTtilfRaNiNTxjm00IUBFd3yeplTPJ";
            return temp;
        }
        return "";
    }
}