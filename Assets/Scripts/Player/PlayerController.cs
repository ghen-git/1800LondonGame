using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //player params
    float speed;
    const float jumpForce = 100f;
    const float groundSpeed = 50f;
    const float airSpeed = 1f;
    const float groundDrag = 6f;
    const float airDrag = 1f;
    const float sprintingCoeff = 2f;
    const float mouseSensitivity = 2f;
    Vector3 idealCameraOffset = new Vector3(-3, -1, 4.86f);
    const float height = 2;
    const float width = 2;

    //player states
    bool isWalking = false;
    bool isSprinting = false;
    bool isGrounded = false;
    bool isJumping = false;

    //bindings
    Rigidbody rb;
    new Transform camera;
    Vector3 movement;
    Vector3 playerRotation;
    float xRotation;
    float yRotation;

    // Start is called before the first frame update
    void Start()
    {
        //bindings
        rb = GetComponent<Rigidbody>();
        camera = GameObject.Find("PlayerCamera").GetComponent<Transform>();

        //rigidbody settings
        rb.freezeRotation = true;
        rb.mass = 40f;
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        UpdateStates();
    }

    void FixedUpdate()
    {
        UpdateForces();
        Move();
    }

    void LateUpdate()
    {
        RotatePlayer();
        PlaceCamera();
    }

    void PlaceCamera()
    {
        camera.position = transform.position - idealCameraOffset; 
        camera.LookAt(transform);
        camera.RotateAround(transform.position, Vector3.up, yRotation);
        camera.RotateAround(transform.position, camera.right, xRotation);


        RaycastHit[] hits = Physics.RaycastAll(camera.position, camera.forward, Vector3.Distance(camera.position, transform.position));
        if(hits.Length > 1)
        {
            RaycastHit hit = hits[hits.Length - 2];
            camera.position = hit.point;
        }
    }

    void RotatePlayer()
    {
        if(isWalking)
            transform.rotation = Quaternion.Euler(0, camera.rotation.eulerAngles.y, 0);
    }

    void Move()
    {
        rb.AddForce(movement.normalized * speed * (isSprinting ? sprintingCoeff : 1), ForceMode.Acceleration);

        if(isJumping)
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void UpdateStates()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, height / 2 + 0.1f);
    }

    void UpdateForces()
    {
        if(isGrounded)
        {
            rb.drag = groundDrag;
            speed = groundSpeed;
        }
        else
        {
            rb.drag = airDrag;
            speed = airSpeed;
        }
    }

    void GetInput()
    {
        //player movement
        movement = transform.forward * Input.GetAxisRaw("Vertical") + transform.right * Input.GetAxisRaw("Horizontal");

        //states
        isWalking = Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0 || Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0;

        if(Input.GetKeyDown(KeyCode.LeftShift) && isGrounded)
            isSprinting = true;

        isSprinting = isSprinting && isWalking;

        if(isJumping)
            isJumping = false;
        if(Input.GetKey(KeyCode.Space) && isGrounded)
            isJumping = true;

        
        //rotation
        yRotation += Input.GetAxis("Mouse X") * mouseSensitivity;
        xRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        yRotation = yRotation % 360;
        xRotation = xRotation % 360;
    }

}
