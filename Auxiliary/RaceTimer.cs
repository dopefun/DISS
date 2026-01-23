using UnityEngine;

public class RaceTimer : MonoBehaviour
{
    public int TurnDuration = 1;                  // time per round/turn
    public int BreakDuration = 1;                 // time per round/turn
    public RaceManager raceManager;               // Ссылка на RaceManager
    public ResetDrone resetDrone;                 // Ссылка на ResetDrone

    private int gameMode;
    private double StartTime;                     // this should could also be a private. i just like to see this in inspector
    private bool startRoundWhenTimeIsSynced;      // used in an edge-case when we wanted to set a start time but don't know it yet.
    private const string StartTimeKey = "st";     // the name of our "start time" custom property.
    private bool timeHasBeenSynced;
    private int phase = 0;

    private void StartRoundNow()
    {
        // Using Unity's Time.time instead of PhotonNetwork.time
        if (Time.time < 0.0001f)
        {
            // We can only start the round when the time is available. Let's check that in Update()
            startRoundWhenTimeIsSynced = true;
            return;
        }
        startRoundWhenTimeIsSynced = false;

        // Store start time using PlayerPrefs
        PlayerPrefs.SetFloat(StartTimeKey, (float)Time.time);
        PlayerPrefs.Save();
    }



    public void Start()
    {
        // You can set gameMode directly here or through the inspector.
        gameMode = 0; // Default value or set it to a specific mode
        this.StartRoundNow();
        StartTime = PlayerPrefs.GetFloat(StartTimeKey, 0);
        timeHasBeenSynced = StartTime > 0;

        GameObject _player = GameObject.FindGameObjectWithTag("Player");
        if (_player != null)
        {
            resetDrone = _player.GetComponent<ResetDrone>();;
        }
    }

    void Update()
    {
        if (startRoundWhenTimeIsSynced)
        {
            this.StartRoundNow();   // The "time is known" check is done inside the method.
        }

        if (gameMode == 0) // Assuming 0 corresponds to the "Race" mode
        {
            if (timeHasBeenSynced)
            {
                double elapsedTime = (Time.time - StartTime);
                double wrappedElapsedTime = elapsedTime % (TurnDuration + BreakDuration);
                switch (phase)
                {
                    case 0:
                        // Break phase
                        raceManager.SetTimeText(string.Format("Старт через: {0:0}", 0.5 + BreakDuration - wrappedElapsedTime));
                        if (wrappedElapsedTime > BreakDuration)
                        {
                            resetDrone.Restart();
                            raceManager.SetTextColor(Color.white);
                            raceManager.StartRace();
                            phase = 1;
                        }
                        break;
                    case 1:
                        // Round phase
                        if (wrappedElapsedTime <= BreakDuration)
                        {
                            raceManager.SetLapText("Ожидание...");
                            raceManager.SetTextColor(Color.red);
                            raceManager.StopRace();
                            phase = 0;
                        }
                        break;
                }
            }
        }
        else
        {
            raceManager.gameObject.SetActive(false);
        }
    }
}