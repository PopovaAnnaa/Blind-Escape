using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    [SerializeField] private bool isLocked = true;
    [SerializeField] private Collider2D blockingCollider;

    private Animator animator;
    private const string IS_OPEN = "IsOpen";

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (isLocked)
        {
            if (player.hasKeycard)
            {
                isLocked = false;
                animator.SetBool(IS_OPEN, true);
                if (blockingCollider != null) blockingCollider.enabled = false;
                Debug.Log("Двері розблоковані ключ-картою!");
            }
            else
            {
                Debug.Log("Двері заблоковані. Потрібна ключ-карта.");
            }
        }
        else
        {
            animator.SetBool(IS_OPEN, true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isLocked)
        {
            animator.SetBool(IS_OPEN, false);
        }
    }
}