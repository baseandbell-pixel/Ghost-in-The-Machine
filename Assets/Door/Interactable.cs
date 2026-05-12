using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string promptMessage = "Press E to Open";
    public GameObject interactionUI;
    protected bool isInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInside = true;
            if (interactionUI != null) interactionUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInside = false;
            if (interactionUI != null) interactionUI.SetActive(false);
        }
    }
}