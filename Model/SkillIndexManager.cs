using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class SkillIndexManager : MonoBehaviour
{
    // ========== ТРЕКЕРЫ КРИТЕРИЕВ ==========
    private EnergyEfficiencyTracker energyTracker;
    private GateAccuracyTracker accuracyTracker;
    private ControlInputSmoothnessTracker controlSmoothnessTracker;
    private OrientationStabilityTracker orientationTracker;
    
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
    
    // ========== МАППИНГ ЭТАЛОНОВ ЭНЕРГОЭФФЕКТИВНОСТИ ПО МОДЕЛЯМ ==========
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
        controlSmoothnessTracker = GetComponent<ControlInputSmoothnessTracker>();
        orientationTracker = GetComponent<OrientationStabilityTracker>();
        
        if (energyTracker == null)
        {
            Debug.LogError("[SkillIndexManager] EnergyEfficiencyTracker не найден на дроне!");
        }

        if (accuracyTracker == null)
        {
            Debug.LogError("[SkillIndexManager] GateAccuracyTracker не найден на дроне!");
        }

        if (controlSmoothnessTracker == null)
        {
            Debug.LogError("[SkillIndexManager] ControlInputSmoothnessTracker не найден на дроне!");
        }

        if (orientationTracker == null)
        {
            Debug.LogError("[SkillIndexManager] OrientationStabilityTracker не найден на дроне!");
        }
        
        string prefabName = gameObject.name.Replace("(Clone)", "").Trim();
        if (modelReferences.ContainsKey(prefabName))
        {
            energyTracker.referenceDistancePerWh = modelReferences[prefabName];
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

        if (controlSmoothnessTracker != null)
        {
            controlSmoothnessTracker.StartRecording();
        }

        if (orientationTracker != null)
        {
            orientationTracker.StartTracking();
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

        if (controlSmoothnessTracker != null)
        {
            controlSmoothnessTracker.StopRecording();
        }

        if (orientationTracker != null)
        {
            orientationTracker.StopTracking();
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

    public ControlSmoothnessReport GetControlSmoothnessMetrics()
    {
        if (controlSmoothnessTracker != null)
            return controlSmoothnessTracker.GetReport();
        return new ControlSmoothnessReport();
    }

    public OrientationStabilityMetrics GetOrientationMetrics()
    {
        if (orientationTracker != null)
            return orientationTracker.GetMetrics();
        return new OrientationStabilityMetrics();
    }
    
    public void SaveResults(float lapTime, string eventType = "Гонка")
    {      
        EnergyEfficiencyMetrics energy = GetEnergyMetrics();
        GateAccuracyMetrics accuracy = GetAccuracyMetrics();
        ControlSmoothnessReport controlSmoothness = GetControlSmoothnessMetrics();
        OrientationStabilityMetrics orientation = GetOrientationMetrics();
        
        // Получить название модели
        string prefabName = gameObject.name.Replace("(Clone)", "").Trim();
        string modelName = modelNames.ContainsKey(prefabName) ? modelNames[prefabName] : prefabName;
        
        // Форматирование времени
        TimeSpan timeSpan = TimeSpan.FromSeconds(lapTime);
        string timeFormatted = string.Format("({0:00}:{1:00}.{2:000})", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        
        string separator = "========================================";
        string logEntry = string.Format(
            "{0}\n" +
            "Время финиша: {1} | Длительность: {2}\n" +
            "Тип события: {3} | Модель: {4}\n" +
            "{0}\n" +
            "КРИТЕРИЙ 1: ЭНЕРГОЭФФЕКТИВНОСТЬ\n" +
            "  Энергоэффективность: {5:F1}% | УЭП: {6:F2} Вт·ч/км\n" +
            "  Дистанция: {7:F1}м | Энергия: {8:F2} Вт·ч\n" +
            "  Сегментов: {9} | Ресетов: {10}\n" +
            "{0}\n" +
            "КРИТЕРИЙ 2: ТОЧНОСТЬ ПРОХОЖДЕНИЯ ВОРОТ\n" +
            "  Точность: {11:F1}% | Среднее отклонение: {12:F2}м\n" +
            "  Пройдено ворот: {13}\n" +
            "{0}\n" +
            "КРИТЕРИЙ 3: ПЛАВНОСТЬ УПРАВЛЕНИЯ\n" +
            "  Плавность: {14:F1}% | СКО производной: {15:F3} 1/с\n" +
            "  Пиковое значение: {16:F2} 1/с\n" +
            "{0}\n" +
            "КРИТЕРИЙ 4: СТАБИЛЬНОСТЬ ОРИЕНТАЦИИ\n" +
            "  Стабильность: {17:F1}% | Дисперсия: {18:F4} рад²/с²\n" +
            "  Замеров угловых скоростей: {19}\n" +
            "{0}\n",
            separator,
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            timeFormatted,
            eventType,
            modelName,
            energy.EEI,
            energy.SEC,
            energy.TotalDistance,
            energy.EnergyConsumed,
            energy.SegmentCount,
            energy.ResetCount,
            accuracy.AccuracyIndex,
            accuracy.AverageDistance,
            accuracy.GateCount,
            controlSmoothness.normalizedScore,
            controlSmoothness.rmsDeviation,
            controlSmoothness.peakDerivative,
            orientation.StabilityIndex,
            orientation.TotalDispersion,
            orientation.SampleCount);
        
        Debug.Log($"[SkillIndexManager] {energy.ToDetailedString()}");
        Debug.Log($"[SkillIndexManager] {accuracy}");
        Debug.Log($"[SkillIndexManager] {controlSmoothness}");
        Debug.Log($"[SkillIndexManager] {orientation}");
        
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