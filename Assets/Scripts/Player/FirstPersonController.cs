using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _movementSpeed = 5.0f;

    [Header("Look Settings")]
    [SerializeField] private float _mouseSensitivity = 2.0f;
    [SerializeField] private float _verticalLookLimit = 80f;

    [Header("Component References")]
    [SerializeField] private Transform _cameraTransform;

    private CharacterController _characterController;
    private PlayerInputActions _inputActions;

    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _xRotation = 0f;
    
    private float _verticalVelocity = 0f;
    private float _gravity = -9.81f;


    void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        if (!_cameraTransform)
        {
            Debug.Log("Error, camara no asignada");
            this.enabled = false;
            return;
        }

        _inputActions = new PlayerInputActions();
    }

    void Start()
    {
        // Esto ya no es necesario, lo maneja UIManager
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    void OnEnable()
    {
        _inputActions.Enable();

        _inputActions.Player.Move.performed += OnMouseInput;
        _inputActions.Player.Move.canceled += OnMouseInput;
        _inputActions.Player.Look.performed += OnLookInput;
        _inputActions.Player.Look.canceled += OnLookInput;
    }

    void OnDisable()
    {
        _inputActions.Player.Move.performed -= OnMouseInput;
        _inputActions.Player.Move.canceled -= OnMouseInput;
        _inputActions.Player.Look.performed -= OnLookInput;
        _inputActions.Player.Look.canceled -= OnLookInput;

        _inputActions.Disable();
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
    }

    private void OnMouseInput(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnLookInput(InputAction.CallbackContext ctx)
    {
        _lookInput = ctx.ReadValue<Vector2>();
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = transform.forward * _moveInput.y + transform.right * _moveInput.x;

        if (_characterController.isGrounded)
        {
            _verticalVelocity = -1f; // Pequeño valor para mantenerlo pegado al suelo
        }
        else
        {
            _verticalVelocity += _gravity * Time.deltaTime;
        }
        
        // Combinar movimiento horizontal y vertical
        moveDirection.y = _verticalVelocity;

        _characterController.Move(moveDirection * _movementSpeed * Time.deltaTime);
    }

    private void HandleLook()
    {
        float mouseX = _lookInput.x * _mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        float mouseY = _lookInput.y * _mouseSensitivity * Time.deltaTime;

        _xRotation -= mouseY;

        _xRotation = Mathf.Clamp(_xRotation, -_verticalLookLimit, +_verticalLookLimit);

        _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
    }
}
