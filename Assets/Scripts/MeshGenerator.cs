using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class MeshGenerator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public static MeshGenerator Instance;
    public event Action<GameObject,Transform,Transform,int> MeshCreated;
    [SerializeField] private Material mat;
    [SerializeField] private float threshold;
    
    private Camera _mainCam;
    private GameObject _meshObj;
    private Mesh _mesh;
    private Coroutine _drawCoroutine;
    private Vector3 _mousePos;
    private Vector3 _worldMousePos;
    private const float OffsetVal = 0.1f;
    private bool _drawing;
    private Transform _firstPoint;
    private Transform _lastPoint;
    private readonly List<Vector3> _vertices = new List<Vector3>();
    private readonly List<int> _triangles = new List<int>();

    private readonly Vector3[] _offsets =
    {
        new Vector3(0, OffsetVal, -0.5f),
        new Vector3(0, OffsetVal, 0.5f),
        new Vector3(0, -OffsetVal, -0.5f),
        new Vector3(0, -OffsetVal, 0.5f)
    };

    private readonly int[] _indices =
    {
        -2, -6, -4,
        -6, -8, -4,
        -8, -7, -4,
        -7, -3, -4,

        -2, -1, -5,
        -6, -2, -5,
        -1, -3, -5,
        -3, -7, -5
    };

    private void Awake()
    {
        Instance = this;
        _mainCam = Camera.main;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _drawing = true;
        _vertices.Clear();
        _mousePos = Input.mousePosition;
        _worldMousePos = _mainCam.ScreenToWorldPoint(transform.InverseTransformPoint(new Vector3(_mousePos.x,
            _mousePos.y,
            _mousePos.z + 10f))) + Vector3.right * _mainCam.orthographicSize;
        _meshObj = new GameObject("Body", typeof(MeshFilter), typeof(MeshRenderer));
        _meshObj.layer = 6;
        _mesh = new Mesh();
        _meshObj.GetComponent<MeshRenderer>().material = mat;
        _meshObj.GetComponent<MeshFilter>().mesh = _mesh;
        _drawCoroutine = StartCoroutine(StartMeshDrawing(_mesh, _worldMousePos));
        _firstPoint = new GameObject().transform;
        _firstPoint.position = _worldMousePos;
        _firstPoint.SetParent(_meshObj.transform);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_drawing) return;
        _drawing = false;
        if (_drawCoroutine != null)
            StopCoroutine(_drawCoroutine);
        var col=_meshObj.AddComponent<MeshCollider>();
        col.convex = true;
        _meshObj.layer = 0;
        FixMeshPivot();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerUp(eventData);
    }

    private void FixMeshPivot()
    {
        if (_vertices.Count < 20)
        {
            Destroy(_meshObj);
            return;
        }
        var pos = Vector3.zero;
        foreach (var v in _vertices)
        {
            pos += v;
        }
        pos /= _vertices.Count;
        var parent = new GameObject("Parent").transform;
        parent.position = pos;
        _meshObj.transform.SetParent(parent);
        MeshCreated?.Invoke(parent.gameObject,_firstPoint,_lastPoint,_vertices.Count);
    }

    private IEnumerator StartMeshDrawing(Mesh drawnMesh, Vector3 mousePosition)
    {
        _vertices.Clear();
        _triangles.Clear();
        _vertices.AddRange(_offsets.Select(e => e + mousePosition));
        _vertices.AddRange(_offsets.Select(e => e + mousePosition));
        _triangles.AddRange(_indices.Select(e => e + _vertices.Count));
        _triangles.AddRange(new[]
        {
            1, 0, 3,
            0, 2, 3,
            7, 4, 5,
            7, 6, 4,
        });
        var lastMousePos = mousePosition;
        while (true)
        {
            mousePosition = _mainCam.ScreenToWorldPoint(transform.InverseTransformPoint(new Vector3(
                Input.mousePosition.x, Input.mousePosition.y,
                Input.mousePosition.z + 10f))) + Vector3.right * _mainCam.orthographicSize;
            _lastPoint = new GameObject().transform;
            _lastPoint.position = mousePosition;
            _lastPoint.SetParent(_meshObj.transform);
            var dist = (lastMousePos - mousePosition).sqrMagnitude;
            _triangles.RemoveRange(_triangles.Count - 12, 12);
            if (dist < threshold)
            {
                _vertices.RemoveRange(_vertices.Count - 4, 4);
                _vertices.AddRange(CalculateNormalizedNewVertices(mousePosition));
            }
            else
            {
                lastMousePos = mousePosition;
                _vertices.AddRange(CalculateNormalizedNewVertices(mousePosition));
                _triangles.AddRange(_indices.Select(e => e + _vertices.Count));
            }

            var lastVertices = _vertices.Count - 4;
            _triangles.AddRange(new[]
            {
                1, 0, 3,
                0, 2, 3,
                lastVertices + 3, lastVertices + 0, lastVertices + 1,
                lastVertices + 3, lastVertices + 2, lastVertices + 0,
            });
            drawnMesh.Clear();
            drawnMesh.vertices = _vertices.ToArray();
            drawnMesh.triangles = _triangles.ToArray();
            drawnMesh.Optimize();
            drawnMesh.RecalculateNormals();
            yield return null;
        }
    }

    private Vector3[] CalculateNormalizedNewVertices(Vector3 mousePosition)
    {
        var topFront = _offsets[0] + mousePosition;
        var topBack = _offsets[1] + mousePosition;
        var bottomFront = Vector3.Cross(Vector3.forward, _vertices[_vertices.Count - 4] - topFront).normalized *
            (_offsets[0] - _offsets[2]).magnitude + topFront;
        var bottomBack = Vector3.Cross(Vector3.forward, _vertices[_vertices.Count - 3] - topBack).normalized *
            (_offsets[1] - _offsets[3]).magnitude + topBack;

        return new[]
        {
            topFront,
            topBack,
            bottomFront,
            bottomBack,
        };
    }
}