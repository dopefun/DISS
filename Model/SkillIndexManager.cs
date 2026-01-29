using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class SkillIndexManager : MonoBehaviour
{
    // ========== ТРЕКЕРЫ КРИТЕРИЕВ ==========
    private EnergyEfficiencyTracker energyTracker;
    private GateAccuracyTracker accuracyTracker;
    // private SmoothnessTracker smoothnessTracker; // Критерий #3 (будущее)
    // private TrajectoryTracker trajectoryTracker;  // Критерий #4 (будущее)
    
    // ========== МАППИНГ МОДЕЛЕЙ ==========
    private Dictionary<string, string> modelNames = new Dictionary<string, string>()
    {
        { "Model_0_Nazgul", "Шмель" },
        { "Model_1_Grach", "Грач" },
        { "Model_2_Snow", "ИРИС Снежинка-Б" },
        { "Model_3_Meteor", "Метеор75" },
        { "Model_4_DJI", "DJI" },
        { "Model_5_Glaz", "Глазница" },
        { "Model_6_MatriceClone", "Матрис" },
        { "Model_7_SnowAmphib", "ИРИС Амфибия" },
        { "Model_8_Mark7", "Марк 7" },
        { "Model_9_SnowWhite", "ИРИС Снежинка" }
    };
    
    // ========== МАППИНГ ЭТАЛОНОВ ПО МОДЕЛЯМ ==========
    private Dictionary<string, float> modelReferences = new Dictionary<string, float>()
    {
        { "Model_0_Nazgul", 400f },
        { "Model_1_Grach", 320f },
        { "Model_2_Snow", 480f },
        { "Model_3_Meteor", 650f },
        { "Model_4_DJI", 550f },
        { "Model_5_Glaz", 600f },
        { "Model_6_MatriceClone", 280f },
        { "Model_7_SnowAmphib", 420f },
        { "Model_8_Mark7", 380f },
        { "Model_9_SnowWhite", 560f }
    };
    
    void Start()
    {
        energyTracker = GetComponent<EnergyEfficiencyTracker>();
        accuracyTracker = GetComponent<GateAccuracyTracker>();
        
        if (energyTracker == null)
        {
            Debug.LogError("[SkillIndexManager] EnergyEfficiencyTracker не найден на дроне!");
            return;
        }

        if (accuracyTracker == null)
        {
            Debug.LogError("[SkillIndexManager] AccuracyTracker не найден на дроне!");
            return;
        }
        
        string prefabName = gameObject.name.Replace("(Clone)", "").Trim();
        if (modelReferences.ContainsKey(prefabName))
        {
            energyTracker.referenceDistancePerWh = modelReferences[prefabName];
            //Debug.Log($"[SkillIndexManager] Эталон для модели {prefabName}: {modelReferences[prefabName]} м/Вт·ч");
        }
        else
        {
            Debug.LogWarning($"[SkillIndexManager] Эталон для модели {prefabName} не найден, используется {energyTracker.referenceDistancePerWh} м/Вт·ч");
        }
    }
    
    // ========== ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ ВЫЗОВА ИЗ RACEMANAGER ==========
    
    public void StartEvaluation()
    {
        if (energyTracker != null)
        {
            energyTracker.StartTracking();
        }

        if (accuracyTracker != null)
        {
            accuracyTracker.StartTracking();
        }
    }
    
    public void StopEvaluation()
    {
        if (energyTracker != null)
        {
            energyTracker.StopTracking();
        }

        if (accuracyTracker != null)
        {
            accuracyTracker.StopTracking();
        }
    }
    
    public EnergyEfficiencyMetrics GetEnergyMetrics()
    {
        if (energyTracker != null)
        {
            return energyTracker.GetMetrics();
        }
        return new EnergyEfficiencyMetrics();
    }

    public GateAccuracyMetrics GetAccuracyMetrics()
    {
        if (accuracyTracker != null)
            return accuracyTracker.GetMetrics();
        return new GateAccuracyMetrics();
    }
    
    public void SaveResults(float lapTime, string eventType = "Race")
    {      
        EnergyEfficiencyMetrics metrics = energyTracker.GetMetrics();
        GateAccuracyMetrics accuracy = GetAccuracyMetrics();
        
        // Получить название модели
        string prefabName = gameObject.name.Replace("(Clone)", "").Trim();
        string modelName = modelNames.ContainsKey(prefabName) ? modelNames[prefabName] : prefabName;
        
        // Форматирование времени
        TimeSpan timeSpan = TimeSpan.FromSeconds(lapTime);
        string timeFormatted = string.Format("({0:00}:{1:00}.{2:000})", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        
        string logEntry = string.Format("{0} {1} | Type: {2} | Model: {3} | EEI: {4:F1}% (avg {5} seg) | SEC: {6:F2} Вт·ч/км | Dist: {7:F1}м | Energy: {8:F2} Вт·ч | Resets: {9} | Accuracy: {10:F1}% | AvgDeviation: {11:F2}м | Gates: {12}",
            timeFormatted,
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            eventType,
            modelName,
            metrics.EEI,
            metrics.SegmentCount,
            metrics.SEC,
            metrics.TotalDistance,
            metrics.EnergyConsumed,
            metrics.ResetCount,
            accuracy.AccuracyIndex,
            accuracy.AverageDistance,    
            accuracy.GateCount); 
        
        Debug.Log($"[SkillIndexManager] {metrics.ToDetailedString()}");
        Debug.Log($"[SkillIndexManager] {accuracy}");
        SaveToFile(logEntry);
    }
    
    private void SaveToFile(string logEntry)
    {
        string dataPath;
#if UNITY_EDITOR
        dataPath = Application.dataPath.Replace("Assets", "");
#else
        dataPath = Application.dataPath;
#endif
        
        string sessionDate = DateTime.Now.ToString("yyyy-MM-dd");
        string fileName = $"SkillIndex_{sessionDate}.txt";
        string fullPath = dataPath + fileName;
        
        try
        {
            File.AppendAllText(fullPath, logEntry + "\n");
            Debug.Log($"[SkillIndexManager] Сохранено в {fullPath}");
        }
        catch (Exception e)
        {
           Debug.LogError($"[SkillIndexManager] Ошибка сохранения: {e.Message}");
        }
    }
}