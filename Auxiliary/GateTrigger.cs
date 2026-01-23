using UnityEngine;

public class GateTrigger : MonoBehaviour
{
    public RaceGate gateController; // Ссылка на родителя (ворота)

    private void OnTriggerEnter(Collider other)
    {
        GameObject root = other.transform.root.gameObject;

        if (gateController.enabledGate && root.name.Contains("Model"))
        {
            gateController.TriggerGate();
        }
    }
}
