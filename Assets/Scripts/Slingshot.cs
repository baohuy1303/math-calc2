using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Slingshot : MonoBehaviour
{
    [Header("Slingshot Components")]
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

    [Header("Visualization UI")]
    [SerializeField] private Text statsText;

    private Vector3 slingshotPosition;
    private bool isGrabbing = false;
    
    void Start()
    {
        ResetSlingshot();
        UpdateUI();
    }

    void Update()
    {
        HandleVisualizationInputs();

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
    }

    private void ResetSlingshot()
    {
        slingshotPosition = centerPoint.position;
        
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
        
        // center - slingshot gives us the direction of the initial velocity, and the magnitude is determined by the shootForceMultiplier
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

    private void HandleVisualizationInputs()
    {
        if (Keyboard.current == null) return;

        bool changed = false;

        // A/D controls time step (dt)
        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            trajectoryTimeStep = Mathf.Clamp(trajectoryTimeStep - 0.01f, 0.01f, 0.5f);
            changed = true;
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            trajectoryTimeStep = Mathf.Clamp(trajectoryTimeStep + 0.01f, 0.01f, 0.5f);
            changed = true;
        }

        // Q/E controls number of iterations (n)
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            trajectoryDrawSteps = Mathf.Clamp(trajectoryDrawSteps - 5, 5, 200);
            changed = true;
        }
        else if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            trajectoryDrawSteps = Mathf.Clamp(trajectoryDrawSteps + 5, 5, 200);
            changed = true;
        }

        if (changed)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (statsText != null)
        {
            statsText.text = $"[A/D] Time Step (dt): {trajectoryTimeStep:F2}\n" + 
                             $"[Q/E] Iterations (n): {trajectoryDrawSteps}\n\n" +
                             $"* The line is a manual Riemann Sum approximation.\n" + 
                             $"* The bullet uses continuous physics engine integration.\n" +
                             $"* Increase dt to see Estimation Error diverge!";
        }
    }
}
