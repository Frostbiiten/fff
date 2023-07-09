using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerCore : MonoBehaviour
{
    public static PlayerCore inst;
    
    // Camera
    [field: Header("Camera")]
    [field: SerializeField] public Camera cam { private set; get; }
    [SerializeField] private float cameraLerpConst;
    [SerializeField] private float mouseCamOffset;
    
    // Input
    [field: Header("Input")]
    [field: SerializeField] public InputActionAsset inputAsset { private set; get; }
    private InputAction _directionalInputAction;
    private InputAction _sprayInputAction;
    private bool directionalInputActive;
    public Vector2 directionalInput { private set; get; }

    [field: Header("Movement")]
    [field: SerializeField] public Rigidbody2D rb { private set; get; }
    [SerializeField] private float movementSpeed;
    
    [field: Header("Misc")]
    public Room currentRoom { private set; get; }
    private int currentFloor;

    [Header("Mechanic")]
    [SerializeField] private Collider2D burnCollider; 
    [SerializeField] private Transform burnPoint; 
    [SerializeField] private float burnRadius; 
    [SerializeField] private float heatSpeed; 

    [Header("Visual")]
    [SerializeField] private int ropePoints = 5;
    [SerializeField] private float maxRopeDist = 500;
    [SerializeField] private float ropeHangMult = 0.01f;
    [SerializeField] private LineRenderer ropeRenderer;
    [SerializeField] private Transform particlesRef;
    [SerializeField] private Transform nozzleRef;
    [SerializeField] private float particlesRefLerp;
    [SerializeField] private Transform mouseTransform;
    [SerializeField] private Transform skinTransform;
    [SerializeField] private Vector3 nozzleOffset;
    [SerializeField] private ParticleSystem sprayParticles;
    [SerializeField] private Animator extinguisherAnimator;
    [SerializeField] private float mouseDistOffset;

    public void Awake()
    {
        if (inst != null) Debug.Break();
        inst = this;
        
        _directionalInputAction = inputAsset.FindAction("Directional");
        _sprayInputAction = inputAsset.FindAction("Spray");
        ropeRenderer.positionCount = ropePoints;
    }

    private void UpdateInput()
    {
        directionalInputActive = _directionalInputAction.inProgress;
        directionalInput = _directionalInputAction.ReadValue<Vector2>();
    }

    private void UpdateCamera()
    {
        Vector3 targetPos = (currentRoom == null || true) ? transform.position : currentRoom.bounds.center;
        targetPos += (cam.ScreenToViewportPoint(Mouse.current.position.value) - Vector3.one / 2f) * mouseCamOffset;
        cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, Time.deltaTime * cameraLerpConst); // lerp w/ deltatime is fine here
        particlesRef.position = Vector3.Lerp(particlesRef.position, transform.position, Time.deltaTime * particlesRefLerp);
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
        extinguisherAnimator.SetFloat("Speed", rb.velocity.magnitude);
    }
    
    public void Update()
    {
        UpdateInput();
        var pos = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        currentRoom = GameMan.inst.map.GetFloor(currentFloor).GetRoom(pos);

        Vector3 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.value) - nozzleRef.position;
        float mouseDist = Mathf.Max(0f, mousePos.magnitude - mouseDistOffset);
        mousePos /= mouseDist;
        
        mouseDist = Mathf.Min(mouseDist, maxRopeDist);
        mousePos *= mouseDist;

        mouseTransform.localPosition = mousePos;
        
        for (int i = 0; i < ropePoints; ++i)
        {
            //Vector3 offset = Vector3.down * i * (ropePoints - 1 - i) * Mathf.Pow(2f, maxRopeDist - mouseDist) * ropeHangMult;
            Vector3 offset = Vector3.down * i * (ropePoints - 1 - i) * Mathf.Pow(maxRopeDist - mouseDist + 0.2f, 1.1f) * ropeHangMult;
            ropeRenderer.SetPosition(i, nozzleRef.position + mousePos * (i / ((float)ropePoints - 1)) + offset + nozzleOffset);
        }

        mousePos = cam.ScreenToWorldPoint(Mouse.current.position.value) - transform.position;
        mousePos.z = 0f;
        mousePos.Normalize();
        if (mousePos.x > 0f)
        {
            skinTransform.right = mousePos;
            skinTransform.localScale = Vector2.one;
        }
        else
        {
            skinTransform.right = -mousePos;
            skinTransform.localScale = new Vector2(-1f, 1f);
        }

        // Heat Spray
        //GameMan.inst.map.GetFloor(currentFloor).HeatTile((Vector3Int)pos, 1f);
        if (_sprayInputAction.inProgress)
        {
            var floor = GameMan.inst.map.GetFloor(currentFloor);
            for (int x = (int)(burnPoint.position.x - burnRadius); x <= burnPoint.position.x + burnRadius; x++)
            {
                for (int y = (int)(burnPoint.position.y - burnRadius); y <= burnPoint.position.y + burnRadius; y++)
                {
                    Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                    float delta = (burnRadius - (burnPoint.position - (Vector3)p).magnitude) * Time.deltaTime * heatSpeed;
                    if (delta > 0f) floor.HeatTile(new Vector3Int(x, y), delta);
                }
            }
        }
        
        // Visual
        if (sprayParticles.isPlaying != _sprayInputAction.inProgress)
        {
            if (_sprayInputAction.inProgress) sprayParticles.Play();
            else sprayParticles.Stop();
        }
        
        skinTransform.up = Vector3.Lerp(skinTransform.up, Vector3.up, 0.6f);
        mouseTransform.right = mousePos;
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
