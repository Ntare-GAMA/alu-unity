using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public GameObject player;
    public float speed = 5f;
    public float horizontalInput;
    public float verticalInput;
    public float jumpForce = 7f;
    public bool isGrounded;
    private Rigidbody rb;
    public Vector3 startPosition;
    public float minHeight = -10f;
    public float rotationSpeed = 10f;
    private Animator animator;
    private bool isRunningSoundPlaying = false;
    private bool hasLandedInitially = true;
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        isFalling();
    }

    void MovePlayer()
    {
        horizontalInput = Input.GetAxis("Horizontal") * Time.deltaTime * speed;
        verticalInput = Input.GetAxis("Vertical") * Time.deltaTime * speed;
        Vector3 movement = new Vector3(horizontalInput, 0, verticalInput);
        if (movement != Vector3.zero && isGrounded && !animator.GetBool("Jump"))
        {
            Quaternion target = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.deltaTime);
            animator.SetBool("Run", true);

            if (!isRunningSoundPlaying)
            {
                FindObjectOfType<AudioManager>().Play("PlayerRun");
                isRunningSoundPlaying = true;
            }

        } else
        {
            animator.SetBool("Run", false);
            if (isRunningSoundPlaying)
            {
                isRunningSoundPlaying = false;
            }
        }

        transform.Translate(verticalInput, 0, -horizontalInput, Space.World);

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Jump(); 
        }
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        animator.SetBool("Jump", true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetBool("Jump", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("touchGround", true);
            if (!hasLandedInitially)
            {
                FindObjectOfType<AudioManager>().Play("PlayerLand");
                hasLandedInitially = true;
            }
            //animator.Play("Happy Idle");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
            if (rb.velocity.y < 0)
            {
                animator.SetBool("isFalling", true);
                animator.SetBool("touchGround", false);
            }
        }
    }

    private void isFalling()
    {
        if (transform.position.y < minHeight)
        {
            hasLandedInitially = false;
            animator.SetBool("isFalling", true);
            transform.position = startPosition + new Vector3(0, 10f, 0);
            rb.velocity = Vector3.zero;
        }
    }



}
