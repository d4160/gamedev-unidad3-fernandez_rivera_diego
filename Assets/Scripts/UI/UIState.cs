using UnityEngine;

public abstract class UIState
{
    protected UIManager m_uiManager;

    public UIState(UIManager uiManager)
    {
        m_uiManager = uiManager;
    }

    public abstract void Enter();
    public abstract void Exit();
}
