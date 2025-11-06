using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Clase base abstracta para todos los estados de la IA
/// </summary>
public abstract class AIState
{
    // Usamos 'protected' para que las clases hijas puedan acceder.
    // El prefijo 'm_' es una convención común para miembros protegidos.
    protected AIController m_controller;
    protected NavMeshAgent m_agent;
    protected Transform m_playerTransform;

    public AIState(AIController controller)
    {
        m_controller = controller;
        m_agent = controller.GetComponent<NavMeshAgent>();
        m_playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public abstract void OnEnter();
    public abstract void UpdateState();
    public abstract void OnExit();
}