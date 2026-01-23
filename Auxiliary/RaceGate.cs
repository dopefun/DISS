using UnityEngine;
using System.Collections;

public class RaceGate : MonoBehaviour
{
    public AudioClip gateSound;
    public Color enabledColor = new Color(0.83f, 0.69f, 0.21f, 1.0f);
    public Color nextColor = new Color(0.9f, 0.75f, 0.28f, 0.6f);
    public Color emissionColor = new Color(0.5f, 0.5f, 0.0f, 0.5f);
    public Color nextEmissionColor = new Color(0.1f, 0.1f, 0.1f, 0.0f);
    public Color finalEmissionColor = new Color (0f, 1f, 0f, 1f);

    private RaceManager raceManager;
    public bool enabledGate = false;

    private AudioSource gateSoundSource;

    void Start()
    {
        gateSoundSource = gameObject.AddComponent<AudioSource>();
        gateSoundSource.playOnAwake = false;
        gateSoundSource.clip = gateSound;
        DisableGate();
    }

    public void SetRaceManager(RaceManager rm)
    {
        raceManager = rm;
    }

    public void EnableGate()
    {
        enabledGate = true;
        GetComponent<Renderer>().material.color = enabledColor;
        GetComponent<Renderer>().material.SetColor("_EmissionColor", emissionColor);
        //Show();
    }

    public void setNextGate()
    {
        enabledGate = false;
        GetComponent<Renderer>().material.color = nextColor;
        GetComponent<Renderer>().material.SetColor("_EmissionColor", nextEmissionColor);
        //Show();
    }

    public void DisableGate()
    {
        enabledGate = false;
        GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(1f, 1f, 1f, 1f));
        //Hide();
    }

    public bool isEnabled()
    {
        return enabledGate;
    }

    public void TriggerGate()
    {
        if (enabledGate)
        {  
            gateSoundSource.Play();
            DisableGate();
            raceManager.NextGate();
        }
    }


    public void Hide()
    {
        GetComponent<Renderer>().enabled = false;
    }

    public void Show()
    {
        GetComponent<Renderer>().enabled = true;
    }
}
