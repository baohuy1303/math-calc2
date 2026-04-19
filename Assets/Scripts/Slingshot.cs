using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class Slingshot : MonoBehaviour
{
    [SerializeField] private LineRenderer leftLineRenderer;
    [SerializeField] private LineRenderer rightLineRenderer;

    [SerializeField] private Transform leftOrigin;
    [SerializeField] private Transform rightOrigin;

    [SerializeField] private Transform centerPoint;

    [SerializeField] private float maxDistance = 3f;

    private Vector3 slingshotPosition;
    
    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            DrawSlingshot();
        }
    }

    private void DrawSlingshot()
    {
        Vector3 touchPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        slingshotPosition = centerPoint.position + Vector3.ClampMagnitude(touchPosition - centerPoint.position, maxDistance);

        SetLines(slingshotPosition);
    }

    private void SetLines(Vector3 slingshotPosition)
    {
        leftLineRenderer.SetPosition(0, slingshotPosition);
        leftLineRenderer.SetPosition(1, leftOrigin.position);
        rightLineRenderer.SetPosition(0, slingshotPosition);
        rightLineRenderer.SetPosition(1, rightOrigin.position);
    }
}
