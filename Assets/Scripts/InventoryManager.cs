using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public Selector selector;
    public List<ItemScript> inventory;
    public Canvas inventoryBox;
    public TMP_Text titleText;
    public TMP_Text controlsText;
    public TMP_Text statsText;
    public List<Image> images;
    public GameObject TextArray;

    public Canvas infoDisplay;
    public TMP_Text itemsText;
    public TMP_Text progressText;
    public TMP_Text swapText;
    public int cheese;
    
    
    public PlayerScript2D playerScript;
    public TMP_Text def;
    public Image def2;
    public (string,bool)[] goals;
    public GameObject icons;

    public int hp;
    public int maxHp;
    public Image HPBar;
    public TMP_Text HPText;

    public int attack;
    public int tempAttack;
    public int defense;
    public int tempDefense;
    public int regen;
    public int crit;
    public Sprite critIcon;
    public Sprite critIcon2;

    public Selector chestSelector;
    public List<ItemScript> chest;
    public Canvas chestBox;
    public List<Image> chestImages;
    public GameObject chestTextArray;
    public int chestPage;
    public TMP_Text chestTitleText;
    public TMP_Text chestControlsText;

    // Start is called before the first frame update
    void Start()
    {
        maxHp = 10;
        hp = 10;
        attack = 1;
        defense = 0;
        regen = 0;
        crit = 10;
        Damage(0);
        chestPage = 0;
        selector = new Selector(5, 3);
        chestSelector = new Selector(5, 3);

        images = new List<Image>();
        chestImages = new List<Image>();

        inventoryBox.GetComponent<Canvas>().enabled = false;
        chestBox.GetComponent<Canvas>().enabled = false;

        cheese = 0;
        for (int i = 0; i < selector.textArray.Length; i++)
        {
            for (int j = 0; j < selector.textArray[i].Length; j++)
            {
                Image img = Instantiate(def2, TextArray.transform);
                img.rectTransform.localPosition = new Vector2(325* j, -225 * i);
                img.rectTransform.sizeDelta = new Vector2(100, 100);
                img.name = "Image" + (i * selector.textArray[i].Length + j + 1).ToString();
                images.Add(img);

                TMP_Text text = Instantiate(def, TextArray.transform);
                text.rectTransform.localPosition = new Vector2(325 * j + 150, -225 * i);
                text.name = "Text" + (i * selector.textArray[i].Length + j + 1).ToString();
                selector.textArray[i][j] = text;

                Image img2 = Instantiate(def2, chestTextArray.transform);
                img2.rectTransform.localPosition = new Vector2(325 * j, -225 * i);
                img2.rectTransform.sizeDelta = new Vector2(100, 100);
                img2.name = "ChestImage" + (i * chestSelector.textArray[i].Length + j + 1).ToString();
                chestImages.Add(img2);

                TMP_Text text2 = Instantiate(def, chestTextArray.transform);
                text2.rectTransform.localPosition = new Vector2(325 * j + 150, -225 * i);
                text2.name = "ChestText" + (i * chestSelector.textArray[i].Length + j + 1).ToString();
                chestSelector.textArray[i][j] = text2;
            }
        }
        Destroy(def);
        Destroy(def2);
        UpdateInfo();
    }

    public void UpdateInfo()
    {
        itemsText.text = "Cheese: None" + "\nItems: " + "UnusedCount(inventory)" + "/15" + "\nFloor: " + playerScript.dungeonScript.dungeonLevel + "\nL " + playerScript.playerLevel + " (" + playerScript.exp+ "/" + playerScript.ExpNeeded() + ")";
        progressText.text = "Enemies: " + playerScript.dungeonScript.ActiveSpawns("Enemy").Count + "\nP:" + playerScript.dungeonScript.turnCounterP + " E:" + playerScript.dungeonScript.turnCounterE + "\nSpeed: " + playerScript.dungeonScript.timeModifier;
        
    }

    public int[] PowerList()
    {
        return new int[] { maxHp, attack, defense, regen, crit, 0, 0, 0, 0 };
    }
    public void OpenInventory()
    {
        inventoryBox.GetComponent<Canvas>().enabled = true;
        playerScript.inInventory = true;
        int curItems = 0;
        for (int i = 0; i < inventory.Count; i++)
        {
            if (!inventory[i].GetComponent<ItemScript>().item.used)
            {
                images[curItems].color = inventory[i].GetComponent<ItemScript>().item.color;
                images[curItems].sprite = inventory[i].GetComponent<SpriteRenderer>().sprite;
                selector.textArray[curItems / selector.width][curItems % selector.width].text = inventory[i].GetComponent<ItemScript>().item.itemName;
                curItems++;
            }
            
        }
        for (int i = curItems; i < selector.width * selector.height; i++)
        {
            images[i].sprite = null;
            images[i].color = new Color32(0,0,0,0);
            selector.textArray[i / selector.width][i % selector.width].text = "Empty";
        }
        if (playerScript.selectingItem || playerScript.selectingChest)
        {
            titleText.text = "Select an Item:";
            controlsText.text = "WASD - Select              E - Choose Item";
            statsText.text = "";
        }
        else
        {
            titleText.text = "Inventory";
            controlsText.text = "WASD - Select | Space - Info | E - Use | R - Drop | I/Q/Esc - Close";
            statsText.text = "Attack: " + attack + " |                                         | Defense: " + defense + "\nTemp Attack: " + tempAttack + " |                                         | Temp Defense: " + tempDefense;
        }
        
        selector.UpdateSelector();
    }
    public void CloseInventory()
    {
        UpdateInfo();
        inventoryBox.GetComponent<Canvas>().enabled = false;
        playerScript.inInventory = false;
    }

    public void OpenChest(int page)
    {
        chestBox.GetComponent<Canvas>().enabled = true;
        playerScript.inChest = true;
        int curItems = 0;
        for (int i = page * 15; i < Mathf.Min(chest.Count, (page + 1) * 15); i++)
        {
            if (!chest[i].GetComponent<ItemScript>().item.used)
            {
                chestImages[curItems].color = chest[i].GetComponent<ItemScript>().item.color;
                chestImages[curItems].sprite = chest[i].GetComponent<SpriteRenderer>().sprite;
                chestSelector.textArray[curItems / chestSelector.width][curItems % chestSelector.width].text = chest[i].GetComponent<ItemScript>().item.itemName;
                curItems++;
            }

        }
        for (int i = curItems; i < chestSelector.width * chestSelector.height; i++)
        {
            chestImages[i].sprite = null;
            chestImages[i].color = new Color32(0, 0, 0, 0);
            chestSelector.textArray[i / chestSelector.width][i % chestSelector.width].text = "Empty";
        }
        chestTitleText.text = "Chest (Page " + (chestPage + 1) + ")";
        chestControlsText.text = "WASD - Select | Space - Info | E (Item) - Take | E (Empty) - Store | C - Close";
        chestSelector.UpdateSelector();
    }
    public void CloseChest()
    {
        UpdateInfo();
        chestBox.GetComponent<Canvas>().enabled = false;
        playerScript.inChest = false;
    }


    public int UnusedCount(List<ItemScript> list)
    {
        int unusedCount = list.Count;
        foreach (ItemScript i in list)
        {
            if (i.item.used)
            {
                unusedCount--;
            }
        }
        return unusedCount;
    }
    public List<ItemScript> ReturnUnusedInv(List<ItemScript> list)
    {
        List<ItemScript> val = new();
        foreach (ItemScript i in list)
        {
            if (!i.item.used)
            {
                val.Add(i);
            }
        }
        return val;
    }
    public void Damage(int amt)
    {
        if (defense + tempDefense >= amt && amt > 0 && Random.Range(0,2) == 0)
        {
            GameObject test = Instantiate(playerScript.dungeonScript.itemEffect, transform.position + Vector3.up, Quaternion.identity);
            StartCoroutine(test.GetComponent<ItemEffect>().FadeOut(1f, Resources.Load<Sprite>("Sprites/Stuff_5")));
        }
        else
        {
            hp -= amt;
        }
        HPBar.fillAmount = (float)hp / maxHp;
        HPText.text = hp + "/" + maxHp;
        if (hp <= 0)
        {
            bool dougFound = false;
            ItemScript doug = null;
            foreach (ItemScript i in inventory)
            {
                if (i.item.itemName == "Doug")
                {
                    dougFound = true;
                    doug = i;
                }
            }
            if (dougFound)
            {
                hp = maxHp;
                Damage(0);
                inventory.Remove(doug);
                GameObject test = Instantiate(playerScript.dungeonScript.itemEffect, transform.position + Vector3.up, Quaternion.identity);
                StartCoroutine(test.GetComponent<ItemEffect>().FadeOut(1.5f, doug.gameObject.GetComponent<SpriteRenderer>().sprite));
                Destroy(doug.gameObject);
            }
            else
            {
                playerScript.LeaveDungeon("died");
            }
            
        }
    }
    public void UseItem(ItemScript item)
    {
        string name = item.item.itemName;
        if (name.Contains("Hammer"))
        {
            name = "Hammer";
        }
        switch(name)
        {
            case "Heal":
                if (hp == maxHp)
                {
                    string[] temp = new string[] { "0You: At full health already." };
                    playerScript.dialogueManager.StartDialogue("Player", temp, 0, playerScript.GetComponent<SpriteRenderer>().sprite);
                }
                else
                {
                    Damage(Mathf.Max(-(maxHp - hp),-(maxHp*3)/10));
                    RemoveItem(item);
                }
                break;
            case "Hoohoo":
                GameObject chosen = GetClosestEnemy(playerScript.dungeonScript.ActiveSpawns("Enemy"), 7);
                if (chosen != null)
                {
                    Destroy(chosen);
                    RemoveItem(item);
                }
                else
                {
                    playerScript.dialogueManager.StartDialogue("Player", new string[] { "0You: No Enemies Nearby." }, 0, playerScript.GetComponent<SpriteRenderer>().sprite);
                }
                break;
            case "Doug":
                playerScript.dialogueManager.StartDialogue("Player", new string[] { "0You: This item activates automatically if I were to die." }, 0, playerScript.GetComponent<SpriteRenderer>().sprite);
                break;
            case "Attack Up":
                tempAttack++;
                RemoveItem(item);
                break;
            case "Defense Up":
                tempDefense++;
                RemoveItem(item);
                break;
            case "Mo":
                if (playerScript.transform.Find("Pointer(Clone)") == null)
                {
                    GameObject point = Instantiate(playerScript.pointer, playerScript.transform.position + new Vector3(0, 1, -3), Quaternion.identity);
                    point.transform.parent = playerScript.gameObject.transform;
                    if (!point.GetComponent<Pointer>().found)
                    {
                        playerScript.dialogueManager.StartDialogue("Player", new string[] { "0You: Unable to find exit." }, 0, playerScript.GetComponent<SpriteRenderer>().sprite);
                        Destroy(point);
                    }
                    else
                    {
                        point.GetComponent<Pointer>().UpdatePointer();
                        RemoveItem(item);
                    }

                }
                else
                {
                    playerScript.dialogueManager.StartDialogue("Player", new string[] { "0You: Already have a pointer." }, 0, playerScript.GetComponent<SpriteRenderer>().sprite);
                }
                
                break;
            case "Hammer":
                playerScript.dialogueManager.StartDialogue("Player", new string[] { "0You: This item is used by facing a wall and pressing E." }, 0, playerScript.GetComponent<SpriteRenderer>().sprite);
                break;
            default:
                playerScript.dialogueManager.StartDialogue("Player", new string[] { "0You: This item doesnt work." }, 0, playerScript.GetComponent<SpriteRenderer>().sprite);
                break;
        }
        CloseInventory();
        OpenInventory();
           
    }

    public GameObject GetClosestEnemy(List<GameObject> list, float max = Mathf.Infinity)
    {
        GameObject tMin = null;
        float minDist = max;
        foreach (GameObject t in list)
        {
            if (t.CompareTag("Enemy"))
            {
                float dist = Vector3.Distance(t.transform.position, playerScript.transform.position);
                if (dist < minDist)
                {
                    tMin = t;
                    minDist = dist;
                }
            }
        }
        return tMin;
    }

    public void RemoveItem(ItemScript item)
    {
        playerScript.takenAction = true;
        inventory.Remove(item);
        CloseInventory();
        OpenInventory();
        Destroy(item.gameObject);
    }

}
