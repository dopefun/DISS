using UnityEngine;

public class LapManager : MonoBehaviour
{
    public GameObject[] Lap;
    public GameObject TimeCan;
    // Start is called before the first frame update
    void Awake()
    {
        if (DataHolder._lap > 0)
        {
            Lap[DataHolder._lap - 1].SetActive(true);
            TimeCan.SetActive(true);
        }
    }
}
