using UnityEngine;

public class ResponsiveCamera : MonoBehaviour
{
    [SerializeField] private float _boundsExpandAmount = 1;
    
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    /*private void OnEnable()
    {
        SwipeController.OnResetRandomPyramid += SetOrthoSize;
    }

    private void OnDisable()
    {
        SwipeController.OnResetRandomPyramid -= SetOrthoSize;
    }*/

    private void Start()
    {
        var (center, size) = CalculateOrthoSize();
        _camera.transform.position = center;
        _camera.orthographicSize = size;
    }

    /*private void SetOrthoSize()
    {
        var (center, size) = CalculateOrthoSize();
        _camera.transform.position = center;
        _camera.orthographicSize = size;
        
        Debug.Log("QQQ");
    }*/

    private (Vector3 center, float size) CalculateOrthoSize()
    {
        var bounds = new Bounds();

        foreach (var col in FindObjectsOfType<Collider2D>())
        {
            bounds.Encapsulate(col.bounds);
        }

        bounds.Expand(_boundsExpandAmount);

        var vertical = bounds.size.y;
        var horizontal = bounds.size.x * _camera.pixelHeight / _camera.pixelWidth;

        var size = Mathf.Max(horizontal, vertical) * 0.5f;
        var center = bounds.center + new Vector3(0, 0, -10);

        return (center, size);
    }

}
