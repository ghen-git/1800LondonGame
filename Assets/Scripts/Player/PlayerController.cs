using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //player params
    float speed;
    const float jumpForce = 1000f;
    const float groundSpeed = 50f;
    const float airSpeed = 1f;
    const float groundDrag = 6f;
    const float airDrag = 1f;
    const float sprintingCoeff = 2f;
    const float mouseSensitivity = 2f;
    const float clippingFixAmount = 1f;
    Vector3 cameraOffset = new Vector3(-1.5f, -0.5f, 1.5f);
    const float height = 2.05f;
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
    Vector3 idealCameraPos;
    Vector3 camSmoothVelocity;

    List<Vector3> rayPos;

    // Start is called before the first frame update
    void Start()
    {
        //bindings
        rb = GetComponent<Rigidbody>();
        camera = GameObject.Find("PlayerCamera").GetComponent<Transform>();

        //rigidbody settings
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.mass = 500;
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
        PlaceCamera();
    }

    void PlaceCamera()
    {
        Vector3 lastCameraPos = camera.position;

        camera.position = transform.position - cameraOffset; 
        camera.LookAt(transform);
        camera.RotateAround(transform.position, Vector3.up, yRotation);
        camera.RotateAround(transform.position, camera.right, xRotation);
        
        RaycastHit hit;
        if(Physics.Raycast(transform.position, -camera.forward, out hit, Vector3.Distance(camera.position, transform.position) - width / 2 + clippingFixAmount))
        {
            camera.position = hit.point + (Vector3.up + camera.forward) * clippingFixAmount / Vector3.Distance(camera.position, transform.position);
        }

        idealCameraPos = camera.position;
        camera.position = Vector3.SmoothDamp(lastCameraPos, idealCameraPos, ref camSmoothVelocity, 0.005f);
    }

    void Move()
    {
        if(isWalking)
            rb.rotation = Quaternion.Euler(0, camera.rotation.eulerAngles.y, 0);
            
        rb.AddForce(movement.normalized * speed * (isSprinting ? sprintingCoeff : 1), ForceMode.Acceleration);

        //more gravity since it doesnt work for some reason
        if(!isGrounded && rb.velocity.y < 0)
            rb.AddForce(Vector3.down * jumpForce * 10);

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

        if(Input.GetKeyDown(KeyCode.Z))
            rb.AddForce(Vector3.up * jumpForce * 1000);

        isSprinting = isSprinting && isWalking;

        if(isJumping)
            isJumping = false;
        if(Input.GetKey(KeyCode.Space) && isGrounded)
            isJumping = true;

        
        //rotation
        yRotation += Input.GetAxis("Mouse X") * mouseSensitivity;
        xRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        yRotation = yRotation % 360;
        xRotation = Mathf.Clamp(xRotation, -70, 70);
    }
}