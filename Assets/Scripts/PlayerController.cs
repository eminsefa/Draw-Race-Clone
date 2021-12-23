using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private MeshGenerator _meshGenerator;
    private GameObject _currentBody;
    private float _defaultMoveSpeed;
    private bool _playing;
    private Vector3 _lastCheckpoint;
    private Vector3 _lastDir;
    private Quaternion _lastRot;
    [SerializeField] private List<Transform> wheels;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask groundLayer;

    private void Start()
    {
        _meshGenerator = MeshGenerator.Instance;
        _meshGenerator.MeshCreated += MeshCreated;
        _defaultMoveSpeed = moveSpeed * 30;
    }

    private void OnDisable()
    {
        _meshGenerator.MeshCreated -= MeshCreated;
    }

    private void FixedUpdate()
    {
        if (!_playing) return;
        var leftOnGround = Physics.Raycast(wheels[0].position, -transform.up, 0.55f, groundLayer);
        var rightOnGround = Physics.Raycast(wheels[1].position, -transform.up, 0.55f, groundLayer);

        var dir = _lastDir;
        var cross = Vector3.zero;
        if (Physics.Raycast(transform.position, Vector3.down, out var hit, 3f, groundLayer))
        {
            dir = hit.transform.right;
            cross = Vector3.Cross(Vector3.up, hit.transform.up);
        }
        if (cross.magnitude == 0 && _lastDir != dir)
        {
            _lastDir = dir;
            rb.velocity = dir * rb.velocity.magnitude;
            rb.angularVelocity = Vector3.zero;
            transform.right = dir;
        }

        if (leftOnGround && rightOnGround)
        {
            rb.AddForce(dir * moveSpeed * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
        else rb.velocity = Vector3.MoveTowards(rb.velocity, Vector3.zero, Time.fixedDeltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag($"Checkpoint"))
        {
            _lastCheckpoint = other.transform.position;
            _lastRot = transform.rotation;
        }
        if (other.CompareTag($"FallTrigger"))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = _lastCheckpoint;
            SetYPos();
            transform.rotation = _lastRot;
        }
    }
    private void MeshCreated(GameObject body, Transform firstPoint, Transform lastPoint, int size)
    {
        _playing = true;
        if (_currentBody) Destroy(_currentBody);
        _currentBody = body;
        var bodyTr = body.transform;
        bodyTr.right = _lastDir;
        rb.angularVelocity = Vector3.zero;
        var cross = Vector3.Cross(rb.velocity, _lastDir);
        if (cross.x < 0) rb.velocity = Vector3.zero;
        
        transform.right = _lastDir;
        
        var firstPos = bodyTr.InverseTransformPoint(firstPoint.position);
        var lastPos = bodyTr.InverseTransformPoint(lastPoint.position);


        wheels[0].position = transform.TransformPoint(firstPos);
        wheels[1].position = transform.TransformPoint(lastPos);
        
        SetYPos();
        bodyTr.SetParent(transform);
        bodyTr.localPosition = Vector3.zero;
        moveSpeed = _defaultMoveSpeed / size;
    }

    private void SetYPos()
    {
        var offset = 0f;
        for (int i = 0; i < wheels.Count; i++)
        {
            var onGround = Physics.Raycast(wheels[0].position, Vector3.down, 1f, groundLayer);
            if (Physics.Raycast(wheels[0].position, Vector3.up, out var hit, 10f, groundLayer) && !onGround)
            {
                var dif = hit.point.y - wheels[0].position.y;
                if (dif > offset) offset = dif;
            }
        }
        var pos = transform.position;
        pos.y += offset + 1f;
        transform.position = pos;
    }

    public void LevelCompleted()
    {
        _playing = false;
        rb.isKinematic = true;
    }
}