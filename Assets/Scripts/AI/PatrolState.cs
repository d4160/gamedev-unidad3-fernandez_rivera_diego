using UnityEngine;

public class PatrolState : AIState
{
    public PatrolState(AIController controller) : base(controller) { }
    
    private int _currentWaypointIndex = 0;

    public override void OnEnter()
    {
        Debug.Log("Entrando en estado de Patrulla.");
        m_agent.speed = m_controller.patrolSpeed;
        GoToNextWaypoint();
    }

    public override void UpdateState()
    {
        // 1. Condición de transición: ¿vemos al jugador?
        if (Vector3.Distance(m_controller.transform.position, m_playerTransform.position) < m_controller.detectionRadius)
        {
            m_controller.ChangeState(new ChaseState(m_controller));
            return;
        }

        // 2. Lógica del estado: ¿hemos llegado al waypoint?
        if (!m_agent.pathPending && m_agent.remainingDistance < 0.5f)
        {
            GoToNextWaypoint();
        }
    }

    public override void OnExit() { /* No necesita lógica por ahora */ }

    private void GoToNextWaypoint()
    {
        if (m_controller.waypoints.Length == 0) return;
        m_agent.destination = m_controller.waypoints[_currentWaypointIndex].position;
        _currentWaypointIndex = (_currentWaypointIndex + 1) % m_controller.waypoints.Length;
    }
}