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
    
    void Start()
    {
        currentCharge = initialCharge;
        //LoadPlayerPrefs();
    }

    void Update()
    {
        if (batteryEnabled)
        {
            // Обновляем заряд аккумулятора
            UpdateCharge(Time.deltaTime);
        }
        //Debug.Log(currentCharge + " " + actualVoltage / cellCount + " " + currentDraw_A + " ");
        //LoadPlayerPrefs();
    }

    public void SetCurrentDraw(float throttleAxis)
    {
        // Преобразуем ось газа из [-1, 1] в [0, 1]
        float throttle = (throttleAxis + 1f) / 2f;
        throttle = Mathf.Clamp(throttle, 0f, 1f); // Ограничиваем в пределах [0, 1]

        // Рассчитываем ток потребления в зависимости от газа
        float baseCurrent = 1.0f; // Базовый ток (например, для электроники)
        float throttleFactor = 5.0f; // Коэффициент увеличения тока при полном газе
        currentDraw_A = baseCurrent + throttleFactor * throttle;

        // Обновляем напряжение с учетом нагрузки
        UpdateVoltage();
    }

    private void UpdateVoltage()
    {
        // Рассчитываем напряжение без нагрузки
        float voltageNoLoad = CellVoltageMin * cellCount + 
                             (CellVoltageMax - CellVoltageMin) * cellCount * (currentCharge / 100f);

        // Учитываем просадку напряжения под нагрузкой
        actualVoltage = voltageNoLoad - currentDraw_A * internalResistance;
        actualVoltage = Mathf.Clamp(actualVoltage, CellVoltageMin * cellCount, CellVoltageMax * cellCount);
    }

    private void UpdateCharge(float deltaTime)
    {
        // Переводим deltaTime из секунд в часы
        float deltaTimeHours = deltaTime / 3600f;

        // Рассчитываем изменение заряда
        float deltaCharge = (currentDraw_A * deltaTimeHours * 100f) / (capacity_mAh / 1000f); // Переводим capacity_mAh в А·ч
        currentCharge -= deltaCharge;

        // Ограничиваем заряд в пределах 0-100%
        currentCharge = Mathf.Clamp(currentCharge, 0f, 100f);

        // Если заряд достиг нуля, дрон "отключается"
        if (currentCharge <= 0f)
        {
            propController = GetComponent<PropellersController>();
            propController.StopAllMotors();
        }
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
        // Устанавливаем количество ячеек
        cellCount = Mathf.Clamp(cells, 1, 6); // Ограничиваем от 1S до 6S
        UpdateVoltage(); // Обновляем напряжение
    }

    public void resetCharge()
    {
        currentCharge = initialCharge;
    }

    /*public void LoadPlayerPrefs()
    {
        bool enableBattery = PlayerPrefs.GetInt("enableBattery") > 0;
            if (enableBattery)
            {
                batteryEnabled = true;
            }
            else
            {
                batteryEnabled = false;
            }
    }*/
}
