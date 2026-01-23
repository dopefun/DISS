using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public LayerMask groundLayer; // Слой, который будет считаться землей
    public float checkRadius = 0.5f; // Радиус проверки
    //public float checkDistance = 1f;
    public float pushForce = 10f; // Сила, с которой объект будет подниматься
    public float offset = 0f;

    private void Update()
    {
        CheckGround();
    }

    private void CheckGround()
    {
        // Получаем позицию центра объекта
        //Vector3 center = transform.position;
        Vector3 center = new Vector3(transform.position.x, transform.position.y + offset, transform.position.z);

        // Проверяем наличие земли под объектом
        Collider[] colliders = Physics.OverlapSphere(center, checkRadius, groundLayer);

        if (colliders.Length > 0)
        {
            // Если земля найдена, проверяем, находится ли центр объекта ниже поверхности
            foreach (Collider collider in colliders)
            {
                if (center.y < collider.bounds.max.y)
                {
                    // Поднимаем объект на поверхность
                    float pushHeight =  0.1f; // +0.1f для небольшого запаса
                    transform.position += Vector3.up * pushHeight;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Рисуем сферу для визуализации области проверки
        Gizmos.color = Color.blue;
        //Gizmos.DrawRay(new Vector3(transform.position.x, transform.position.y + offset, transform.position.z), Vector3.down * checkDistance);
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y + offset, transform.position.z), checkRadius);
    }
}