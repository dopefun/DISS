using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Критерий 1: Энергоэффективность
/// Оценивает физическую эффективность использования энергии батареи
/// Формула: EEI = (Distance / Energy) / reference × 100%
/// </summary>
public class EnergyEfficiencyTracker : MonoBehaviour
{
    private BatteryManager battery;
    
    // Накопленные данные текущего сегмента
    private float segmentDistance = 0f;
    private Vector3 lastPosition;
    
    // История сегментов
    private List<SegmentMetrics> segments = new List<SegmentMetrics>();
    
    // Счётчик ресетов
    private int resetCount = 0;
    
    [Header("Настройки эталона")]
    [Tooltip("Эталонная эффективность: метров на 1 Вт·ч")]
    public float referenceDistancePerWh = 300f;
    
    [Header("Настройки ресетов")]
    [Tooltip("Режим обработки ресетов дрона")]
    public ResetHandlingMode resetMode = ResetHandlingMode.Development;
    
    [Tooltip("Штраф за один ресет (только для режима Penalty)")]
    [Range(0f, 0.5f)]
    public float resetPenalty = 0.1f;
    
    [Tooltip("Минимальная дистанция сегмента для учёта (м)")]
    public float minSegmentDistance = 10f;
    
    // Флаг отслеживания
    private bool isTracking = false;
    private bool segmentSaved = false;
    
    void Start()
    {
        battery = GetComponent<BatteryManager>();
        
        if (battery == null)
            Debug.LogError("[EnergyEfficiencyTracker] BatteryManager не найден!");
        lastPosition = transform.position;
    }
    
    void FixedUpdate()
    {
        if (!isTracking) return;
        float deltaDistance = Vector3.Distance(transform.position, lastPosition);
        segmentDistance += deltaDistance;
        lastPosition = transform.position;
    }
    
    // ========== ПУБЛИЧНЫЕ МЕТОДЫ ==========
    
    /// <summary>
    /// Начать отслеживание (вызывается из RaceManager)
    /// </summary>
    public void StartTracking()
    {
        isTracking = true;
        segmentSaved = false;
        segmentDistance = 0f;
        resetCount = 0;
        segments.Clear();
        lastPosition = transform.position;
        
        if (battery != null)
        {
            battery.ResetEnergyStats();
        }
    }
    
    /// <summary>
    /// Остановить отслеживание
    /// </summary>
    public void StopTracking()
    {
        if (!isTracking) return; // ← Уже остановлен
        isTracking = false;
        // Сохранить только если ещё не сохранили
        if (!segmentSaved)
        {
            SaveCurrentSegment("Финиш");
            segmentSaved = true; // ← Пометить как сохранённый
        }
    }
    
    /// <summary>
    /// Зарегистрировать ресет дрона (вызывается из ResetDrone)
    /// </summary>
    public void RegisterReset()
    {
        if (!isTracking) return;
        resetCount++; 
        // Сохранить текущий сегмент перед ресетом
        SaveCurrentSegment($"Ресет #{resetCount}");
        // Сбросить статистику для нового сегмента
        segmentDistance = 0f;
        lastPosition = transform.position;        
        if (battery != null)
        {
            battery.ResetEnergyStats();
        }
    }
    
    /// <summary>
    /// Получить итоговые метрики энергоэффективности
    /// </summary>
    public EnergyEfficiencyMetrics GetMetrics()
    {
        // Общая дистанция и энергия всех сегментов
        float totalDistance = segments.Sum(s => s.Distance);
        float totalEnergy = segments.Sum(s => s.Energy);
        
        // Средний EEI по всем сегментам
        float averageEEI = CalculateAverageEEI();
        
        // SEC (усреднённый по всем сегментам)
        float avgSEC = totalDistance > 0.001f 
            ? totalEnergy / (totalDistance / 1000f) 
            : 0f;
        
        return new EnergyEfficiencyMetrics
        {
            EEI = averageEEI,
            SEC = avgSEC,
            TotalDistance = totalDistance,
            EnergyConsumed = totalEnergy,
            ResetCount = resetCount,
            SegmentCount = segments.Count,
            Segments = new List<SegmentMetrics>(segments), // Копия для безопасности
            ResetMode = resetMode.ToString()
        };
    }
    
    /// <summary>
    /// Получить текущую статистику (для отладки)
    /// </summary>
    public string GetCurrentStats()
    {
        float energy = battery != null ? battery.GetTotalEnergyConsumed() : 0f;
        return $"Сегмент #{segments.Count + 1} | Дистанция: {segmentDistance:F1}м | Энергия: {energy:F2} Вт·ч | Ресетов: {resetCount}";
    }
    
    // ========== ВНУТРЕННИЕ МЕТОДЫ ==========
    
