using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class DialogueManager : MonoBehaviour
{
    public TMP_Text dialogueText;
    public TMP_Text nameText;
    public TMP_Text spaceText;
    public bool typing;
    public bool changed;
    public bool keepTalkerInfo;
    public bool talkingToNPC;
    public bool playerCurrentlyTalking;
    public float typingSpeed = 0.03f;
    public bool hasMoreText;
    public bool hiddenText;
    public bool isRiddle;
    public PlayerScript2D playerScript;
    public DialogueEvents eventScript;
    public Canvas textBox;
    public GameObject background;
    public Image imageFrame;
    public Sprite currentImage; //just the talker for now

    public Sprite hasNewText;
    public Sprite hasHiddenText;
    public Sprite noMoreText;

    public Sprite storedStatus;

    public List<string> sentences;
    public string ogTalkerName;

    public bool question;
    void Start()
    {
        textBox.GetComponent<Canvas>().enabled = false;
        sentences = new List<string>();
    }

    public void StartDialogue(string name, string[] dialogue, int talkCounter, Sprite talkerImage)
    {
        talkingToNPC = playerScript.currentTarget != null && playerScript.currentTarget.CompareTag("Sign");
        typingSpeed = 1.5f * ((5 - playerScript.menuManager.optionSelector.selections[2].x)  / 100) - 0.01f; 
        hasMoreText = false;
        hiddenText = false;
        sentences.Clear();
        ogTalkerName = "";
        playerScript.inDialogue = true;
        eventScript.dialogueData[0] = name;
        question = false;
        if (!playerScript.inInventory)
        {
            playerScript.aboveTalker = false;
        }
        if (name.Contains("Follower"))
        {
            eventScript.dialogueData[0] = "Follower";
        }
        if (name.Contains("TypeDude"))
        {
            eventScript.dialogueData[0] = "TypeDude";
        }
        eventScript.dialogueData[1] = talkCounter.ToString();
        if (!changed)
        {
            eventScript.dialogueData[2] = "-1";
        }
        textBox.GetComponent<Canvas>().enabled = true;
        PositionBox();
        currentImage = talkerImage;

        foreach (string sentence in dialogue)
        {
            int index = 0;
            for (int ctr = 0; ctr < sentence.Length; ctr++)
            {
                if (!Char.IsDigit(sentence[ctr]))
                {
                    index = ctr;
                    break;
                }
            }
            if (index == 0)
            {
                sentences.Add("Aidan:Fix up the formatting of the text plz something is missing the talk counter :P");
                break;
            }
            else if (Int32.Parse(sentence.Substring(0, index)) == talkCounter)
            {
                sentences.Add(sentence[index..]);
            }
            if (Int32.Parse(sentence.Substring(0, index)) == talkCounter + 1)
            {
                hasMoreText = true;
                hiddenText = false;
            }
            else if (!hasMoreText && Int32.Parse(sentence.Substring(0, index)) > talkCounter + 1)
            {
                hiddenText = true;
            }
        }
        if (changed && eventScript.dialogueData[2] != "-1")
        {
            int num = Int32.Parse(eventScript.dialogueData[2]);
            for (int i = 0; i < num + 1; i++)
            {
                sentences.RemoveAt(0);
            }
            changed = false;
        }
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        
        eventScript.dialogueData[2] = (Int32.Parse(eventScript.dialogueData[2]) + 1).ToString();
        spaceText.text = "";
        
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences[0];
        while (sentence.Contains("@"))
        {
            sentence = sentence.Substring(0, sentence.IndexOf("@")) + playerScript.playerName + sentence[(sentence.IndexOf("@") + 1)..];
        }
        if (sentence[0] == '#')
        {
            sentence = sentence[1..];
            dialogueText.color = Color.magenta;
            dialogueText.fontStyle = FontStyles.Bold;
        }
        else
        {
            dialogueText.color = Color.white;
            dialogueText.fontStyle = FontStyles.Normal;
        }
        if (sentence.Contains("(Y/N)"))
        {
            question = true;
        }
        StopAllCoroutines();

        if (hasMoreText)
        {
            storedStatus = hasNewText;
        }
        else if (hiddenText)
        {
            storedStatus = hasHiddenText;
        }
        else
        {
            storedStatus = noMoreText;
        }
        playerCurrentlyTalking = false;
        nameText.text = sentence.Substring(0,sentence.IndexOf(":")+1);
        if (nameText.text == "")
        {
            nameText.text = "Aidan:";
            StartCoroutine(TypeSentence("Fix up the formatting of the text plz its missing the semicolon after the name :P"));
        }
        else if (nameText.text == "You:")
        {
            playerCurrentlyTalking = true;
            playerScript.vcam.Follow = playerScript.transform;
            nameText.text = playerScript.playerName + ":";
            imageFrame.sprite = playerScript.GetComponent<SpriteRenderer>().sprite;
            if (sentence[sentence.IndexOf(":") + 2] == '^')
            {
                GameObject otherDude = GameObject.Find(sentence.Substring(sentence.IndexOf("^") + 1, sentence.IndexOf(" ", sentence.IndexOf("^")) - sentence.IndexOf("^") - 1));
                playerScript.vcam.Follow = otherDude.transform;
                sentence = nameText.text + sentence[sentence.IndexOf(" ", sentence.IndexOf("^"))..];
            }
            eventScript.EventTrigger();
            if (sentence[sentence.IndexOf(":") + 1] == ' ')
            {
                StartCoroutine(TypeSentence(sentence[(sentence.IndexOf(":") + 2)..]));
            }
            else
            {
                StartCoroutine(TypeSentence(sentence[(sentence.IndexOf(":") + 1)..]));
            }  
        }
        else
        {
            if (ogTalkerName == "")
            {
                ogTalkerName = nameText.text;
            }
            if (nameText.text == ogTalkerName && playerScript.currentTarget != null && playerScript.currentTarget.CompareTag("Sign"))
            {
                playerScript.vcam.Follow = playerScript.currentTarget.transform;
            }
            if (sentence[sentence.IndexOf(":") + 2] == '^')
            {
                GameObject otherDude = GameObject.Find(sentence.Substring(sentence.IndexOf("^") + 1, sentence.IndexOf(" ", sentence.IndexOf("^")) - sentence.IndexOf("^") - 1));
                playerScript.vcam.Follow = otherDude.transform;
                sentence = nameText.text + sentence[sentence.IndexOf(" ", sentence.IndexOf("^"))..];
            }
            eventScript.EventTrigger();
            if (sentence[sentence.IndexOf(":") + 1] == ' ')
            {
                StartCoroutine(TypeSentence(sentence[(sentence.IndexOf(":") + 2)..]));
            }
            else
            {
                StartCoroutine(TypeSentence(sentence[(sentence.IndexOf(":") + 1)..]));
            }
        }
    }
    public void ChangeDialogue(int talkCounter, bool keepTalking, string continueLine = "-1")
    {
        keepTalkerInfo = true;
        EndDialogue();
        playerScript.currentTarget.GetComponent<SignTextScript>().talkCounter = talkCounter;
        eventScript.dialogueData[2] = continueLine;
        if (keepTalking)
        {
            changed = true;
            StartDialogue(eventScript.dialogueData[0], playerScript.currentTarget.GetComponent<SignTextScript>().dialogue, talkCounter, currentImage);
        }
    }


    public IEnumerator TypeSentence(string sentence)
    {
        float maxRectHeight = 0;
        int typed = 0;
        if (playerCurrentlyTalking)
        {
            Sprite curSprite = playerScript.GetComponent<SpriteRenderer>().sprite;
            imageFrame.rectTransform.sizeDelta = new Vector2(curSprite.rect.width * (300 / curSprite.rect.height), 300);
            imageFrame.color = playerScript.GetComponent<SpriteRenderer>().color;
            maxRectHeight = curSprite.rect.height;
        }
        else if (talkingToNPC)
        {
            if (nameText.text == ogTalkerName)
            {
                currentImage = playerScript.currentTarget.GetComponent<SpriteRenderer>().sprite;
                imageFrame.color = playerScript.currentTarget.GetComponent<SpriteRenderer>().color;
            }
            else
            {
                currentImage = playerScript.vcam.Follow.GetComponent<SpriteRenderer>().sprite;
                imageFrame.color = playerScript.vcam.Follow.GetComponent<SpriteRenderer>().color;
            }
            imageFrame.sprite = currentImage;
            imageFrame.rectTransform.sizeDelta = new Vector2(currentImage.rect.width * (300 / currentImage.rect.height), 300);
            
            if (playerScript.currentTarget.name.Contains("Follower") && eventScript.dialogueData[1] == "0")
            {
                if (playerScript.follower != null)
                {
                    playerScript.follower.GetComponent<SignTextScript>().talkCounter = 0;
                }
                playerScript.follower = playerScript.currentTarget;
            }
        }
        else if (ogTalkerName.Contains("Silly"))
        {
            imageFrame.sprite = currentImage;
        }
        else
        {
            currentImage = playerScript.vcam.Follow.GetComponent<SpriteRenderer>().sprite;
            imageFrame.color = playerScript.vcam.Follow.GetComponent<SpriteRenderer>().color;
            imageFrame.sprite = currentImage;
        }
        typing = true;
        dialogueText.text = "";
        
        foreach (char letter in sentence.ToCharArray())
        {
            if (!typing)
            {
                break;
            }
            dialogueText.text += letter;


            Sprite curSprite = null;
            if (talkingToNPC && !playerCurrentlyTalking)
            {
                
                if (nameText.text == ogTalkerName)
                {
                    curSprite = playerScript.currentTarget.GetComponent<SpriteRenderer>().sprite;

                }
                else
                {
                    curSprite = playerScript.vcam.Follow.GetComponent<SpriteRenderer>().sprite;
                }
                if (curSprite.rect.height > maxRectHeight)
                {
                    maxRectHeight = curSprite.rect.height;
                }
                imageFrame.rectTransform.sizeDelta = new Vector2(curSprite.rect.width * (300 / maxRectHeight), 300);
                imageFrame.sprite = curSprite;
            }
            yield return new WaitForSeconds(typingSpeed);
            typed++;
        }
        dialogueText.text = sentence;
        typing = false;
        if (talkingToNPC && !playerCurrentlyTalking)
        {
            playerScript.currentTarget.GetComponent<SpriteRenderer>().sprite = playerScript.currentTarget.GetComponent<SignTextScript>().talkerImage;
            if (nameText.text == ogTalkerName)
            {
                imageFrame.sprite = playerScript.currentTarget.GetComponent<SignTextScript>().talkerImage;
            }
            else
            {
                imageFrame.sprite = playerScript.vcam.Follow.GetComponent<SpriteRenderer>().sprite;
            }
            
        }
        yield return new WaitForSeconds(0.75f);
        if (question)
        {
            spaceText.text = "Yes or No? (Y/N)";
        }
        else
        {
            spaceText.text = "Space to Continue";
        }
        
    }

    void EndDialogue()
    {
        if (playerScript.currentTarget != null && playerScript.currentTarget.CompareTag("Sign") && talkingToNPC)
        {
            playerScript.currentTarget.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = storedStatus;
            int alreadyTracked = -1;
            for (int i = 0; i < eventScript.storedTalks.Count; i++)
            {
                if (eventScript.storedTalks[i].arr[0] == eventScript.dialogueData[0])
                {
                    alreadyTracked = i;
                }
            }
            if (alreadyTracked != -1)
            {
                eventScript.storedTalks[alreadyTracked] = new Storage((string[])eventScript.dialogueData.Clone());
                eventScript.talkerStats[alreadyTracked] = eventScript.talkerStatImgs.IndexOf(playerScript.currentTarget.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite);
            }
            else
            {
                if (SceneManager.GetActiveScene().name != "ImagePuzzle")
                {
                    eventScript.storedTalks.Add(new Storage((string[])eventScript.dialogueData.Clone()));
                    eventScript.talkerStats.Add(eventScript.talkerStatImgs.IndexOf(playerScript.currentTarget.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite));
                }
            }
            
            talkingToNPC = false;
        }
        if (hasMoreText)
        {
            playerScript.currentTarget.GetComponent<SignTextScript>().talkCounter += 1;
        }
        playerScript.inDialogue = false;
        textBox.GetComponent<Canvas>().enabled = false;
        playerScript.invManager.UpdateInfo();
        playerScript.vcam.Follow = playerScript.transform;
        if (!keepTalkerInfo)
        {
            playerScript.currentTarget = null;
        }
        keepTalkerInfo = false;
    }

    public void PositionBox()
    {
        if (playerScript.aboveTalker)
        {
            background.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 125, 300);
            imageFrame.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 125, 300);
        }
        else
        {
            background.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 75, 300);
            imageFrame.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 75, 300);
        }
    }
}