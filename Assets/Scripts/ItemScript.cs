using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ItemScript : MonoBehaviour
{
    public Item item;
    void Start()
    {
        if (item.path == "" || item.path == null || item.itemObjName == "Puzzle Skip Temp")
        {
            item.path = GetComponent<SpriteRenderer>().sprite.name;
        }
        else
        {
            GetComponent<SpriteRenderer>().sprite = (Sprite) Resources.Load("Sprites/" + item.path);
        }
        item.itemObjName = name;
        item.color = GetComponent<SpriteRenderer>().color;

    }

}
[Serializable]
public class Item
{
    public string itemObjName;
    public string itemName;
    public string pickupText;
    public string itemLore;
    public string path;
    public Color32 color;
    public bool used;
}
