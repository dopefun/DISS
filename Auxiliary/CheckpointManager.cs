using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckpointManager : MonoBehaviour
{

    public List<GameObject> checkpoints; // список контрольных точек
    public int currentCheckpointIndex; // индекс текущей контрольной точки
    public float timeLeft; // оставшееся время для прохождения полосы препятствий
    public Text timeText; // текстовый объект для отображения времени
    public bool _isActive = true;
    public bool _finish = false;
    public GameObject player; // объект игрока
    public TutorialManager tutorialManager;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player"); // получение ссылки на объект игрока
        currentCheckpointIndex = 0;
        timeLeft = 200f; // 60 секунд на прохождение полосы препятствий
        timeText.text = "Время: " + timeLeft.ToString("0"); // отображение начального времени

        for (int element = 0; element < checkpoints.Count - 1; element++)
        {
            Vector3 direction = checkpoints[element + 1].transform.position - checkpoints[element].transform.position;
                    Quaternion rotation = Quaternion.LookRotation(direction);
                    rotation *= Quaternion.Euler(-90f, 0f, 0f);
                    checkpoints[element].transform.rotation = rotation;
        }

    }

    void Update()
    {
        if (_isActive == true)
        {
            timeLeft -= Time.deltaTime; // отсчет времени
            timeText.text = "Время: " + timeLeft.ToString("0"); // отображение оставшегося времени

            if (timeLeft <= 0f)
            {
                // время вышло, игрок проиграл
                Debug.Log("Time's up!");
            }

            if (currentCheckpointIndex < checkpoints.Count)
            {
                    // проверка прохождения текущей контрольной точки
                if (checkpoints[currentCheckpointIndex].GetComponent<BoxCollider>() != null && checkpoints[currentCheckpointIndex].GetComponent<BoxCollider>().bounds.Contains(player.transform.position)
    || checkpoints[currentCheckpointIndex].GetComponent<SphereCollider>() != null && checkpoints[currentCheckpointIndex].GetComponent<SphereCollider>().bounds.Contains(player.transform.position))
                {
                    Destroy(checkpoints[currentCheckpointIndex]);
                    if (currentCheckpointIndex == 1)
                    {
                        tutorialManager.ShowNextInstruction();
                    }
                    if (currentCheckpointIndex == 5)
                    {
                        tutorialManager.ShowNextInstruction();
                    }
                    if (currentCheckpointIndex == 11)
                    {
                        tutorialManager.ShowNextInstruction();
                    }
                    currentCheckpointIndex++;
                    Debug.Log("Checkpoint " + currentCheckpointIndex + " reached!");
                    checkpoints[currentCheckpointIndex+1].SetActive(true); 
                }
            }
            else
            {
                // все контрольные точки пройдены, игрок выиграл
                Debug.Log("You win!");
                _isActive = false;
                _finish = true;
            }
        }
    }
}
