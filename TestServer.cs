using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class TestServer : MonoBehaviour
{
    public Dropdown serverList;
    public Text selectedServerName;
    public InputField inputText;
    public RawImage img;

    ServerRoot serverData = new ServerRoot();
    CharacterRoot characterData = new CharacterRoot();
    string serverId = "";
    string characterName = "";
    string characterId = "";

    void Start()
    {
        // 게임 시작 시 서버에 데이터 요청
        StartCoroutine(ServerRequest());
    }

    public void CharacterSearch()
    {
        string temp = selectedServerName.text;
        serverId = serverData.rows.Find(x => x.serverName == temp).serverId;
        characterName = UnityWebRequest.EscapeURL(inputText.text);

        print(serverId);
        // 서버와 캐릭터 이름으로 캐릭터 데이터 요청
        StartCoroutine(CharacterRequest(serverId, characterName));
    }

    IEnumerator ServerRequest()
    {
        string url = 
            "https://api.neople.co.kr/df/servers?apikey=QVeSShdHGYEDwGSiAre5JLVZ9RIPVxNE";
        UnityWebRequest www = UnityWebRequest.Get(url);

        yield return www.SendWebRequest();

        if(www.error == null)
        {
            Debug.Log(www.downloadHandler.text);
            serverData = JsonUtility.FromJson<ServerRoot>(www.downloadHandler.text);
            print(serverData.rows[0].serverName);

            // 서버 드롭다운 옵션 추가
            foreach(var sd in serverData.rows)
            {
                Dropdown.OptionData option = new Dropdown.OptionData();
                option.text = sd.serverName;

                serverList.options.Add(option);
            }
        }
        else
        {
            Debug.Log(www.error);
        }
    }

    IEnumerator CharacterRequest(string serverId, string characterName)
    {
        string url = 
            $"https://api.neople.co.kr/df/servers/{serverId}/characters?characterName={characterName}&apikey=QVeSShdHGYEDwGSiAre5JLVZ9RIPVxNE";
        UnityWebRequest www = UnityWebRequest.Get(url);

        yield return www.SendWebRequest();

        if(www.error == null)
        {
            print(www.downloadHandler.text);

            // 받아온 캐릭터 정보를 데이터 구조체에 저장
            characterData = JsonUtility.FromJson<CharacterRoot>(www.downloadHandler.text);
            // 입력한 캐릭터 이름과 일치하는 캐릭터의 ID를 찾아 저장
            characterId = characterData.rows.Find(x => x.characterName == inputText.text).characterId;
            // 캐릭터 이미지 요청 함수 호출
            StartCoroutine(CharacterImageRequest(serverId, characterId));
        }
        else
        {
            Debug.LogError(www.error);
        }
    }

    IEnumerator CharacterImageRequest(string serverId, string characterId)
    {
        string url = 
            $"https://img-api.neople.co.kr/df/servers/{serverId}/characters/{characterId}?zoom=1";
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);

        yield return www.SendWebRequest();

        if(www.error == null)
        {
            // 받아온 텍스처를 RawImage에 저장
            img.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
        else
        {
            Debug.LogError(www.error);
        }
    }
}

[System.Serializable]
public class ServerInfo
{
    public string serverId;
    public string serverName;
}
[System.Serializable]
public class ServerRoot
{
    public List<ServerInfo> rows;
}
[System.Serializable]
public class CharacterInfo
{
    public string serverId;
    public string characterId;
    public string characterName;
    public int level;
    public string jobId;
    public string jobGrowId;
    public string jobName;
    public string jobGrowName;
}
[System.Serializable]
public class CharacterRoot
{
    public List<CharacterInfo> rows;
}