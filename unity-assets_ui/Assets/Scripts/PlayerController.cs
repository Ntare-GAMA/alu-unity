using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    public LayerMask groundMask;
    public float groundCheckDistance = 0.2f;

    public Vector3 startPosition;
    public float fallThreshold = -10f;
    public float respawnHeight = 15f; // how high above start the player falls from

    private Vector3 velocity;
    private bool isGrounded;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        startPosition = transform.position;
    }

    void Update()
    {
        // Check if player fell off the level
        if (transform.position.y < fallThreshold)
        {
            Respawn();
        }

        GroundCheck();

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(horizontal, 0f, vertical);

        controller.Move(move * moveSpeed * Time.deltaTime);

        HandleJump();

        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = jumpForce;
        }

        velocity.y += gravity * Time.deltaTime;
    }

    private void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);
    }

    private void Respawn()
    {
        // Spawn above the start position
        Vector3 respawnPosition = startPosition + Vector3.up * respawnHeight;
        controller.enabled = false;          // avoid CharacterController glitch
        transform.position = respawnPosition;
        controller.enabled = true;

        velocity = Vector3.zero;
    }
}
