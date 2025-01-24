using UnityEngine;
using UnityEngine.InputSystem;

public class Submarine : MonoBehaviour
{
    [SerializeField] 
    private int             playerId = 0;
    [SerializeField]
    private float           maxSpeed = 200.0f; 
    [SerializeField]
    private float           acceleration = 200.0f; 
    [SerializeField]
    private float           drag = 1.0f; 
    [SerializeField, Header("Input")] 
    private PlayerInput     playerInput;
    [SerializeField, InputPlayer(nameof(playerInput))] 
    private InputControl    moveControl;
    [SerializeField, Header("Animation")] 
    private float           maxAngleZ;

    private Vector2     movementVector;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        MasterInputManager.SetupInput(playerId, playerInput);
        moveControl.playerInput = playerInput;
    }

    void FixedUpdate()
    {
        Vector2 velocity = rb.linearVelocity;

        velocity = velocity - velocity * drag * Time.fixedDeltaTime;
        velocity = velocity + movementVector * acceleration * Time.fixedDeltaTime;
        velocity = velocity.normalized * Mathf.Clamp(velocity.magnitude, 0, maxSpeed);

        rb.linearVelocity = velocity;
    }

    private void Update()
    {
        movementVector = moveControl.GetAxis2();

        var velocity = movementVector;
        if (movementVector.sqrMagnitude > 1e-3)
        {
            var direction = movementVector.normalized;

            var targetRotation = Quaternion.LookRotation(Vector3.forward, new Vector2(-velocity.y, velocity.x));

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * 720.0f);
        }
    }
}
