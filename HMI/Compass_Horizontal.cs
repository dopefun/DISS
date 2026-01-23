using EasyRoads3Dv3;
using UnityEngine;

public class Compass_Horizontal : MonoBehaviour
{
    [SerializeField]
    private Transform m_PlayerCamera;

    private RectTransform m_CompassRT_Hor;
    private Vector3 m_CompassDirection_Hor;
    private float m_CompassNativeHeight;
    private float m_CompassNativeWidth;
    public float m_CompassZeroOffset;

    void Start()
    {
        GameObject _camera = GameObject.FindGameObjectWithTag("Player");
        if (_camera != null)
        {
            m_PlayerCamera = _camera.transform;
        }
        m_CompassRT_Hor = this.GetComponent<RectTransform>();
        m_CompassNativeHeight = m_CompassRT_Hor.rect.height;
        m_CompassNativeWidth = m_CompassRT_Hor.rect.width;
        //m_CompassZeroOffset = m_CompassNativeHeight / 2 ;
        //m_CompassZeroOffset = -150;
    }

    void Update()
    {
        var playerRotation = m_PlayerCamera.rotation.eulerAngles;

        float coercedRotationAngle = CoerceAngle(playerRotation.x);

        m_CompassDirection_Hor = new Vector3(0, coercedRotationAngle, 0); 

        var offset = Map_Hor(m_CompassDirection_Hor.y, -180, 180, m_CompassZeroOffset + m_CompassNativeWidth , m_CompassZeroOffset - m_CompassNativeWidth);
        //var offset = m_CompassZeroOffset;
        var temp = m_CompassRT_Hor.localPosition;
        m_CompassRT_Hor.localPosition = new Vector3(temp.x, offset, temp.z);
    }

    float CoerceAngle(float angle)
    {
        if (angle >= 360f)
            angle -= 360f;

        if (angle > 180f)
            angle -= 360f;

        return angle;
    }

    float Map_Hor(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }
}