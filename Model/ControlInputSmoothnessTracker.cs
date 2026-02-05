using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.IO;
using System;

public class ControlInputSmoothnessTracker : MonoBehaviour
{
    [Header("Настройки")]
    public float samplingInterval = 0.02f;
    
    [Header("Пороги для геймпада")]
    public float gamepadMaxAcceptableRMS = 20.0f;
    
    [Header("Пороги для клавиатуры")]
    public float keyboardMaxAcceptableRMS = 40.0f;
    
    [Header("Фильтрация аномалий")]
    public float maxReasonableDerivative = 50f;
    
    private ControllerManager controller;
    private InputActionAsset inputActionAsset;
    private InputAction throttleAction;
    private InputAction rudderAction;
    private InputAction aileronAction;
    private InputAction elevatorAction;
    
    private List<FlightSegment> segments = new List<FlightSegment>();
    private FlightSegment currentSegment;
    private Vector4 previousInputs;
    private float sampleTimer = 0f;
    private bool isRecording = false;
    private float recordingStartTime = 0f;
    private bool isInitialized = false;
    
    private bool isUsingGamepad = false;
    private float maxAcceptableRMS;

    void Start()
    {
        controller = GetComponent<ControllerManager>();
        
        if (controller == null)
        {
            Debug.LogError("ControlInputSmoothnessTracker: ControllerManager не найден!");
            enabled = false;
            return;
        }
        
        inputActionAsset = controller.inputActionAsset;
        throttleAction = inputActionAsset.FindAction("Throttle");
        rudderAction = inputActionAsset.FindAction("Rudder");
        aileronAction = inputActionAsset.FindAction("Aileron");
        elevatorAction = inputActionAsset.FindAction("Elevator");
        
        DetectInputDevice();
    }

    void DetectInputDevice()
    {
        if (Gamepad.current != null && Gamepad.current.enabled)
        {
            isUsingGamepad = true;
            maxAcceptableRMS = gamepadMaxAcceptableRMS;
        }
        else
        {
            isUsingGamepad = false;
            maxAcceptableRMS = keyboardMaxAcceptableRMS;
        }
    }

    void FixedUpdate()
    {
        if (!isRecording || currentSegment == null) return;

        if (Time.frameCount % 50 == 0)
        {
            DetectInputDevice();
        }

        if (!isInitialized)
        {
            previousInputs = GetCurrentInputs();
            isInitialized = true;
            return;
        }

        sampleTimer += Time.fixedDeltaTime;
        
        if (sampleTimer >= samplingInterval)
        {
            Vector4 currentInputs = GetCurrentInputs();
            Vector4 derivative = (currentInputs - previousInputs) / sampleTimer;
            
            if (derivative.magnitude <= maxReasonableDerivative)
            {
                currentSegment.samples.Add(new InputDerivativeSample
                {
                    timestamp = Time.time - recordingStartTime,
                    derivative = derivative
                });
            }
            else
            {
                Debug.LogWarning($"ControlInputSmoothnessTracker: Отброшено аномальное значение {derivative.magnitude:F1} 1/с");
            }
            
            previousInputs = currentInputs;
            sampleTimer = 0f;
        }
    }

    private Vector4 GetCurrentInputs()
    {
        return new Vector4(
            throttleAction.ReadValue<float>() * (controller.invertAxisT ? -1 : 1),
            rudderAction.ReadValue<float>() * (controller.invertAxisR ? -1 : 1),
            aileronAction.ReadValue<float>() * (controller.invertAxisA ? -1 : 1),
            elevatorAction.ReadValue<float>() * (controller.invertAxisE ? -1 : 1)
        );
    }

    public void StartRecording()
    {
        isRecording = true;
        isInitialized = false;
        segments.Clear();
        currentSegment = new FlightSegment();
        previousInputs = GetCurrentInputs();
        sampleTimer = 0f;
        recordingStartTime = Time.time;
    }

    public void StopRecording()
    {
        isRecording = false;
        
        if (currentSegment != null && currentSegment.samples.Count > 0)
        {
            segments.Add(currentSegment);
        }
    }

    public void OnDroneReset()
    {
        if (!isRecording) return;
        if (currentSegment != null && currentSegment.samples.Count > 0)
        {
            segments.Add(currentSegment);
        }
        currentSegment = new FlightSegment();
        isInitialized = false;
        previousInputs = GetCurrentInputs();
        sampleTimer = 0f;
    }

