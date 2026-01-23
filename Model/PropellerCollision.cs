using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerCollision : MonoBehaviour
{
    public int motorid;
    // Ссылка на скрипт с функцией StopMotor()
    public PropellersController motorController; // Замените на ваш класс


    // Или, если используете триггер:
    private void OnTriggerEnter(Collider other)
    {
        if (motorController != null && other.tag != "Player")
        {
            motorController.StopMotor(motorid);
        }
    }
}
