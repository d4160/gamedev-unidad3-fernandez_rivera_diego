using UnityEngine;

/// <summary>
/// Implementación concreta de IInteractable para un cofre de botín
/// Solo se puede interactuar una vez para abrirlo
/// </summary>
public class LootChestController : MonoBehaviour, IInteractable
{
    private bool _isOpened = false;

    public void Interact()
    {
        if (_isOpened)
        {
            Debug.Log("Este cofre ya ha sido abierto.");
            return;
        }

        _isOpened = true;
        Debug.Log("¡Has abierto el cofre y encontrado un tesoro!");
    }
}