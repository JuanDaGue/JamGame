using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float airControlMultiplier = 0.35f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float jumpForce = 6f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.25f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall Handling")]
    [SerializeField] private float wallDotLimit = 0.5f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;

    private Rigidbody rb;
    private Vector2 moveInput;
    [SerializeField] private bool isGrounded;

    // Wall data
    private bool touchingWall;
    private Vector3 wallNormal;

    // ================= UNITY =================

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void Update()
    {
        CheckGround();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    // ================= INPUT =================

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetTrigger("Jump");
        }
    }

    // ================= MOVEMENT =================

    private void HandleMovement()
    {
        if (cameraTransform == null) return;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = (forward * moveInput.y + right * moveInput.x).normalized;

        // ⭐ PROYECCIÓN SOBRE NORMAL (FIX DEFINITIVO)
        if (!isGrounded && touchingWall)
        {
            moveDir = Vector3.ProjectOnPlane(moveDir, wallNormal);
        }

        float speed = isGrounded ? moveSpeed : moveSpeed * airControlMultiplier;

        rb.linearVelocity = new Vector3(
            moveDir.x * speed,
            rb.linearVelocity.y,
            moveDir.z * speed
        );

        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }

    // ================= GROUND =================

    private void CheckGround()
    {
        if (groundCheck == null) return;

        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundLayer
        );

        animator.SetBool("IsGrounded", isGrounded);
    }

    // ================= WALL DETECTION =================

    private void OnCollisionStay(Collision collision)
    {
        if (isGrounded) return;

        foreach (ContactPoint contact in collision.contacts)
        {
            // Si la normal no apunta hacia arriba → es pared
            if (Vector3.Dot(contact.normal, Vector3.up) < wallDotLimit)
            {
                touchingWall = true;
                wallNormal = contact.normal;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        touchingWall = false;
    }

    // ================= ANIMATION =================

    private void UpdateAnimations()
    {
        float horizontalSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        animator.SetBool("IsWalking", horizontalSpeed > 0.1f);
    }

    // ================= DEBUG =================

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}
