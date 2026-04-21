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

    private int defaultDrawSteps;
    private float defaultTimeStep;
    private float holdTimer = 0f;

    [Header("Visualization UI")]
    [SerializeField] private Text statsText;
    [SerializeField] private SpriteRenderer hoverZoneSprite;
    [SerializeField] private Color idleHoverColor = new Color(1f, 1f, 1f, 0.1f);
    [SerializeField] private Color activeHoverColor = new Color(0f, 1f, 1f, 0.4f);
    [SerializeField] private Color draggingColor = new Color(1f, 0.5f, 0f, 0.6f);

    [Header("Character Animation")]
    [SerializeField] private SpriteRenderer characterSpriteRenderer;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite hitSprite;
    [SerializeField] private Sprite[] dragSprites = new Sprite[3]; // drag1, drag2, drag3

    private Vector3 slingshotPosition;
    private bool isGrabbing = false;
    private Coroutine fadeRoutine;
    private Coroutine hitFlashRoutine;
    
    void Start()
    {
        defaultDrawSteps = trajectoryDrawSteps;
        defaultTimeStep = trajectoryTimeStep;
        ResetSlingshot(true);
        UpdateUI();
        SetupLineGradient();
    }

    private void SetupLineGradient()
    {
        if (trajectoryLineRenderer == null) return;
        
        Color startColor = Color.white;
        Color endColor = Color.white;
        ColorUtility.TryParseHtmlString("#FFC077", out startColor);
        ColorUtility.TryParseHtmlString("#FF8606", out endColor);

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(startColor, 0.0f), 
                new GradientColorKey(endColor, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(1.0f, 0.15f), 
                new GradientAlphaKey(0.0f, 0.20f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        trajectoryLineRenderer.colorGradient = gradient;
    }

    void Update()
    {
        HandleVisualizationInputs();

        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 touchPosition = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        touchPosition.z = centerPoint.position.z; // Keep depth identical to centerPoint for consistent 2D behavior

        UpdateHoverUX(touchPosition);

        // Check if mouse was pressed this frame to begin grabbed state
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Vector3.Distance(touchPosition, centerPoint.position) <= grabDistance)
            {
                isGrabbing = true;
                if (fadeRoutine != null) StopCoroutine(fadeRoutine);
                SetupLineGradient(); // Reset to normal colors
            }
        }

        // Draw slingshot and trajectory while mouse is held
        if (Mouse.current.leftButton.isPressed && isGrabbing)
        {
            DrawSlingshot(touchPosition);
            DrawTrajectory();
            UpdateCharacterSprite();
        }

        // Shoot when mouse is released
        if (Mouse.current.leftButton.wasReleasedThisFrame && isGrabbing)
        {
            isGrabbing = false;
            Shoot();
            ResetSlingshot(false); // Don't hide line yet
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(KeepLineVisibleRoutine());
            if (hitFlashRoutine != null) StopCoroutine(hitFlashRoutine);
            hitFlashRoutine = StartCoroutine(HitFlashRoutine());
        }

        if (!isGrabbing)
            SetCharacterSprite(idleSprite);
    }

    private void UpdateHoverUX(Vector3 touchPosition)
    {
        if (hoverZoneSprite == null) return;
        
        Color targetColor = idleHoverColor;
        
        if (isGrabbing)
        {
            targetColor = draggingColor;
        }
        else if (Vector3.Distance(touchPosition, centerPoint.position) <= grabDistance)
        {
            // Mouse is waiting inside the circle
            targetColor = activeHoverColor;
        }
        
        // Smoothly shift to the new color state
        hoverZoneSprite.color = Color.Lerp(hoverZoneSprite.color, targetColor, Time.deltaTime * 15f);
    }

    private void SetCharacterSprite(Sprite sprite)
    {
        if (characterSpriteRenderer != null && sprite != null)
            characterSpriteRenderer.sprite = sprite;
    }

    private void UpdateCharacterSprite()
    {
        if (dragSprites.Length < 3) return;

        float pull = Vector3.Distance(slingshotPosition, centerPoint.position);
        float t = pull / maxDistance; // 0..1

        Sprite target = t < 0.05f ? dragSprites[0]
                      : t < 0.3f ? dragSprites[1]
                                  : dragSprites[2];
        SetCharacterSprite(target);
    }

    private IEnumerator HitFlashRoutine()
    {
        SetCharacterSprite(hitSprite);
        yield return null; // wait 1 frame
        yield return null; // wait 2 frames
        SetCharacterSprite(idleSprite);
    }

    private void DrawSlingshot(Vector3 touchPosition)
    {
        slingshotPosition = centerPoint.position + Vector3.ClampMagnitude(touchPosition - centerPoint.position, maxDistance);
    }

    private void ResetSlingshot(bool clearLine = true)
    {
        slingshotPosition = centerPoint.position;
        
        if (clearLine && trajectoryLineRenderer != null)
        {
            trajectoryLineRenderer.positionCount = 0; // Hide the trajectory
        }
    }

    private IEnumerator KeepLineVisibleRoutine()
    {
        if (trajectoryLineRenderer != null)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.cyan, 0.0f), new GradientColorKey(Color.blue, 1.0f) },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(0.8f, 0.0f), 
                    new GradientAlphaKey(0.8f, 0.15f), 
                    new GradientAlphaKey(0.0f, 0.20f), 
                    new GradientAlphaKey(0.0f, 1.0f) 
                }
            );
            trajectoryLineRenderer.colorGradient = gradient;
        }

        yield return new WaitForSeconds(3f);

        if (trajectoryLineRenderer != null)
        {
            trajectoryLineRenderer.positionCount = 0;
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

        Vector2 currentPosition = centerPoint.position;
        
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
        /* if (projectilePrefab == null)
        {
            Debug.LogWarning("No projectile prefab assigned! Please assign it in the Unity Inspector.");
            return;
        }

        // Projectile instantiation exactly at the center point character
        GameObject projectile = Instantiate(projectilePrefab, centerPoint.position, Quaternion.identity);
        
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
        } */

        if (projectilePrefab == null) return;
        GameObject projectile = Instantiate(projectilePrefab, centerPoint.position, Quaternion.identity);
        
        // 1. Calculate the starting velocity
        Vector2 initialVelocity = (centerPoint.position - slingshotPosition) * shootForceMultiplier;
        // 2. Add our manual calculation script
        NumericalProjectile manualMove = projectile.AddComponent<NumericalProjectile>();
        
        // 3. Pass the current 'dt' from your UI to the projectile
        manualMove.velocity = initialVelocity;
        manualMove.dt = trajectoryTimeStep;
    }

    private void HandleVisualizationInputs()
    {
        if (Keyboard.current == null) return;
        bool changed = false;

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            trajectoryDrawSteps = defaultDrawSteps;
            trajectoryTimeStep = defaultTimeStep;
            changed = true;
        }

        holdTimer -= Time.deltaTime;
        
        bool anyKeyHeld = Keyboard.current.aKey.isPressed || Keyboard.current.dKey.isPressed ||
                          Keyboard.current.qKey.isPressed || Keyboard.current.eKey.isPressed;
                          
        bool tapThisFrame = Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame ||
                            Keyboard.current.qKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame;

        if (tapThisFrame || (anyKeyHeld && holdTimer <= 0f))
        {
            if (Keyboard.current.aKey.isPressed)
                trajectoryTimeStep = Mathf.Clamp(trajectoryTimeStep - 0.01f, 0.01f, 0.5f);
            if (Keyboard.current.dKey.isPressed)
                trajectoryTimeStep = Mathf.Clamp(trajectoryTimeStep + 0.01f, 0.01f, 0.5f);

            if (Keyboard.current.qKey.isPressed)
                trajectoryDrawSteps = Mathf.Clamp(trajectoryDrawSteps - 1, 5, 200);
            if (Keyboard.current.eKey.isPressed)
                trajectoryDrawSteps = Mathf.Clamp(trajectoryDrawSteps + 1, 5, 200);

            changed = true;
            holdTimer = tapThisFrame ? 0.3f : 0.03f; 
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
                             $"* The object and line is a manual Riemann Sum approximation.\n" +
                             $"* Increase dt to see Estimation Error diverge!" +
                             "\n[R] Reset";
        }
    }
}
