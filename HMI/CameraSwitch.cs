using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CameraSwitch : MonoBehaviour
{
    public GameObject[] objects;
    public Camera FPVCamera;
    public GameObject target;
    public RaceManager rmanager;
    public float value;

    private int m_CurrentActiveObject;

    private InputActionAsset inputActionAsset;
    private InputAction cameraSwitchAction;
    private InputAction cameraTiltUpAction;
    private InputAction cameraTiltDownAction;

    private void Awake()
    {
        inputActionAsset = Resources.Load<InputActionAsset>("InputActions");
        cameraSwitchAction = inputActionAsset.FindAction("CameraSwitch");
        cameraTiltUpAction = inputActionAsset.FindAction("CameraTiltUp");
        cameraTiltDownAction = inputActionAsset.FindAction("CameraTiltDown");

        cameraSwitchAction.performed += OnCameraSwitchPerformed;
        cameraTiltUpAction.performed += OnCameraTiltUpPerformed;
        cameraTiltDownAction.performed += OnCameraTiltDownPerformed;
    }

    public void NextCamera()
    {
        int nextactiveobject = m_CurrentActiveObject + 1 >= objects.Length ? 0 : m_CurrentActiveObject + 1;

        for (int i = 0; i < objects.Length; i++)
        {
            objects[i].SetActive(i == nextactiveobject);
        }

        m_CurrentActiveObject = nextactiveobject;
    }

    public void Start()
    {
        LoadPlayerPrefs();
        rmanager = (RaceManager)FindObjectOfType(typeof(RaceManager));
    }

    public void AttachToTarget(GameObject drone)
    {
        target = drone;
        FPVCamera.transform.parent = drone.transform;
        FPVCamera.transform.localPosition = new Vector3(0, 0.07f, 0.32f);
        value = 0;
        PlayerPrefs.SetFloat("cameraTilt", value);
        LoadPlayerPrefs();
    }

    public void Update()
    {
        RaceSettings();
        SwitchCameraTilt();
    }

    private void RaceSettings(){
        if (rmanager != null)
        {
            if (rmanager.raceStarted)
            {
                objects[0].SetActive(false);
                objects[1].SetActive(true);
                //objects[2].SetActive(false);
            }
        }
    }

    public void SwitchCameraTilt()
    {
        if (value < 45)
        {
            value = value + 15;
            PlayerPrefs.SetFloat("cameraTilt", value);
            LoadPlayerPrefs();
        }
    }

    public void LoadPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("cameraTilt"))
        {
            FPVCamera.transform.localEulerAngles = new Vector3(-PlayerPrefs.GetFloat("cameraTilt"), 0, 0);
        }
        if (PlayerPrefs.HasKey("cameraFOV"))
        {
            FPVCamera.fieldOfView = PlayerPrefs.GetFloat("cameraFOV");
        }
    }

    private void OnCameraSwitchPerformed(InputAction.CallbackContext context)
    {
        NextCamera();
    }

    private void OnCameraTiltUpPerformed(InputAction.CallbackContext context)
    {
        if (value < 45)
        {
            value = value + 15;
            PlayerPrefs.SetFloat("cameraTilt", value);
            LoadPlayerPrefs();
        }
    }

    private void OnCameraTiltDownPerformed(InputAction.CallbackContext context)
    {
        if (value > -45)
        {
            value = value - 15;
            PlayerPrefs.SetFloat("cameraTilt", value);
            LoadPlayerPrefs();
        }
    }
}