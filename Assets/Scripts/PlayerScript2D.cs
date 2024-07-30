using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Text;
using Alteruna;

public class MoveableObject : MonoBehaviour
{
    public string[] WallChecker(GameObject obj, bool diag = false, int distance = 0)
    {
        string[] uldr = new string[] { "", "", "", "" };
        Vector3[] dirs = new Vector3[] { Vector3.up, Vector3.left, Vector3.down, Vector3.right };
        if (diag)
        {
            //upleft,downleft,downright,upright
            dirs = new Vector3[] { new Vector2 (-1,1), -Vector2.one, new Vector2(1,-1), Vector2.one };
        }
        if (obj.CompareTag("Player") && obj.GetComponent<PlayerScript2D>().ignoreWalls)
        {
            return uldr;
        }
        for (int i = 0; i < dirs.Length; i++)
        {
            RaycastHit2D hitData = Physics2D.Raycast(obj.transform.position + dirs[i] * 0.51f, dirs[i], 0.5f + distance);
            if (hitData.collider != null)
            {
                uldr[i] = hitData.collider.tag;
                if (hitData.collider.CompareTag("Switch") || hitData.collider.CompareTag("SaveArea"))
                {
                    uldr[i] = "";
                }
            }
        }
        return uldr;
    }

    public string[] WallChecker(Vector3 obj, bool diag = false, int distance = 0)
    {
        string[] uldr = new string[] { "", "", "", "" };
        Vector3[] dirs = new Vector3[] { Vector3.up, Vector3.left, Vector3.down, Vector3.right };
        if (diag)
        {
            //upleft,downleft,downright,upright
            dirs = new Vector3[] { new Vector2(-1, 1), -Vector2.one, new Vector2(1, -1), Vector2.one };
        }
        for (int i = 0; i < dirs.Length; i++)
        {
            RaycastHit2D hitData = Physics2D.Raycast(obj + dirs[i] * 0.51f, dirs[i], 0.5f + distance);
            if (hitData.collider != null)
            {
                uldr[i] = hitData.collider.tag;
                if (hitData.collider.CompareTag("Switch") || hitData.collider.CompareTag("SaveArea"))
                {
                    uldr[i] = "";
                }
            }
        }
        return uldr;
    }


    public IEnumerator AttackAnim(GameObject mover, float seconds, Vector3 direction, int modifier)
    {
        if (mover.CompareTag("Player"))
        {
            mover.GetComponent<PlayerScript2D>().moving = true;
        }
        float elapsedTime = 0f;
        Vector3 start = mover.transform.position;
        Vector3 end = mover.transform.position + direction * 0.5f;
        while (elapsedTime < seconds)
        {
            Vector3 data = Vector3.Lerp(start, end, (elapsedTime / seconds));
            mover.transform.position = new Vector3(data.x, data.y, mover.transform.position.z);
            elapsedTime += Time.deltaTime * modifier;
            yield return new WaitForEndOfFrame();
        }
        if (mover.CompareTag("Player"))
        {
            float critBonus = 1;
            if (UnityEngine.Random.Range(1,101) <= mover.GetComponent<PlayerScript2D>().invManager.crit)
            {
                if (UnityEngine.Random.Range(0, 10) == 9)
                {
                    critBonus = 4;
                    GameObject test = Instantiate(mover.GetComponent<PlayerScript2D>().dungeonScript.itemEffect, mover.transform.position + Vector3.up, Quaternion.identity);
                    StartCoroutine(test.GetComponent<ItemEffect>().FadeOut(1.5f, mover.GetComponent<PlayerScript2D>().invManager.critIcon2));
                }
                else
                {
                    critBonus = 1.5f;
                    GameObject test = Instantiate(mover.GetComponent<PlayerScript2D>().dungeonScript.itemEffect, mover.transform.position + Vector3.up, Quaternion.identity);
                    StartCoroutine(test.GetComponent<ItemEffect>().FadeOut(1.5f, mover.GetComponent<PlayerScript2D>().invManager.critIcon));
                }
            }
            mover.GetComponent<PlayerScript2D>().currentTarget.GetComponent<EnemyScript>().DamageEnemy(Mathf.CeilToInt((mover.GetComponent<PlayerScript2D>().invManager.attack + mover.GetComponent<PlayerScript2D>().invManager.tempAttack) * critBonus));
        }
        else if (mover.CompareTag("Enemy"))
        {
            if (mover.GetComponent<EnemyScript>().prevTurnAdjacent)
            {
                mover.GetComponent<EnemyScript>().targetedPlayer.GetComponent<PlayerScript2D>().invManager.Damage(Mathf.Max(mover.GetComponent<EnemyScript>().attack/2,1));
            }
            else
            {
                mover.GetComponent<EnemyScript>().targetedPlayer.GetComponent<PlayerScript2D>().invManager.Damage(mover.GetComponent<EnemyScript>().attack);
            }
        }    
        elapsedTime = 0f;
        while (elapsedTime < seconds)
        {
            Vector3 data = Vector3.Lerp(end, start, (elapsedTime / seconds));
            mover.transform.position = new Vector3(data.x, data.y, mover.transform.position.z);
            elapsedTime += Time.deltaTime * modifier;
            yield return new WaitForEndOfFrame();
        }
        if (mover.CompareTag("Player"))
        {
            mover.GetComponent<PlayerScript2D>().moving = false;
            mover.GetComponent<PlayerScript2D>().takenAction = true;
        }
        else if (mover.CompareTag("Enemy"))
        {
            mover.GetComponent<EnemyScript>().EndEnemyTurn();
        }
    }
    public IEnumerator GridMove(GameObject mover, Vector3 end, float seconds, int modifier)
    {
        Vector3 dir = end - mover.transform.position;
        if (mover.CompareTag("Player"))
        {
            if (mover.GetComponent<PlayerScript2D>().follower != null)
            {
                StartCoroutine(GridMove(mover.GetComponent<PlayerScript2D>().follower, transform.position, seconds, 1));
            }
            mover.GetComponent<PlayerScript2D>().moving = true;
        }
        Vector3 start = mover.transform.position;
        float elapsedTime = 0;
        while (elapsedTime < seconds)
        {
            Vector3 data = Vector3.Lerp(start, end, (elapsedTime / seconds));
            mover.transform.position = new Vector3(data.x, data.y, mover.transform.position.z);
            elapsedTime += Time.deltaTime * modifier;
            yield return new WaitForEndOfFrame();
        }
        mover.transform.position = new Vector3(Mathf.Round(mover.transform.position.x), Mathf.Round(mover.transform.position.y), mover.transform.position.z);
        if (mover.CompareTag("Player"))
        {
            mover.GetComponent<PlayerScript2D>().moving = false;
            mover.GetComponent<PlayerScript2D>().wallTouchList = WallChecker(mover);
            if (mover.GetComponent<PlayerScript2D>().follower != null && mover.GetComponent<PlayerScript2D>().follower.GetComponent<SignTextScript>() == null)
            {
                mover.GetComponent<PlayerScript2D>().follower = null;
            }
        }

        if (mover.CompareTag("Sign")) 
        {
            GameObject.Find("Player").GetComponent<PlayerScript2D>().vcam.Follow = GameObject.Find("Player").transform;
        }
        else if (mover.CompareTag("Enemy"))
        {
            mover.GetComponent<EnemyScript>().EndEnemyTurn();
        }
    }
}
[System.Serializable]
public class ItemSet
{

