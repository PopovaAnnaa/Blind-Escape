using UnityEngine;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private Animator animator;
    private const string IS_RUNNING = "IsRunning";
    private const string MOVE_X = "MoveX";
    private const string MOVE_Y = "MoveY";

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Vector2 direction = Player.Instance.GetMovingDirection();

        animator.SetBool(IS_RUNNING, Player.Instance.IsRunning());
        animator.SetFloat(MOVE_X, direction.x);
        animator.SetFloat(MOVE_Y, direction.y);
    }
}
