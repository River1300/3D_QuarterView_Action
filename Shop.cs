using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public GameObject[] itmeObj;
    public int[] itemPrice;
    public Transform[] itemPos;
    public Text textTalk;
    public string[] talkData;

    public RectTransform uiGroup;
    public Animator anim;
    public Player enterPlayer;

    public void Enter(Player player)
    {
        enterPlayer = player;
        uiGroup.anchoredPosition = Vector3.zero;
    }

    public void Exit()
    {
        uiGroup.anchoredPosition = Vector3.down * 1000;
        anim.SetTrigger("doHello");
    }

    public void Buy(int index)
    {
        int price = itemPrice[index];

        if(enterPlayer.coin < price)
        {
            StopCoroutine(Talk());
            StartCoroutine(Talk());
            return;
        }
        else
        {
            enterPlayer.coin -= price;
            Vector3 ranVec = Vector3.right * Random.Range(-3, 3) + Vector3.forward * Random.Range(-3, 3);
            Instantiate(itmeObj[index], itemPos[index].position + ranVec, itemPos[index].rotation);
        }
    }

    IEnumerator Talk()
    {
        textTalk.text = talkData[1];
        yield return new WaitForSeconds(2.0f);
        textTalk.text = talkData[0];
    }
}