    /// <summary>
    /// Сохранить текущий сегмент в историю
    /// </summary>
    private void SaveCurrentSegment(string reason)
    {
        float segmentEnergy = battery != null ? battery.GetTotalEnergyConsumed() : 0f;
        
        // Игнорировать слишком короткие сегменты (краши сразу после старта)
        if (segmentDistance < minSegmentDistance)
        {
            return;
        }
        
        // Вычислить EEI сегмента
        float segmentEEI = CalculateSegmentEEI(segmentDistance, segmentEnergy);
        
        SegmentMetrics segment = new SegmentMetrics
        {
            SegmentNumber = segments.Count + 1,
            Distance = segmentDistance,
            Energy = segmentEnergy,
            EEI = segmentEEI,
            Reason = reason
        };
        
        segments.Add(segment);

    }
    
    /// <summary>
    /// Вычислить EEI для одного сегмента
    /// </summary>
    private float CalculateSegmentEEI(float distance, float energy)
    {
        if (energy < 0.01f) return 0f;
        
        float distancePerWh = distance / energy;
        float normalizedEfficiency = Mathf.Clamp01(distancePerWh / referenceDistancePerWh);
        
        return normalizedEfficiency * 100f;
    }
    
    /// <summary>
    /// Вычислить средний EEI по всем сегментам
    /// </summary>
    private float CalculateAverageEEI()
    {
        if (segments.Count == 0) return 0f;
        
        // Базовый средний EEI
        float avgEEI = segments.Average(s => s.EEI);
        
        // Применение модификаторов в зависимости от режима
        switch (resetMode)
        {
            case ResetHandlingMode.Development:
                // Без изменений
                break;
                
            case ResetHandlingMode.Penalty:
                // Штраф за количество ресетов
                float penaltyMultiplier = Mathf.Max(0f, 1f - resetCount * resetPenalty);
                avgEEI *= penaltyMultiplier;
                break;
                
            case ResetHandlingMode.Strict:
                // Если был хотя бы один ресет - провал
                if (resetCount > 0)
                {
                    avgEEI = 0f;
                }
                break;
                
            case ResetHandlingMode.ResetStats:
                // В этом режиме берём только последний сегмент
                if (segments.Count > 0)
                {
                    avgEEI = segments.Last().EEI;
                }
                break;
        }
        
        return avgEEI;
    }
}

/// <summary>
/// Режимы обработки ресетов дрона
/// </summary>
public enum ResetHandlingMode
{
    [Tooltip("Разработка: средний EEI по всем сегментам, без штрафов")]
    Development,
    
    [Tooltip("Сброс статистики: учитывается только последний сегмент")]
    ResetStats,
    
    [Tooltip("Штраф: средний EEI × (1 - N × penalty)")]
    Penalty,
    
    [Tooltip("Строгий: любой ресет = провал (EEI = 0%)")]
    Strict
}

/// <summary>
/// Метрики одного сегмента гонки
/// </summary>
[System.Serializable]
public struct SegmentMetrics
{
    public int SegmentNumber;    // Номер сегмента
    public float Distance;       // Дистанция (м)
    public float Energy;         // Энергия (Вт·ч)
    public float EEI;            // Эффективность (%)
    public string Reason;        // Причина окончания ("Ресет #3", "Финиш")
    
    public override string ToString()
    {
        return $"Сегмент #{SegmentNumber} ({Reason}): {Distance:F1}м, {Energy:F2} Вт·ч, EEI: {EEI:F1}%";
    }
}

/// <summary>
/// Структура итоговых метрик энергоэффективности
/// </summary>
[System.Serializable]
public struct EnergyEfficiencyMetrics
{
    public float EEI;              // Средний EEI (%)
    public float SEC;              // Усреднённый SEC (Вт·ч/км)
    public float TotalDistance;    // Общая дистанция всех сегментов (м)
    public float EnergyConsumed;   // Общая энергия всех сегментов (Вт·ч)
    public int ResetCount;         // Количество ресетов
    public int SegmentCount;       // Количество сегментов
    public List<SegmentMetrics> Segments; // История сегментов
    public string ResetMode;       // Режим обработки
    
    public override string ToString()
    {
        string result = $"EEI: {EEI:F1}% | Удельное потребление: {SEC:F2} Вт·ч/км | " +
                       $"Общая дистанция: {TotalDistance:F1}м | Общая энергия: {EnergyConsumed:F2} Вт·ч";
        
        if (ResetCount > 0)
        {
            result += $" | Ресетов: {ResetCount} ({ResetMode})";
        }
        
        return result;
    }
    
    /// <summary>
    /// Детальный вывод с разбивкой по сегментам
    /// </summary>
    public string ToDetailedString()
    {
        string result = ToString() + "\n\nСегменты:\n";
        
        if (Segments != null)
        {
            foreach (var segment in Segments)
            {
                result += $"  {segment}\n";
            }
        }
        
        return result;
    }
}