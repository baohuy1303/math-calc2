using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class Slingshot : MonoBehaviour
{
    [Header("Slingshot Components")]
    [SerializeField] private LineRenderer leftLineRenderer;
    [SerializeField] private LineRenderer rightLineRenderer;
    [SerializeField] private Transform leftOrigin;
    [SerializeField] private Transform rightOrigin;
    [SerializeField] private Transform centerPoint;

    [Header("Slingshot Settings")]
    [SerializeField] private float maxDistance = 3f;
    [SerializeField] private float grabDistance = 2f;

    [Header("Shooting & Trajectory")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootForceMultiplier = 5f;
    [SerializeField] private LineRenderer trajectoryLineRenderer;
    [SerializeField] private int trajectoryDrawSteps = 50;
    [SerializeField] private float trajectoryTimeStep = 0.05f;

    private Vector3 slingshotPosition;
    private bool isGrabbing = false;
    
    void Start()
    {
        ResetSlingshot();
    }

    void Update()
    {
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 touchPosition = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        touchPosition.z = centerPoint.position.z; // Keep depth identical to centerPoint for consistent 2D behavior

        // Check if mouse was pressed this frame to begin grabbed state
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Vector3.Distance(touchPosition, centerPoint.position) <= grabDistance)
            {
                isGrabbing = true;
            }
        }

        // Draw slingshot and trajectory while mouse is held
        if (Mouse.current.leftButton.isPressed && isGrabbing)
        {
            DrawSlingshot(touchPosition);
            DrawTrajectory();
        }

        // Shoot when mouse is released
        if (Mouse.current.leftButton.wasReleasedThisFrame && isGrabbing)
        {
            isGrabbing = false;
            Shoot();
            ResetSlingshot();
        }
    }

    private void DrawSlingshot(Vector3 touchPosition)
    {
        slingshotPosition = centerPoint.position + Vector3.ClampMagnitude(touchPosition - centerPoint.position, maxDistance);
        SetLines(slingshotPosition);
    }

    private void SetLines(Vector3 position)
    {
        // Make sure lines are actually active when set
        leftLineRenderer.SetPosition(0, position);
        leftLineRenderer.SetPosition(1, leftOrigin.position);
        rightLineRenderer.SetPosition(0, position);
        rightLineRenderer.SetPosition(1, rightOrigin.position);
    }

    private void ResetSlingshot()
    {
        slingshotPosition = centerPoint.position;
        SetLines(centerPoint.position);
        
        if (trajectoryLineRenderer != null)
        {
            trajectoryLineRenderer.positionCount = 0; // Hide the trajectory
        }
    }

    private void DrawTrajectory()
    {
        if (trajectoryLineRenderer == null) return;

        trajectoryLineRenderer.positionCount = trajectoryDrawSteps;

        // ===================================================================
        // CALCULUS 2 APPLICATION: Numerical Integration & Antiderivatives
        // ===================================================================
        // 
        // Standard physics kinematics uses a pre-solved quadratic equation: 
        // r(t) = r_0 + v_0 * t + 0.5 * a * t^2
        // That formula is actually just the exact double-antiderivative of constant acceleration.
        // 
        // Below, we show the underlying Calculus by using Numerical Integration (Euler's Method).
        // We approximate the integral using Riemann Sums with a step size of 'dt'.
        // 
        // If a(t) is acceleration, then:
        // Velocity   v(t) = Integral( a(t) dt )
        // Position   r(t) = Integral( v(t) dt )

        Vector2 currentPosition = slingshotPosition;
        Vector2 currentVelocity = (centerPoint.position - slingshotPosition) * shootForceMultiplier;
        Vector2 acceleration = Physics2D.gravity;
        
        // 'dt' is the differential time step (like 'dx' in an integral Riemann sum)
        float dt = trajectoryTimeStep; 

        for (int i = 0; i < trajectoryDrawSteps; i++)
        {
            // Plot the currently known position
            trajectoryLineRenderer.SetPosition(i, new Vector3(currentPosition.x, currentPosition.y, centerPoint.position.z));
            
            // -------------------------------------------------------------
            // Step 1: Derivative of Velocity is Acceleration ( dv/dt = a )
            // Therefore, integrating acceleration over dt yields the change in velocity.
            currentVelocity += acceleration * dt; 
            
            // -------------------------------------------------------------
            // Step 2: Derivative of Position is Velocity ( dr/dt = v )
            // Therefore, integrating velocity over dt yields the change in position.
            currentPosition += currentVelocity * dt;
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("No projectile prefab assigned! Please assign it in the Unity Inspector.");
            return;
        }

        // Projectile instantiation at drawn-back positioning
        GameObject projectile = Instantiate(projectilePrefab, slingshotPosition, Quaternion.identity);
        
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Apply the instantaneous calculated velocity 
            Vector2 initialVelocity = (centerPoint.position - slingshotPosition) * shootForceMultiplier;
            rb.linearVelocity = initialVelocity;
        }
        else
        {
            Debug.LogWarning("The projectile prefab requires a Rigidbody2D component to work with these Physics properties!");
        }
    }
}
