using System;
using UnityEngine;
using UnityEngine.InputSystem; // 신형 인풋 시스템 추가

public class DragController : MonoBehaviour
{
    public LineRenderer line;
    public Rigidbody2D rb;

    public float dragLimit = 3f;
    public float forceToAdd = 10f;
    [SerializeField] private GoalGlove glove;
    [SerializeField] private TrajectoryLine trajectoryLine;

    public GoalGlove Glove
    {
        get => glove;
        set => glove = value;
    }
    [SerializeField] private Rigidbody2D ballRb;
    [SerializeField] private Collider2D ballCol;

    private Camera cam;
    private bool isDragging;
    private bool inputEnabled = true;
    private Vector3 dragStartPos;
    private Vector3 lastDragVector;
    
    public void EnableInput(bool enable)
    {
        inputEnabled = enable;

        if (!enable)
        {
            isDragging = false;
            if (line != null) line.enabled = false;
            if (trajectoryLine != null) trajectoryLine.Hide();
        }
    }


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
        if (line != null)
        {
            line.positionCount = 2;
            line.enabled = false;
        }

        if (rb == null) 
            rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!inputEnabled) return;
        if (Mouse.current == null) return;

        var leftButton = Mouse.current.leftButton;

        if (leftButton.wasPressedThisFrame && !isDragging)
            DragStart();

        if (!isDragging) return;

        Drag();

        if (leftButton.wasReleasedThisFrame)
            DragEnd();
    }

    private void DragStart()
    {
        if (glove == null || rb == null) return;

        isDragging = true;
        dragStartPos = rb != null ? (Vector3)rb.position : transform.position;
        lastDragVector = Vector3.zero;

        if (line != null)
        {
            line.enabled = true;
            if (line.positionCount < 2) line.positionCount = 2;
            line.SetPosition(0, dragStartPos);
            line.SetPosition(1, dragStartPos);
        }
    }

    private void Drag()
    {
            Vector3 startPos = rb != null ? (Vector3)rb.position : dragStartPos;

            if (line != null)
            {
                if (line.positionCount < 2) line.positionCount = 2;
                line.SetPosition(0, startPos);
            }

            Vector3 currentPos = MousePosition;
            Vector3 distance = currentPos - startPos;

            Vector3 endPos;
            if (distance.magnitude <= dragLimit)
                endPos = currentPos;
            else
                endPos = startPos + (distance.normalized * dragLimit);

            if (line != null)
            {
                if (line.positionCount < 2) line.positionCount = 2;
                line.SetPosition(1, endPos);
            }

            lastDragVector = endPos - startPos;
            if (trajectoryLine != null)
            {
                Vector2 v0 = -(Vector2)lastDragVector * forceToAdd / rb.mass;
                trajectoryLine.Show(startPos, v0);
            }
        }

    private void DragEnd()
    {
            isDragging = false;
            if (line != null) line.enabled = false;
            if (trajectoryLine != null) trajectoryLine.Hide();

            if (glove == null || rb == null) return;

            // (중요) 이 샷에서만 점수 1회 되도록 GoalGlove 쪽을 "무장"
            glove.ArmShot(ballCol);

            // 발사 벡터 계산
            Vector3 dragVector = lastDragVector;

            // 발사 전 물리 리셋(이전 속도 누적 방지)
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // 던지는 방향: 당긴 반대 방향
            rb.AddForce(-(Vector2)dragVector * forceToAdd, ForceMode2D.Impulse);
        }
}

