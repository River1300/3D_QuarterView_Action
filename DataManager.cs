using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    public PlayerData curPlayer = new PlayerData();

    public int curSlot;
    public string path;

    void Awake()
    {
        #region 싱글톤
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(instance.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
        #endregion
    
        path = Application.persistentDataPath + "/save";
    }

    void Start()
    {

    }

    public void SaveData()
    {
        string data = JsonUtility.ToJson(curPlayer);
        File.WriteAllText(path + curSlot.ToString(), data);
    }

    public void LoadData()
    {
        string data = File.ReadAllText(path + curSlot.ToString());
        curPlayer = JsonUtility.FromJson<PlayerData>(data);
    }

    public void DataClear()
    {
        curSlot = -1;
        curPlayer = new PlayerData();
    }
}

public class PlayerData
{
    public string name;
    public int stage = 1;
    public int score;
    public bool[] hasWeapon = new bool[3];
    public int ammo;
    public int grenade;
    public int coin = 1000;
}