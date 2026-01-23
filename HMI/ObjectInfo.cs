using UnityEngine;
using UnityEngine.UI;

public class ObjectInfo : MonoBehaviour
{
    public GameObject objectObject;
    public Rigidbody objectRigidbody;
    public ControllerManager controllerManager;
    public BatteryManager batteryScript;
    public PropellersController propeller;
    public Text flightMode;
    public Text yawtext;
    public Text pitchtext;
    public Text speedText;
    public Text rpmText;
    public Text massText;
    public Text heightText;
    public Text batteryVoltageText;
    public Text batteryVoltagePerCellText;
    public Text batteryPowerText;
    public Text batteryCapacityText;
    public GameObject batteryLow;
    public Text flighttimeText;
    public Text distanceText;

    private float distanceTraveled = 0f;
    float timeToDisplay;

    void Start()
    {
        objectObject = GameObject.FindGameObjectWithTag("Player");
        objectRigidbody = objectObject.GetComponent<Rigidbody>();
        controllerManager = objectObject.GetComponent<ControllerManager>();
        batteryScript = objectObject.GetComponent<BatteryManager>();
        propeller = objectObject.GetComponent<PropellersController>();
        Time.timeScale = 1f;
        timeToDisplay = 0;
        distanceTraveled = 0f;
    }
    
    void Update()
    {
        timeToDisplay += Time.deltaTime;
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);  
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        flighttimeText.text = string.Format("flyMin {0:00}:{1:00} min", minutes, seconds) ;

        
        switch (controllerManager.anglesControl)
                {
                    case ControllerManager.ControlMode.Stabilized:
                        flightMode.text = "Angle";
                        break;
                    case ControllerManager.ControlMode.Manual:
                        flightMode.text = "ACRO";
                        break;
                }

        float height = objectObject.transform.position.y;
        float mass = objectRigidbody.mass;

        
        float[] batteryCharge = {batteryScript.GetActualVoltagePerCell(), batteryScript.GetActualVoltage(), batteryScript.GetCurrentDraw(), batteryScript.GetCurrentCharge()};
        float _rpm = propeller.getRPM(1);
        // Получаем скорость
        Vector3 velocity = objectRigidbody.velocity;
        float speed = velocity.magnitude;

        float distanceThisFrame = speed * Time.deltaTime;
        distanceTraveled += distanceThisFrame; 

        // Выводим информацию в текст
        distanceText.text = "DST " + (distanceTraveled/1000).ToString("F2") + "km";
        massText.text = mass.ToString("F2") + " kg";
        speedText.text = Mathf.Round(speed).ToString()+ " km/h";
        rpmText.text = "RPM " + _rpm.ToString("F2")+ " ";
        heightText.text = "ALT " + Mathf.Round(height).ToString()+ " m";

        batteryVoltagePerCellText.text = batteryCharge[0].ToString("F2") + " V";
        batteryVoltageText.text = batteryCharge[1].ToString("F2") + " V";
        batteryPowerText.text = batteryCharge[2].ToString("F2") + " A";
        batteryCapacityText.text = batteryCharge[3].ToString("F2") + " %";

        if (batteryCharge[0]<3.5f)
        {
            batteryLow.SetActive(true);
        }
        else
        {
            batteryLow.SetActive(false);
        }
    }
}