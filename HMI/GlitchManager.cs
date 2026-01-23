using UnityEngine;
using UnityEngine.UI;
using Kino;

public class GlitchManager : MonoBehaviour
{
    [SerializeField] private float Intensity;
    [SerializeField] private float ScanLineJitter;
    [SerializeField] private float VerticalJump;
    [SerializeField] private float ColorDrift;
    [SerializeField] private float TimeToReturn = 7f;

    [SerializeField] private Text timerText; // Текстовый объект для отображения таймера
    [SerializeField] private Font timerFont; // Шрифт для текста таймера
    [SerializeField] private DigitalGlitch[] digitalGlitch;
    [SerializeField] private AnalogGlitch[] analogGlitch;

    private float timer = 0f;
    private bool isPlayerInTrigger = false;

    void Start()
    {
        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        if (mainCamera != null)
        {
            //digitalGlitch = mainCamera.GetComponent<DigitalGlitch>();
            //analogGlitch = mainCamera.GetComponent<AnalogGlitch>();
            digitalGlitch = Resources.FindObjectsOfTypeAll<DigitalGlitch>();
            analogGlitch = Resources.FindObjectsOfTypeAll<AnalogGlitch>();

            if (digitalGlitch == null || analogGlitch == null)
            {
                Debug.LogError("DigitalGlitch or AnalogGlitch component not found on MainCamera.");
            }
        }
        else
        {
            Debug.LogError("No object with tag 'MainCamera' found.");
        }

        if (timerText != null)
        {
            timerText.font = timerFont;
            timerText.text = ""; // Инициализация текста
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            timer = 0f;

            if (digitalGlitch != null && analogGlitch != null)
            for (int i = 0; i < digitalGlitch.Length; i++)
            {
                digitalGlitch[i].intensity = Intensity;
                analogGlitch[i].scanLineJitter = ScanLineJitter;
                analogGlitch[i].verticalJump = VerticalJump;
                analogGlitch[i].colorDrift = ColorDrift;
            }
        }
    }

    private void OnTriggerStay(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            timer = 0f;

            if (digitalGlitch != null && analogGlitch != null)
            for (int i = 0; i < digitalGlitch.Length; i++)
            {
                digitalGlitch[i].intensity = 0f;
                analogGlitch[i].scanLineJitter = 0f;
                analogGlitch[i].verticalJump = 0f;
                analogGlitch[i].colorDrift = 0f;
            }
        }
    }

    private void Update()
    {
        if (!isPlayerInTrigger)
        {
            timer += Time.deltaTime;
            if (timerText != null)
            {
                timerText.text = "Возврат через: " + Mathf.Ceil(TimeToReturn - timer).ToString();
            }
            if (timer >= TimeToReturn)
            {
                ResetDrone();
            }
        }
        else
        {
            if (timerText != null)
            {
                timerText.text = ""; // Сброс текста, если игрок в триггере
            }
        }
    }

    private void ResetDrone()
    {
        if (digitalGlitch != null && analogGlitch != null)
        for (int i = 0; i < digitalGlitch.Length; i++)
            {
                digitalGlitch[i].intensity = 0f;
                analogGlitch[i].scanLineJitter = 0f;
                analogGlitch[i].verticalJump = 0f;
                analogGlitch[i].colorDrift = 0f;
            }
        GameObject drone = GameObject.FindGameObjectWithTag("Player");
        drone.GetComponent<ResetDrone>().Restart();
        Debug.Log("ResetDrone called.");
    }
}