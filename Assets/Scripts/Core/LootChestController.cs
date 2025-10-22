using UnityEngine;

/// <summary>
/// Implementación concreta de IInteractable para un cofre de botín.
/// Solo se puede interactuar una vez para abrirlo.
/// </summary>
public class LootChestController : MonoBehaviour, IInteractable
{
    private bool _isOpened = false;

    /// <summary>
    /// Implementación del método Interact() exigido por la interfaz IInteractable.
    /// Abre el cofre si no ha sido abierto antes.
    /// </summary>
    public void Interact()
    {
        if (_isOpened)
        {
            Debug.Log("Este cofre ya ha sido abierto.");
            return; // Salimos del método para no ejecutar más código.
        }

        _isOpened = true;
        Debug.Log("¡Has abierto el cofre y encontrado un tesoro!");

        // En un juego real, aquí instanciarías un item, añadirías oro al inventario, etc.
    }
}