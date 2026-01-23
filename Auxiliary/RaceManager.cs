using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using UnityEngine.InputSystem;

public class RaceManager : MonoBehaviour
{
    public bool isSprint = false;
    public GameObject[] gates;
    public int numberOfLaps = 3;
    public GameObject timeCanvas;
    public GameObject scoreCanvas;
    public Text lapText;
    public Text timeText;

    public bool raceStarted = false;
    private int currentGate = 0;
    private int currentLap = 0;
    private double startTime;
    private double endTime;
    private bool shouldShowScoreCanvas;

    private float lapTime;
    private string lapTimeText;

    private InputActionAsset inputActionAsset;
    private InputAction scoreAction;

    private GameObject _player;
    private ControllerManager _controllermanager;

    void Awake()
    {
        if (numberOfLaps == 0) {numberOfLaps = DataHolder._laps;}
         // Дочерки в RaceGate
        Transform[] children = GetComponentsInChildren<Transform>(true);
        int numChildrenWithCondition = 0;
        for (int i = 1; i < children.Length; i++)
        {
            if (children[i].name.Contains("RaceGate"))
            {
                numChildrenWithCondition++;
            }
        }
        gates = new GameObject[numChildrenWithCondition];
        int index = 0;
        for (int i = 1; i < children.Length; i++)
        {
            if (children[i].name.Contains("RaceGate"))
            {
                gates[index] = children[i].gameObject;
                index++;
            }
        }
        
        inputActionAsset = Resources.Load<InputActionAsset>("InputActions");
        scoreAction = inputActionAsset.FindAction("Score");

        scoreAction.performed += OnScorePerformed;
        scoreAction.canceled += OnScoreCanceled;
        
        
    }

    
    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _controllermanager = _player.GetComponent<ControllerManager>();
        // Register gates;
        for (int i = 0; i < gates.Length; i++)
        {
            gates[i].GetComponent<RaceGate>().SetRaceManager(this);
        }
        // Init race
        StopRace();
        lapText.text = "Ожидание...";
        SetTextColor(Color.red);
    }

    public void StopRace()
    {
        Debug.Log("Stop Race");
        for (int i = 0; i < gates.Length; i++)
        {
            gates[i].GetComponent<RaceGate>().DisableGate();
        }
        currentGate = 0;
        currentLap = 0;
    }

    private void OnScorePerformed(InputAction.CallbackContext context)
    {
        scoreCanvas.SetActive(true);
        scoreCanvas.GetComponentInChildren<RaceScoreList>().shouldUpdate = true;

    }

    private void OnScoreCanceled(InputAction.CallbackContext context)
    {
        scoreCanvas.SetActive(false);
    }

    public void StartRace()
    {
        Debug.Log("Start Race");
        if (gates.Length > 0)
        {
            gates[(currentGate + 1) % gates.Length].GetComponent<RaceGate>().setNextGate();
            gates[currentGate].GetComponent<RaceGate>().EnableGate();
            raceStarted = true;
            //startTime = Time.time;
            lapText.text = "Круг: 1/" + numberOfLaps;
            ResetMyScore();
            shouldShowScoreCanvas = false;
        }
    }

    public void NextGate()
    {
        currentGate = (currentGate + 1) % gates.Length;
        gates[(currentGate + 1) % gates.Length].GetComponent<RaceGate>().setNextGate();
        gates[currentGate].GetComponent<RaceGate>().EnableGate();
        if (isSprint)
        {
            if (currentGate == 0)
            {
                NextLap();
                NextLap();
            }
        }
        else{
            if (currentGate == 1)
            {
                NextLap();
            }
        }
    }

    void NextLap()
    {
        // Save score
        if (currentLap > 0)
        {
            PlayerPrefs.SetInt("Lap", currentLap);
            PlayerPrefs.SetFloat("Time", (float)(Time.time - startTime));
        }

        // End of race
        if (currentLap == numberOfLaps)
        {
            // Change UI
            SetTextColor(Color.green);
            lapText.text = "Финишировал!";

            // End race
            raceStarted = false;
            gates[currentGate].GetComponent<RaceGate>().DisableGate();
            shouldShowScoreCanvas = true;
            TimeSpan timeSpan = TimeSpan.FromSeconds(PlayerPrefs.GetFloat("Time"));
            SetTimeText(String.Format("{0:D2}:{1:D2}:{2:D3}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds));

            int minutes = timeSpan.Minutes;
            int seconds = timeSpan.Seconds;
            int milliseconds = timeSpan.Milliseconds;

            DateTime now = DateTime.Now;
            string dateText = now.ToString("yyyy-MM-dd HH:mm:ss");

            lapTimeText = string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);

            lapTimeText += ") " + dateText;

            // Получаем путь к папке построения
            string dataPath;
#if UNITY_EDITOR
        dataPath = Application.dataPath.Replace("Assets", "");;
#else
        dataPath = Application.dataPath;
#endif

            // Задаем имя файла
            string fileName = "LapTimes.txt";

            // Проверяем, существует ли файл
            if (File.Exists(dataPath + fileName))
            {
                // Если файл существует, добавляем время круга в конец файла
                File.AppendAllText(dataPath + fileName, lapTimeText + "\n");
                Debug.Log("Сохранено в " + dataPath + fileName);
            }
            else
            {
                // Если файла не существует, создаем его и записываем время круга
                File.WriteAllText(dataPath + fileName, lapTimeText + "\n");
                Debug.Log("Сохранено в " + dataPath + fileName);
            }
        }
        else
        {
            // New lap
            currentLap++;
            lapText.text = "Круг: " + currentLap + "/" + numberOfLaps;
        }
    }

    public void SetLapText(string s)
    {
        lapText.text = s;
    }

    public void SetTimeText(string s)
    {
        timeText.text = s;
    }

    public void SetTextColor(Color c)
    {
        lapText.color = c;
        timeText.color = c;
    }

    public Transform PreviousGatePosition()
    {
        return currentGate > 0 ? gates[currentGate - 1].transform : null;
    }

    public void ResetMyScore()
    {
        PlayerPrefs.SetInt("Lap", 0);
        PlayerPrefs.SetFloat("Time", 0);
    }

    void Update()
    {
        if (_controllermanager.startTick)
        {
            if (raceStarted)
            {
                if (startTime == 0)
                {
                    startTime = Time.time;
                }
                double duration = Time.time - startTime;
                TimeSpan timeSpan = TimeSpan.FromSeconds(duration);
                SetTimeText(String.Format("{0:D2}:{1:D2}:{2:D3}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds));
            }
        }
    }
    
}
