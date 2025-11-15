using UnityEngine;

public class GunRay : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private Transform _centre;
    [SerializeField] private Transform _point;
    [SerializeField] private float _pointSize;
    private Transform _camera;

    private void Awake() {
        _camera = Camera.main.transform;
    }
    void Update()
    {
        Ray ray = new Ray(_centre.position, _centre.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 50f, _layerMask, QueryTriggerInteraction.Ignore)) {
            _centre.localScale = new Vector3(1, 1, hit.distance);
            _point.position = hit.point;
            float distance = Vector3.Distance(_camera.position, hit.point);
            _point.localScale = Vector3.one * distance * _pointSize;
        }
    }
}
