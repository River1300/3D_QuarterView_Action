using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject menuCam;
    public GameObject gameCam;
    public GameObject menuPanel;
    public GameObject gamePanel;
    public GameObject overPanel;
    public GameObject searchPanel;
    public GameObject notice;
    public GameObject itemShop;
    public GameObject weaponShop;
    public GameObject startZone;
    public Transform[] enemyZones;
    public GameObject[] enemies;
    public List<int> enemyList;

    public Player player;
    public Boss boss;

    public RectTransform bossHealthGroup;
    public RectTransform bossHealthBar;

    public int stage;
    public float playTime;

    public Text maxScoreTxt;
    public Text scoreTxt;
    public Text stageTxt;
    public Text playTimeTxt;

    public Text playerHealthTxt;
    public Text playerAmmoTxt;
    public Text playerCoinTxt;

    public int enemyCntA;
    public int enemyCntB;
    public int enemyCntC;
    public int enemyCntD;

    public Text enemyATxt;
    public Text enemyBTxt;
    public Text enemyCTxt;

    public Image weapon1Img;
    public Image weapon2Img;
    public Image weapon3Img;
    public Image weaponRImg;

    public Text curScoreTxt;
    public Text bestTxt;

    public bool isBattle;

    public List<Spawn> spawnList;
    public int spawnIndex;
    public bool spawnEnd;

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

        enemyList = new List<int>();
        spawnList = new List<Spawn>();

        maxScoreTxt.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore"));
    }

    void Update()
    {
        if(isBattle)
            playTime += Time.deltaTime;
    }

    void LateUpdate()
    {
        if(boss != null)
        {
            bossHealthGroup.anchoredPosition = Vector3.zero;
            bossHealthBar.localScale = new Vector3((float)boss.curHealth / boss.maxHealth, 1, 1);
        }
        else
        {
            bossHealthGroup.anchoredPosition = Vector3.up * 300;
        }

        scoreTxt.text = string.Format("{0:n0}", player.score);

        stageTxt.text = "STAGE " + stage;
        int hour = (int)(playTime / 3600);
        int min = (int)((playTime - hour * 3600) / 60);
        int second = (int)(playTime % 60);
        playTimeTxt.text = string.Format("{0:00}", hour) + ":" + 
            string.Format("{0:00}", min) + ":" + string.Format("{0:00}", second);

        playerHealthTxt.text = string.Format(player.health + " / " + player.maxHealth);
        playerCoinTxt.text = string.Format("{0:n0}", player.coin);
        if(player.equipWeapon == null || player.equipWeapon.type == Weapon.Type.Melee)
        {
            playerAmmoTxt.text = string.Format("- / " + player.ammo);
        }
        else
        {
            playerAmmoTxt.text = player.equipWeapon.curAmmo + " / " + player.ammo;
        }

        weapon1Img.color = new Color(1, 1, 1, player.hasWeapon[0] ? 1 : 0);
        weapon2Img.color = new Color(1, 1, 1, player.hasWeapon[1] ? 1 : 0);
        weapon3Img.color = new Color(1, 1, 1, player.hasWeapon[2] ? 1 : 0);
        weaponRImg.color = new Color(1, 1, 1, player.grenade > 0 ? 1 : 0);

        enemyATxt.text = enemyCntA.ToString();
        enemyBTxt.text = enemyCntB.ToString();
        enemyCTxt.text = enemyCntC.ToString();
    }

    void ReadSpawnFile()
    {
        spawnList.Clear();
        spawnIndex = 0;
        spawnEnd = false;

        TextAsset textFile = Resources.Load("Stage " + stage) as TextAsset;
        StringReader stringReader = new StringReader(textFile.text);

        while(stringReader != null)
        {
            string line = stringReader.ReadLine();

            if(line == null)
                break;

            Spawn spawnData = new Spawn();
            spawnData.delay = float.Parse(line.Split(',')[0]);
            spawnData.type = int.Parse(line.Split(',')[1]);
            spawnData.point = int.Parse(line.Split(',')[2]);
            spawnList.Add(spawnData);
        }

        stringReader.Close();
    }

    public void GameStart()
    {
        LoadPlayer();

        menuCam.SetActive(false);
        menuPanel.SetActive(false);
        searchPanel.SetActive(false);

        gameCam.SetActive(true);
        gamePanel.SetActive(true);

        player.gameObject.SetActive(true);
    }

    public void StageStart()
    {
        if(stage % 5 != 0)
            ReadSpawnFile();

        isBattle = true;

        itemShop.SetActive(false);
        weaponShop.SetActive(false);
        startZone.SetActive(false);

        foreach(Transform zone in enemyZones)
        {
            zone.gameObject.SetActive(true);
        }

        StartCoroutine(InBattle());
    }

    public void StageEnd()
    {
        isBattle = false;

        itemShop.SetActive(true);
        weaponShop.SetActive(true);
        startZone.SetActive(true);

        foreach(Transform zone in enemyZones)
        {
            zone.gameObject.SetActive(false);
        }

        stage++;

        player.transform.position = Vector3.up * 1.0f;

        SavePlayer();
        NoticeSave();
        DataManager.instance.SaveData();
    }

    IEnumerator InBattle()
    {
        if(stage % 5 == 0)
        {
            enemyCntD++;
            GameObject insBoss = 
                Instantiate(enemies[3], enemyZones[0].position, enemyZones[0].rotation);
            Enemy enemy = insBoss.GetComponent<Enemy>();
            enemy.target = player.transform;
            enemy.manager = this;
            boss = insBoss.GetComponent<Boss>();
        }
        else
        {
            for(int i = 0; i < spawnList.Count; i++)
            {
                enemyList.Add(spawnList[spawnIndex].type);

                switch(spawnList[spawnIndex].type)
                {
                case 0:
                    enemyCntA++;
                    break;
                case 1:
                    enemyCntB++;
                    break;
                case 2:
                    enemyCntC++;
                    break;
                }
            }

            while(enemyList.Count > 0)
            {
                int Zon = spawnList[spawnIndex].point;

                GameObject insEnemy = Instantiate(enemies[enemyList[0]], enemyZones[Zon].position, enemyZones[Zon].rotation);
                Enemy enemy = insEnemy.GetComponent<Enemy>();
                enemy.target = player.transform;
                enemy.manager = this;
                enemyList.RemoveAt(0);
                spawnIndex++;
                if(spawnIndex == spawnList.Count)
                {
                    spawnEnd = true;
                    spawnIndex -= 1;
                }
                yield return new WaitForSeconds(spawnList[spawnIndex].delay);
            }
        }

        while(enemyCntA + enemyCntB + enemyCntC + enemyCntD > 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(4.0f);

        boss = null;
        StageEnd();
    }

    public void GameOver()
    {
        overPanel.SetActive(true);
        gamePanel.SetActive(false);

        curScoreTxt.text = scoreTxt.text;

        int maxScore = PlayerPrefs.GetInt("MaxScore");
        if(player.score > maxScore)
        {
            bestTxt.gameObject.SetActive(true);
            PlayerPrefs.SetInt("MaxScore", player.score);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    void SavePlayer()
    {
        DataManager.instance.curPlayer.stage = stage;
        DataManager.instance.curPlayer.score = player.score;
        for(int i = 0; i < player.hasWeapon.Length; i++)
        {
            DataManager.instance.curPlayer.hasWeapon[i] = player.hasWeapon[i];
        }
        DataManager.instance.curPlayer.ammo = player.ammo;
        DataManager.instance.curPlayer.grenade = player.grenade;
        DataManager.instance.curPlayer.coin = player.coin;
    }

    void LoadPlayer()
    {
        stage = DataManager.instance.curPlayer.stage;
        player.score = DataManager.instance.curPlayer.score;
        for(int i = 0; i < player.hasWeapon.Length; i++)
        {
            player.hasWeapon[i] = DataManager.instance.curPlayer.hasWeapon[i];
        }
        player.ammo = DataManager.instance.curPlayer.ammo;
        player.grenade = DataManager.instance.curPlayer.grenade;
        player.coin = DataManager.instance.curPlayer.coin;
    }

    void NoticeSave()
    {
        notice.transform.localScale = Vector3.one;
        Invoke("EndNotice", 3.0f);
    }

    void EndNotice()
    {
        notice.transform.localScale = Vector3.zero;
    }
}