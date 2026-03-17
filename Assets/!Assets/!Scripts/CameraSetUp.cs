using UnityEngine;

public class CameraSetUp : MonoBehaviour
{
    private void Start()
    {
        Camera _camera = FindFirstObjectByType<Camera>();
        GetComponent<Canvas>().worldCamera = _camera;
    }
}
