using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class TestData
{
    public string name;
    public int level;

    public TestData(string name = "River", int level = 0)
    {
        this.name = name;
        this.level = level;
    }
}

public class TestUserData : MonoBehaviour
{
    TestData testPlayer = new TestData();

    void Start()
    {
        string testJson = JsonUtility.ToJson(testPlayer);
        print(testJson);
        TestData testResive = JsonUtility.FromJson<TestData>(testJson);
        print(testResive.name);
        print(testResive.level);
    }
}
