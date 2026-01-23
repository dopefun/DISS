using UnityEngine;
using System;
using System.Collections;

public class PropellersController : MonoBehaviour 
{
    public float maxRPM = 12000;

    [SerializeField] private float motor1RPM;
    [SerializeField] private float motor2RPM;
    [SerializeField] private float motor3RPM;
    [SerializeField] private float motor4RPM;

    public float motorEfficiency = 0.8f; // Коэффициент эффективности моторов, можно подстроить под конкретные моторы
    public float motorCurrentFactor = 0.02f; // Фактор для расчета тока на основе RPM, зависит от характеристик моторов

    private Vector3[] propellersPositions;
    private Vector4 propellersCW;

    private Vector3 thrustModel;
    private Vector3 torqueModel;

    private Vector4 motorStarted;
    
    private Vector3 totalForce;
    private Vector3 totalTorque;

    private float b0_roll;
    private float b0_pitch;
    private float b0_yaw;
    private float b0_z;
    [Header("Quadrocopter Parameters")]
    [SerializeField] public  Vector3 _thrustModel = new Vector3(1.597752e-006f, -9.509696e-004f, 0 * 0.799216e+000f);
    [SerializeField] Vector3 _torqueModel = new Vector3(2.230120e-007f, -9.213740e-005f, -0 * 0.034894e+000f);
    [SerializeField] float _Lx = 0.10f;
    [SerializeField] float _Ly = 0.10f;

    //[SerializeField] float _Ix = 10e-4f;
    //[SerializeField] float _Iy = 12e-4f;
    //[SerializeField] float _Iz = 15e-4f;
    [SerializeField] float _L = 0.22f;
    [SerializeField] float _W = 0.96f;
    [SerializeField] float _H = 0.9f;
    [SerializeField] float heightFeedforward = 8200f;


    float _m;

    // Динамическое потребление тока
    private float[] targetCurrent = new float[4];
    private float[] currentMotorCurrent = new float[4];
    public float currentSmoothingFactor = 0.1f;

    // Учет ускорения
    private float[] previousRPM = new float[4];
    public float accelerationFactor = 1.5f;
    public float accelerationDuration = 0.5f;
    private float[] accelerationTimer = new float[4];

  
    
    void Start() 
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        //heightFeedforward = (float)Math.Sqrt((rb.mass*1000*9.81f)/(4f * _thrustModel.x));

        motor1RPM = motor2RPM = motor3RPM = motor4RPM = 0;

        thrustModel = _thrustModel / 1000 * 9.81f;
        torqueModel = _torqueModel / 1000;

        float Lx = _Lx;
        float Ly = _Ly;
        propellersPositions = new Vector3[4];
        propellersPositions[0] = new Vector3(-Lx, 0, Ly);
        propellersPositions[1] = new Vector3(Lx, 0, Ly);
        propellersPositions[2] = new Vector3(Lx, 0, -Ly);
        propellersPositions[3] = new Vector3(-Lx, 0, -Ly);
        propellersCW = new Vector4(-1, 1, -1, 1);
        motorStarted = Vector4.one;

        float m = rb.mass;

