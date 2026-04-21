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

    void Start()
    {
        acceleration = Physics2D.gravity;
        rb = GetComponent<Rigidbody2D>();
        if (rb != null){
            rb.bodyType = RigidbodyType2D.Kinematic; 
            rb.useFullKinematicContacts = true;
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
            velocity += acceleration * dt;
            nextLogicalPosition = currentLogicalPosition + velocity * dt;
        }

        // Smoothly move (Lerp) the object along the straight line between the calculated points
        float interpolation = timer / dt;
        Vector2 interpolatedPos = Vector2.Lerp((Vector3)currentLogicalPosition, (Vector3)nextLogicalPosition, interpolation);
        
        // USE MOVEPOSITION instead of transform.position
        rb.MovePosition(interpolatedPos);
    }
}
