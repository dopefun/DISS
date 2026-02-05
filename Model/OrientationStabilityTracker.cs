using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Критерий 4: Стабильность ориентации
/// Измеряет дисперсию угловых скоростей дрона относительно среднего значения
/// </summary>
public class OrientationStabilityTracker : MonoBehaviour
{
    private Rigidbody rb;
    
    // История угловых скоростей
    private List<Vector3> angularVelocitySamples = new List<Vector3>();
    
    [Header("Настройки эталона")]
    [Tooltip("Эталонная дисперсия (идеальная стабильность), рад²/с²")]
    public float referenceDispersion = 0.05f; // Для опытных пилотов
    
    [Tooltip("Максимальная допустимая дисперсия, рад²/с²")]
    public float maxDispersion = 2.0f; // Для новичков
    
    [Header("Настройки сбора данных")]
    [Tooltip("Частота дискретизации (Hz)")]
    public float samplingRate = 50f; // 50 Hz (каждые 0.02с в FixedUpdate)
    
    private bool isTracking = false;
    private float sampleTimer = 0f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (rb == null)
        {
            Debug.LogError("[OrientationStabilityTracker] Rigidbody не найден!");
        }
    }
    
    void FixedUpdate()
    {
        if (!isTracking) return;
        
        // Дискретизация с заданной частотой
        sampleTimer += Time.fixedDeltaTime;
        float sampleInterval = 1f / samplingRate;
        
        if (sampleTimer >= sampleInterval)
        {
            // Получить текущие угловые скорости из гироскопа
            Vector3 omega = rb.angularVelocity; // рад/с
            angularVelocitySamples.Add(omega);
            
            sampleTimer = 0f;
        }
    }
    
    // ========== ПУБЛИЧНЫЕ МЕТОДЫ ==========
    
    public void StartTracking()
    {
        isTracking = true;
        angularVelocitySamples.Clear();
        sampleTimer = 0f;
    }
    
    public void StopTracking()
    {
        isTracking = false;
    }
    
    /// <summary>
    /// Получить итоговые метрики стабильности ориентации
    /// </summary>
    public OrientationStabilityMetrics GetMetrics()
    {
        if (angularVelocitySamples.Count < 2)
        {
            return new OrientationStabilityMetrics
            {
                StabilityIndex = 0f,
                DispersionX = 0f,
                DispersionY = 0f,
                DispersionZ = 0f,
                TotalDispersion = 0f,
                SampleCount = 0
            };
        }
        
        // Вычислить средний вектор угловых скоростей
        Vector3 omegaMean = CalculateMeanVector(angularVelocitySamples);
        
        // Вычислить дисперсию по каждой оси
        float Dx = CalculateDispersion(angularVelocitySamples, omegaMean, 0); // roll
        float Dy = CalculateDispersion(angularVelocitySamples, omegaMean, 1); // pitch
        float Dz = CalculateDispersion(angularVelocitySamples, omegaMean, 2); // yaw
        
        // Общая дисперсия (среднеквадратичное)
        float Dtotal = Mathf.Sqrt((Dx + Dy + Dz) / 3f);
        
        // Индекс стабильности (нормализация)
        float stabilityIndex = CalculateStabilityIndex(Dtotal);
        
        return new OrientationStabilityMetrics
        {
            StabilityIndex = stabilityIndex,
            DispersionX = Dx,
            DispersionY = Dy,
            DispersionZ = Dz,
            TotalDispersion = Dtotal,
            SampleCount = angularVelocitySamples.Count
        };
    }
    
    public void RegisterReset()
    {
        // При ресете продолжаем накапливать данные (Development mode)
    }
    
    // ========== ВНУТРЕННИЕ МЕТОДЫ ==========
    
    /// <summary>
    /// Вычислить средний вектор
    /// </summary>
    private Vector3 CalculateMeanVector(List<Vector3> samples)
    {
        Vector3 sum = Vector3.zero;
        foreach (var sample in samples)
        {
            sum += sample;
        }
        return sum / samples.Count;
    }
    
    /// <summary>
    /// Вычислить дисперсию по одной оси
    /// D_ω = (1/T) ∫(ω(t) - ω̄)² dt
    /// </summary>
    private float CalculateDispersion(List<Vector3> samples, Vector3 mean, int axis)
    {
        float sumSquaredDeviation = 0f;
        
        foreach (var sample in samples)
        {
            float deviation = sample[axis] - mean[axis];
            sumSquaredDeviation += deviation * deviation;
        }
        
        return sumSquaredDeviation / samples.Count;
    }
    
    /// <summary>
    /// Нормализовать дисперсию в индекс стабильности (0-100%)
    /// </summary>
    private float CalculateStabilityIndex(float dispersion)
    {
        // Обратная нормализация: меньше дисперсия = выше индекс
        // SI = (1 - D/D_max) × 100%
        float normalized = 1f - Mathf.Clamp01(dispersion / maxDispersion);
        return normalized * 100f;
    }
}

/// <summary>
/// Итоговые метрики стабильности ориентации
/// </summary>
[System.Serializable]
public struct OrientationStabilityMetrics
{
    public float StabilityIndex;      // Индекс стабильности (%)
    public float DispersionX;         // Дисперсия по оси X (roll), рад²/с²
    public float DispersionY;         // Дисперсия по оси Y (pitch), рад²/с²
    public float DispersionZ;         // Дисперсия по оси Z (yaw), рад²/с²
    public float TotalDispersion;     // Общая дисперсия √((Dx+Dy+Dz)/3)
    public int SampleCount;           // Количество замеров
    
    public override string ToString()
    {
        return $"Стабильность: {StabilityIndex:F1}% | " +
               $"Дисперсия: {TotalDispersion:F4} рад²/с² | " +
               $"Roll: {DispersionX:F4}, Pitch: {DispersionY:F4}, Yaw: {DispersionZ:F4} | " +
               $"Замеров: {SampleCount}";
    }
}