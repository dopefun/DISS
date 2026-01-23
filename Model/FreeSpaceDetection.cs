using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class FreeSpaceDetection : MonoBehaviour {

    public int numHorizontalPoints;
    public int numBins;
    public float fov_degrees = 0;
    public float maxDist = 100;

    public bool displayFreespace = true; // Включить визуализацию свободного пространства

    private NativeArray<RaycastCommand> commands;
    private LineRenderer[] lineRenderers;

    void Start() {
        if (Mathf.Abs(fov_degrees) < 1e-8)
            fov_degrees = Camera.main.fieldOfView;
        
        if (numBins > numHorizontalPoints) {
            throw new UnityException("More bins than points!");
        }
        if (numHorizontalPoints % numBins != 0) {
            throw new UnityException("numHorizontalPoints must be a multiple of numBins!");
        }

        commands = new NativeArray<RaycastCommand>(numHorizontalPoints, Allocator.Temp);
        
        // Создаем LineRenderer для каждой точки
        lineRenderers = new LineRenderer[numHorizontalPoints];
        for (int i = 0; i < numHorizontalPoints; i++) {
            GameObject lineObj = new GameObject($"Ray_{i}");
            lineObj.transform.parent = transform;
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.green;
            lr.endColor = Color.green;
            lineRenderers[i] = lr;
        }
    }
    
    public float[] batchRaycast() {
        var results = new NativeArray<RaycastHit>(numHorizontalPoints, Allocator.Temp);

        Vector3 origin = transform.localPosition;

        // Линейное распределение углов
        for (int i = 0; i < numHorizontalPoints; i++) {
            float theta = -(fov_degrees / 2.0f) + i * fov_degrees / numHorizontalPoints;
            var angle_rot = Quaternion.AngleAxis(theta, Vector3.up);
            var direction = transform.rotation * angle_rot * Vector3.forward;
            commands[i] = new RaycastCommand(origin, direction, maxDist);
        }

        var handle = RaycastCommand.ScheduleBatch(commands, results, 1);
        handle.Complete();

        // Визуализация результатов рейкастов
        bool freeSpaceFound = false;

        if (displayFreespace) {
            for (int i = 0; i < numHorizontalPoints; i++) {
                if (results[i].collider != null) {
                    lineRenderers[i].SetPosition(0, origin);
                    lineRenderers[i].SetPosition(1, results[i].point);
                    lineRenderers[i].startColor = Color.red;
                    lineRenderers[i].endColor = Color.red;
                } else {
                    lineRenderers[i].SetPosition(0, origin);
                    lineRenderers[i].SetPosition(1, origin + commands[i].direction * maxDist);
                    lineRenderers[i].startColor = Color.green;
                    lineRenderers[i].endColor = Color.green;
                    freeSpaceFound = true; // Найдено свободное пространство
                }
            }
        }

        if (!freeSpaceFound) {
            Debug.Log("Нет свободного пространства перед объектом!");
        }

        // Суммирование данных по "корзинам"
        float[] output = new float[numBins];
        for (int i = 0; i < numBins; i++) {
            output[i] = 0.0f;
        }

        int elsPerBin = ((int)results.Length / numBins);
        for (int i = 0; i < results.Length; i++) {
            int bin = i / elsPerBin;
            output[bin] += results[i].distance / elsPerBin;
        }

        float totalSum = 0.0f;
        for (int i = 0; i < output.Length; i++) {
            totalSum += output[i];
        }

        float[] norm_output = new float[numBins + 1];
        for (int i = 0; i < output.Length; i++) {
            norm_output[i] = output[i] / totalSum;
        }
        norm_output[numBins] = totalSum;

        results.Dispose();
        return norm_output;
    }

    void Update() {
        // Выполняем рейкасты каждое обновление и выводим результаты
        batchRaycast();
    }

    private void OnDestroy() {
        commands.Dispose();
    }
}
