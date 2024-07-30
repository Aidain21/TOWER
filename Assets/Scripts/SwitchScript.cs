using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class SwitchScript : MonoBehaviour
{
    public string switchType;
    public string switchEffect;
    public string switchData;
    public Transform warpEnd;
    public bool onValue;
    public bool playerOnSpike = false;
    public Texture2D puzzleImage;
    public GameObject[] affectedObjects = new GameObject[1];
    // Start is called before the first frame update
    void Start()
    {
        if (switchType == "pressurePlate++" || switchType == "physical++")
        {
            for (int i = 0; i < GameObject.Find("Player").transform.childCount; i++)
            {
                if (GameObject.Find("Player").transform.GetChild(i).name.Equals(name))
                {
                    Destroy(gameObject);
                }
            }
        }

    }

    // Update is called once per frame

    public void UseSwitch()
    {
        //what the switch does
        foreach (GameObject item in affectedObjects)
        {
            switch (switchEffect)
            {
                case "activate":
                    item.SetActive(true);
                    break;
                case "chest":
                    item.GetComponent<PlayerScript2D>().invManager.OpenChest(item.GetComponent<PlayerScript2D>().invManager.chestPage);
                    break;
                case "upgrade":
                    item.GetComponent<PlayerScript2D>().menuManager.OpenUpgrade();
                    break;
                case "warp":
                    if (switchData.Length == 0)
                    {
                        item.GetComponent<PlayerScript2D>().StopAllCoroutines();
                        item.GetComponent<PlayerScript2D>().moving = false;
                        float test;
                        if (item.GetComponent<PlayerScript2D>().direction == Vector3.up || item.GetComponent<PlayerScript2D>().direction == Vector3.down)
                        {
                            test = item.transform.InverseTransformPoint(transform.position).x;
                            item.transform.position = new Vector3(warpEnd.position.x - test, warpEnd.position.y, 0) + item.GetComponent<PlayerScript2D>().direction;
                        }
                        else
                        {
                            test = item.transform.InverseTransformPoint(transform.position).y;
                            item.transform.position = new Vector3(warpEnd.position.x, warpEnd.position.y - test, 0) + item.GetComponent<PlayerScript2D>().direction;
                        }
                        item.GetComponent<PlayerScript2D>().invManager.UpdateInfo();
                    }
                    break;
                case "talk":
                    SignTextScript signScript = GetComponent<SignTextScript>();
                    item.GetComponent<PlayerScript2D>().currentTarget = gameObject;
                    item.GetComponent<PlayerScript2D>().dialogueManager.StartDialogue(signScript.name, signScript.dialogue, signScript.talkCounter, signScript.talkerImage);
                    break;
                case "dungeonGen":
                    item.GetComponent<PlayerScript2D>().EnterDungeon();
                    break;
                case "safeFloor":
                    item.GetComponent<PlayerScript2D>().EnterDungeon("safe");
                    break;
                case "safeLeave":
                    item.GetComponent<PlayerScript2D>().dialogueManager.StartDialogue("SafeExit", new string[] { "0Exit: Do you want to leave? You will lose any unspent EXP (Y/N)" }, 0, item.GetComponent<SpriteRenderer>().sprite);
                    break;

            }
        }
        //what happens to the switch
        switch (switchType)
        {
            case "pressurePlate+":
                Destroy(gameObject);
                break;
            case "pressurePlate++":
                transform.parent = affectedObjects[0].GetComponent<PlayerScript2D>().transform;
                gameObject.SetActive(false);
                break;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Item") && switchType == "floor" && switchData == collision.name)
        {
            affectedObjects[0] = collision.gameObject;
            UseSwitch();
        }
        if (collision.CompareTag("Sign") && switchType == "floor" && collision.name[9..] == switchData)
        {
            affectedObjects[0] = collision.gameObject;
            UseSwitch();
        }
        if (collision.CompareTag("Player") && (switchType == "pressurePlate" || switchType == "pressurePlate+" || switchType == "pressurePlate++"))
        {
            affectedObjects[0] = collision.gameObject;
            UseSwitch();
        }
    }
}
