using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class Select : MonoBehaviour
{
    public GameObject creat;
    public Text[] slotText;
    public Text newPlayerName;

    bool[] savefile = new bool[4];

    void Start()
    {
        for(int i = 0; i < 4; i++)
        {
            if(File.Exists(DataManager.instance.path + $"{i}"))
            {
                savefile[i] = true;
                DataManager.instance.curSlot = i;
                DataManager.instance.LoadData();
                slotText[i].text = DataManager.instance.curPlayer.name;
            }
            else
            {
                slotText[i].text = "비어있음";
            }
        }
        DataManager.instance.DataClear();
    }

    public void Slot(int slotNum)
    {
        DataManager.instance.curSlot = slotNum;

        if(!savefile[slotNum])
        {
            CreatSlot();
        }
        else
        {
            DataManager.instance.LoadData();
            GoGame();
        }
    }

    public void CreatSlot()
    {
        creat.SetActive(true);
    }

    public void GoGame()
    {
        if(!savefile[DataManager.instance.curSlot])
        {
            DataManager.instance.curPlayer.name = newPlayerName.text;
            DataManager.instance.SaveData();
        }

        SceneManager.LoadScene(1);
    }
}
