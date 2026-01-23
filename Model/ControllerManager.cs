using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class ControllerManager : MonoBehaviour
{
    public enum ControlMode { Stabilized, Manual };
    public BatteryManager batteryManager;

    public ControlMode anglesControl;
    public float tiltAngle;
    
    //Center sens Actual rates
    public float centreRollRate = 220f;
    public float centrePitchRate = 220f;
    public float centreYawRate = 200f;
    //Max sens Actual rates
    public float maxRollRate = 800f;
    public float maxPitchRate = 800f;
    public float maxYawRate = 800f;
    //Expo Actual rates
    public float expoRoll = 0.5f;
    public float expoPitch = 0.5f;
    public float expoYaw = 0.5f;
    
    public float tiltAngleKp;
    public float yawAngleKp;
    public float tiltRateKp;
    public float yawRateKp;

    public ControlMode heightControl;
    public float verticalSpeedSensitivity;
    public float heightSensitivity;
    public float verticalSpeedKp;

    public bool squareMapping;
    public bool invertAxisT;
    public bool invertAxisR;
    public bool invertAxisE;
    public bool invertAxisA;
    public bool startTick = false;

    private float rollRef;
    private float pitchRef;
    private float yawRef;
    private Vector3 angularRatesRef;
    private float verticalSpeedRef;
    private Rigidbody rb;
    private Vector3 acceleration;
    private float commandsMultiplier;
    private float lastUserRudderInput;
    public float heightFeedforward = 8200;

    private PropellersController propellersController;

    public InputActionAsset inputActionAsset;
    private InputAction throttleAction;
    private InputAction rudderAction;
    private InputAction aileronAction;
    private InputAction elevatorAction;
    private InputAction boostAction;
    private InputAction armingAction;
    public bool isPaused = false;
    private bool isHovering = false;
    private float hoveringForce;


    [SerializeField] private GameObject AttentionCanvas;

    void Start()
    {
        inputActionAsset = Resources.Load<InputActionAsset>("InputActions");
        propellersController = GetComponent<PropellersController>();
        rb = GetComponent<Rigidbody>();

        throttleAction = inputActionAsset.FindAction("Throttle");
        rudderAction = inputActionAsset.FindAction("Rudder");
        aileronAction = inputActionAsset.FindAction("Aileron");
        elevatorAction = inputActionAsset.FindAction("Elevator");
        boostAction = inputActionAsset.FindAction("Boost");
        armingAction = inputActionAsset.FindAction("Arming");

        throttleAction.performed += OnThrottlePerformed;
        throttleAction.canceled += OnThrottleCanceled;
        rudderAction.performed += OnRudderPerformed;
        rudderAction.canceled += OnRudderCanceled;
        aileronAction.performed += OnAileronPerformed;
        aileronAction.canceled += OnAileronCanceled;
        elevatorAction.performed += OnElevatorPerformed;
        elevatorAction.canceled += OnElevatorCanceled;
        boostAction.performed += OnBoostPerformed;
        boostAction.canceled += OnBoostCanceled;
        armingAction.performed += OnArmPerformed;
        armingAction.canceled += OnArmCanceled;

        rb.inertiaTensor = new Vector3(12e-4f, 15e-4f, 10e-4f);

        //heightFeedforward = (float)Math.Sqrt((rb.mass*1000*9.81f)/(4f * propellersController._thrustModel.x));

        LoadPlayerPrefs();
        resetYawRef();
        Dbg.Trace("phi, theta, psi, phi_ref, theta_ref, psi_ref, p, q, r, p_ref, q_ref, r_ref, torque_x, torque_y, torque_z, vz, vz_ref, thrust_Z");
    }

    void FixedUpdate()
    {
        ArmingMode();
        //CheckingStart();
        if (startTick)
        {
            if (isHovering)
            {
                rb.AddForce(Vector3.up * hoveringForce); // Компенсация гравитации
            }
                    
            if ((true))// && startTick)
            {
                rb.isKinematic = false;
                float heightCommandCompensated = 0;
                float heightFeedforwardCompensated = 0;
                Vector3 anglesCommand = Vector3.zero;

                switch (heightControl)
                {
                    case ControlMode.Stabilized:
                        heightFeedforwardCompensated = applyAngleCompensation(heightFeedforward);
                        heightCommandCompensated = applyAngleCompensation(controlVerticalSpeed());
                        break;
                    case ControlMode.Manual:
                        heightFeedforwardCompensated = heightFeedforward + controlThrottle();
                        heightCommandCompensated = 0;
                        break;
                }
                switch (anglesControl)
                {
                    case ControlMode.Stabilized:
                        anglesCommand = controlAngles();
                        break;
                    case ControlMode.Manual:
                        anglesCommand = controlRates();
                        break;
                }

                propellersController.MixRPM(heightFeedforwardCompensated, heightCommandCompensated, anglesCommand.x, anglesCommand.y, anglesCommand.z);
                Vector3 f = propellersController.getTotalForce();
                Vector3 t = propellersController.getTotalTorque();
                rb.AddRelativeForce(f);
                rb.AddRelativeTorque(t);
                Dbg.Trace(string.Format("{0:0.0000000}, {1:0.0000000}, {2:0.0000000}, {3:0.0000000}, {4:0.0000000}, {5:0.0000000}, " +
                                        "{6:0.0000000}, {7:0.0000000}, {8:0.0000000}, {9:0.0000000}, {10:0.0000000}, {11:0.0000000}, " +
                                        "{12:0.0000000}, {13:0.0000000}, {14:0.0000000}, {15:0.0000000}, {16:0.0000000}, {17:0.0000000}",
                                        transform.eulerAngles.z, transform.eulerAngles.x, transform.eulerAngles.y, rollRef, pitchRef, yawRef,
                                        -rb.angularVelocity.z, -rb.angularVelocity.x, rb.angularVelocity.y,
                                        angularRatesRef.x, angularRatesRef.y, angularRatesRef.z, -t.z, -t.x, t.y,
                                        rb.velocity.y, verticalSpeedRef, f.y));
                acceleration = transform.rotation * f / rb.mass - 9.81f * Vector3.up;
            }
            else
            {
                propellersController.StopAllMotors();
            }
        }
    }

    void ArmingMode()
    {
        if (armingAction != null)
        {
            string bindingDisplayString = armingAction.GetBindingDisplayString();

            if (string.IsNullOrEmpty(bindingDisplayString))
            {
                startTick = true;
            }
            else
            {
                if (armingAction.ReadValue<float>() > 0)
                {
                    propellersController.StartMotors();
                    if (!startTick) startTick = true;
                }
                else 
                {
                    propellersController.StopAllMotors();
                }
            }
        }
    }


    float controlThrottle()
    {

        float userInput = throttleAction.ReadValue<float>() * (invertAxisT ? -1 : 1);
        float throttle = heightSensitivity * userInput;
        batteryManager.SetCurrentDraw(throttle);
        return throttle;
    }

    float controlVerticalSpeed()
    {
        float userInput = throttleAction.ReadValue<float>() * (invertAxisT ? -1 : 1);
        verticalSpeedRef = verticalSpeedSensitivity * userInput;
        return heightLoop(verticalSpeedRef);
    }

    float heightLoop(float verticalSpeedRef)
    {
        float verticalSpeed = rb.velocity.y;
        float Cz = 1.4f;
        return verticalSpeedKp * (verticalSpeedRef - verticalSpeed) + Cz * verticalSpeed;
    }

    float applyAngleCompensation(float heightCommand)
    {
        float coscos = Mathf.Cos(transform.eulerAngles.x * Mathf.Deg2Rad) * Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad);
        if (coscos > 0)
        {
            return heightCommand / Mathf.Sqrt(coscos);
        }
        else
        {
            return 0;
        }
    }

    // Функция для расчета Actual Rates
    private float ExpoFactor(float x, float g)
    {
        return Mathf.Sign(x) * Mathf.Abs(x) * (Mathf.Pow(Mathf.Abs(x), 5) * g + Mathf.Abs(x) * (1 - g));
    }

    private float ActualRates(float x, float d, float f, float g)
    {
        float h = ExpoFactor(x, g);
        if (d < f)
            return (d * x) + ((f - d) * h);
        else
            return (d * x);
    }

    // Функция для расчета BF
    private float BfSuperFactor(float x, float b)
    {
        return 1 / (1 - (x * b));
    }

    private float BfRcCommandFactor(float x, float c)
    {
        return (Mathf.Pow(x, 4) * c) + (x * (1 - c));
    }

    private float BfExpoFactor(float x, float a, float c)
    {
        float q = BfRcCommandFactor(x, c);
        return 200 * q * a;
    }

    private float BetaflightRates(float x, float a, float b, float c)
    {
        float p = BfSuperFactor(x, b);
        float r = BfExpoFactor(x, a, c);
        return r * p;
    }

    Vector3 controlRates()
    {
        float userRudderInput = rudderAction.ReadValue<float>() * (invertAxisR ? -1 : 1);
        float userAileronInput = aileronAction.ReadValue<float>() * (invertAxisA ? -1 : 1);
        float userElevatorInput = elevatorAction.ReadValue<float>() * (invertAxisE ? -1 : 1);

        float rollRateRef = Mathf.Deg2Rad * ActualRates(userAileronInput, centreRollRate, maxRollRate, expoRoll);
        float pitchRateRef = Mathf.Deg2Rad * ActualRates(userElevatorInput, centrePitchRate, maxPitchRate, expoPitch);
        float yawRateRef = Mathf.Deg2Rad * ActualRates(userRudderInput, centreYawRate, maxYawRate, expoYaw);

        angularRatesRef = new Vector3(rollRateRef, pitchRateRef, yawRateRef);

        return rateLoop(angularRatesRef);
    }


    Vector3 controlAngles()
    {
        float userRudderInput = rudderAction.ReadValue<float>() * (invertAxisR ? -1 : 1);
        float userAileronInput = aileronAction.ReadValue<float>() * (invertAxisA ? -1 : 1);
        float userElevatorInput = elevatorAction.ReadValue<float>() * (invertAxisE ? -1 : 1);

        float boost = boostAction.ReadValue<float>();
        commandsMultiplier = 1 + boost;

        userRudderInput *= commandsMultiplier;
        userAileronInput *= commandsMultiplier;
        userElevatorInput *= commandsMultiplier;

        rollRef = tiltAngle * userAileronInput;
        pitchRef = -tiltAngle * userElevatorInput;
        yawRef += userRudderInput * centreYawRate * Time.deltaTime;
        if ((lastUserRudderInput != 0) && (userRudderInput == 0))
        {
            yawRef += angularRatesRef.z * 1 / yawRateKp;
        }
        lastUserRudderInput = userRudderInput;

        return quaternionLoop(Quaternion.Euler(-pitchRef, yawRef, -rollRef));
    }
    public void EnableHovering()
    {
        hoveringForce = rb.mass * Physics.gravity.magnitude;
        isHovering = true;
    }
    public void DisableHovering()
    {
        isHovering = false;
    }

    public void AllignDrone(Transform target, ref bool isAlligned)
    {
        float rotationSpeed = 10f;
        rb.velocity = Vector3.zero; // Остановить линейное движение
        rb.angularVelocity = Vector3.zero; // Остановить вращение

        Quaternion targetRotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (transform.rotation.x == target.transform.rotation.x && transform.rotation.z == target.transform.rotation.z)
        {
            isAlligned = true;
        }

    }

    Vector3 quaternionLoop(Quaternion quaternionRef)
    {
        Quaternion quaternionEst = transform.rotation;
        Quaternion quaternionError = Quaternion.Inverse(quaternionEst) * quaternionRef;
        Vector3 axis = Vector3.zero; float angle = 0;
        quaternionError.ToAngleAxis(out angle, out axis);
        if (angle > 180) angle -= 360;
        if (angle < -180) angle += 180;
        if (double.IsInfinity(axis.magnitude))
            axis = Vector3.zero;
        angularRatesRef = angle * Mathf.Deg2Rad * (new Vector3(-axis.z, -axis.x, axis.y));
        angularRatesRef.x *= tiltAngleKp;
        angularRatesRef.y *= tiltAngleKp;
        angularRatesRef.z *= yawAngleKp;
        return rateLoop(angularRatesRef);
    }

    Vector3 rateLoop(Vector3 angularRatesRef)
    {
        Vector3 omega = transform.InverseTransformDirection(rb.angularVelocity);
        float rollCommand = tiltRateKp * (angularRatesRef.x + omega.z);
        float pitchCommand = tiltRateKp * (angularRatesRef.y + omega.x);
        float yawCommand = yawRateKp * (angularRatesRef.z - omega.y);
        Debug.Log("AngularVel:" + Mathf.Round(angularRatesRef.x) + "  " + Mathf.Round(angularRatesRef.y) + "  " + Mathf.Round(angularRatesRef.z));
        Debug.Log("OmegaVel:" + Mathf.Round(omega.z) + "  " + Mathf.Round(omega.x) + "  " + Mathf.Round(omega.y));
        Debug.Log("Controller:" + Mathf.Round(rollCommand) + "  " + Mathf.Round(pitchCommand) + "  " + Mathf.Round(yawCommand));
        return new Vector3(rollCommand, pitchCommand, yawCommand);
    }

    public void resetYawRef()
    {
        yawRef = transform.eulerAngles.y;
    }

    public void LoadPlayerPrefs()
    {
        
        if (PlayerPrefs.HasKey("centreRollRate"))
        {
            centreRollRate = PlayerPrefs.GetFloat("centreRollRate");
        }
        if (PlayerPrefs.HasKey("maxRollRate"))
        {
            maxRollRate = PlayerPrefs.GetFloat("maxRollRate");
        }
        if (PlayerPrefs.HasKey("expoRoll"))
        {
           expoRoll = PlayerPrefs.GetFloat("expoRoll");
        }

        if (PlayerPrefs.HasKey("centrePitchRate"))
        {
            centrePitchRate = PlayerPrefs.GetFloat("centrePitchRate");
        }
        if (PlayerPrefs.HasKey("maxPitchRate"))
        {
            maxPitchRate = PlayerPrefs.GetFloat("maxPitchRate");
        }
        if (PlayerPrefs.HasKey("expoPitch"))
        {
           expoPitch = PlayerPrefs.GetFloat("expoPitch");
        }

        if (PlayerPrefs.HasKey("centreYawRate"))
        {
            centreYawRate = PlayerPrefs.GetFloat("centreYawRate");
        }
        if (PlayerPrefs.HasKey("maxYawRate"))
        {
            maxYawRate = PlayerPrefs.GetFloat("maxYawRate");
        }
        if (PlayerPrefs.HasKey("expoYaw"))
        {
            expoYaw = PlayerPrefs.GetFloat("expoYaw");
        }


        if (PlayerPrefs.HasKey("heightSensitivity"))
        {
            heightSensitivity = PlayerPrefs.GetFloat("heightSensitivity");
        }
        if (PlayerPrefs.HasKey("invertAxisT"))
        {
            invertAxisT = (PlayerPrefs.GetInt("invertAxisT") > 0);
        }
        if (PlayerPrefs.HasKey("invertAxisR"))
        {
            invertAxisR = (PlayerPrefs.GetInt("invertAxisR") > 0);
        }
        if (PlayerPrefs.HasKey("invertAxisE"))
        {
            invertAxisE = (PlayerPrefs.GetInt("invertAxisE") > 0);
        }
        if (PlayerPrefs.HasKey("invertAxisA"))
        {
            invertAxisA = (PlayerPrefs.GetInt("invertAxisA") > 0);
        }
        if (PlayerPrefs.HasKey("tiltRateKp"))
        {
            tiltRateKp = PlayerPrefs.GetFloat("tiltRateKp");
        }
        if (PlayerPrefs.HasKey("yawRateKp"))
        {
            yawRateKp = PlayerPrefs.GetFloat("yawRateKp");
        }
        if (PlayerPrefs.HasKey("enableStabilization"))
        {
            bool enableStabilization = (PlayerPrefs.GetInt("enableStabilization") > 0);
            if (enableStabilization)
            {
                anglesControl = ControlMode.Stabilized;
                heightControl = ControlMode.Stabilized;
            }
            else
            {
                anglesControl = ControlMode.Manual;
                heightControl = ControlMode.Manual;
            }
        }
        if (PlayerPrefs.HasKey("tiltAngle"))
        {
            tiltAngle = PlayerPrefs.GetFloat("tiltAngle");
        }
        if (PlayerPrefs.HasKey("verticalSpeed"))
        {
            verticalSpeedSensitivity = PlayerPrefs.GetFloat("verticalSpeed");
        }
        if (PlayerPrefs.HasKey("tiltAngleKp"))
        {
            tiltAngleKp = PlayerPrefs.GetFloat("tiltAngleKp");
        }
        if (PlayerPrefs.HasKey("yawAngleKp"))
        {
            yawAngleKp = PlayerPrefs.GetFloat("yawAngleKp");
        }
        if (PlayerPrefs.HasKey("verticalSpeedKp"))
        {
            verticalSpeedKp = PlayerPrefs.GetFloat("verticalSpeedKp");
        }
    }

    public Vector3 getAcceleration()
    {
        return acceleration;
    }

    private void OnThrottlePerformed(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        batteryManager.SetCurrentDraw(value);
        // Use the value to control the throttle
    }

    private void OnThrottleCanceled(InputAction.CallbackContext context)
    {
        // Handle the throttle being released
    }

    private void OnRudderPerformed(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        // Use the value to control the rudder
    }

    private void OnRudderCanceled(InputAction.CallbackContext context)
    {
        // Handle the rudder being released
    }

    private void OnAileronPerformed(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        // Use the value to control the aileron
    }

    private void OnAileronCanceled(InputAction.CallbackContext context)
    {
        // Handle the aileron being released
    }

    private void OnElevatorPerformed(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        // Use the value to control the elevator
    }

    private void OnElevatorCanceled(InputAction.CallbackContext context)
    {
        // Handle the elevator being released
    }

    private void OnBoostPerformed(InputAction.CallbackContext context)
    {
        // Handle the boost being pressed
    }

    private void OnBoostCanceled(InputAction.CallbackContext context)
    {
        // Handle the boost being released
    }
    private void OnArmPerformed(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
    }
    private void OnArmCanceled(InputAction.CallbackContext context)
    {
        // Handle the boost being released
    }
}