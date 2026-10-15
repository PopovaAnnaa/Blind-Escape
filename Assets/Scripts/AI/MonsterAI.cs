using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAI : MonoBehaviour
{
    public enum AIState { Patrol, Investigate, Chase, Search }

    [Header("Поточний стан FSM")]
    public AIState currentState = AIState.Patrol;

    [Header("Навігація (A* без NavMesh)")]
    public AStarPathfinding pathfinding;
    public Grid2D gridManager;
    public float baseSpeed = 2.5f;
    private List<Vector3> currentPath;
    private int currentPathIndex = 0;

    [Header("Сприйняття (Зір)")]
    public Transform playerTransform;
    public float viewDistance = 8f;
    public float viewAngle = 90f;
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    [Header("Математика підозрілості")]
    [Range(0, 100)] public float suspicion = 0f;
    public float suspicionThresholdInvestigate = 30f;
    public float suspicionThresholdChase = 70f;
    public float suspicionDecayRate = 10f;

    [Header("Патрулювання та Пошук")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    public Vector3 lastKnownPlayerPosition;
    public float maxSearchTime = 5f;
    private float searchTimer = 0f;
    private bool isPlayerHiding = false;

    // Події для Студента 2 (Game Over та Звук)
    public static System.Action OnPlayerCaught;
    public static System.Action<AIState> OnAIStateChanged;

    private void Update()
    {
        UpdatePerception();
        UpdateFSMStates();
        ExecuteCurrentState();
        MoveAlongPath();
    }

    private void UpdatePerception()
    {
        bool canSeePlayer = CheckLineOfSight();

        if (canSeePlayer)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            float distanceFactor = 1f - Mathf.Clamp01(distance / viewDistance);
            suspicion += (50f + distanceFactor * 50f) * Time.deltaTime;
            lastKnownPlayerPosition = playerTransform.position;
        }
        else
        {
            suspicion -= suspicionDecayRate * Time.deltaTime;
        }

        suspicion = Mathf.Clamp(suspicion, 0f, 100f);
    }

    private bool CheckLineOfSight()
    {
        if (playerTransform == null || isPlayerHiding) return false;

        Vector2 dirToPlayer = (playerTransform.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= viewDistance)
        {
            float angleBetween = Vector2.Angle(transform.up, dirToPlayer);
            if (angleBetween <= viewAngle / 2f)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, distanceToPlayer, obstacleMask | playerMask);
                if (hit.collider != null && ((1 << hit.collider.gameObject.layer) & playerMask) != 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void OnHearNoise(Vector3 noisePosition, float loudness)
    {
        float distance = Vector3.Distance(transform.position, noisePosition);
        if (distance <= 0.1f) distance = 0.1f;

        float effectiveNoise = loudness / (distance * distance);
        suspicion += effectiveNoise * 20f;

        if (suspicion >= suspicionThresholdInvestigate)
        {
            lastKnownPlayerPosition = noisePosition;
            SetNewDestination(lastKnownPlayerPosition);
        }
    }

    public void SetPlayerHidingState(bool isHiding)
    {
        isPlayerHiding = isHiding;
        if (isHiding && currentState == AIState.Chase)
        {
            suspicion = suspicionThresholdInvestigate + 10f;
            ChangeState(AIState.Search);
            searchTimer = maxSearchTime;
        }
    }

    private void UpdateFSMStates()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                if (suspicion >= suspicionThresholdChase) ChangeState(AIState.Chase);
                else if (suspicion >= suspicionThresholdInvestigate) ChangeState(AIState.Investigate);
                break;

            case AIState.Investigate:
                if (suspicion >= suspicionThresholdChase) ChangeState(AIState.Chase);
                else if (suspicion < 10f) ChangeState(AIState.Patrol);
                break;

            case AIState.Chase:
                if (suspicion < suspicionThresholdChase)
                {
                    searchTimer = maxSearchTime;
                    ChangeState(AIState.Search);
                }
                break;

            case AIState.Search:
                if (suspicion >= suspicionThresholdChase) ChangeState(AIState.Chase);
                else if (searchTimer <= 0) ChangeState(AIState.Patrol);
                break;
        }
    }

    private void ChangeState(AIState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            OnAIStateChanged?.Invoke(currentState);
        }
    }

    private void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                if (currentPath == null || currentPathIndex >= currentPath.Count)
                {
                    if (patrolPoints.Length > 0)
                    {
                        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                        SetNewDestination(patrolPoints[currentPatrolIndex].position);
                    }
                }
                break;

            case AIState.Investigate:
                SetNewDestination(lastKnownPlayerPosition);
                break;

            case AIState.Chase:
                SetNewDestination(playerTransform.position);
                break;

            case AIState.Search:
                searchTimer -= Time.deltaTime;
                if (currentPath == null || currentPathIndex >= currentPath.Count)
                {
                    PathNode hottestNode = gridManager.GetHottestNodeInRadius(lastKnownPlayerPosition, 5f);
                    if (hottestNode != null) SetNewDestination(hottestNode.worldPosition);
                }
                break;
        }
    }

    private void SetNewDestination(Vector3 targetPos)
    {
        if (pathfinding != null)
        {
            List<Vector3> newPath = pathfinding.FindPath(transform.position, targetPos);
            if (newPath != null && newPath.Count > 0)
            {
                currentPath = newPath;
                currentPathIndex = 0;
            }
        }
    }

    private void MoveAlongPath()
    {
        if (currentPath != null && currentPathIndex < currentPath.Count)
        {
            Vector3 targetPosition = currentPath[currentPathIndex];
            float speed = baseSpeed * (currentState == AIState.Chase ? 1.6f : 1.0f);

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            // Поворот спрайта у напрямку руху
            Vector3 moveDir = (targetPosition - transform.position).normalized;
            if (moveDir != Vector3.zero)
            {
                float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                currentPathIndex++;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isPlayerHiding)
        {
            if (currentState == AIState.Chase || currentState == AIState.Investigate)
            {
                Debug.Log("💀 ГРАВЦЯ ВБИТО!");
                OnPlayerCaught?.Invoke();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 leftDir = Quaternion.Euler(0, 0, viewAngle / 2f) * transform.up;
        Vector3 rightDir = Quaternion.Euler(0, 0, -viewAngle / 2f) * transform.up;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, leftDir * viewDistance);
        Gizmos.DrawRay(transform.position, rightDir * viewDistance);
    }
}