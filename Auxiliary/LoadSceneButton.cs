using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadSceneButton : MonoBehaviour
{
    // Название сцены, которую нужно загрузить
    public string sceneName;

    private void Start()
    {
        // Получаем компонент Button на этом GameObject
        Button button = GetComponent<Button>();
        if (button != null)
        {
            // Добавляем слушатель события для нажатия на кнопку
            button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("Компонент Button не найден на GameObject.");
        }
    }

    // Метод, который будет вызван при нажатии на кнопку
    private void OnButtonClick()
    {
        // Загружаем указанную сцену
        SceneManager.LoadScene(sceneName);
    }
}
