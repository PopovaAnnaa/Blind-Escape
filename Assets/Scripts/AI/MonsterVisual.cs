using UnityEngine;

public class MonsterVisual : MonoBehaviour
{
    private Animator animator;
    private MonsterAI monsterAI;
    private const string IS_RUNNING = "IsRunning";
    private const string MOVE_X = "MoveX";
    private const string MOVE_Y = "MoveY";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        monsterAI = GetComponentInParent<MonsterAI>();
    }

    private void Update()
    {
        Vector2 direction = monsterAI.GetMovementDirection();

        animator.SetFloat(MOVE_X, direction.x);
        animator.SetFloat(MOVE_Y, direction.y);
        animator.SetBool(IS_RUNNING, monsterAI.IsRunning());
    }
}
