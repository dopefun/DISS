using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Критерий 3: Точность прохождения ворот
/// Измеряет среднее отклонение от центра ворот
/// </summary>
public class GateAccuracyTracker : MonoBehaviour
{
    // История прохождений ворот
    private List<GatePassData> gatePasses = new List<GatePassData>();
    
    [Header("Настройки эталона")]
    [Tooltip("Эталонное расстояние (идеальное прохождение), метры")]
    public float referenceDistance = 0.2f; // 0.2f 0.3f 0.5f
    
    [Tooltip("Максимальное допустимое отклонение, метры")]
    public float maxDistance = 0.56f; // 0.56f 0.8f 1.2f
    
    private bool isTracking = false;
    
    // ========== ПУБЛИЧНЫЕ МЕТОДЫ ==========
    
    public void StartTracking()
    {
        isTracking = true;
        gatePasses.Clear();
    }
    
    public void StopTracking()
    {
        isTracking = false;
    }
    
    /// <summary>
    /// Зарегистрировать прохождение ворот (вызывается из RaceGate)
    /// </summary>
    public void RegisterGatePass(int gateNumber, float distanceFromCenter)
    {
        //Debug.Log($"[GateAccuracyTracker] RegisterGatePass called: gate={gateNumber}, distance={distanceFromCenter:F2}, isTracking={isTracking}");
        if (!isTracking) 
        {
            return;
        }
        GatePassData pass = new GatePassData
        {
            GateNumber = gateNumber,
            DistanceFromCenter = distanceFromCenter,
            AccuracyScore = CalculateAccuracyScore(distanceFromCenter)
        };
        
        gatePasses.Add(pass);
    }
    
    /// <summary>
    /// Получить итоговые метрики точности
    /// </summary>
    public GateAccuracyMetrics GetMetrics()
    {
        if (gatePasses.Count == 0)
        {
            return new GateAccuracyMetrics
            {
                AccuracyIndex = 0f,
                AverageDistance = 0f,
                GateCount = 0,
                GatePasses = new List<GatePassData>()
            };
        }
        
        float avgDistance = gatePasses.Average(g => g.DistanceFromCenter);
        float avgScore = gatePasses.Average(g => g.AccuracyScore);
        
        return new GateAccuracyMetrics
        {
            AccuracyIndex = avgScore * 100f, // В процентах
            AverageDistance = avgDistance,
            GateCount = gatePasses.Count,
            GatePasses = new List<GatePassData>(gatePasses)
        };
    }
    
    // ========== ВНУТРЕННИЕ МЕТОДЫ ==========
    
    private float CalculateAccuracyScore(float distance)
    {
        // Нормализация: 0м = 100%, maxDistance = 0%
        float normalized = 1f - Mathf.Clamp01(distance / maxDistance);
        return normalized;
    }
        public void RegisterReset()
    {
        //Debug.Log("[GateAccuracyTracker] Reset registered, but data NOT cleared (accumulating)");
    }
}

/// <summary>
/// Данные одного прохождения ворот
/// </summary>
[System.Serializable]
public struct GatePassData
{
    public int GateNumber;
    public float DistanceFromCenter; // метры
    public float AccuracyScore; // 0..1
    
    public override string ToString()
    {
        return $"Ворота #{GateNumber}: {DistanceFromCenter:F2}м (точность: {AccuracyScore * 100:F1}%)";
    }
}

/// <summary>
/// Итоговые метрики точности
/// </summary>
[System.Serializable]
public struct GateAccuracyMetrics
{
    public float AccuracyIndex; // Средняя точность (%)
    public float AverageDistance; // Среднее отклонение (м)
    public int GateCount;
    public List<GatePassData> GatePasses;
    
    public override string ToString()
    {
        return $"Точность: {AccuracyIndex:F1}% | Среднее отклонение: {AverageDistance:F2}м | Ворот: {GateCount}";
    }
}