using UnityEngine;

public class PauseMenuState : UIState
{
    public PauseMenuState(UIManager uiManager) : base(uiManager) { }

    public override void Enter()
    {
        Debug.Log("Entrando al estado de En Pausa");
        m_uiManager.pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void Exit()
    {
        Debug.Log("Saliendo del estado de Menú Principal");
        m_uiManager.pauseMenuPanel.SetActive(false);
    }
}