        float Ix = (m/12) * (_W * _W + _H * _H);
        float Iy =(m/12) * (_L * _L + _H * _H);
        float Iz = (m/12) * (_W * _W + _L * _L);
        _m = m;
        b0_roll = 4 * Lx * (2 * thrustModel.x * heightFeedforward + thrustModel.y) / Ix;
        b0_pitch = 4 * Ly * (2 * thrustModel.x * heightFeedforward + thrustModel.y) / Iy;
        b0_yaw = 4 * (2 * torqueModel.x * heightFeedforward + torqueModel.y) / Iz;
        b0_z = 4 * (2 * thrustModel.x * heightFeedforward + thrustModel.y) / m;
    }

    public void MixRPM(float heightFeedforward, float heightCommand, float rollCommand, float pitchCommand, float yawCommand) 
    {
        BatteryManager batteryManager = GetComponent<BatteryManager>();
        float currentVoltage = batteryManager.GetActualVoltage();
        float voltageFactor = currentVoltage / batteryManager.GetMaxVoltage(); // Коэффициент напряжения

        // Корректируем команды с учетом напряжения
        rollCommand = rollCommand / b0_roll;
        pitchCommand = pitchCommand / b0_pitch;
        yawCommand = yawCommand / b0_yaw;
        heightCommand = heightCommand / b0_z;

        // Применяем команды
        Vector4 RPM = new Vector4(
            pitchCommand + rollCommand - yawCommand,
            pitchCommand - rollCommand + yawCommand,
            -pitchCommand - rollCommand - yawCommand,
            -pitchCommand + rollCommand + yawCommand
        );

        // Ограничение высоты с учетом напряжения
        if (heightFeedforward < 2000)
            heightFeedforward = 2000;
        float heightTotal = heightFeedforward + heightCommand;
        for (int i = 0; i < 4; i++) {
            if (RPM[i] + heightTotal > maxRPM * voltageFactor) 
            {
                heightTotal = maxRPM * voltageFactor - RPM[i];
            }
        }

        // Общая команда
        RPM = RPM + new Vector4(heightTotal, heightTotal, heightTotal, heightTotal);

        // Ограничение RPM с учетом напряжения
        for (int i = 0; i < 4; i++) {
            if (RPM[i] < 0) 
            {
                RPM[i] = 0;
            } 
            else if (RPM[i] > maxRPM * voltageFactor) 
            {
                RPM[i] = maxRPM * voltageFactor;
            }
        }

        motor1RPM = RPM[0] * motorStarted[0];
        motor2RPM = RPM[1] * motorStarted[1];
        motor3RPM = RPM[2] * motorStarted[2];
        motor4RPM = RPM[3] * motorStarted[3];

        ApplyForces();
    }

    void ApplyForces() 
    {
        totalTorque = Vector3.zero;
        totalForce = Vector3.zero;
        float[] RPM = { motor1RPM, motor2RPM, motor3RPM, motor4RPM };
        for (int i = 0; i < 4; i++)
        {
            float propThrust = thrustModel.x * RPM[i] * RPM[i] + thrustModel.y * RPM[i] + thrustModel.z;
            float propDrag = torqueModel.x * RPM[i] * RPM[i] + torqueModel.y * RPM[i] + torqueModel.z;
            Vector3 propForce = propThrust * Vector3.up;
            Vector3 propTorque = propDrag * propellersCW[i] * Vector3.up + Vector3.Cross(propellersPositions[i], propForce);
            totalForce += propForce;
            totalTorque += propTorque;
        }
    }

    public float getPower() 
    {
        float[] RPM = { motor1RPM, motor2RPM, motor3RPM, motor4RPM };
        float totalPower = 0;
        for (int i = 0; i < 4; i++) {
            totalPower += RPM[i] * RPM[i];
        }
        return totalPower;
    }

    public float getMaxPower() 
    {
        return 4 * maxRPM * maxRPM;
    }

    /*public float GetCurrentConsumption()
    {
        float totalCurrent = 0;
        float[] RPM = { motor1RPM, motor2RPM, motor3RPM, motor4RPM };
        BatteryManager batteryManager = GetComponent<BatteryManager>();
        float currentVoltage = batteryManager.GetActualVoltage();

        for (int i = 0; i < 4; i++) {
            // Проверяем, было ли резкое увеличение RPM
            if (RPM[i] > previousRPM[i]) {
                accelerationTimer[i] = accelerationDuration; // Запускаем таймер
            }
            previousRPM[i] = RPM[i];

            // Целевой ток для каждого двигателя
            float target = (RPM[i] * RPM[i] * motorCurrentFactor) / (currentVoltage * motorEfficiency);

            // Увеличиваем ток при ускорении
            if (accelerationTimer[i] > 0) {
                target *= accelerationFactor;
                accelerationTimer[i] -= Time.deltaTime;
            }

            // Плавное изменение тока
            currentMotorCurrent[i] = Mathf.Lerp(currentMotorCurrent[i], target, currentSmoothingFactor);

            // Добавляем случайные колебания ±5%
            float randomFactor = 1f + UnityEngine.Random.Range(-0.05f, 0.05f);
            currentMotorCurrent[i] *= randomFactor;

            totalCurrent += currentMotorCurrent[i];
            Debug.Log(totalCurrent);
        }
        return totalCurrent;
    }*/

    public float getRPM(int i) 
    {
        switch (i) {
            case 0: return motor1RPM;
            case 1: return motor2RPM;
            case 2: return motor3RPM;
            case 3: return motor4RPM;
            default: return 0;
        }
    }

    public void StopMotor(int motor) 
    {
        motorStarted[motor] = 0;
        switch (motor)
        {
            case 0: motor1RPM = 0; break;
            case 1: motor2RPM = 0; break;
            case 2: motor3RPM = 0; break;
            case 3: motor4RPM = 0; break;
        }
        ApplyForces();
    }
    public void StartMotors() 
    {
        for (int i = 0; i < 4; i++)
        {
            motorStarted[i] = 1;
        }
        ApplyForces();
    }

    public void startMotor(int motor) 
    {
        motorStarted[motor] = 1;
        ApplyForces();
    }

    public void StopAllMotors()
    {
        for (int i = 0; i < 4; i++)
        {
            motorStarted[i] = 0;
        }
        motor1RPM = 0;
        motor2RPM = 0;
        motor3RPM = 0;
        motor4RPM = 0;
        ApplyForces();
    }

    public Vector3 getTotalForce() 
    {
        return totalForce;
    }

    public Vector3 getTotalTorque() 
    {
        return totalTorque;
    }
}