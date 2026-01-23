using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Destroy : MonoBehaviour
{
    public GameObject explosionPrefab;
    public float damageThreshold = 100f;

    private float currentDamage = 0f;

    private bool exploded = false;
    [SerializeField] private float explodeArea = 100f;

    private InputActionAsset inputActionAsset;
    private InputAction explosionAction;

    private bool check = true;

    public Transform parentTransform;

    void Awake()
    {
        inputActionAsset = Resources.Load<InputActionAsset>("InputActions");
        
        explosionAction = inputActionAsset.FindAction("Explosion");

        explosionAction.performed += OnExplosionPerformed;
        //explosionAction.performed -= OnExplosionPerformed;
    }

    public void OnKamikazeHit() //Попадание дрона по цели
    {
        Debug.Log("OnKamikazeHit");
        if (gameObject.GetComponent<SphereCollider>() == null)
        {
            var explodedArea = gameObject.AddComponent<SphereCollider>();
            explodedArea.radius = explodeArea;
        }
        //RestartMenuPopUp();
        StartCoroutine(RestartLevel());
        StartCoroutine(ExplodeDrone());
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Enemy" && check)
        {
            OnKamikazeHit();
            check = false;
            //Destroy(col.gameObject);
        }
    }

    void OnHitByMissile() //Попадание ракеты
    {
        currentDamage += damageThreshold;
        if (currentDamage >= damageThreshold)
        {
            Instantiate(explosionPrefab, transform.position, transform.rotation);
            StartCoroutine(RestartLevel());
            //Destroy(gameObject);
        }
    }

    
    void CameraInsta(Transform trans)  // Добавление камеры после уничтожения дрона
    {
        GameObject cameraObject = new GameObject("MyCustomCamera");
        cameraObject.transform.position = trans.position;

        // Добавляем компонент Camera к созданному объекту
        Camera cameraComponent = cameraObject.AddComponent<Camera>();
        //AudioListener audioListener = cameraObject.AddComponent<AudioListener>();

        // Устанавливаем основные параметры камеры
        cameraComponent.clearFlags = CameraClearFlags.Skybox;  // Способ очистки фона
        cameraComponent.backgroundColor = Color.cyan;          // Цвет фона камеры
        cameraComponent.orthographic = false;                  // Проекционный режим (перспектива)
        cameraComponent.fieldOfView = 60f;                     // Угол обзора (для перспективной камеры)
        cameraComponent.nearClipPlane = 0.3f;                  // Ближняя граница обзора
        cameraComponent.farClipPlane = 1000f;  
        cameraComponent.tag = "MainCamera";                // Дальняя граница обзора

        // Устанавливаем позицию и вращение камеры
        cameraObject.transform.position = transform.position;  // Позиция камеры
        //cameraObject.transform.LookAt(Vector3.zero);               // Поворот к центру сцены
        FlareLayer flareLayer =  cameraObject.AddComponent<FlareLayer>();
        AudioListener audioListener = cameraObject.AddComponent<AudioListener>();
    }

    IEnumerator RestartLevel() //Перезапуск после попадания
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator ExplodeDrone() // Функция взрыва
    {
        Instantiate(explosionPrefab, transform.position, transform.rotation);
        CameraInsta(transform);
        //StartCoroutine(RestartLevel());
        yield return new WaitForSeconds(0.01f);
        Time.timeScale = 0f;
        
        //return null;
    }

    void OnExplosionPerformed(InputAction.CallbackContext context) // Взрыв по кнопке
    {
        OnKamikazeHit();
        check = false;
    }

    public void RestartMenuPopUp()
    {
         Debug.Log("SomeMethod called");
        
        // Создаем Canvas
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        //PlayerInput pi = canvasObject.AddComponent<PlayerInput>();

        // Настраиваем компонент PlayerInput по вашему усмотрению
        //pi.actions = Resources.Load<InputActionAsset>("InputActions"); // Пример загрузки InputActionAsset из Resources
       // pi.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
        //pi.camera = Camera.main; // Пример назначения основной камеры

        // Устанавливаем родителя для Canvas
        canvasObject.transform.SetParent(parentTransform, false);

        // Создаем текст
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(canvasObject.transform, false);
        Text text = textObject.AddComponent<Text>();
        text.text = "Поражение цели";
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 20;
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleCenter;
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchoredPosition = new Vector2(0, 50);
        textRect.sizeDelta = new Vector2(400, 100);

        // Создаем кнопку
        GameObject buttonObject = new GameObject("Button");
        buttonObject.transform.SetParent(canvasObject.transform, false);

        // Добавляем компонент Image для фона кнопки
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.8f, 0.8f, 0.8f, 1.0f); // Устанавливаем цвет фона кнопки

        // Добавляем компонент Button
        Button button = buttonObject.AddComponent<Button>();

        // Создаем текст для кнопки
        GameObject buttonTextObject = new GameObject("ButtonText");
        buttonTextObject.transform.SetParent(buttonObject.transform, false);
        Text buttonText = buttonTextObject.AddComponent<Text>();
        buttonText.text = "Перезапустить";
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 24;
        buttonText.color = Color.black;
        buttonText.alignment = TextAnchor.MiddleCenter;
        RectTransform buttonTextRect = buttonTextObject.GetComponent<RectTransform>();
        buttonTextRect.sizeDelta = new Vector2(200, 50);
        buttonTextRect.anchoredPosition = Vector2.zero;

        // Устанавливаем размеры кнопки
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(200, 50);
        buttonRect.anchoredPosition = new Vector2(0, -50);

        // Добавляем OnClick событие для кнопки
        button.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });

        // Создаем кнопку выхода в главное меню
        GameObject mainMenuButtonObject = new GameObject("MainMenuButton");
        mainMenuButtonObject.transform.SetParent(canvasObject.transform, false);

        // Добавляем компонент Image для фона кнопки выхода в главное меню
        Image mainMenuButtonImage = mainMenuButtonObject.AddComponent<Image>();
        mainMenuButtonImage.color = new Color(0.8f, 0.8f, 0.8f, 1.0f); // Устанавливаем цвет фона кнопки

        // Добавляем компонент Button
        Button mainMenuButton = mainMenuButtonObject.AddComponent<Button>();

        // Создаем текст для кнопки выхода в главное меню
        GameObject mainMenuButtonTextObject = new GameObject("MainMenuButtonText");
        mainMenuButtonTextObject.transform.SetParent(mainMenuButtonObject.transform, false);
        Text mainMenuButtonText = mainMenuButtonTextObject.AddComponent<Text>();
        mainMenuButtonText.text = "Главное меню";
        mainMenuButtonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        mainMenuButtonText.fontSize = 24;
        mainMenuButtonText.color = Color.black;
        mainMenuButtonText.alignment = TextAnchor.MiddleCenter;
        RectTransform mainMenuButtonTextRect = mainMenuButtonTextObject.GetComponent<RectTransform>();
        mainMenuButtonTextRect.sizeDelta = new Vector2(200, 50);
        mainMenuButtonTextRect.anchoredPosition = Vector2.zero;

        // Устанавливаем размеры кнопки выхода в главное меню
        RectTransform mainMenuButtonRect = mainMenuButtonObject.GetComponent<RectTransform>();
        mainMenuButtonRect.sizeDelta = new Vector2(200, 50);
        mainMenuButtonRect.anchoredPosition = new Vector2(0, -150);

        // Добавляем OnClick событие для кнопки выхода в главное меню
        mainMenuButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("LoadScene"); // Замените "MainMenuScene" на имя вашей сцены главного меню
        });
    }
}
