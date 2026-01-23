using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GamepadInput : MonoBehaviour
{
    public float height = 100f;
    private Vector2 offsetL;
    private Vector2 offsetR;
    public Image leftStickImage; 
    public Image rightStickImage; 

    private InputActionAsset inputActionAsset;
    private InputAction leftStickXAction;
    private InputAction leftStickYAction;
    private InputAction rightStickXAction;
    private InputAction rightStickYAction;

    private bool axisToggleT;
    private bool axisToggleR;
    private bool axisToggleA;
    private bool axisToggleE;

    private void Awake()
    {
        inputActionAsset = Resources.Load<InputActionAsset>("InputActions");
        leftStickXAction = inputActionAsset.FindAction("Rudder");
        leftStickYAction = inputActionAsset.FindAction("Throttle");
        rightStickXAction = inputActionAsset.FindAction("Aileron");
        rightStickYAction = inputActionAsset.FindAction("Elevator");

        offsetL = leftStickImage.rectTransform.anchoredPosition;
        offsetR = rightStickImage.rectTransform.anchoredPosition;
    }

    void Update()
    {
        if (leftStickImage == null)
        {
            Debug.LogError("No left stick Image set!");
            return;
        }

        if (rightStickImage == null)
        {
            Debug.LogError("No right stick Image set!");
            return;
        }
        float leftStickX = leftStickXAction.ReadValue<float>();;
        float leftStickY = leftStickYAction.ReadValue<float>();
        float rightStickX = rightStickXAction.ReadValue<float>();;
        float rightStickY = rightStickYAction.ReadValue<float>();;
        
        if (PlayerPrefs.HasKey("invertAxisT"))
            {
                axisToggleT = (PlayerPrefs.GetInt("invertAxisT") > 0);
            }
        if (PlayerPrefs.HasKey("invertAxisR"))
            {
                axisToggleR = (PlayerPrefs.GetInt("invertAxisR") > 0);
            }
        if (PlayerPrefs.HasKey("invertAxisE"))
            {
                axisToggleE = (PlayerPrefs.GetInt("invertAxisE") > 0);
            }
        if (PlayerPrefs.HasKey("invertAxisA"))
            {
                axisToggleA = (PlayerPrefs.GetInt("invertAxisA") > 0);
            }
        
        if (axisToggleT || axisToggleR || axisToggleA || axisToggleE)
        {
        
            if (axisToggleR)
            {
                leftStickX = leftStickXAction.ReadValue<float>() * -1f;
            }
            if (axisToggleT)
            {
                leftStickY = leftStickYAction.ReadValue<float>() * -1f;
            }
            if (axisToggleA)
            {
                rightStickX = rightStickXAction.ReadValue<float>() * -1f;
            }
            if (axisToggleE)
            {
                rightStickY = rightStickYAction.ReadValue<float>() * -1f;
            }
    
        }

        // Вычисляем позицию изображения левого стика
        Vector2 leftStickPosition = new Vector2(leftStickX, leftStickY) * 0.5f;
        leftStickImage.rectTransform.anchoredPosition = leftStickPosition * height + offsetL;

        // Вычисляем позицию изображения правого стика
        Vector2 rightStickPosition = new Vector2(rightStickX, rightStickY) * 0.5f;
        rightStickImage.rectTransform.anchoredPosition = rightStickPosition * height + offsetR;
    }
}