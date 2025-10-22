using UnityEngine;

/// <summary>
/// Implementación concreta de IInteractable para una puerta simple.
/// Al interactuar, simula abrirse y cerrarse.
/// </summary>
public class DoorController : MonoBehaviour, IInteractable
{
    private bool _isOpen = false;

    /// <summary>
    /// Implementación del método Interact() exigido por la interfaz IInteractable.
    /// Cambia el estado de la puerta y muestra un mensaje en la consola.
    /// </summary>
    public void Interact()
    {
        _isOpen = !_isOpen; // Invierte el estado booleano
        Debug.Log(_isOpen ? "La puerta se ha ABIERTO." : "La puerta se ha CERRADO.");
        
        // En un juego real, aquí activarías una animación o rotarías el objeto.
        // Por ejemplo: transform.Rotate(0, 90 * (_isOpen ? 1 : -1), 0);
    }
}