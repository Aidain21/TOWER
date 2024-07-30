using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class EnemyScript : MoveableObject
{
    public int statPool;
    public int health;
    public int fullHealth;
    public int attack;

    public string type;
    public bool enemyAction;
    public string[] adjacents;
    public TMP_Text healthCounter;
    readonly Vector3[] dirIndex = new Vector3[] { Vector3.up, Vector3.left, Vector3.down, Vector3.right };
    readonly Vector3[] diagIndex = new Vector3[] { new Vector2 (-1,1), -Vector2.one, new Vector2(1,-1), Vector2.one };
    public Vector3 prevPosition;

    public GameObject targetedPlayer;
    public bool slept;
    public bool prevTurnAdjacent;
    public bool speedMove;

    public DungeonActiveScript dungeonScript;

    // Start is called before the first frame update
    void Start()
    {
        dungeonScript = GameObject.Find("Grid").GetComponent<DungeonActiveScript>();
        statPool = UnityEngine.Random.Range(dungeonScript.dungeonLevel / 3 + 4, dungeonScript.dungeonLevel + 3);
        fullHealth = UnityEngine.Random.Range(2 + dungeonScript.dungeonLevel / 3, statPool);
        health = fullHealth;
        attack = statPool - fullHealth;
        int rand = UnityEngine.Random.Range(0, 3);
        type = rand switch
        {
            0 => "norm",
            1 => "speedy",
            2 => "shoot",
            _ => "norm"
        };
        GetComponent<SpriteRenderer>().color = rand switch
        {
            1 => Color.blue,
            _ => Color.white
        };
        GetComponent<SpriteRenderer>().sprite = rand switch
        {
            2 => Resources.Load<Sprite>("Sprites/HDGREMLIN"),
            _ => GetComponent<SpriteRenderer>().sprite
        };
        attack = rand switch
        {
            1 => Mathf.Max(1,attack/2),
            _ => attack,
        };
        fullHealth = rand switch
        {
            2 => Mathf.Max(1, health / 2),
            _ => health,
        };
        health = fullHealth;
        DamageEnemy(0);
        speedMove = true;
    }

    // Update is called once per frame

    public void EnemyTurn()
    {
        slept = false;
        dungeonScript.enemyAlreadyTakingTurn = true;
        string action = "Move";
        Vector3 actionDirection = Vector3.zero;
        adjacents = WallChecker(gameObject);
        float playerDistance = 10000000;
        foreach (GameObject p in dungeonScript.players)
        {
            if (Mathf.Abs(Vector3.Distance(transform.position, p.transform.position)) < playerDistance)
            {
                playerDistance = Mathf.Abs(Vector3.Distance(transform.position, p.transform.position));
                targetedPlayer = p;
            }
        }
        
        if (type == "shoot")
        {
            adjacents = WallChecker(gameObject, false, 3);
        }
        if (Mathf.Abs(Vector3.Distance(transform.position,targetedPlayer.transform.position)) > 20)
        {
            action = "Sleep";
        }
        for (int s = 0; s < adjacents.Length; s++)
        {
            if (adjacents[s] == "Player")
            {
                action = "AttackPlayer";
                actionDirection = dirIndex[s];
                prevTurnAdjacent = false;
                break;
            }
        }
        if (prevTurnAdjacent)
        {
            string[] diags = WallChecker(gameObject, true);
            for (int s = 0; s < diags.Length; s++)
            {
                if (diags[s] == "Player")
                {
                    action = "AttackPlayer";
                    actionDirection = diagIndex[s];
                    break;
                }
            }
        }

        if (action == "Move")
        {
            float[] distances = new float[] { Vector3.Distance(targetedPlayer.transform.position, transform.position + Vector3.up), Vector3.Distance(targetedPlayer.transform.position, transform.position + Vector3.left), Vector3.Distance(targetedPlayer.transform.position, transform.position + Vector3.down), Vector3.Distance(targetedPlayer.transform.position, transform.position + Vector3.right) };
            for (int i = 0; i < 4; i++)
            {
                if (adjacents[i] != "")
                {
                    distances[i] += 100;
                }
                else if (type == "shoot" && Array.IndexOf(WallChecker(transform.position + dirIndex[i], false, 3), "Player") != -1)
                {
                    distances[i] -= 50;
                        
                }
                else if ((dirIndex[i] + transform.position == prevPosition && Mathf.Abs(Vector3.Distance(transform.position, targetedPlayer.transform.position)) > 3))
                {
                    distances[i] += 50;
                }
                
                
            }
            float path = Mathf.Min(distances[0], distances[1], distances[2], distances[3]);
            prevPosition = transform.position;
            if (path == distances[0])
            {
                StartCoroutine(GridMove(gameObject, transform.position + Vector3.up, 0.2f, dungeonScript.timeModifier));
            }
            else if (path == distances[1])
            {
                StartCoroutine(GridMove(gameObject, transform.position + Vector3.left, 0.2f, dungeonScript.timeModifier));
            }
            else if (path == distances[2])
            {
                StartCoroutine(GridMove(gameObject, transform.position + Vector3.down, 0.2f, dungeonScript.timeModifier));
            }
            else if (path == distances[3])
            {
                StartCoroutine(GridMove(gameObject, transform.position + Vector3.right, 0.2f, dungeonScript.timeModifier));
            }
        }
        else if (action == "AttackPlayer")
        {
            if (type == "shoot")
            {
                GameObject test = Instantiate(dungeonScript.itemEffect, transform.position, Quaternion.identity);
                test.GetComponent<SpriteRenderer>().color = Color.cyan;
                StartCoroutine(test.GetComponent<ItemEffect>().Travel(transform.position + actionDirection * Mathf.Abs(Vector3.Distance(transform.position, targetedPlayer.transform.position)), 0.4f* Mathf.Abs(Vector3.Distance(transform.position, targetedPlayer.transform.position)), dungeonScript.timeModifier, Resources.Load<Sprite>("Sprites/Pew"), this));
            }
            else
            {
                StartCoroutine(AttackAnim(gameObject, 0.35f, actionDirection, dungeonScript.timeModifier));
            }
        }
        if (action == "Sleep")
        {
            EndEnemyTurn();
            slept = true;
        }
    }

    public void DamageEnemy(int amt)
    {
        health -= amt;
        if (health <= 0)
        {
            dungeonScript.turnCounterE = dungeonScript.turnCounterP;
            targetedPlayer.GetComponent<PlayerScript2D>().exp += statPool;
            Destroy(gameObject);
        }
        healthCounter.text = health + "/" + fullHealth + " " + attack;
    }

    public void EndEnemyTurn()
    {
        if (type == "speedy" && speedMove == true)
        {
            speedMove = false;
            EnemyTurn();
            
        }
        else
        {
            enemyAction = true;
            speedMove = true;
            prevTurnAdjacent = Array.IndexOf(WallChecker(gameObject), "Player") != -1;
            dungeonScript.enemyAlreadyTakingTurn = false;
        }
        
    }
}
