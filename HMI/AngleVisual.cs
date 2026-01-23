using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AngleVisual : MonoBehaviour
{
    public Text VeritcalLine;
    public Text HorizontalLine;


    // Update is called once per frame
    void Update()
    {
        GameObject drone = GameObject.FindGameObjectWithTag ("Player");
		if (drone != null)
			VeritcalLine.text = "Верт: " + Mathf.Round(drone.transform.localEulerAngles [0]).ToString() + "°";

        if (drone != null)
            HorizontalLine.text = "Гориз: " + Mathf.Round(drone.transform.localEulerAngles [2]).ToString() + "°";
    }
}
