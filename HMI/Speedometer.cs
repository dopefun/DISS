using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Speedometer : MonoBehaviour
{
    public Rigidbody targetRigidbody; // Объект, скорость которого будет отображаться
    public Text speedText; // Текстовый объект, в котором будет отображаться скорость
    public float speedMultiplier = 1f; // Множитель для скорости (если нужно)

    void Start()
    {
        GameObject target = GameObject.FindGameObjectWithTag("Player");
        targetRigidbody = target.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (targetRigidbody == null)
        {
            Debug.LogError("No target Rigidbody set!");
            return;
        }

        if (speedText == null)
        {
            Debug.LogError("No speed Text set!");
            return;
        }

        // Получаем скорость объекта
        float speed = targetRigidbody.velocity.magnitude * speedMultiplier;

        speed = Mathf.RoundToInt(speed);

        // Отображаем скорость в текстовом объекте
        speedText.text = speed.ToString() + " км/ч";
    }
}
