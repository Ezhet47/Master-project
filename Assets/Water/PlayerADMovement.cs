using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerADMovement : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float acceleration = 45f;
    [SerializeField] private float deceleration = 55f;

    private Rigidbody2D rb;
    private float inputX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");
    }

    private void FixedUpdate()
    {
        float targetSpeed = inputX * maxSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float rate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float movement = speedDiff * rate * Time.fixedDeltaTime;

        rb.AddForce(Vector2.right * movement, ForceMode2D.Force);

        // Keep movement stable.
        Vector2 v = rb.linearVelocity;
        v.x = Mathf.Clamp(v.x, -maxSpeed, maxSpeed);
        rb.linearVelocity = v;
    }
}
