using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class DrawScript : MonoBehaviour
{
    [SerializeField]
    private SpriteShapeController controller;
    [SerializeField]
    private float magnitude;
    [SerializeField]
    private float height = 0.1f;
    [SerializeField]
    private Camera overlay;
    [SerializeField]
    private List<GameObject> destroy;
    
    private int index;
    private bool isDrawing;

    public static event Action<Vector2[]> GetPoints;

    private Vector3 minCorner;
    private Vector3 maxCorner;
    private Vector3 Range => maxCorner - minCorner;

    private void Start()
    {
        var t = GetComponent<RectTransform>();
        var v = new Vector3[4];
        t.GetWorldCorners(v);
        minCorner = v[0];
        maxCorner = v[2];
    }
    
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Mouse0))
            StopDraw();
        if (Input.GetKeyDown(KeyCode.Mouse0) && EventSystem.current.IsPointerOverGameObject())
            StartDraw();
        if (Input.GetKey(KeyCode.Mouse0) && EventSystem.current.IsPointerOverGameObject())
            Move();
    }

    private void StopDraw()
    {
        isDrawing = false;
        controller.gameObject.SetActive(false);
        var points = Enumerable.Range(0, controller.spline.GetPointCount())
            .Select(i => controller.spline.GetPosition(i))
            .Select(point => point - minCorner - Range / 2)
            .Select(point => new Vector2(point.x / Range.x, point.y / Range.y) * 2).ToArray();
        GetPoints?.Invoke(points);
        destroy.ForEach(Destroy);
    }

    private void StartDraw()
    {
        if (!Input.GetKeyDown(KeyCode.Mouse0))
            return;
        isDrawing = true;
        controller.spline.Clear();
        index = -1;
        var point = GetPoint();
        AddPoint(point);
    }

    private void Move()
    {
        if (!isDrawing) return;
        var point = GetPoint();
        if (((Vector2)controller.spline.GetPosition(index) - point).magnitude > magnitude)
        {
            AddPoint(point);
            controller.gameObject.SetActive(true);
        }
    }

    private void AddPoint(Vector3 point)
    {
        try
        {
            controller.spline.InsertPointAt(++index, point);
            controller.spline.SetHeight(index, height);
            controller.spline.SetTangentMode(index, ShapeTangentMode.Continuous);
            controller.BakeMesh();
        }
        catch
        {
            index--;
        }
    }

    private Vector2 GetPoint()
    {
        return overlay.ScreenToWorldPoint(Input.mousePosition);
    }
}
