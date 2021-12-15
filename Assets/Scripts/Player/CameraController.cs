using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 1f;
    [SerializeField] private float wheelSpeed = 1f;

    private Camera _camera;
    private Transform _transform;

    private void Start()
    {
        _transform = GetComponent<Transform>();
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        UpdateCameraInput();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GameManager.Instance.GameMode = GameMode.DefaultMode;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GameManager.Instance.GameMode = GameMode.ConstructionMode;
        }

        GameManager.Instance.InputReceiver.UpdateMousePosition(_camera);

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            GameManager.Instance.InputReceiver.OnLeftMouseClicked(_camera);
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            GameManager.Instance.InputReceiver.OnRightMouseClicked(_camera);
        }
    }

    private void UpdateCameraInput()
    {
        var frameSpeed = movementSpeed * Time.deltaTime;
        var mouseWheel = -Input.GetAxis("Mouse ScrollWheel") * wheelSpeed;

        _transform.position +=
            new Vector3
            (
                frameSpeed * Input.GetAxisRaw("Horizontal"),
                mouseWheel,
                frameSpeed * Input.GetAxisRaw("Vertical")
            );
    }
}