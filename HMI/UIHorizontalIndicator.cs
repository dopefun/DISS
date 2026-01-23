using UnityEngine;
using System.Collections;

public class UIHorizontalIndicator : MonoBehaviour {

    public float smoothFactor = 0.1f; // Коэффициент плавности
    private float currentAngle;

    // Update is called once per frame
    void Update () {
        GameObject drone = GameObject.FindGameObjectWithTag ("Player");
        if (drone != null) {
            float targetAngle = drone.transform.localEulerAngles.z;

            // Нормализуем углы, чтобы они были в диапазоне от -180 до 180
            targetAngle = NormalizeAngle(targetAngle);
            currentAngle = NormalizeAngle(currentAngle);

            // Плавное изменение угла
            currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, smoothFactor);
            transform.localEulerAngles = new Vector3(0, 0, currentAngle);
        }
    }

    // Функция для нормализации углов
    private float NormalizeAngle(float angle) {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}