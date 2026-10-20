using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator animator;
    private const string IS_OPEN = "IsOpen";

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            animator.SetBool(IS_OPEN, true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            animator.SetBool(IS_OPEN, false);
        }
    }
}