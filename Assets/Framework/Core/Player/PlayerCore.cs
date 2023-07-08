using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerCore : MonoBehaviour
{
    // Camera
    [field: Header("Camera")]
    [field: SerializeField] public Camera cam { private set; get; }
    [SerializeField] private float cameraLerpConst;
    
    // Input
    [field: Header("Input")]
    [field: SerializeField] public InputActionAsset inputAsset { private set; get; }
    private InputAction _directionalInputAction;
    private bool directionalInputActive;
    public Vector2 directionalInput { private set; get; }

    [field: Header("Movement")]
    [field: SerializeField] public Rigidbody2D rb { private set; get; }
    [SerializeField] private float movementSpeed;
    
    [Header("Misc")]
    private Room currentRoom;
    private int currentFloor;

    public void Awake()
    {
        _directionalInputAction = inputAsset.FindAction("Directional");
    }

    private void UpdateInput()
    {
        directionalInputActive = _directionalInputAction.inProgress;
        directionalInput = _directionalInputAction.ReadValue<Vector2>();
    }

    private void UpdateCamera()
    {
        Vector3 targetPos = (currentRoom == null || true) ? transform.position : currentRoom.bounds.center;
        cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, 1 - Mathf.Pow(cameraLerpConst, Time.deltaTime));
    }

    private void UpdateMovement()
    {
        if (directionalInputActive)
        {
            rb.velocity = directionalInput * movementSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }
    
    public void Update()
    {
        UpdateInput();
        var pos = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        currentRoom = GameMan.inst.map.GetFloor(currentFloor).GetRoom(pos);
        
        // Heat current tile
        Debug.Log(pos);
        GameMan.inst.map.GetFloor(currentFloor).HeatTile((Vector3Int)pos, Time.deltaTime * 10f);
    }

    public void LateUpdate()
    {
        UpdateCamera();
    }

    public void FixedUpdate()
    {
        UpdateMovement();
    }
}
