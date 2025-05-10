using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PinPhysics : MonoBehaviour
{
    [Header("Physics Settings")]
    public float gravityMultiplier = 2.5f;       // Makes the ball fall faster
    public float verticalBounceDamping = 0.3f;   // Controls vertical bounce (lower = less bounce)
    public float maxVerticalVelocity = 3.0f;     // Maximum upward velocity

    [Header("Constraints")]
    public bool constrainRotation = true;        // Limits rotation to Y-axis only for better rolling

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody component required");
            return;
        }

        // Apply rotation constraints if needed
        if (constrainRotation)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    void FixedUpdate()
    {
        // Only apply when not kinematic
        if (rb != null && !rb.isKinematic)
        {
            // Apply additional downward force to make the ball fall faster
            rb.AddForce(Physics.gravity * (gravityMultiplier - 1.0f) * rb.mass);

            // Limit upward velocity to prevent excessive bouncing
            if (rb.velocity.y > maxVerticalVelocity)
            {
                Vector3 velocity = rb.velocity;
                velocity.y = maxVerticalVelocity;
                rb.velocity = velocity;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Skip floor collisions for bounce dampening
        if (collision.gameObject.CompareTag("Floor"))
            return;

        if (rb != null && !rb.isKinematic)
        {
            // Only dampen vertical component on bounce
            // Maintain horizontal momentum for better gameplay
            Vector3 velocity = rb.velocity;

            // Reduce the upward component of velocity (dampens bouncing)
            if (velocity.y > 0)
            {
                velocity.y *= verticalBounceDamping;
                rb.velocity = velocity;
            }
        }
    }
}