    [SerializeField]
    public Item item;
    public int amt;
    public ItemSet(Item item, int amt)
    {
        this.item = item;
        this.amt = amt;
    }

}



[System.Serializable]
public class SaveData
{
    public string saveTime;
    public string playerName;
    [SerializeField]
    public Vector2[] optionsData;
    [SerializeField]
    public List<ItemSet> inventory;
    [SerializeField]
    public List<ItemSet> chest;
    public int cpsOpen;
    public int playerLevel;
    public int[] powerList;


    public SaveData()
    {
        saveTime = "???";
        playerName = "???";
    }

    public void SaveVars(PlayerScript2D player)
    {
        saveTime = DateTime.Now.ToString();
        playerName = player.playerName;
        optionsData = player.menuManager.optionSelector.selections;
        cpsOpen = player.cpsOpen;
        playerLevel = player.playerLevel;
        powerList = player.invManager.PowerList();
        foreach (ItemScript i in player.invManager.inventory)
        {
            bool inInv = true;
            foreach (ItemSet s in inventory)
            {
                if (s.item.itemName == i.item.itemName)
                {
                    s.amt++;
                }
                else
                {
                    inInv = false;
                }
            }
            if (inventory.Count == 0 || !inInv)
            {
                inventory.Add(new ItemSet(i.item, 1));
            }
        }
        foreach (ItemScript i in player.invManager.chest)
        {
            bool inChest = true;
            foreach (ItemSet s in chest)
            {
                if (s.item.itemName == i.item.itemName)
                {
                    s.amt++;
                }
                else
                {
                    inChest = false;
                }
            }
            if (chest.Count == 0 || !inChest)
            {
                chest.Add(new ItemSet(i.item, 1));
            }
        }
    }
    public void SaveGame(PlayerScript2D player, bool copyText)
    {
        inventory = new();
        chest = new();
        SaveVars(player);
        string savePlayerData = JsonUtility.ToJson(this);
        if (copyText)
        {
            player.menuManager.OpenInput();
            player.menuManager.typeBox.text = Convert.ToBase64String(Encoding.UTF8.GetBytes(savePlayerData));
        }
        else
        {
            string saveFilePath = Application.persistentDataPath + "/PlayerData.json";
            File.WriteAllText(saveFilePath, savePlayerData);
        }
        
    }
    public (List<ItemSet>, List<ItemSet>) LoadGame(PlayerScript2D player, string data = "++12")
    {
        bool worked = false;
        if (data != "++12")
        {
            Span<byte> buffer = new(new byte[data.Length]);
            if (Convert.TryFromBase64String(data, buffer, out _))
            {
                byte[] decodedBytes = Convert.FromBase64String(data);
                string decodedString = Encoding.UTF8.GetString(decodedBytes);
                if (IsValidJson(decodedString))
                {
                    data = decodedString;
                    worked = true;
                }
            }
        }
        else
        {
            string saveFilePath = Application.persistentDataPath + "/PlayerData.json";
            if (File.Exists(saveFilePath))
            {
                data = File.ReadAllText(saveFilePath);
                worked = true;
            }
        }
        if (worked)
        {
            JsonUtility.FromJsonOverwrite(data, this);
            player.playerName = playerName;
            player.cpsOpen = cpsOpen;
            player.playerLevel = playerLevel;
            player.invManager.maxHp = powerList[0];
            player.invManager.attack = powerList[1];
            player.invManager.defense = powerList[2];
            player.invManager.regen = powerList[3];
            player.invManager.crit = powerList[4];

            if (optionsData == null)
            {
                optionsData = new Vector2[] { Vector2.zero, Vector2.up, new Vector2(3,2), new Vector2(1,3), new Vector2(2,4), new Vector2(2,5), new Vector2(1,6) };
            }
            player.optionsData = optionsData;
            return (inventory,chest);
        }
        
        return (null,null);
    }
    private bool IsValidJson(string jsonString)
    {
        try
        {
            JsonUtility.FromJsonOverwrite(jsonString, new object());
            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }
}




public class PlayerScript2D : MoveableObject
{
    private Alteruna.Avatar avatar;
    public SaveData save = new();
    public bool moving;
    public Vector3 direction;
    public bool sameDir;
    public Vector3 prevDir;
    public bool running;
    public float timeBetweenTiles;
    
    //current walls next to player

    public string[] wallTouchList;
    public bool ignoreWalls;
    //lets the player start dialogue
    public DialogueManager dialogueManager;
    public InventoryManager invManager;
    public MenuMapManager menuManager;
    //Tracks current dialogue instance and place in dialogue. dialogueData[0] is name, dialogueData[1] is position
    public GameObject currentTarget;
    public GameObject follower = null;

    public CinemachineVirtualCamera vcam;

    public bool isLoading;
    public bool inMap;
    public bool selectingItem;
    public GameObject selection = null;
    public string input = "gQprk73vInHt51GHQNA8rTtilfRaNiNTxjm00IUBFd3yeplTPJ";
    public bool typingStuff;
    public bool inDialogue;
    public bool inInventory;
    public bool inMenu;
    public bool inOptions;
    public bool inStats;
    public bool inRecords;

    public bool aboveTalker;
    public Vector2 spawnPoint;
    static PlayerScript2D instance;

    public GameObject itemDef;

    public string playerName;
    public Sprite[] idleSprites;

    public bool loader;
    public Vector2[] optionsData;
    public (bool,bool) doneee;

    public string oldScene;

