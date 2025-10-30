using UnityEngine;

public class TerminalController : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Terminal activado. Disparando evento OnObjectiveActivated.");
        GameEvents.TriggerObjectiveActivated();

        // opcional:
        // gameObject.GetComponent<Collider>().enabled = false; 
        // para que solo funcione 1 vez
    }
}
