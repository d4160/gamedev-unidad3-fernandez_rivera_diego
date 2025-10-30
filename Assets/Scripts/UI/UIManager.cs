using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // Para la carga de escenas

/// <summary>
/// Gestiona los estados de la UI y las transiciones entre ellos
/// Utiliza el Patrón de Diseño State, arquitectura limpia y escalable
/// Implementa un Singleton para un acceso global sencillo
/// </summary>
public class UIManager : MonoBehaviour
{
    // Singleton Pattern
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject pauseMenuPanel;
    public GameObject inGameHudPanel;

    // Estados de la UI
    private UIState _currentState;
    public MainMenuState MainMenuState { get; private set; }
    public InGameState InGameState { get; private set; }
    public PauseMenuState PauseMenuState { get; private set; }

    private void Awake()
    {
        // Configuración del Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Para que persista entre escenas

        // Inicialización de los estados
        MainMenuState = new MainMenuState(this);
        InGameState = new InGameState(this);
        PauseMenuState = new PauseMenuState(this);
    }

    private void Start()
    {
        // El estado inicial al arrancar el juego
        ChangeState(MainMenuState);
    }
    
    private void Update()
    {
        // Lógica para pausar el juego
        Keyboard keyboard = Keyboard.current;
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            if (_currentState == InGameState)
            {
                ChangeState(PauseMenuState);
            }
            else if (_currentState == PauseMenuState)
            {
                ChangeState(InGameState);
            }
        }
    }

    public void ChangeState(UIState newState)
    {
        // Salir del estado actual si existe
        _currentState?.Exit();
        
        // Entrar en el nuevo estado
        _currentState = newState;
        _currentState.Enter();
    }

    // Métodos para los botones de la UI
    public void OnPlayButtonClicked()
    {
        ChangeState(InGameState);
    }

    public void OnResumeButtonClicked()
    {
        ChangeState(InGameState);
    }

    public void OnExitButtonClicked()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}