    //new stuff
    public bool inDungeon;
    public int cpsOpen;

    public int exp;
    public int playerLevel;


    public bool playersTurn;
    public bool takenAction;

    public bool inChest;
    public bool inUpgrade;
    public bool selectingChest;
    public GameObject grid;
    public GameObject pointer;
    public DungeonActiveScript dungeonScript;


    public bool canSave;
    public bool infiniUp;

    void Awake()
    {
        avatar = GetComponent<Alteruna.Avatar>();
        if (!avatar.IsMe)
        {
            return;
        }
        //Cursor.visible = false;
        if (instance == null)
        {
            instance = this;
            if (PlayerPrefs.HasKey("name"))
            {
                playerName = PlayerPrefs.GetString("name");
                PlayerPrefs.DeleteAll();
            }
            DontDestroyOnLoad(gameObject);
            spawnPoint = Vector3.zero;
        }
    }
    void Start()
    {
        if (!avatar.IsMe)
        {
            return;
        }
        save = new();
        direction = Vector3.right;
        oldScene = SceneManager.GetActiveScene().name;
        timeBetweenTiles = 0.2f;
        prevDir = Vector3.zero;
        dialogueManager.eventScript.RunPastEvents();
        input = "gQprk73vInHt51GHQNA8rTtilfRaNiNTxjm00IUBFd3yeplTPJ";
        if (playerName == "LoadNameThatsLong")
        {
            PlayerEndLoadGame();
        }
        playersTurn = true;
        grid = GameObject.Find("Grid");
        dungeonScript = grid.GetComponent<DungeonActiveScript>();
        dungeonScript.players.Add(gameObject);

        playerLevel = 1;
        invManager.UpdateInfo();
    }
    void Update()
    {
        if (!avatar.IsMe)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (dungeonScript.timeModifier < 4)
            {
                dungeonScript.timeModifier++;
            }
            else
            {
                dungeonScript.timeModifier = 1;
            }
            invManager.UpdateInfo();
        }
        if (doneee.Item1 && doneee.Item2)
        {
            menuManager.optionSelector.SetData(optionsData);
            vcam.m_Lens.OrthographicSize = menuManager.optionSelector.selections[4].x + 3;
            GetComponent<AudioSource>().volume = menuManager.optionSelector.selections[5].x * 0.25f;
            doneee.Item1 = false;
        }
        if (!playersTurn)
        {
            bool allEnemiesDone = true;
            bool allEnemiesSlept = true;
            foreach (GameObject enemy in dungeonScript.ActiveSpawns("Enemy"))
            {
                if (!enemy.GetComponent<EnemyScript>().enemyAction)
                {
                    if (!dungeonScript.enemyAlreadyTakingTurn)
                    {
                        enemy.GetComponent<EnemyScript>().EnemyTurn();
                    }
                    allEnemiesDone = false;
                    break;
                }
            }
            if (allEnemiesDone)
            {
                foreach (GameObject enemy in dungeonScript.ActiveSpawns("Enemy"))
                {
                    if (!enemy.GetComponent<EnemyScript>().slept)
                    {
                        allEnemiesSlept = false;
                        break;
                    }
                }
                if (!allEnemiesSlept)
                {
                    dungeonScript.turnCounterE++;
                }
                playersTurn = true;
                takenAction = false;
                invManager.UpdateInfo();
                foreach (GameObject enemy in dungeonScript.ActiveSpawns("Enemy"))
                {
                    enemy.GetComponent<EnemyScript>().enemyAction = false;
                }
            }
        }
        else if (takenAction && !moving)
        {
            if (inDungeon)
            {
                dungeonScript.EndTurnActions();
                dungeonScript.turnCounterP++;
            }
            if (dungeonScript.ActiveSpawns("Enemy").Count > 0 && inDungeon)
            {
                playersTurn = false;
                if(dungeonScript.turnCounterP % (65 - invManager.regen*5) == 0 && invManager.regen > 0 && invManager.hp < invManager.maxHp)
                {
                    invManager.Damage(-1);
                }
                invManager.UpdateInfo();
            }
            else
            {
                invManager.UpdateInfo();
                takenAction = false;
            }
            if (transform.Find("Pointer(Clone)") != null)
            {
                GameObject pointer = transform.Find("Pointer(Clone)").gameObject;
                var dir = pointer.GetComponent<Pointer>().target.transform.position - pointer.transform.position;
                var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                pointer.transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
            }
        }
        else if (!isLoading && !inDialogue && !inMap && !inInventory && !inChest && !inOptions && !inMenu && !inStats && !inRecords && !typingStuff && playersTurn && !moving && !inUpgrade)
        {
            if (direction == Vector3.up)
            {
                GetComponent<SpriteRenderer>().sprite = idleSprites[0];
            }
            else if (direction == Vector3.left)
            {
                GetComponent<SpriteRenderer>().sprite = idleSprites[1];
            }
            else if (direction == Vector3.down)
            {
                GetComponent<SpriteRenderer>().sprite = idleSprites[2];
            }
            else if (direction == Vector3.right)
            {
                GetComponent<SpriteRenderer>().sprite = idleSprites[3];
            }
            GetPlayerMovement();
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M))
            {
                menuManager.OpenMenu();
            }
            else if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Q))
            {
                invManager.OpenInventory();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                invManager.OpenChest(invManager.chestPage);
            }
            else if (Input.GetKeyDown(KeyCode.U))
            {
                menuManager.OpenUpgrade();
            }
            //Cheat
            else if (Input.GetKeyDown(KeyCode.Equals))
            {
                ignoreWalls = !ignoreWalls;
                if (ignoreWalls)
                {
                    GetComponent<SpriteRenderer>().color = new Color32(255, 255,255, 125);
                }
                else
                {
                    GetComponent<SpriteRenderer>().color = Color.white;
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                exp += 10000;
                invManager.UpdateInfo();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                infiniUp = !infiniUp;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                GameObject test = Instantiate(dungeonScript.itemEffect, transform.position, Quaternion.identity);
                StartCoroutine(test.GetComponent<ItemEffect>().Travel(transform.position + direction * 3, 2, dungeonScript.timeModifier, GetComponent<SpriteRenderer>().sprite));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Instantiate(dungeonScript.items[6], transform.position + direction, Quaternion.identity);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                if (dungeonScript.dungeonLevel % 5 == 0)
                {
                    GameObject star = Instantiate(dungeonScript.stairs2, transform.position + direction, Quaternion.identity);
                    star.transform.parent = dungeonScript.spawns.transform;
                }
                else
                {
                    GameObject stair = Instantiate(dungeonScript.stairs, transform.position + direction, Quaternion.identity);
                    stair.transform.parent = dungeonScript.spawns.transform;
                }
                
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                string[] lsit = WallChecker(gameObject, false, 2);
                string output = "|";
                foreach (string s in lsit)
                {
                    output += s + "|";
                }
                Debug.Log(output);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                RaycastHit2D hitData = Physics2D.Raycast(transform.position + direction * 0.51f, direction, 0.5f);
                if (hitData.collider != null)
                {
                    currentTarget = hitData.collider.gameObject;
                    Interact(currentTarget);
                }
            }
            else if (Input.GetKey(KeyCode.Space) && dungeonScript.ActiveSpawns("Enemy").Count > 0)
            {
                takenAction = true;
            }
        }
        else if (inDialogue && !selectingItem && !typingStuff) //Controls for in dialogue
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (dialogueManager.typing)
                {
                    dialogueManager.typing = false;
                }
                else if (!dialogueManager.typing && !dialogueManager.question)
                {
                    dialogueManager.sentences.RemoveAt(0);
                    dialogueManager.eventScript.EndEventTrigger();
                    if (!dialogueManager.changed)
                    {
                        dialogueManager.DisplayNextSentence();
                    }
                    dialogueManager.changed = false;
                }
            }
            if (Input.GetKeyDown(KeyCode.Y) && dialogueManager.question && !dialogueManager.typing)
            {
                dialogueManager.sentences.RemoveAt(0);
                dialogueManager.eventScript.EndEventTrigger(1);
                if (!dialogueManager.changed)
                {
                    dialogueManager.DisplayNextSentence();
                }
                dialogueManager.changed = false;
            }
            if (Input.GetKeyDown(KeyCode.N) && dialogueManager.question && !dialogueManager.typing)
            {
                dialogueManager.sentences.RemoveAt(0);
                dialogueManager.eventScript.EndEventTrigger(2);
                if (!dialogueManager.changed)
                {
                    dialogueManager.DisplayNextSentence();
                }
                dialogueManager.changed = false;
            }

        }
        else if (inInventory)
        {
            aboveTalker = invManager.selector.selectorPos.y != 0;
            if ((Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.X)) && !selectingItem && !selectingChest)
            {
                invManager.CloseInventory();
            }
            GetSelectorMovement(invManager.selector);
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                invManager.selector.prevSelection = invManager.selector.selection;
                invManager.selector.selection = invManager.selector.selectorPos;
                invManager.selector.UpdateSelector();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (invManager.selector.selection.y * invManager.selector.width + invManager.selector.selection.x < invManager.UnusedCount(invManager.inventory))
                {
                    ItemScript itemScript = invManager.ReturnUnusedInv(invManager.inventory)[Mathf.RoundToInt(invManager.selector.selection.y) * invManager.selector.width + Mathf.RoundToInt(invManager.selector.selection.x)].GetComponent<ItemScript>();
                    dialogueManager.StartDialogue(itemScript.gameObject.name, new string[] { itemScript.item.itemLore }, 0, GetComponent<SpriteRenderer>().sprite);
                }
                else
                {
                    string[] temp = new string[] { "0You:There's nothing here :(" };
                    dialogueManager.StartDialogue("Player", temp, 0, GetComponent<SpriteRenderer>().sprite);
                }
            }
            if (Input.GetKeyDown(KeyCode.E) && !selectingItem && !selectingChest)
            {
                if (invManager.selector.selection.y * invManager.selector.width + invManager.selector.selection.x < invManager.UnusedCount(invManager.inventory))
                {
                    ItemScript itemScript = invManager.ReturnUnusedInv(invManager.inventory)[Mathf.RoundToInt(invManager.selector.selection.y) * invManager.selector.width + Mathf.RoundToInt(invManager.selector.selection.x)].GetComponent<ItemScript>();
                    invManager.UseItem(itemScript);
                }
                else
                {
                    string[] temp = new string[] { "0You: Can't use Nothing :(" };
                    dialogueManager.StartDialogue("Player", temp, 0, GetComponent<SpriteRenderer>().sprite);
                }
            }
            if (Input.GetKeyDown(KeyCode.E) && selectingItem)
            {
                selectingItem = false;
                if (invManager.selector.selection.y * invManager.selector.width + invManager.selector.selection.x < invManager.UnusedCount(invManager.inventory))
                {
                    selection = invManager.ReturnUnusedInv(invManager.inventory)[Mathf.RoundToInt(invManager.selector.selection.y) * invManager.selector.width + Mathf.RoundToInt(invManager.selector.selection.x)].gameObject;
                }
                else
                {
                    selection = currentTarget;
                }
                invManager.CloseInventory();
                dialogueManager.eventScript.EndEventTrigger();
            }
            if (Input.GetKeyDown(KeyCode.E) && selectingChest)
            {
                selectingChest = false;
                ItemScript item = null;
                if (invManager.selector.selection.y * invManager.selector.width + invManager.selector.selection.x < invManager.UnusedCount(invManager.inventory))
                {
                    item = invManager.ReturnUnusedInv(invManager.inventory)[Mathf.RoundToInt(invManager.selector.selection.y) * invManager.selector.width + Mathf.RoundToInt(invManager.selector.selection.x)];
                }
                if (item != null)
                {
                    invManager.inventory.Remove(item);
                    invManager.chest.Add(item);
                }
                invManager.CloseInventory();
                invManager.OpenChest(invManager.chestPage);
            }
            if (Input.GetKeyDown(KeyCode.R) && !selectingItem && !selectingChest)
            {
                string[] walls = WallChecker(gameObject);
                bool frontClear = (walls[0] == "" && direction == Vector3.up) || (walls[1] == "" && direction == Vector3.left) || (walls[2] == "" && direction == Vector3.down) || (walls[3] == "" && direction == Vector3.right);
                if ((invManager.selector.selection.y * invManager.selector.width + invManager.selector.selection.x < invManager.UnusedCount(invManager.inventory) && frontClear))
                {

                    ItemScript itemScript = invManager.ReturnUnusedInv(invManager.inventory)[Mathf.RoundToInt(invManager.selector.selection.y) * invManager.selector.width + Mathf.RoundToInt(invManager.selector.selection.x)];
                    if (itemScript.item.itemName == "Lantern" || itemScript.item.itemName == "???" || itemScript.item.itemName == "Miep's Trust" || itemScript.item.itemName == "Map")
                    {
                        string[] temp = new string[] { "0You:This seems too important to drop." };
                        dialogueManager.StartDialogue("Player", temp, 0, GetComponent<SpriteRenderer>().sprite);
                    }
                    else
                    {
                        itemScript.gameObject.transform.parent = grid.transform;
                        itemScript.gameObject.transform.parent = null;
                        itemScript.gameObject.SetActive(true);
                        itemScript.transform.position = new Vector3(transform.position.x + direction.x, transform.position.y + direction.y, itemScript.transform.position.z);
                        invManager.inventory.Remove(itemScript);
                        invManager.OpenInventory();
                    }
                }
                else if (invManager.selector.selection.y * invManager.selector.width + invManager.selector.selection.x >= invManager.UnusedCount(invManager.inventory))
                {
                    string[] temp = new string[] { "0You:Can't drop nothing :(" };
                    dialogueManager.StartDialogue("Player", temp, 0, GetComponent<SpriteRenderer>().sprite);
                }
                else
                {
                    string[] temp = new string[] { "0You:Not enough space to drop this. I should move to a better spot." };
                    dialogueManager.StartDialogue("Player", temp, 0, GetComponent<SpriteRenderer>().sprite);
                }

            }
        }
        else if (inChest)
        {
            aboveTalker = invManager.chestSelector.selectorPos.y != 0;
            for (int i = 0; i < 9; i++)
            {
                if (Input.GetKeyDown((KeyCode)(49 + i)))
                {
                    invManager.CloseChest();
                    invManager.chestPage = i;
                    invManager.OpenChest(invManager.chestPage);
                }
            }
            if ((Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.X)))
            {
                invManager.CloseChest();
            }
            GetSelectorMovement(invManager.chestSelector);
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                invManager.chestSelector.prevSelection = invManager.chestSelector.selection;
                invManager.chestSelector.selection = invManager.chestSelector.selectorPos;
                invManager.chestSelector.UpdateSelector();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if ((invManager.chestPage * 15) + invManager.chestSelector.selection.y * invManager.chestSelector.width + invManager.chestSelector.selection.x < invManager.UnusedCount(invManager.chest))
                {
                    ItemScript itemScript = invManager.ReturnUnusedInv(invManager.chest)[(invManager.chestPage * 15) + Mathf.RoundToInt(invManager.chestSelector.selection.y) * invManager.chestSelector.width + Mathf.RoundToInt(invManager.chestSelector.selection.x)].GetComponent<ItemScript>();
                    dialogueManager.StartDialogue(itemScript.gameObject.name, new string[] { itemScript.item.itemLore }, 0, GetComponent<SpriteRenderer>().sprite);
                }
                else
                {
                    string[] temp = new string[] { "0You:There's nothing here :(" };
                    dialogueManager.StartDialogue("Player", temp, 0, GetComponent<SpriteRenderer>().sprite);
                }
            }
            if (Input.GetKeyDown(KeyCode.E) && !selectingItem)
            {
                if ((invManager.chestPage * 15) + invManager.chestSelector.selection.y * invManager.chestSelector.width + invManager.chestSelector.selection.x < invManager.UnusedCount(invManager.chest))
                {
                    ItemScript itemScript = invManager.ReturnUnusedInv(invManager.chest)[(invManager.chestPage * 15) + Mathf.RoundToInt(invManager.chestSelector.selection.y) * invManager.chestSelector.width + Mathf.RoundToInt(invManager.chestSelector.selection.x)].GetComponent<ItemScript>();
                    if (invManager.UnusedCount(invManager.inventory) == invManager.selector.width * invManager.selector.height)
                    {
                        string[] temp = new string[] { "0You: I don't have any room left in my Inventory." };
                        dialogueManager.StartDialogue("Player", temp, 0, GetComponent<SpriteRenderer>().sprite);
                    }
                    else
                    {
                        invManager.inventory.Add(itemScript);
                        invManager.chest.Remove(itemScript);
                        invManager.CloseChest();
                        invManager.OpenChest(invManager.chestPage);
                    }
                }
                else
                {
                    selectingChest = true;
                    invManager.OpenInventory();
                    invManager.CloseChest();
                }
            }
        }
        else if (inMenu)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.X))
            {
                menuManager.CloseMenu();
            }
            //Cheat
            if (Input.GetKeyDown(KeyCode.R))
            {
                menuManager.CloseMenu();
                switch (Mathf.RoundToInt(menuManager.menuSelector.selectorPos.y))
                {
                    case 4:
                        if (canSave)
                        {
                            save.SaveGame(this, true);
                        }
                        else
                        {
                            dialogueManager.StartDialogue("Player", new string[] { "0You: Can't save here." }, 0, GetComponent<SpriteRenderer>().sprite);
                        }
                        
                        break;
                    case 5:
                        loader = true;
                        menuManager.OpenInput();
                        break;
                }
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                menuManager.CloseMenu();
                switch (Mathf.RoundToInt(menuManager.menuSelector.selectorPos.y))
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        menuManager.OpenOptions();
                        break;
                    case 3:
                        if (inDungeon)
                        {
                            dialogueManager.StartDialogue("SadExit", new string[] { "0You: I will lose all my items and EXP if I leave like this. Is that fine? (Y/N)" }, 0, GetComponent<SpriteRenderer>().sprite);
                        }
                        else
                        {
                            dialogueManager.StartDialogue("Player", new string[] { "0You: Not in a dungeon." }, 0, GetComponent<SpriteRenderer>().sprite);
                            menuManager.OpenMenu();
                        }
                        break;
                    case 4:
                        if (canSave)
                        {
                            save.SaveGame(this, false);
                        }
                        else
                        {
                            dialogueManager.StartDialogue("Player", new string[] { "0You: Can't save here." }, 0, GetComponent<SpriteRenderer>().sprite);
                        }
                        menuManager.OpenMenu();
                        break;
                    case 5:
                        bool loaded = PlayerEndLoadGame();
                        if (!loaded)
                        {
                            dialogueManager.StartDialogue("Player", new string[] { "0You: There is no save data to load from..." }, 0, GetComponent<SpriteRenderer>().sprite);
                        }
                        else
                        {
                            menuManager.OpenMenu();
                        }
                        break;
                    case 6:
                        save.SaveGame(this,false);
                        SceneManager.LoadScene("TitleScreen");
                        Destroy(gameObject);
                        break;
                }
            }
            GetSelectorMovement(menuManager.menuSelector);
        }
        else if (inOptions)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.M))
            {
                menuManager.CloseOptions();
                menuManager.OpenMenu();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (menuManager.optionSelector.selections[Mathf.RoundToInt(menuManager.optionSelector.selectorPos.y)] != menuManager.optionSelector.selectorPos)
                {
                    menuManager.optionSelector.prevSelections[Mathf.RoundToInt(menuManager.optionSelector.selectorPos.y)] = menuManager.optionSelector.selections[Mathf.RoundToInt(menuManager.optionSelector.selectorPos.y)];
                    menuManager.optionSelector.selections[Mathf.RoundToInt(menuManager.optionSelector.selectorPos.y)] = menuManager.optionSelector.selectorPos;
                    menuManager.optionSelector.UpdateSelector();
                }
                if (menuManager.optionSelector.selectorPos == Vector2.zero)
                {
                    menuManager.CloseOptions();
                    menuManager.OpenMenu();
                }
                if (menuManager.optionSelector.selectorPos.y == 4)
                {
                    vcam.m_Lens.OrthographicSize = menuManager.optionSelector.selectorPos.x + 3;
                }
                if (menuManager.optionSelector.selectorPos.y == 5)
                {
                    GetComponent<AudioSource>().volume = menuManager.optionSelector.selectorPos.x * 0.25f;
                }
            }
            GetSelectorMovement(menuManager.optionSelector);
        }
        else if (typingStuff)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Escape))
            {
                menuManager.typeBox.ActivateInputField();
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                typingStuff = false;
                input = menuManager.typeBox.text;
                menuManager.CloseInput();
                if (currentTarget != null && !loader)
                {
                    dialogueManager.eventScript.EndEventTrigger();
                }
                else if (loader)
                {
                    PlayerEndLoadGame(input);
                }
            }
        }
        else if (inUpgrade)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.U) || Input.GetKeyDown(KeyCode.X))
            {
                menuManager.CloseUpgrade();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (exp < ExpNeeded())
                {
                    dialogueManager.StartDialogue("Player", new string[] { "0You: I don't have enough exp to level up." }, 0, GetComponent<SpriteRenderer>().sprite);
                }
                else
                {
                    switch (Mathf.RoundToInt(menuManager.upgradeSelector.selectorPos.y * menuManager.upgradeSelector.textArray[0].Length + menuManager.upgradeSelector.selectorPos.x))
                    {
                        case 0:
                            invManager.maxHp += 5;
                            invManager.hp += 5;
                            invManager.Damage(0);
                            break;
                        case 1:
                            invManager.attack += 1;
                            break;
                        case 2:
                            invManager.defense += 1;
                            break;
                        case 3:
                            invManager.regen += 1;
                            break;
                        case 4:
                            invManager.crit += 10;
                            break;
                        default:
                            break;
                    }
                    exp -= ExpNeeded();
                    playerLevel++;
                    menuManager.CloseUpgrade();
                    menuManager.OpenUpgrade();
                    invManager.UpdateInfo();
                }
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                switch (Mathf.RoundToInt(menuManager.upgradeSelector.selectorPos.y * menuManager.upgradeSelector.textArray[0].Length + menuManager.upgradeSelector.selectorPos.x))
                {
                    case 0:
                        dialogueManager.StartDialogue("Player", new string[] { "0You: Increases Max HP by 5 and heals for 5." }, 0, GetComponent<SpriteRenderer>().sprite);
                        break;
                    case 1:
                        dialogueManager.StartDialogue("Player", new string[] { "0You: Increases base attack damage by 1." }, 0, GetComponent<SpriteRenderer>().sprite);
                        break;
                    case 2:
                        dialogueManager.StartDialogue("Player", new string[] { "0You: Increases Defense by 1. Defense has aa 50% chance of blocking damage if the damage is less than or equal to the defense." }, 0, GetComponent<SpriteRenderer>().sprite);
                        break;
                    case 3:
                        dialogueManager.StartDialogue("Player", new string[] { "0You: Increases Regen by one. While in the dungeon and at least one regen, every certain amount of turns you will heal 1HP. The certain amount of turns goes down with the more regen." }, 0, GetComponent<SpriteRenderer>().sprite);
                        break;
                    case 4:
                        dialogueManager.StartDialogue("Player", new string[] { "0You: Increases Crit Chance by 10%. If lucky enough to roll a crit, an attack will deal 1.5x damage rounded up.", "0You: If you roll a crit, there is also a 10% chance that the crit attack does quadruple damage instead." }, 0, GetComponent<SpriteRenderer>().sprite);
                        break;
                    default:
                        dialogueManager.StartDialogue("Player", new string[] { "0You: Nothing here yet." }, 0, GetComponent<SpriteRenderer>().sprite);
                        break;
                }
            }
            GetSelectorMovement(menuManager.upgradeSelector);
        }

    }
    public void GetSelectorMovement(Selector selector)
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            selector.prevSelectorPos = selector.selectorPos;
            if (selector.selectorPos.y > 0)
            {
                selector.selectorPos.y -= 1;
            }
            else
            {
                selector.selectorPos.y += selector.textArray.Length - 1;
            }
            if (selector.textArray[Mathf.RoundToInt(selector.selectorPos.y)].Length - 1 < selector.selectorPos.x)
            {
                selector.selectorPos.x = selector.textArray[Mathf.RoundToInt(selector.selectorPos.y)].Length - 1;
            }
            selector.UpdateSelector();
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selector.prevSelectorPos = selector.selectorPos;
            if (selector.selectorPos.x > 0)
            {
                selector.selectorPos.x -= 1;
            }
            else
            {
                selector.selectorPos.x += selector.textArray[Mathf.RoundToInt(selector.selectorPos.y)].Length - 1;
            }
            selector.UpdateSelector();
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            selector.prevSelectorPos = selector.selectorPos;
            if (selector.selectorPos.y < selector.textArray.Length - 1)
            {
                selector.selectorPos.y += 1;
            }
            else
            {
                selector.selectorPos.y -= selector.textArray.Length - 1;
            }
            if (selector.textArray[Mathf.RoundToInt(selector.selectorPos.y)].Length - 1 < selector.selectorPos.x)
            {
                selector.selectorPos.x = selector.textArray[Mathf.RoundToInt(selector.selectorPos.y)].Length - 1;
            }
            selector.UpdateSelector();
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            selector.prevSelectorPos = selector.selectorPos;
            
            if (selector.selectorPos.x < selector.textArray[Mathf.RoundToInt(selector.selectorPos.y)].Length - 1)
            {
                selector.selectorPos.x += 1;
            }
            else
            {
                selector.selectorPos.x -= selector.textArray[Mathf.RoundToInt(selector.selectorPos.y)].Length - 1;
            }
            selector.UpdateSelector();
        }
    }

    public void EnterDungeon(string action = "")
    {
        StopAllCoroutines();
        moving = false;
        invManager.tempAttack = 0;
        invManager.tempDefense = 0;
        if (transform.Find("Pointer(Clone)") != null)
        {
            Destroy(transform.Find("Pointer(Clone)").gameObject);
        }
        if (action == "safe")
        {   if (dungeonScript.inSafeRoom || canSave)
            {
                dungeonScript.dungeonLevel += 5;
            }
            dungeonScript.inSafeRoom = true;
            inDungeon = true;
            transform.position = new Vector3(-153, 10, 0);
            for (int i = dungeonScript.ActiveSpawns("all").Count - 1; i >= 0; i--)
            {
                Destroy(dungeonScript.ActiveSpawns("all")[i]);
            }
            if (cpsOpen > dungeonScript.dungeonLevel / 5)
            {
                GameObject star = Instantiate(dungeonScript.stairs2, new Vector3(-153, 12, 0), Quaternion.identity);
                star.transform.parent = dungeonScript.spawns.transform;
            }
            else
            {
                cpsOpen = dungeonScript.dungeonLevel / 5;
                if (infiniUp)
                {
                    GameObject star = Instantiate(dungeonScript.stairs2, new Vector3(-153, 12, 0), Quaternion.identity);
                    star.transform.parent = dungeonScript.spawns.transform;
                }
            }
        }
        else
        {
            dungeonScript.inSafeRoom = false;
            Vector2Int s = grid.GetComponent<DungeonGenScript>().GenerateDungeon(new Vector2Int(50 + dungeonScript.dungeonLevel /10 *5, 50 + dungeonScript.dungeonLevel / 10 * 5), 7 + dungeonScript.dungeonLevel /10);
            transform.position = new Vector3(s.x, s.y, transform.position.z);
            if (inDungeon)
            {
                for (int i = dungeonScript.ActiveSpawns("all").Count - 1; i >= 0; i--)
                {
                    Destroy(dungeonScript.ActiveSpawns("all")[i]);
                }
                dungeonScript.dungeonLevel++;
            }
            else
            {
                dungeonScript.dungeonLevel = 1;
            }
            dungeonScript.Setup();
            inDungeon = true;
        }
    }
    public int ExpNeeded()
    {
        return Mathf.RoundToInt(Mathf.Pow(playerLevel / 0.4f, 1.95f));
    }
    public void LeaveDungeon(string action)
    {
        inDungeon = false;
        dungeonScript.dungeonLevel = 0;
        invManager.tempAttack = 0;
        invManager.tempDefense = 0;
        StopAllCoroutines();
        moving = false;
        transform.position = new Vector3(-153, -13, 0);
        for (int i = dungeonScript.ActiveSpawns("all").Count - 1; i >= 0; i--)
        {
            Destroy(dungeonScript.ActiveSpawns("all")[i]);
        }
        if (transform.Find("Pointer(Clone)") != null)
        {
            Destroy(transform.Find("Pointer(Clone)").gameObject);
        }
        if (cpsOpen > 0)
        {
            GameObject star = Instantiate(dungeonScript.stairs2, new Vector3(-153, -11, 0), Quaternion.identity);
            star.transform.parent = dungeonScript.spawns.transform;
        }
        dungeonScript.enemyAlreadyTakingTurn = false;
        if (action == "died")
        {
            string[] temp2 = new string[] { "0You: Oh no i dieded. Oops dropped all my stuff sadghe." };
            dialogueManager.StartDialogue("Player", temp2, 0, GetComponent<SpriteRenderer>().sprite);
            invManager.inventory.Clear();
            invManager.hp = invManager.maxHp;
            exp = 0;
        }
        else if (action == "abandon")
        {
            string[] temp2 = new string[] { "0You: Oh no im too scared. Oops dropped all my stuff sadghe." };
            dialogueManager.StartDialogue("Player", temp2, 0, GetComponent<SpriteRenderer>().sprite);
            invManager.inventory.Clear();
            invManager.hp = invManager.maxHp;
            exp = 0;
        }
        else if (action == "safe")
        {
            string[] temp2 = new string[] { "0You: Ayy I escape d. I didn;t lose studd this time!!" };
            dialogueManager.StartDialogue("Player", temp2, 0, GetComponent<SpriteRenderer>().sprite);
            invManager.hp = invManager.maxHp;
            exp = 0;
        }
        else if (action == "load")
        {
            invManager.hp = invManager.maxHp;
            exp = 0;
        }
        invManager.Damage(0);
        invManager.UpdateInfo();
    }
    public void GetPlayerMovement()
    {
        if (Input.GetKey(KeyCode.W))
        {
            direction = Vector3.up;
            GetComponent<SpriteRenderer>().sprite = idleSprites[0];
            wallTouchList = WallChecker(gameObject);
            bool swapFollower = (follower != null && follower.transform.position.x == (transform.position + direction).x && follower.transform.position.y == (transform.position + direction).y);
            if ((wallTouchList[0] == "" || swapFollower))
            {
                sameDir = prevDir == Vector3.up && !sameDir;
                prevDir = direction;
                StartCoroutine(GridMove(gameObject,transform.position + direction, timeBetweenTiles, dungeonScript.timeModifier));
                takenAction = true;
            }
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction = Vector3.left;
            GetComponent<SpriteRenderer>().sprite = idleSprites[1];
            wallTouchList = WallChecker(gameObject);
            bool swapFollower = (follower != null && follower.transform.position.x == (transform.position + direction).x && follower.transform.position.y == (transform.position + direction).y);
            if ((wallTouchList[1] == "" || swapFollower))
            {
                sameDir = prevDir == Vector3.left && !sameDir;
                prevDir = direction;
                StartCoroutine(GridMove(gameObject,transform.position + direction, timeBetweenTiles, dungeonScript.timeModifier));
                takenAction = true;
            }
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction = Vector3.down;
            GetComponent<SpriteRenderer>().sprite = idleSprites[2];
            wallTouchList = WallChecker(gameObject);
            bool swapFollower = (follower != null && follower.transform.position.x == (transform.position + direction).x && follower.transform.position.y == (transform.position + direction).y);
            if ((wallTouchList[2] == "" || swapFollower))
            {
                sameDir = prevDir == Vector3.down && !sameDir;
                prevDir = direction;
                StartCoroutine(GridMove(gameObject,transform.position + direction, timeBetweenTiles, dungeonScript.timeModifier));
                takenAction = true;
            }
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction = Vector3.right;
            GetComponent<SpriteRenderer>().sprite = idleSprites[3];
            wallTouchList = WallChecker(gameObject);
            bool swapFollower = (follower != null && follower.transform.position.x == (transform.position + direction).x && follower.transform.position.y == (transform.position + direction).y);
            if ((wallTouchList[3] == "" || swapFollower))
            {
                sameDir = prevDir == Vector3.right && !sameDir;
                prevDir = direction;
                StartCoroutine(GridMove(gameObject,transform.position + direction, timeBetweenTiles, dungeonScript.timeModifier));
                takenAction = true;
            }

        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction = Vector3.up;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direction = Vector3.left;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            direction = Vector3.down;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            direction = Vector3.right;
        }
    }
    public void Interact(GameObject target)
    {
        switch (target.tag)
        {
            case "Sign":
                SignTextScript signScript = target.GetComponent<SignTextScript>();
                dialogueManager.StartDialogue(signScript.name, signScript.dialogue, signScript.talkCounter, signScript.talkerImage);
                break;
            case "Item":
                if (invManager.UnusedCount(invManager.inventory) == invManager.selector.width * invManager.selector.height)
                {
                    string[] temp = new string[] { "0You:I don't have any room left to pick items up. I should drop or use one." };
                    dialogueManager.StartDialogue("Player", temp, 0, GetComponent<SpriteRenderer>().sprite);
                }
                else
                {
                    target.transform.parent = transform;
                    invManager.inventory.Add(target.GetComponent<ItemScript>());
                    //ItemScript itemScript = target.GetComponent<ItemScript>();
                    //dialogueManager.StartDialogue(itemScript.gameObject.name, new string[] { itemScript.item.pickupText }, 0, itemScript.GetComponent<SpriteRenderer>().sprite);
                    target.SetActive(false);
                }
                break;
            case "Enemy":
                StartCoroutine(AttackAnim(gameObject, 0.35f, direction, dungeonScript.timeModifier));
                break;
            case "Wall":
                if (dungeonScript.gameObject.GetComponent<DungeonGenScript>().wallTile == dungeonScript.gameObject.GetComponent<DungeonGenScript>().walls.GetTile(new Vector3Int((int)transform.position.x -1, (int)transform.position.y - 1) + new Vector3Int((int)direction.x, (int)direction.y)))
                {
                    if (HasItem("Hammer",true))
                    {
                        ItemScript hammer = GetItem("Hammer", true);
                        if (Int32.Parse(hammer.item.itemName.Substring(8,1)) == 1)
                        {
                            invManager.inventory.Remove(hammer);
                            GameObject test = Instantiate(dungeonScript.itemEffect, transform.position + Vector3.up, Quaternion.identity);
                            StartCoroutine(test.GetComponent<ItemEffect>().FadeOut(1.5f, hammer.gameObject.GetComponent<SpriteRenderer>().sprite));
                            Destroy(hammer.gameObject);
                        }
                        else
                        {
                            hammer.item.itemName = "Hammer (" + (Int32.Parse(hammer.item.itemName.Substring(8, 1)) - 1) + ")";
                        }
                        dungeonScript.gameObject.GetComponent<DungeonGenScript>().walls.SetTile(new Vector3Int((int)transform.position.x - 1, (int)transform.position.y - 1) + new Vector3Int((int)direction.x, (int)direction.y), null);
                        dungeonScript.gameObject.GetComponent<DungeonGenScript>().tilemap.SetTile(new Vector3Int((int)transform.position.x - 1, (int)transform.position.y - 1) + new Vector3Int((int)direction.x, (int)direction.y), dungeonScript.gameObject.GetComponent<DungeonGenScript>().tile);
                    }
                }
                else
                {
                    Debug.Log("BaddWall");
                }
                break;
            default:
                currentTarget = null;
                break;
        }
        invManager.UpdateInfo();

    }
    
    public bool PlayerEndLoadGame(string load = "++12")
    {
        (List<ItemSet>, List<ItemSet>) execData = save.LoadGame(this, load);
        if (execData.Item1 == null)
        {
            return false;
        }
        invManager.inventory.Clear();
        invManager.chest.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).CompareTag("Item") && !transform.GetChild(i).GetComponent<ItemScript>().item.used)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        foreach (ItemSet i in execData.Item1)
        {
            for (int j = 0; j < i.amt; j++)
            {
                GameObject item = Instantiate(itemDef, transform);
                item.name = i.item.itemObjName;
                item.GetComponent<ItemScript>().item = i.item;
                item.GetComponent<SpriteRenderer>().sprite = (Sprite)Resources.Load("Sprites/" + i.item.path, typeof(Sprite));
                invManager.inventory.Add(item.GetComponent<ItemScript>());
                item.SetActive(false);
            }
            
        }
        foreach (ItemSet i in execData.Item2)
        {
            for (int j = 0; j < i.amt; j++)
            {
                GameObject item2 = Instantiate(itemDef, transform);
                item2.name = i.item.itemObjName;
                item2.GetComponent<ItemScript>().item = i.item;
                item2.GetComponent<SpriteRenderer>().sprite = (Sprite)Resources.Load("Sprites/" + i.item.path, typeof(Sprite));
                invManager.chest.Add(item2.GetComponent<ItemScript>());
                item2.SetActive(false);
            }
        }
        doneee.Item2 = true;
        LeaveDungeon("load");
        return true;
    }
    public bool HasItem(string name, bool contatins = false)
    {
        foreach (ItemScript g in invManager.inventory)
        {
            if (g.item.itemName == name || (g.item.itemName.Contains(name) && contatins))
            {
                return true;
            }
        }
        return false;
    }
    public ItemScript GetItem(string name, bool contatins = false)
    {
        foreach (ItemScript g in invManager.inventory)
        {
            if (g.item.itemName == name || (g.item.itemName.Contains(name) && contatins))
            {
                return g;
            }
        }
        return null;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("SaveArea"))
        {
            canSave = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("SaveArea"))
        {
            canSave = false;
        }
    }
}

