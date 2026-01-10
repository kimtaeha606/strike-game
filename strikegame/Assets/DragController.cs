using System;
using UnityEngine;
using UnityEngine.InputSystem; // 신형 인풋 시스템 추가

public class DragController : MonoBehaviour
{
    public LineRenderer line;
    public Rigidbody2D rb;

    public float dragLimit = 3f;
    public float forceToAdd = 10f;

    private Camera cam;
    private bool isDragging;

    Vector3 MousePosition
    {
        get
        {
            // Mouse.current.position.ReadValue() 사용
            Vector3 mousePos = Mouse.current.position.ReadValue();
            Vector3 pos = cam.ScreenToWorldPoint(mousePos);
            pos.z = 0f;
            return pos;
        }
    }

    private void Start()
    {
        cam = Camera.main;
        line.positionCount = 2;
        line.enabled = false;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Mouse.current.leftButton 사용
        var leftButton = Mouse.current.leftButton;

        if (leftButton.wasPressedThisFrame && !isDragging)
        {
            DragStart();
        }

        if (isDragging)
        {
            Drag();
            
            if (leftButton.wasReleasedThisFrame)
            {
                DragEnd();
            }
        }
    }

    void DragStart()
    {
        isDragging = true;
        line.enabled = true;
        line.SetPosition(0, transform.position);
        line.SetPosition(1, transform.position);
    }

    void Drag()
    {
        Vector3 startPos = transform.position;
        line.SetPosition(0, startPos);

        Vector3 currentPos = MousePosition;
        Vector3 distance = currentPos - startPos;

        if (distance.magnitude <= dragLimit)
        {
            line.SetPosition(1, currentPos);
        }
        else
        {
            Vector3 limitVector = startPos + (distance.normalized * dragLimit);
            line.SetPosition(1, limitVector);
        }
    }

    void DragEnd()
    {
        isDragging = false;
        line.enabled = false;

        Vector3 startPos = line.GetPosition(0);
        Vector3 endPos = line.GetPosition(1);
        Vector3 dragVector = endPos - startPos;

        rb.AddForce(-dragVector * forceToAdd, ForceMode2D.Impulse);
    }
}