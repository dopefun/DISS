using UnityEngine;

public class GateTrigger : MonoBehaviour
{
    public RaceGate gateController;

    private void OnTriggerEnter(Collider other)
    {
        GameObject root = other.transform.root.gameObject;

        if (gateController.enabledGate && root.name.Contains("Model"))
        {
            float distanceFromCenter = Vector3.Distance(
                gateController.gateCenter.position, 
                other.transform.position
            );

            GateAccuracyTracker accuracyTracker = root.GetComponent<GateAccuracyTracker>();
            if (accuracyTracker != null)
            {
                accuracyTracker.RegisterGatePass(gateController.GateNumber, distanceFromCenter);
                //Debug.Log($"[GateTrigger] Registered pass: gate={gateController.GateNumber}, distance={distanceFromCenter:F2}m");
            }
            else
            {
                //Debug.LogWarning($"[GateTrigger] GateAccuracyTracker not found on {root.name}");
            }

            gateController.TriggerGate();
        }
    }
}