using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class RaceScore : MonoBehaviour
{
    public const string PlayerTimeProp = "time";
    public const string PlayerLapProp = "lap";
}

static class ScoreExtensions
{
    private static Dictionary<string, float> playerTimes = new Dictionary<string, float>();
    private static Dictionary<string, int> playerLaps = new Dictionary<string, int>();

    public static void SetTime(this GameObject player, float time)
    {
        string playerId = player.GetInstanceID().ToString();
        if (playerTimes.ContainsKey(playerId))
        {
            playerTimes[playerId] = time;
        }
        else
        {
            playerTimes.Add(playerId, time);
        }
    }

    public static float GetTime(this GameObject player)
    {
        string playerId = player.GetInstanceID().ToString();
        if (playerTimes.TryGetValue(playerId, out float time))
        {
            return time;
        }
        return 0;
    }

    public static void SetLap(this GameObject player, int lap)
    {
        string playerId = player.GetInstanceID().ToString();
        if (playerLaps.ContainsKey(playerId))
        {
            playerLaps[playerId] = lap;
        }
        else
        {
            playerLaps.Add(playerId, lap);
        }
    }

    public static int GetLap(this GameObject player)
    {
        string playerId = player.GetInstanceID().ToString();
        if (playerLaps.TryGetValue(playerId, out int lap))
        {
            return lap;
        }
        return 0;
    }
}
