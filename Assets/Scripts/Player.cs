using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float movingSpeed = 7f;

    private Rigidbody2D rb;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        Vector2 inputVector = new Vector2(0, 0);

        Keyboard keyboard = Keyboard.current;

        if (keyboard.wKey.isPressed)
        {
            inputVector.y = 1f;
        }
        if (keyboard.aKey.isPressed)
        {
            inputVector.x = -1f;
        }
        if (keyboard.sKey.isPressed)
        {
            inputVector.y = -1f;
        }
        if (keyboard.dKey.isPressed)
        {
            inputVector.x = 1f;
        }

        inputVector = inputVector.normalized;

        rb.MovePosition(rb.position + inputVector * (movingSpeed * Time.fixedDeltaTime));
    }
}