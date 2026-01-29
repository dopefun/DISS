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

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        groundLayer = LayerMask.GetMask("Default");
        rb = GetComponent<Rigidbody>();
        raceManager = (RaceManager)FindObjectOfType(typeof(RaceManager));
    }

    void Update()
    {
        if (IsCrashed() && IsFlippedOver())
        {
            flipTime += Time.deltaTime;
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
        GetComponent<BatteryManager>().resetCharge();

        EnergyEfficiencyTracker energyTracker = GetComponent<EnergyEfficiencyTracker>();
        if (energyTracker != null)
        {
            energyTracker.RegisterReset();
        }

        GateAccuracyTracker accuracyTracker = GetComponent<GateAccuracyTracker>(); // ← ДОБАВИТЬ
        if (accuracyTracker != null)
        {
            accuracyTracker.RegisterReset();
        }
        GetComponent<ControllerManager>().resetYawRef();
    }

    public void setRaceManager(RaceManager rm)
    {
        raceManager = rm;
    }

    bool IsCrashed()
    {
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
        }
        return false;
    }

    bool IsFlippedOver()
    {
        float angle = Vector3.Angle(Vector3.up, rb.transform.up);
        return angle > maxAngle;
    }

        private void OnDestroy()
    {
        if (restartAction != null)
        {
            restartAction.performed -= OnRestartPerformed;
        }
    }

    private void OnRestartPerformed(InputAction.CallbackContext context)
    {
        if (this != null && gameObject != null)
        {
            Restart();
        }
    }


}