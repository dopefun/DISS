using System;
using UnityEngine;

public class AeroplanePropellerAnimator : MonoBehaviour
{
    [SerializeField] private Transform[] m_PropellorModel;  // The model of the aeroplane's propellor.
    [SerializeField] private Transform[] m_PropellorBlur;   // The plane used for the blurred propellor textures.
    [SerializeField] private Texture2D[] m_PropellorBlurTextures; // An array of increasingly blurred propellor textures.

    private PropellersController propellers; // Reference to the aeroplane controller.
    private BatteryManager batteryManager; // Reference to the battery manager.
    private const float k_RpmToDps = 6.28f * 60f; // For converting from revs per minute to degrees per second.

    private float[] currentRPM; // Current RPM for smooth stopping

    private void Awake()
    {
        // Set up the reference to the aeroplane controller.
        propellers = GetComponent<PropellersController>();
        batteryManager = GetComponent<BatteryManager>();
        currentRPM = new float[m_PropellorModel.Length];
    }

    private void Update()
    {
        for (int propellerIndex = 0; propellerIndex < m_PropellorModel.Length; propellerIndex++)
        {

            // Set current RPM to propeller RPM
            currentRPM[propellerIndex] = propellers.getRPM(propellerIndex);

            if (m_PropellorModel[propellerIndex] != null)
            {
                m_PropellorModel[propellerIndex].Rotate(0, currentRPM[propellerIndex] * Time.deltaTime * k_RpmToDps, 0, Space.Self);
            }
        }
    }
}
