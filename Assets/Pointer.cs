using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    public GameObject target;
    public bool found;
    void Awake()
    {
        found = false;
        foreach (Transform t in GameObject.Find("Spawns").transform)
        {
            if (t.gameObject.name == "DungeonGen(Clone)" || t.gameObject.name == "SafeFloor(Clone)")
            {
                target = t.gameObject;
                found = true;
            }
        }
    }


    public void UpdatePointer()
    {
        var dir = target.transform.position - transform.position;
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
    }
}
