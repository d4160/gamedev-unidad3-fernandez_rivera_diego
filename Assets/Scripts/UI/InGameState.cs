using UnityEngine;

public class InGameState : UIState
{
    public InGameState(UIManager uiManager) : base(uiManager) { }

    public override void Enter()
    {
        Debug.Log("Entrando al estado de En Juego");

        Object.FindFirstObjectByType<FirstPersonController>().enabled = true;

        m_uiManager.inGameHudPanel.SetActive(true);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void Exit()
    {
        Debug.Log("Saliendo del estado de Menú Principal");
        m_uiManager.inGameHudPanel.SetActive(false);
    }
}