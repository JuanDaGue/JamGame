using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [SerializeField] private float jumpForce = 2.5f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform cameraTransform;

    [SerializeField] private Animator animator;
    private Rigidbody rb;
    [SerializeField] private bool isGround = false;
    private Vector2 moveInput;

    private float idleTimer = 0f;
    [SerializeField] private string takeoffStateName = "Takeoff";

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 direction = forward * moveInput.y + right * moveInput.x;
        Vector3 moveVelocity = direction * moveSpeed;
        rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.15f);
        }

        float speed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        animator.SetBool("IsWalking", speed > 0.1f);
    }

    void Update()
    {
        // Contador de inactividad para bailar
        if (moveInput == Vector2.zero && isGround)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= 15f)
            {
                animator.SetBool("IsDancing", true);
            }
        }
        else
        {
            idleTimer = 0f;
            animator.SetBool("IsDancing", false);
        }

        // Tecla R para bailar manualmente
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            animator.SetBool("IsDancing", true);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGround = true;
            animator.SetBool("IsGrounded", isGround);
            Debug.Log("Grounded: " + isGround);
        }
    }

    public void Jump(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed && isGround)
        {
            animator.SetTrigger("Jump");
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGround = false;
            animator.SetBool("IsGrounded", isGround);
            Debug.Log("Jump Triggered, Grounded: " + isGround);
        }
    }
}