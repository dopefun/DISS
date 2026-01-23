using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class RaceScoreList : MonoBehaviour
{
    public GameObject playerScoreEntryPrefab;
    public bool shouldUpdate = true;

    private List<PlayerData> playerList = new List<PlayerData>();

    void Update()
    {
        if (!shouldUpdate)
        {
            return;
        }

        // Clear
        while (this.transform.childCount > 0)
        {
            Transform c = this.transform.GetChild(0);
            c.SetParent(null);  // Become Batman
            Destroy(c.gameObject);
        }

        // Populate
        foreach (PlayerData player in playerList)
        {
            GameObject go = (GameObject)Instantiate(playerScoreEntryPrefab);
            go.transform.SetParent(this.transform);
            go.transform.Find("Username").GetComponent<Text>().text = player.name;
            go.transform.Find("Lap").GetComponent<Text>().text = player.lap.ToString();
            TimeSpan timeSpan = TimeSpan.FromSeconds(player.time);
            go.transform.Find("Time").GetComponent<Text>().text = String.Format("{0:D2}:{1:D2}:{2:D3}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        }
        shouldUpdate = false;
    }

    public void UpdatePlayerList(List<PlayerData> newPlayerList)
    {
        playerList = newPlayerList;
        shouldUpdate = true;
    }

    [Serializable]
    public class PlayerData
    {
        public string name;
        public int lap;
        public float time;

        public PlayerData(string name, int lap, float time)
        {
            this.name = name;
            this.lap = lap;
            this.time = time;
        }
    }
}
