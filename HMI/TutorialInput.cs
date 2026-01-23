using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialInput : MonoBehaviour
{
   public GameObject RStick;
    public GameObject LStick;
    public GameObject RPlatform;
    public GameObject LPlatform;
    public GameObject drone;

    // Настройки
    public float platformRotateAngle = 30f;    // угол вращения платформы (вперёд-назад по оси Y)
    public float stickRotateAngle = 30f;       // угол поворота стика (влево/вправо)
    public float maxDroneHeight = 0.5f;        // высота подъема дрона
    public float maxDroneRotation = 60f;       // наклон дрона по осям

     // Позиции и повороты для LStick (Throttle)
    [Header("Throttle Position and Rotation")]
    public Vector3 topPos = new Vector3(-0.0201f, 0.0107f, 0.03902249f);
    public Vector3 middlePos = new Vector3(-0.01901774f, 0.01171468f, 0.03902249f);
    public Vector3 bottomPos = new Vector3(-0.0177f, 0.0122f, 0.039f);

    public Vector3 topRot = new Vector3(70.671f, -90f, 90f);
    public Vector3 middleRot = new Vector3(89.607f, -90.002f, 90f);
    public Vector3 bottomRot = new Vector3(109.985f, -90f, 90f);

    // Позиции и повороты для Elevator (Z-ось)
    [Header("Elevator Position and Rotation")]
    public Vector3 elevatorTopPos = new Vector3(-0.0201f, 0.0107f, -0.03902249f); // Положительное значение Z
    public Vector3 elevatorMiddlePos = new Vector3(-0.01901774f, 0.01171468f, -0.03902249f);
    public Vector3 elevatorBottomPos = new Vector3(-0.0177f, 0.0122f, -0.039f);

    public Vector3 elevatorTopRot = new Vector3(70.671f, -90f, 90f);
    public Vector3 elevatorMiddleRot = new Vector3(89.607f, -90.002f, 90f);
    public Vector3 elevatorBottomRot = new Vector3(109.985f, -90f, 90f);

    public float lerpSpeed = 20f;

    private Vector3 droneStartPos;

    private InputAction throttleAction;
    private InputAction rudderAction;
    private InputAction aileronAction;
    private InputAction elevatorAction;

    void Awake()
    {
        var inputActionAsset = Resources.Load<InputActionAsset>("InputActions");
        throttleAction = inputActionAsset.FindAction("Throttle");
        rudderAction = inputActionAsset.FindAction("Rudder");
        aileronAction = inputActionAsset.FindAction("Aileron");
        elevatorAction = inputActionAsset.FindAction("Elevator");

        droneStartPos = drone.transform.position;
    }

    void OnEnable()
    {
        throttleAction.Enable();
        rudderAction.Enable();
        aileronAction.Enable();
        elevatorAction.Enable();
    }

    void OnDisable()
    {
        throttleAction.Disable();
        rudderAction.Disable();
        aileronAction.Disable();
        elevatorAction.Disable();
    }

    void Update()
    {
        float throttle = throttleAction.ReadValue<float>();   // Вперёд-назад LPlatform
        float rudder = rudderAction.ReadValue<float>();       // Поворот LStick
        float elevator = elevatorAction.ReadValue<float>();   // Вперёд-назад RPlatform
        float aileron = aileronAction.ReadValue<float>();     // Поворот RStick

        // Для Throttle: получаем значение от -1 до 1 и преобразуем в диапазон от 0 до 1
        float throttleInput = throttleAction.ReadValue<float>();
        float throttleT = (throttleInput+ 1f) / 2f;;
        
        float elevatorInput = elevatorAction.ReadValue<float>();
        float elevatorT = (elevatorInput+ 1f) / 2f;;

        // Интерполяция для Throttle (LStick)
        Vector3 targetThrottlePos, targetThrottleRotEuler;

        if (throttleT < 0.5f)
        {
            float throttleLow = throttleT / 0.5f;
            targetThrottlePos = Vector3.Lerp(bottomPos, middlePos, throttleLow);
            targetThrottleRotEuler = Vector3.Lerp(bottomRot, middleRot, throttleLow);
        }
        else
        {
            float throttleHigh = (throttleT - 0.5f) / 0.5f;
            targetThrottlePos = Vector3.Lerp(middlePos, topPos, throttleHigh);
            targetThrottleRotEuler = Vector3.Lerp(middleRot, topRot, throttleHigh);
        }

        // Плавное применение позиции и поворота для LStick
        LPlatform.transform.localPosition = Vector3.Lerp(LPlatform.transform.localPosition, targetThrottlePos, Time.deltaTime * lerpSpeed);
        Quaternion targetThrottleRot = Quaternion.Euler(targetThrottleRotEuler);
        LPlatform.transform.localRotation = Quaternion.Lerp(LPlatform.transform.localRotation, targetThrottleRot, Time.deltaTime * lerpSpeed);

        // Интерполяция для Elevator (ElevatorStick)
        Vector3 targetElevatorPos, targetElevatorRotEuler;

        if (elevatorT  < 0.5f)
        {
            float elevatorLow = elevatorT / 0.5f;
            targetElevatorPos = Vector3.Lerp(elevatorBottomPos, elevatorMiddlePos, elevatorLow);
            targetElevatorRotEuler = Vector3.Lerp(elevatorBottomRot, elevatorMiddleRot, elevatorLow);
        }
        else
        {
            float elevatorHigh = (elevatorT - 0.5f) / 0.5f;
            targetElevatorPos = Vector3.Lerp(elevatorMiddlePos, elevatorTopPos, elevatorHigh);
            targetElevatorRotEuler = Vector3.Lerp(elevatorMiddleRot, elevatorTopRot, elevatorHigh);
        }

        // Плавное применение позиции и поворота для ElevatorStick
        RPlatform.transform.localPosition = Vector3.Lerp(RPlatform.transform.localPosition, targetElevatorPos, Time.deltaTime * lerpSpeed);
        Quaternion targetElevatorRot = Quaternion.Euler(targetElevatorRotEuler);
        RPlatform.transform.localRotation = Quaternion.Lerp(RPlatform.transform.localRotation, targetElevatorRot, Time.deltaTime * lerpSpeed);
        
        // === Стики крутятся влево/вправо ===
        LStick.transform.localRotation = Quaternion.Euler(0f, 180f, -rudder * stickRotateAngle - 10f);
        RStick.transform.localRotation = Quaternion.Euler(0f, 180f, -aileron * stickRotateAngle - 10f);

        // === Дрон поднимается и наклоняется ===
        float droneY = Mathf.Lerp(0, maxDroneHeight, (throttle + 1f) / 2f);
        drone.transform.localPosition = droneStartPos + Vector3.up * droneY;

        float yaw = rudder * maxDroneRotation;;
        float pitch = elevator * maxDroneRotation;
        float roll = -aileron * maxDroneRotation;

        drone.transform.localRotation = Quaternion.Euler(pitch, yaw, roll);
    }
}
