using UnityEngine;

public class NumericalProjectile : MonoBehaviour
{
    public Vector2 velocity;
    public float dt; // The "Calculus" time step
    private Vector2 acceleration;
    private float timer;

    private Vector2 currentLogicalPosition;
    private Vector2 nextLogicalPosition;
    private Rigidbody2D rb;
    [Header("Physics")]
    [SerializeField] private float bounciness = 0.5f;
    [SerializeField] private int maxBounces = 5;
    private int currentBounces = 0;

    void Start()
    {
        acceleration = Physics2D.gravity;
        rb = GetComponent<Rigidbody2D>();
        if (rb != null){
            rb.gravityScale = 0;
        }

        currentLogicalPosition = transform.position;
        
        // Calculate the first segment's end point
        if (dt > 0)
        {
            velocity += acceleration * dt;
            nextLogicalPosition = currentLogicalPosition + velocity * dt;
        }
        else
        {
            nextLogicalPosition = currentLogicalPosition;
        }
    }

    void Update()
    {
        if (dt <= 0) return;

        timer += Time.deltaTime;

        // If real time has passed the dt threshold, calculate the next segment
        while (timer >= dt)
        {
            timer -= dt; 
            
            // The old end point is our new start point
            currentLogicalPosition = nextLogicalPosition;
            
            // Calculate the next step via Euler's Method just like Slingshot.cs
            // v = v_old + a*dt
            velocity = velocity + acceleration * dt;
            // r = r_old + v*dt
            nextLogicalPosition = currentLogicalPosition + velocity * dt;
        }

        // Smoothly move (Lerp) the object along the line between the calculated points
        float interpolation = timer / dt;
        Vector2 interpolatedPos = Vector2.Lerp((Vector3)currentLogicalPosition, (Vector3)nextLogicalPosition, interpolation);
        
        // USE MOVEPOSITION instead of transform.position to ensure physics calculations are accurate
        rb.MovePosition(interpolatedPos);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        currentBounces++;
        if (currentBounces >= maxBounces)
        {
            Destroy(gameObject);
            return; 
        }

        if (!collision.collider.isTrigger)
        {
            // normal is the vector perpendicular to the surface of the object it collided with
            Vector2 normal = collision.contacts[0].normal;
            
            // Reflect the velocity vector
            // reflect_velocity = original_velocity - 2 * dot(original_velocity, normal) * normal
            // That's what it's doing under Vector2.Reflect
            velocity = Vector2.Reflect(velocity, normal);
            velocity *= bounciness;

            // Reset the manual math positions so it continues from the bounce point
            // So that it doesn't get stuck inside the object it collided with, but still moves
            currentLogicalPosition = transform.position;
            nextLogicalPosition = currentLogicalPosition + (velocity * dt);
            timer = 0; 
        }
    }
}