    public float CalculateRMSDeviation()
    {
        int totalSamples = 0;
        float sumSquaredMagnitudes = 0f;
        
        foreach (var segment in segments)
        {
            foreach (var sample in segment.samples)
            {
                sumSquaredMagnitudes += sample.derivative.sqrMagnitude;
                totalSamples++;
            }
        }
        if (totalSamples == 0)
        {
            return 0f;
        }
        float variance = sumSquaredMagnitudes / totalSamples;
        return Mathf.Sqrt(variance);
    }

    public float GetNormalizedSmoothnessScore()
    {
        float rms = CalculateRMSDeviation();
        
        if (maxAcceptableRMS <= 0f)
        {
            return 0f;
        }
        float normalized = Mathf.Clamp01(1f - (rms / maxAcceptableRMS));
        return normalized * 100f;
    }

    public ControlSmoothnessReport GetReport()
    {
        float rms = CalculateRMSDeviation();
        
        return new ControlSmoothnessReport
        {
            rmsDeviation = rms,
            normalizedScore = GetNormalizedSmoothnessScore(),
            sampleCount = GetTotalSampleCount(),
            segmentCount = segments.Count,
            totalDuration = GetTotalSampleCount() * samplingInterval,
            peakDerivative = GetPeakDerivative(),
            inputDeviceType = isUsingGamepad ? "Gamepad" : "Keyboard"
        };
    }

    private float GetPeakDerivative()
    {
        float peak = 0f;
        
        foreach (var segment in segments)
        {
            foreach (var sample in segment.samples)
            {
                float magnitude = sample.derivative.magnitude;
                if (magnitude > peak)
                    peak = magnitude;
            }
        }
        
        return peak;
    }

    private int GetTotalSampleCount()
    {
        int total = 0;
        foreach (var segment in segments)
        {
            total += segment.samples.Count;
        }
        return total;
    }

    public void SaveDetailedLog()
    {
        if (segments.Count == 0)
        {
            Debug.LogWarning("ControlInputSmoothnessTracker: Нет данных для сохранения");
            return;
        }

        string path = GetLogFilePath();
        
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("Segment,Time(s),dT/dt(1/s),dR/dt(1/s),dA/dt(1/s),dE/dt(1/s),Magnitude(1/s)");
            
            for (int i = 0; i < segments.Count; i++)
            {
                foreach (var sample in segments[i].samples)
                {
                    writer.WriteLine($"{i + 1}," +
                                   $"{sample.timestamp:F3}," +
                                   $"{sample.derivative.x:F4}," +
                                   $"{sample.derivative.y:F4}," +
                                   $"{sample.derivative.z:F4}," +
                                   $"{sample.derivative.w:F4}," +
                                   $"{sample.derivative.magnitude:F4}");
                }
            }
        }
    }

    private string GetLogFilePath()
    {
        string dataPath;
#if UNITY_EDITOR
        dataPath = Application.dataPath.Replace("Assets", "");
#else
        dataPath = Application.dataPath;
#endif
        
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        return Path.Combine(dataPath, $"ControlSmoothnessLog_{timestamp}.csv");
    }
}

[System.Serializable]
public class FlightSegment
{
    public List<InputDerivativeSample> samples = new List<InputDerivativeSample>();
}

[System.Serializable]
public struct InputDerivativeSample
{
    public float timestamp;
    public Vector4 derivative;
}

[System.Serializable]
public struct ControlSmoothnessReport
{
    public float rmsDeviation;
    public float normalizedScore;
    public int sampleCount;
    public int segmentCount;
    public float totalDuration;
    public float peakDerivative;
    public string inputDeviceType; // НОВОЕ
    
    public override string ToString()
    {
        return $"=== ОТЧЁТ О ПЛАВНОСТИ УПРАВЛЕНИЯ ===\n" +
               $"Устройство ввода: {inputDeviceType}\n" +
               $"RMS производной команд: {rmsDeviation:F3} 1/с\n" +
               $"Нормализованная оценка: {normalizedScore:F1}%\n" +
               $"Пиковая производная: {peakDerivative:F2} 1/с\n" +
               $"Сегментов: {segmentCount}\n" +
               $"Замеров: {sampleCount} ({totalDuration:F1} сек)";
    }
}