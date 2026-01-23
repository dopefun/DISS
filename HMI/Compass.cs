using UnityEngine;

public class Compass : MonoBehaviour
{
    [SerializeField]
    private Transform m_PlayerCamera;

    private RectTransform m_CompassRT;
    private Vector3 m_CompassDirection;
    private float m_CompassNativeWidth;
    private float m_CompassZeroOffset;

    void Start()
    {
        GameObject _camera = GameObject.FindGameObjectWithTag("MainCamera");
        if (_camera != null)
        {
            m_PlayerCamera = _camera.transform;
        }
        m_CompassRT = this.GetComponent<RectTransform>();
        m_CompassNativeWidth = m_CompassRT.rect.width;
        m_CompassZeroOffset = m_CompassNativeWidth / 2;
    }

    void Update()
    {
        var playerRotation = m_PlayerCamera.rotation.eulerAngles;

        float coercedRotationAngle = CoerceAngle(playerRotation.y);

        m_CompassDirection = new Vector3(0, coercedRotationAngle, 0); 

        var offset = Map(m_CompassDirection.y, -180, 180, m_CompassZeroOffset + m_CompassNativeWidth / 6, m_CompassZeroOffset - m_CompassNativeWidth / 6);

        var temp = m_CompassRT.localPosition;
        m_CompassRT.localPosition = new Vector3(offset, temp.y, temp.z);
    }

    float CoerceAngle(float angle)
    {
        if (angle >= 360f)
            angle -= 360f;

        if (angle > 180f)
            angle -= 360f;

        return angle;
    }

    float Map(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }
}