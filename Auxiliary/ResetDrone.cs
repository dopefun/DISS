using UnityEngine;
using UnityEngine.InputSystem;

public class ResetDrone : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private RaceManager raceManager;

    public float minHeight = 1f; // Минимальная высота, при которой квадрокоптер считается упавшим
    public float maxAngle = 80f; // Максимальный угол, при котором квадрокоптер считается перевернувшимся
    public float flipTimeThreshold = 3f; // Пороговое время, в течение которого квадрокоптер должен быть перевернутым
    public LayerMask groundLayer; // Лейер маска, определяющая, какие слои считать землей

    private Rigidbody rb;
    private float flipTime = 0f;

    private InputActionAsset inputActionAsset;
    private InputAction restartAction;

    private void Awake()
    {
        inputActionAsset = Resources.Load<InputActionAsset>("InputActions");
        restartAction = inputActionAsset.FindAction("Restart");

        restartAction.performed += OnRestartPerformed;
    }

    // Use this for initialization
    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        groundLayer = LayerMask.GetMask("Default");
        rb = GetComponent<Rigidbody>();
        raceManager = (RaceManager)FindObjectOfType(typeof(RaceManager));
    }

    // Update is called once per frame
    void Update()
    {
        if (IsCrashed() && IsFlippedOver())
        {
            flipTime += Time.deltaTime;
            //Debug.Log(flipTime);

            if (flipTime >= flipTimeThreshold)
            {
                Restart();
            }
        }
        else
        {
            flipTime = 0f;
        }
    }

    public void Restart()
    {
        if (raceManager != null && raceManager.raceStarted && raceManager.PreviousGatePosition() != null)
        {
            if (raceManager.PreviousGatePosition().Find("SpawnPoint") != null)
            {
                transform.position = raceManager.PreviousGatePosition().Find("SpawnPoint").position;
                transform.rotation = raceManager.PreviousGatePosition().Find("SpawnPoint").rotation * Quaternion.Euler(0, 90, 0);
            }
            else
            {
                transform.position = raceManager.PreviousGatePosition().position;
                transform.rotation = raceManager.PreviousGatePosition().rotation;
                
            }
        }
        else
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        GetComponent<ControllerManager>().resetYawRef();
        GetComponent<BatteryManager>().resetCharge();
    }

    public void setRaceManager(RaceManager rm)
    {
        raceManager = rm;
    }

    bool IsCrashed()
    {
        // Вычисляем высоту террейна под квадрокоптером с помощью Raycast
        RaycastHit hit;
        if (Physics.Raycast(rb.transform.position, Vector3.down, out hit, Mathf.Infinity, groundLayer))
        {
            if (rb.transform.position.y - hit.point.y < minHeight)
            {
                Debug.DrawRay(rb.transform.position, Vector3.down * minHeight, Color.red);
                return true;
            }
            else
            {
                Debug.DrawRay(rb.transform.position, Vector3.down * minHeight, Color.yellow);
                return false;
            }
            // Если высота квадрокоптера над землей ниже минимальной, считаем его упавшим
           // return rb.transform.position.y - hit.point.y < minHeight;
        }
        //Debug.DrawRay(rb.transform.position, Vector3.down * hit.distance, Color.green);

        // Если Raycast не нашел землю, считаем, что квадрокоптер не упал
        return false;
    }

    bool IsFlippedOver()
    {
        // Вычисляем угол между вектором вверх и вертикалью квадрокоптера
        float angle = Vector3.Angle(Vector3.up, rb.transform.up);

        // Если угол больше максимального, считаем квадрокоптер перевернувшимся
        return angle > maxAngle;
    }

    private void OnRestartPerformed(InputAction.CallbackContext context)
    {
        Restart();
    }

}