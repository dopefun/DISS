using UnityEngine;
using UnityEngine.SceneManagement;

public class QuadcopterReset : MonoBehaviour
{
    public float minHeight = 0.1f; // Минимальная высота, при которой квадрокоптер считается упавшим
    public float maxAngle = 80f; // Максимальный угол, при котором квадрокоптер считается перевернувшимся
    public float flipTimeThreshold = 5f; // Пороговое время, в течение которого квадрокоптер должен быть перевернутым
    public LayerMask groundLayer; // Лейер маска, определяющая, какие слои считать землей

    private Rigidbody rb;
    private float flipTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Проверяем, что квадрокоптер упал и перевернулся
        if (IsCrashed() && IsFlippedOver())
        {
            flipTime += Time.deltaTime;

            if (flipTime >= flipTimeThreshold)
            {
                // Перезапускаем сцену
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        else
        {
            flipTime = 0f;
        }
    }

    bool IsCrashed()
    {
        // Вычисляем высоту террейна под квадрокоптером с помощью Raycast
        RaycastHit hit;
        if (Physics.Raycast(rb.transform.position, Vector3.down, out hit, Mathf.Infinity, groundLayer))
        {
            // Если высота квадрокоптера над землей ниже минимальной, считаем его упавшим
            return rb.transform.position.y - hit.point.y < minHeight;
        }

        // Если Raycast не нашел землю, считаем, что квадрокоптер не упал
        return false;
    }

    bool IsFlippedOver()
    {
        // Вычисляем угол между вектором вверх и вертикалью квадрокоптера
        float angle = Vector3.Angle(Vector3.up, rb.transform.up);

        // Если угол больше максимального, считаем квадрокоптер перевернувшимся
        return angle > maxAngle;
    }
}