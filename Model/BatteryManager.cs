using UnityEngine;

public class BatteryManager : MonoBehaviour
{
    // Параметры аккумулятора
    public PropellersController propController;
    public float capacity_mAh = 1500f; // Емкость в мА·ч
    public int cellCount = 4; // Количество ячеек (например, 4S, 5S, 6S)
    public float internalResistance = 0.05f; // Внутреннее сопротивление в омах
    public float initialCharge = 100f; // Начальный заряд в процентах

    private float currentCharge; // Текущий заряд в процентах
    private float currentVoltage; // Текущее напряжение
    private float currentDraw_A; // Текущий потребляемый ток в амперах
    private float actualVoltage; // Фактическое напряжение с учетом просадки

    // Напряжение одной ячейки
    private const float CellVoltageMax = 4.2f; // Максимальное напряжение ячейки
    private const float CellVoltageMin = 3.0f; // Минимальное напряжение ячейки
    public PropellersController propellersController;
    bool batteryEnabled = true;

    private const float k_motor = 0.00002f; // Подбирается под конкретный мотор
    private float[] motorCurrents = new float[4];

    private float totalEnergyConsumed_Wh = 0f; // Накопление энергии
    
    void Start()
    {
        currentCharge = initialCharge;
        currentDraw_A = 1.0f; // Начальный ток авионики
        UpdateVoltage();
        totalEnergyConsumed_Wh = 0f;
    }

    public void SetCurrentFromMotors(float motorCurrent)
    {
        // Общий ток = моторы + авионика
        float avinonicsCurrent = 1.0f; // FC (0.5A) + приёмник (0.3A) + телеметрия (0.2A)
        currentDraw_A = motorCurrent + avinonicsCurrent;
        currentDraw_A = Mathf.Clamp(currentDraw_A, 0f, 60f);       
        UpdateVoltage();
    }

    void Update()
    {
        if (batteryEnabled)
        {
            UpdateCharge(Time.deltaTime);
        }
    }

    private void UpdateVoltage()
    {
        float voltageNoLoad = CellVoltageMin * cellCount + (CellVoltageMax - CellVoltageMin) * cellCount * (currentCharge / 100f);
        actualVoltage = voltageNoLoad - currentDraw_A * internalResistance;
        actualVoltage = Mathf.Clamp(actualVoltage, CellVoltageMin * cellCount, CellVoltageMax * cellCount);
    }

    private void UpdateCharge(float deltaTime)
    {
        float deltaTimeHours = deltaTime / 3600f;

        float deltaEnergy_Wh = actualVoltage * currentDraw_A * deltaTimeHours;
        totalEnergyConsumed_Wh += deltaEnergy_Wh;

        float deltaCharge = (currentDraw_A * deltaTimeHours * 100f) / (capacity_mAh / 1000f);
        currentCharge -= deltaCharge;
        currentCharge = Mathf.Clamp(currentCharge, 0f, 100f);

        if (currentCharge <= 0f)
        {
            propController = GetComponent<PropellersController>();
            propController.StopAllMotors();
        }
    }

    public float GetTotalEnergyConsumed()
    {
        return totalEnergyConsumed_Wh;
    }

    public void ResetEnergyStats()
    {
        totalEnergyConsumed_Wh = 0f;
    }

    public float GetCurrentCharge()
    {
        return currentCharge;
    }

    public float GetActualVoltage()
    {
        return actualVoltage;
    }

    public float GetActualVoltagePerCell()
    {
        return actualVoltage / cellCount;
    }

    public float GetCurrentDraw()
    {
        return currentDraw_A;
    }

    public float GetMaxVoltage()
    {
        return CellVoltageMax * cellCount;
    }

    public void SetCellCount(int cells)
    {
        cellCount = Mathf.Clamp(cells, 1, 6);
        UpdateVoltage();
    }

    public void resetCharge()
    {
        currentCharge = initialCharge;
    }
}