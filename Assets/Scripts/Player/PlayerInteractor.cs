using UnityEngine;
using UnityEngine.InputSystem; // Añadido

/// <summary>
/// Gestiona la capacidad del jugador para interactuar con objetos en el mundo.
/// Utiliza un Raycast para detectar objetos que implementen la interfaz IInteractable.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float _interactionDistance = 2f;
    // La variable _interactionKey ha sido eliminada.

    private Camera _mainCamera;
    private PlayerInputActions _inputActions; // Añadido

    private void Awake()
    {
        _mainCamera = Camera.main;
        _inputActions = new PlayerInputActions(); // Añadido
    }

    // --- Añadimos OnEnable y OnDisable para gestionar los eventos ---
    private void OnEnable()
    {
        _inputActions.Player.Enable();
        _inputActions.Player.Interact.performed += OnInteract;
    }

    private void OnDisable()
    {
        _inputActions.Player.Interact.performed -= OnInteract;
        _inputActions.Player.Disable();
    }

    private void Update()
    {
        // La lógica de detección puede permanecer en Update si queremos dar feedback visual continuo (ej. UI)
        // pero la acción de interactuar se moverá a su propio método de evento.
        // Por ahora, para simplificar, lo dejamos todo junto, pero la llamada a Interact() ya no está aquí.
        DetectAndShowFeedback();
    }
    
    // Este es nuestro nuevo método de callback para la interacción.
    private void OnInteract(InputAction.CallbackContext context)
    {
        PerformRaycastInteraction();
    }

    // Extraemos la lógica del raycast a su propio método para poder llamarla desde OnInteract.
    private void PerformRaycastInteraction()
    {
        Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }
    
    // Opcional: un método para feedback visual en Update
    private void DetectAndShowFeedback()
    {
        Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance))
        {
            if (hit.collider.GetComponent<IInteractable>() != null)
            {
                Debug.Log("Objeto interactuable a la vista: " + hit.collider.name);
                // Aquí iría la lógica para mostrar "[E] Interactuar" en la UI.
            }
        }
    }
}