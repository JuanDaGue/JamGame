using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float jumpForce = 2.5f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform cameraTransform; 

    //private Animator animator;
    private Rigidbody rb;
    private bool isGround;
    private Vector2 moveInput;
    [SerializeField] private string takeoffStateName = "Takeoff";

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //animator = GetComponent<Animator>();
    }

    public void Jump(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed && isGround)
        {
            // animator.SetBool("isJump", true);
            // animator.CrossFadeInFixedTime(takeoffStateName, 0f);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGround = false;

        }
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
        //animator.SetFloat("speed", speed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGround = true;
            //animator.SetBool("isJump", false);
        }
    }
}
