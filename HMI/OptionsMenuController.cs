using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class OptionsMenuController : MonoBehaviour
{
    private bool initialized = false;
    private ControllerManager controllerManager;
    private CameraSwitch cameraSwitch;
    private BatteryManager batteryManager;
    //private Cargo cargoManager;
    public GameObject _UI;
    public bool optionMenuPaused = false;

    public GameObject drone;
    public GameObject[] cargo;
    public GameObject optionsPanel;

    
    public InputField centreRollRateInputField;
    public InputField maxRollRateInputField;
    public InputField expoRollInputField;

    public InputField centrePitchRateInputField;
    public InputField maxPitchRateInputField;
    public InputField expoPitchInputField;

    public InputField centreYawRateInputField;
    public InputField maxYawRateInputField;
    public InputField expoYawInputField;
    //public Toggle squareMappingToggle;
    public Toggle axisToggleT;
    public Toggle axisToggleR;
    public Toggle axisToggleE;
    public Toggle axisToggleA;
    public Slider tiltRateKpSlider;
    public Slider yawRateKpSlider;
    public Toggle enableStabilizationToggle;
    public Slider tiltAngleSlider;
    public Slider verticalSpeedSlider;
    public Slider tiltAngleKpSlider;
    public Slider yawAngleKpSlider;
    public Slider verticalSpeedKpSlider;
    public Slider cameraTiltSlider;
    public Slider cameraFOVSlider;
    public Toggle batteryToggle;
    public Toggle fixCargoToggle;
    public Toggle modeCargoToggle;

    private float defaultCentreRollRate;
    private float defaultMaxRollRate;
    private float defaultExpoRoll;

    private float defaultCentrePitchRate;
    private float defaultMaxPitchRate;
    private float defaultExpoPitch;

    private float defaultCentreYawRate;
    private float defaultMaxYawRate;
    private float defaultExpoYaw;


    private bool defaultInvertAxisT;
    private bool defaultInvertAxisR;
    private bool defaultInvertAxisE;
    private bool defaultInvertAxisA;
    private float defaultTiltRateKp;
    private float defaultYawRateKp;
    private bool defaultEnableStabilization;
    private float defaultTiltAngle;
    private float defaultVerticalSpeed;
    private float defaultTiltAngleKp;
    private float defaultYawAngleKp;
    private float defaultVerticalSpeedKp;
    private float defaultCameraTilt;
    private float defaultCameraFOV;
    private bool defaultEnabledBattery;
    private bool defaultFixToggle;
    private bool defaultModeToggle;


    private InputActionAsset inputActionAsset;
    private InputAction droneOptionsAction;

    void Start()
    {
        if (drone != null)
        {
            SetDrone(drone);
        }
        else
        {
            drone = GameObject.FindGameObjectWithTag("Player");
            SetDrone(drone);
        }

        GameObject cameras = GameObject.Find("Cameras");
        if (cameras != null)
        {
            cameraSwitch = cameras.GetComponent<CameraSwitch>();
        }
        
        batteryManager = drone.GetComponent<BatteryManager>();

        cargo = GameObject.FindGameObjectsWithTag("CargoItem");


        inputActionAsset = Resources.Load<InputActionAsset>("InputActions");
        droneOptionsAction = inputActionAsset.FindAction("DroneOptions");

        droneOptionsAction.performed += OnDroneOptionsPerformed;
        droneOptionsAction.performed -= OnDroneOptionsCanceled;
    }
    

    public void SetDrone(GameObject drone)
    {
        controllerManager = drone.GetComponent<ControllerManager>();
        if (controllerManager != null)
        {
            Initialize();
        }
        else
        {
            Debug.LogError("ControllerManager component not found on the drone.");
        }
    }


    void Initialize()
    {
        if (!initialized && optionsPanel.activeSelf)
        {
            
            // Set default
            float.TryParse(centreRollRateInputField.text, out defaultCentreRollRate);
            float.TryParse(maxRollRateInputField.text, out defaultMaxRollRate);
            float.TryParse(expoRollInputField.text, out defaultExpoRoll);

            float.TryParse(centrePitchRateInputField.text, out defaultCentrePitchRate);
            float.TryParse(maxPitchRateInputField.text, out defaultMaxPitchRate);
            float.TryParse(expoPitchInputField.text, out defaultExpoPitch);

            float.TryParse(centreYawRateInputField.text, out defaultCentreYawRate);
            float.TryParse(maxYawRateInputField.text, out defaultMaxYawRate);
            float.TryParse(expoYawInputField.text, out defaultExpoYaw);
            
            defaultInvertAxisT = axisToggleT.isOn;
            defaultInvertAxisR = axisToggleR.isOn;
            defaultInvertAxisE = axisToggleE.isOn;
            defaultInvertAxisA = axisToggleA.isOn;
            defaultTiltRateKp = tiltRateKpSlider.value;
            defaultYawRateKp = yawRateKpSlider.value;
            defaultEnableStabilization = enableStabilizationToggle.isOn;
            defaultTiltAngle = tiltAngleSlider.value;
            defaultVerticalSpeed = verticalSpeedSlider.value;
            defaultTiltAngleKp = tiltAngleKpSlider.value;
            defaultYawAngleKp = yawAngleKpSlider.value;
            defaultVerticalSpeedKp = verticalSpeedKpSlider.value;
            defaultCameraTilt = cameraTiltSlider.value;
            defaultCameraFOV = cameraFOVSlider.value;
            defaultEnabledBattery = batteryToggle.isOn;
            defaultFixToggle = batteryToggle.isOn;
            defaultModeToggle = batteryToggle.isOn;

            // Load PlayerPrefs
            if (PlayerPrefs.HasKey("centreRollRate"))
            {
                centreRollRateInputField.text = PlayerPrefs.GetFloat("centreRollRate", 0.0f).ToString();
            }
            if (PlayerPrefs.HasKey("maxRollRate"))
            {
                maxRollRateInputField.text = PlayerPrefs.GetFloat("maxRollRate", 0.0f).ToString();
            }
            if (PlayerPrefs.HasKey("expoRoll"))
            {
                expoRollInputField.text = PlayerPrefs.GetFloat("expoRoll", 0.0f).ToString("F2");
            }

            if (PlayerPrefs.HasKey("centrePitchRate"))
            {
                centrePitchRateInputField.text = PlayerPrefs.GetFloat("centrePitchRate", 0.0f).ToString();
            }
            if (PlayerPrefs.HasKey("maxPitchRate"))
            {
                maxPitchRateInputField.text = PlayerPrefs.GetFloat("maxPitchRate", 0.0f).ToString();
            }
            if (PlayerPrefs.HasKey("expoPitch"))
            {
                expoPitchInputField.text = PlayerPrefs.GetFloat("expoPitch", 0.0f).ToString("F2");
            }

            if (PlayerPrefs.HasKey("centreYawRate"))
            {
                centreYawRateInputField.text = PlayerPrefs.GetFloat("centreYawRate", 0.0f).ToString();
            }
            if (PlayerPrefs.HasKey("maxYawRate"))
            {
                maxYawRateInputField.text = PlayerPrefs.GetFloat("maxYawRate", 0.0f).ToString();
            }
            if (PlayerPrefs.HasKey("expoYaw"))
            {
                expoYawInputField.text = PlayerPrefs.GetFloat("expoYaw", 0.0f).ToString("F2");
            }

            if (PlayerPrefs.HasKey("invertAxisT"))
            {
                axisToggleT.isOn = (PlayerPrefs.GetInt("invertAxisT") > 0);
            }
            if (PlayerPrefs.HasKey("invertAxisR"))
            {
                axisToggleR.isOn = (PlayerPrefs.GetInt("invertAxisR") > 0);
            }
            if (PlayerPrefs.HasKey("invertAxisE"))
            {
                axisToggleE.isOn = (PlayerPrefs.GetInt("invertAxisE") > 0);
            }
            if (PlayerPrefs.HasKey("invertAxisA"))
            {
                axisToggleA.isOn = (PlayerPrefs.GetInt("invertAxisA") > 0);
            }
            if (PlayerPrefs.HasKey("tiltRateKp"))
            {
                tiltRateKpSlider.value = PlayerPrefs.GetFloat("tiltRateKp");
            }
            if (PlayerPrefs.HasKey("yawRateKp"))
            {
                yawRateKpSlider.value = PlayerPrefs.GetFloat("yawRateKp");
            }
            if (PlayerPrefs.HasKey("enableStabilization"))
            {
                enableStabilizationToggle.isOn = (PlayerPrefs.GetInt("enableStabilization") > 0);
            }
            if (PlayerPrefs.HasKey("tiltAngle"))
            {
                tiltAngleSlider.value = PlayerPrefs.GetFloat("tiltAngle");
            }
            if (PlayerPrefs.HasKey("verticalSpeed"))
            {
                verticalSpeedSlider.value = PlayerPrefs.GetFloat("verticalSpeed");
            }
            if (PlayerPrefs.HasKey("tiltAngleKp"))
            {
                tiltAngleKpSlider.value = PlayerPrefs.GetFloat("tiltAngleKp");
            }
            if (PlayerPrefs.HasKey("yawAngleKp"))
            {
                yawAngleKpSlider.value = PlayerPrefs.GetFloat("yawAngleKp");
            }
            if (PlayerPrefs.HasKey("verticalSpeedKp"))
            {
                verticalSpeedKpSlider.value = PlayerPrefs.GetFloat("verticalSpeedKp");
            }
            if (PlayerPrefs.HasKey("cameraTilt"))
            {
                cameraTiltSlider.value = PlayerPrefs.GetFloat("cameraTilt");
            }
            if (PlayerPrefs.HasKey("cameraFOV"))
            {
                cameraFOVSlider.value = PlayerPrefs.GetFloat("cameraFOV");
            }
            if (PlayerPrefs.HasKey("enableBattery"))
            {
                batteryToggle.isOn = PlayerPrefs.GetInt("enableBattery") > 0;
            }
            if (PlayerPrefs.HasKey("enableAutoMode"))
            {
                modeCargoToggle.isOn = PlayerPrefs.GetInt("enableAutoMode") > 0;
            }
            if (PlayerPrefs.HasKey("enableAutoFixMode"))
            {
                fixCargoToggle.isOn = PlayerPrefs.GetInt("enableAutoFixMode") > 0;
            }

            // Update texts
            UpdateSliderTexts();

            initialized = true;
        }
    }

    void Update()
    {
        if (optionMenuPaused)
        {
            _UI?.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
        }
        else
        {
            _UI?.SetActive(true);
            //Cursor.lockState = CursorLockMode.Locked;
            //Time.timeScale = 1f;
        }
    }

    

    
    // Методы для Roll
    public void SetCentreRollRateFromInput(string value)
    {
        if (float.TryParse(value, out float floatValue))
        {
            setCentreRollRate(floatValue);
        }
        else
        {
            Debug.LogWarning($"Невозможно преобразовать '{value}' в float.");
        }
    }
    public void SetMaxRollRateFromInput(string value)
    {
        if (float.TryParse(value, out float floatValue))
        {
            setMaxRollRate(floatValue);
        }
        else
        {
            Debug.LogWarning($"Невозможно преобразовать '{value}' в float.");
        }
    }
    public void SetExpoRollFromInput(string value)
    {
        if (float.TryParse(value, out float floatValue))
        {
            setExpoRoll(floatValue);
        }
        else
        {
            Debug.LogWarning($"Невозможно преобразовать '{value}' в float.");
        }
    }

    // Методы для Pitch
    public void SetCentrePitchRateFromInput(string value)
    {
        if (float.TryParse(value, out float floatValue))
        {
            setCentrePitchRate(floatValue);
        }
        else
        {
            Debug.LogWarning($"Невозможно преобразовать '{value}' в float.");
        }
    }
    public void SetMaxPitchRateFromInput(string value)
    {
        if (float.TryParse(value, out float floatValue))
        {
            setMaxPitchRate(floatValue);
        }
        else
        {
            Debug.LogWarning($"Невозможно преобразовать '{value}' в float.");
        }
    }
    public void SetExpoPitchFromInput(string value)
    {
        if (float.TryParse(value, out float floatValue))
        {
            setExpoPitch(floatValue);
        }
        else
        {
            Debug.LogWarning($"Невозможно преобразовать '{value}' в float.");
        }
    }

    // Методы для Yaw
    public void SetCentreYawRateFromInput(string value)
    {
        if (float.TryParse(value, out float floatValue))
        {
            setCentreYawRate(floatValue);
        }
        else
        {
            Debug.LogWarning($"Невозможно преобразовать '{value}' в float.");
        }
    }
    public void SetMaxYawRateFromInput(string value)
    {
        if (float.TryParse(value, out float floatValue))
        {
            setMaxYawRate(floatValue);
        }
        else
        {
            Debug.LogWarning($"Невозможно преобразовать '{value}' в float.");
        }
    }
    public void SetExpoYawFromInput(string value)
    {
        if (float.TryParse(value, out float floatValue))
        {
            setExpoYaw(floatValue);
        }
        else
        {
            Debug.LogWarning($"Невозможно преобразовать '{value}' в float.");
        }
    }
    
    public void setCentreRollRate(float value)
    {
        if (initialized)
    {
        PlayerPrefs.SetFloat("centreRollRate", value);
        centreRollRateInputField.text = value.ToString(); // Обновляем текст в InputField
        controllerManager.LoadPlayerPrefs();
    }
    }
    public void setMaxRollRate(float value)
    {
        if (initialized)
        {
            PlayerPrefs.SetFloat("maxRollRate", value);
            maxRollRateInputField.text = value.ToString();
            controllerManager.LoadPlayerPrefs();
        }
    }
    public void setExpoRoll(float value)
    {
        if (initialized)
        {
            PlayerPrefs.SetFloat("expoRoll", value);
            expoRollInputField.text = value.ToString("F2");
            controllerManager.LoadPlayerPrefs();
        }
    }

    public void setCentrePitchRate(float value)
    {
        if (initialized)
        {
            PlayerPrefs.SetFloat("centrePitchRate", value);
            centrePitchRateInputField.text = value.ToString();
            controllerManager.LoadPlayerPrefs();
        }
    }
    public void setMaxPitchRate(float value)
    {
        if (initialized)
        {
            PlayerPrefs.SetFloat("maxPitchRate", value);
            maxPitchRateInputField.text = value.ToString();
            controllerManager.LoadPlayerPrefs();
        }
    }
    public void setExpoPitch(float value)
    {
        if (initialized)
        {
            PlayerPrefs.SetFloat("expoPitch", value);
            expoPitchInputField.text = value.ToString("F2");
            controllerManager.LoadPlayerPrefs();
        }
    }

    public void setCentreYawRate(float value)
    {
        if (initialized)
        {
            PlayerPrefs.SetFloat("centreYawRate", value);
            centreYawRateInputField.text = value.ToString();
            controllerManager.LoadPlayerPrefs();
        }
    }
    public void setMaxYawRate(float value)
    {
        if (initialized)
        {
            PlayerPrefs.SetFloat("maxYawRate", value);
            maxYawRateInputField.text = value.ToString();
            controllerManager.LoadPlayerPrefs();
        }
    }
    public void setExpoYaw(float value)
    {
        if (initialized)
        {
            PlayerPrefs.SetFloat("expoYaw", value);
            expoYawInputField.text = value.ToString("F2");
            controllerManager.LoadPlayerPrefs();
        }
    }

    
    public void setInvertAxisT(bool value)
    {
        if (initialized)
        {
            PlayerPrefs.SetInt("invertAxisT", value ? 1 : 0);
            controllerManager.LoadPlayerPrefs();
        }
    }

    public void setInvertAxisR(bool value)
    {
        if (initialized)
        {
            PlayerPrefs.SetInt("invertAxisR", value ? 1 : 0);
            controllerManager.LoadPlayerPrefs();
        }
    }

    public void setInvertAxisE(bool value)
    {
        if (initialized)
        {
            PlayerPrefs.SetInt("invertAxisE", value ? 1 : 0);
            controllerManager.LoadPlayerPrefs();
        }
    }

    public void setInvertAxisA(bool value)
    {
        if (initialized)
        {
            PlayerPrefs.SetInt("invertAxisA", value ? 1 : 0);
            controllerManager.LoadPlayerPrefs();
        }
    }

    public void setTiltRateKp(float value)
    {
        if (initialized)
        {
            PlayerPrefs.SetFloat("tiltRateKp", value);
            tiltRateKpSlider.GetComponentInChildren<Text>().text = "Пропорциональный коэффициент скорости наклона (Kp): " + value.ToString("F0") + " Гц";
            controllerManager.LoadPlayerPrefs();
        }
    }

    public void setYawRateKp(float value)
    {
        if (initialized)
        {
            PlayerPrefs.SetFloat("yawRateKp", value);
            yawRateKpSlider.GetComponentInChildren<Text>().text = "Пропорциональный коэффициент скорости рыскания (Kp): " + value.ToString("F0") + " Гц";
            controllerManager.LoadPlayerPrefs();
        }
    }

    public void setEnableStabilization(bool value)
    {
        if (initialized)
        {
            PlayerPrefs.SetInt("enableStabilization", value ? 1 : 0);
            if (value)
                controllerManager.resetYawRef();
            controllerManager.LoadPlayerPrefs();
        }
    }

    public void setTiltAngle(float value)
    {
        if (initialized)
        {
            PlayerPrefs.SetFloat("tiltAngle", value);
            tiltAngleSlider.GetComponentInChildren<Text>().text = "Угол наклона: " + value.ToString("F0") + "°";
            controllerManager.LoadPlayerPrefs();
        }
    }

    public void setVerticalSpeed(float value)
    {
        if (initialized)
        {
            PlayerPrefs.SetFloat("verticalSpeed", value);
            verticalSpeedSlider.GetComponentInChildren<Text>().text = "Вертикальная скорость: " + value.ToString("F0") + "м/с";
            controllerManager.LoadPlayerPrefs();
        }
    }

    public void setTiltAngleKp(float value)
    {
        if (initialized)
        {
            PlayerPrefs.SetFloat("tiltAngleKp", value);
            tiltAngleKpSlider.GetComponentInChildren<Text>().text = "Пропорциональный коэффициент угла наклона (Kp): " + value.ToString("F0") + " Гц";
            controllerManager.LoadPlayerPrefs();
        }
    }

    public void setYawAngleKp(float value)
    {
        if (initialized)
        {
            PlayerPrefs.SetFloat("yawAngleKp", value);
            yawAngleKpSlider.GetComponentInChildren<Text>().text = "Пропорциональный коэффициент угла рыскания (Kp): " + value.ToString("F0") + " Гц";
            controllerManager.LoadPlayerPrefs();
        }
    }

    public void setVerticalSpeedKp(float value)
    {
        if (initialized)
        {
            PlayerPrefs.SetFloat("verticalSpeedKp", value);
            verticalSpeedKpSlider.GetComponentInChildren<Text>().text = "Пропорциональный коэффициент вертикальной скорости (Kp): " + value.ToString("F0") + " Гц";
            controllerManager.LoadPlayerPrefs();
        }
    }

    public void setCameraTilt(float value)
    {
        if (initialized)
        {
            PlayerPrefs.SetFloat("cameraTilt", value);
            cameraTiltSlider.GetComponentInChildren<Text>().text = "Наклон FPV камеры: " + value.ToString("F0") + "°";
            cameraSwitch.LoadPlayerPrefs();
        }
    }

    public void setCameraFOV(float value)
    {
        if (initialized)
        {
            PlayerPrefs.SetFloat("cameraFOV", value);
            cameraFOVSlider.GetComponentInChildren<Text>().text = "Угол обзора FPV камеры: " + value.ToString("F0") + "°";
            cameraSwitch.LoadPlayerPrefs();
        }
    }

    public void setBatteryToggle(bool value)
    {
        if (initialized)
        {
            PlayerPrefs.SetInt("enableBattery", value ? 1 : 0);
            Debug.Log(value);
            //batteryManager.LoadPlayerPrefs();
        }
    }
    public void setFixToggle(bool value)
    {
        if (initialized)
        {
            PlayerPrefs.SetInt("enableAutoFixMode", value ? 1 : 0);
            Debug.Log(value);
            for (int i=0; i<cargo.Length; i++)
            {
                //cargoManager = cargo[i].GetComponent<Cargo>();
                //cargoManager.LoadPlayerPrefs();
            }
        }
    }
    public void setModeToggle(bool value)
    {
        if (initialized)
        {
            PlayerPrefs.SetInt("enableAutoMode", value ? 1 : 0);
            Debug.Log(value);
            for (int i=0; i<cargo.Length; i++)
            {
                //cargoManager = cargo[i].GetComponent<Cargo>();
                //cargoManager.LoadPlayerPrefs();
            }
        }
    }

    private void OnDroneOptionsPerformed(InputAction.CallbackContext context)
    {
        OpenDroneMenu();
    }
    
    public void OpenDroneMenu()
    {
        optionsPanel.SetActive(!optionsPanel.activeSelf);
        Initialize();
        optionMenuPaused = !optionMenuPaused;
    }

    private void OnDroneOptionsCanceled(InputAction.CallbackContext context)
    {

    }

    public void resetDefaults()
    {
        setCentreRollRate(defaultCentreRollRate);
        setMaxRollRate(defaultMaxRollRate);
        setExpoRoll(defaultExpoRoll);
        setCentrePitchRate(defaultCentrePitchRate);
        setMaxPitchRate(defaultMaxRollRate);
        setExpoPitch(defaultExpoPitch);
        setCentreYawRate(defaultCentreYawRate);
        setMaxYawRate(defaultMaxYawRate);
        setExpoYaw(defaultExpoYaw);

        setInvertAxisT(defaultInvertAxisT);
        setInvertAxisR(defaultInvertAxisR);
        setInvertAxisE(defaultInvertAxisE);
        setInvertAxisA(defaultInvertAxisA);
        setTiltRateKp(defaultTiltRateKp);
        setYawRateKp(defaultYawRateKp);
        setEnableStabilization(defaultEnableStabilization);
        setTiltAngle(defaultTiltAngle);
        setVerticalSpeed(defaultVerticalSpeed);
        setTiltAngleKp(defaultTiltAngleKp);
        setYawAngleKp(defaultYawAngleKp);
        setVerticalSpeedKp(defaultVerticalSpeedKp);
        setCameraTilt(defaultCameraTilt);
        setCameraFOV(defaultCameraFOV);
        setBatteryToggle(defaultEnabledBattery);
        setFixToggle(defaultFixToggle);
        setModeToggle(defaultModeToggle);

        
        centreRollRateInputField.text = PlayerPrefs.GetFloat("centreRollRate").ToString();
        maxRollRateInputField.text = PlayerPrefs.GetFloat("maxRollRate").ToString();
        expoRollInputField.text = PlayerPrefs.GetFloat("expoRoll").ToString("F2");

        centrePitchRateInputField.text = PlayerPrefs.GetFloat("centrePitchRate").ToString();
        maxPitchRateInputField.text = PlayerPrefs.GetFloat("maxPitchRate").ToString();
        expoPitchInputField.text = PlayerPrefs.GetFloat("expoPitch").ToString("F2");

        centreYawRateInputField.text = PlayerPrefs.GetFloat("centreYawRate").ToString();
        maxYawRateInputField.text = PlayerPrefs.GetFloat("maxYawRate").ToString();
        expoYawInputField.text = PlayerPrefs.GetFloat("expoYaw").ToString("F2");

        axisToggleT.isOn = (PlayerPrefs.GetInt("invertAxisT") > 0);
        axisToggleR.isOn = (PlayerPrefs.GetInt("invertAxisR") > 0);
        axisToggleE.isOn = (PlayerPrefs.GetInt("invertAxisE") > 0);
        axisToggleA.isOn = (PlayerPrefs.GetInt("invertAxisA") > 0);
        tiltRateKpSlider.value = PlayerPrefs.GetFloat("tiltRateKp");
        yawRateKpSlider.value = PlayerPrefs.GetFloat("yawRateKp");
        enableStabilizationToggle.isOn = (PlayerPrefs.GetInt("enableStabilization") > 0);
        tiltAngleSlider.value = PlayerPrefs.GetFloat("tiltAngle");
        verticalSpeedSlider.value = PlayerPrefs.GetFloat("verticalSpeed");
        tiltAngleKpSlider.value = PlayerPrefs.GetFloat("tiltAngleKp");
        yawAngleKpSlider.value = PlayerPrefs.GetFloat("yawAngleKp");
        verticalSpeedKpSlider.value = PlayerPrefs.GetFloat("verticalSpeedKp");
        cameraTiltSlider.value = PlayerPrefs.GetFloat("cameraTilt");
        cameraFOVSlider.value = PlayerPrefs.GetFloat("cameraFOV");
        batteryToggle.isOn = (PlayerPrefs.GetInt("enableBattery") > 0);
        batteryToggle.isOn = (PlayerPrefs.GetInt("enableAutoFixMode") > 0);
        batteryToggle.isOn = (PlayerPrefs.GetInt("enableAutoMode") > 0);

    }

    private void UpdateSliderTexts()
    {
        //tiltSensitivitySlider.GetComponentInChildren<Text>().text = "Чувствительность наклона: " + tiltSensitivitySlider.value.ToString("F0") + "°/с";
        //yawSensitivitySlider.GetComponentInChildren<Text>().text = "Чувствительность рыскания: " + yawSensitivitySlider.value.ToString("F0") + "°/с";
        //heightSensitivitySlider.GetComponentInChildren<Text>().text = "Чувствительность высоты: " + heightSensitivitySlider.value.ToString("F0") + " об/мин";
        tiltRateKpSlider.GetComponentInChildren<Text>().text = "Пропорциональный коэффициент скорости наклона (Kp): " + tiltRateKpSlider.value.ToString("F0") + " Гц";
        yawRateKpSlider.GetComponentInChildren<Text>().text = "Пропорциональный коэффициент скорости рыскания (Kp): " + yawRateKpSlider.value.ToString("F0") + " Гц";
        tiltAngleSlider.GetComponentInChildren<Text>().text = "Угол наклона: " + tiltAngleSlider.value.ToString("F0") + "°";
        verticalSpeedSlider.GetComponentInChildren<Text>().text = "Вертикальная скорость: " + verticalSpeedSlider.value.ToString("F0") + " м/с";
        tiltAngleKpSlider.GetComponentInChildren<Text>().text = "Пропорциональный коэффициент угла наклона (Kp): " + tiltAngleKpSlider.value.ToString("F0") + " Гц";
        yawAngleKpSlider.GetComponentInChildren<Text>().text = "Пропорциональный коэффициент угла рыскания (Kp): " + yawAngleKpSlider.value.ToString("F0") + " Гц";
        verticalSpeedKpSlider.GetComponentInChildren<Text>().text = "Пропорциональный коэффициент вертикальной скорости (Kp): " + verticalSpeedKpSlider.value.ToString("F0") + " Гц";
        cameraTiltSlider.GetComponentInChildren<Text>().text = "Наклон FPV камеры: " + cameraTiltSlider.value.ToString("F0") + "°";
        cameraFOVSlider.GetComponentInChildren<Text>().text = "Угол обзора FPV камеры: " + cameraFOVSlider.value.ToString("F0") + "°";
    }
}
