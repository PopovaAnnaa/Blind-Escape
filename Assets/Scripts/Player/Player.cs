using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    [SerializeField] private float movingSpeed = 7f;
    public bool hasKeycard = false;

    private float minMovingSpeed = 0.1f;
    private bool isRunning = false;

    private Vector2 movingDirection = new Vector2(0, 0);

    private Rigidbody2D rb;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        HandleMovement();
    }

    private void HandleMovement()
    {
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

        if (Mathf.Abs(inputVector.x) > minMovingSpeed || Mathf.Abs(inputVector.y) > minMovingSpeed)
        {
            isRunning = true;
            movingDirection = inputVector;
        }
        else
        {
            isRunning = false;
            movingDirection = new Vector2(0,0);
        }
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public Vector2 GetMovingDirection()
    {
        return movingDirection;
    }
}