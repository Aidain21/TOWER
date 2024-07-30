using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alteruna;

public class DungeonActiveScript : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject player;
    public DungeonGenScript dungeon;

    public GameObject enemy;
    public GameObject itemEffect;
    public List<GameObject> items;
    public GameObject stairs;
    public GameObject stairs2;
    public GameObject spawns;

    public int turnCounterP;
    public int turnCounterE;
    public int dungeonLevel;
    public bool enemyAlreadyTakingTurn;
    public bool inSafeRoom;

    public int timeModifier;

    public List<GameObject> players;
    void Start()
    {
        timeModifier = 1;
        turnCounterE = 0;
        turnCounterP = 0;
    }


    public void Setup()
    {
        for (int i = 0; i < 5 + dungeonLevel/3 ; i++)
        {
            GenerateStuff(items[Random.Range(0,items.Count)]);
        }
        if (dungeonLevel%5 == 0)
        {
            GenerateStuff(stairs2);
        }
        else
        {
            GenerateStuff(stairs);
        }
        
    }
    // Update is called once per frame
    public void EndTurnActions()
    {
        if (turnCounterP > 10 + turnCounterE && ActiveSpawns("Enemy").Count < 1 && !inSafeRoom)
        {
            enemyAlreadyTakingTurn = false;
            GenerateStuff(enemy,3);
        } 
        else if (turnCounterP > 15 * (ActiveSpawns("Enemy").Count + 1) + turnCounterE && !inSafeRoom)
        {
            enemyAlreadyTakingTurn = false;
            GenerateStuff(enemy);
        }
    }

    public void GenerateStuff(GameObject spawn, int amt = 1)
    {
        for (int i = 0; i < amt; i++)
        {
            Rect room = dungeon.roomDims[Random.Range(0, dungeon.roomDims.Length)];
            Vector3 roomPos = new(Random.Range((int)room.x + 1, (int)room.xMax), Random.Range((int)room.y + 1, (int)room.yMax), 0);
            GameObject thing = Instantiate(spawn, roomPos, Quaternion.identity);
            thing.transform.parent = spawns.transform;
        }
    }
    public List<GameObject> ActiveSpawns(List<string> tags)
    {
        List<GameObject> spawnList = new();
        foreach(Transform t in spawns.transform)
        {
            if (tags.Contains(t.tag))
            {
                spawnList.Add(t.gameObject);
            }
        }
        return spawnList;
    }

    public List<GameObject> ActiveSpawns(string tag)
    {
        List<GameObject> spawnList = new();
        foreach (Transform t in spawns.transform)
        {
            if (tag == "all" || t.CompareTag(tag))
            {
                spawnList.Add(t.gameObject);
            }
        }
        return spawnList;
    }
}
