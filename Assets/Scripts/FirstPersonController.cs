using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;
    public float groundedCheckRadius = 0.3f;
    public LayerMask groundMask = ~0; // set to your Ground layer if you have one
    public Transform groundCheck;     // optional: assign a child at feet; else auto

    [Header("Mouse Look")]
    public Transform cameraPivot;
    public float mouseSensitivity = 120f;
    public float minPitch = -85f;
    public float maxPitch = 85f;

    CharacterController cc;
    float pitch;
    float vSpeed;         // vertical velocity (m/s)
    bool grounded;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!groundCheck)
        {
            // auto-create a feet check if none assigned
            var t = new GameObject("GroundCheck").transform;
            t.SetParent(transform);
            t.localPosition = new Vector3(0, -cc.height * 0.5f + 0.05f, 0);
            groundCheck = t;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // --- Mouse look ---
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up, mx);
        pitch = Mathf.Clamp(pitch - my, minPitch, maxPitch);
        if (cameraPivot) cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // --- Ground check (more reliable than CharacterController.isGrounded alone) ---
        grounded = Physics.CheckSphere(groundCheck.position, groundedCheckRadius, groundMask, QueryTriggerInteraction.Ignore);
        if (grounded && vSpeed < 0f) vSpeed = -2f; // small stick-to-ground force

        // --- Move (WASD) ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 move = (transform.right * h + transform.forward * v).normalized;
        cc.Move(move * moveSpeed * Time.deltaTime);

        // --- Jump (works with legacy Input OR direct key) ---
        bool jumpPressed = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);
        if (jumpPressed && grounded)
            vSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // --- Gravity ---
        vSpeed += gravity * Time.deltaTime;
        cc.Move(new Vector3(0f, vSpeed, 0f) * Time.deltaTime);

        // Optional: unlock cursor with Esc
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Helps visualize the ground check sphere
    void OnDrawGizmosSelected()
    {
        if (!cc) cc = GetComponent<CharacterController>();
        var pos = groundCheck ? groundCheck.position : transform.position + Vector3.down * (cc ? (cc.height * 0.5f - 0.05f) : 1f);
        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(pos, groundedCheckRadius);
    }
}
