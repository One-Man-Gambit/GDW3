using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2 : MonoBehaviour
{
    /*
        NOTE TO SELF
        Need to find a way to stop rigid velocity from being canceled/negated upon landing from a jump or fall
    */

    [Header("Components")]
    public Camera cam;
    public GameObject camera_pivot;
    private Rigidbody rigid;    
    

    [Header("Camera")]
    public float f_MouseSensitivity = 1.0f;
    public bool b_InvertMouse = false;
    public Vector2 hCameraClamp = new Vector2(-60, 30);
    public Vector2 FovRange = new Vector2(60, 90);

    [Header("Motion")]
    public float RunSpeed = 5.0f;
    public float MaxSpeed = 10.0f;
    public float BrakeSpeed = 0.1f;
    public float JumpForce = 1.0f;
    public float MaxFallSpeed = 20.0f;
    public bool IsJumping = false;
    public bool IsGrounded = false;
    public bool GroundCheck = true;
    public float f_AirTime = 0.0f;  // Track how long the player remains airborne.

    [Header("Interactions")]
    public float InteractionDistance = 5.0f;
    public Interactable targetInteractable;
     

    [Header("Debugging")]
    public bool DebugInteractionRadius = false;
    public Vector3 Velocity;    
    public float VelocityMagnitude;
    public float CurrentSpeedRatio;

    private void Awake() 
    {
        rigid = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() 
    {
        Camera();
        HorizontalMotion();
        VerticalMotion();
        Interactables();
    }

    private void Camera()
    {
        // Dynamically adjust FoV based on current speed relative to maximum speed, [60 - 90 FoV]
        // When at 0 magnitude, FoV 60
        // When at MaxSpeed magnitude, FOV 90
        float currentSpeedRatio = VelocityMagnitude / MaxSpeed;
        float resultingFoV = FovRange.x + currentSpeedRatio * (FovRange.y - FovRange.x);           // Starting at 60, add on the ratio multiplied by the different between min and max FOV 
        cam.fieldOfView = Mathf.Clamp(resultingFoV, FovRange.x, FovRange.y);
        
        // Get the mouse input for horizontal and vertical axis and store as a float variable
        float cameraX = Input.GetAxis("Mouse Y") * f_MouseSensitivity * ((b_InvertMouse) ? 1 : -1);
        float cameraY = Input.GetAxis("Mouse X") * f_MouseSensitivity;

        // Rotate the camera relative to the mouse input
        camera_pivot.transform.Rotate(cameraX, cameraY, 0);

        // Set the local Euler Angles and clamp the angles to prevent weird camera movements        
        camera_pivot.transform.localEulerAngles = new Vector3(ClampAngle(camera_pivot.transform.localEulerAngles.x, hCameraClamp.x, hCameraClamp.y), camera_pivot.transform.localEulerAngles.y, 0);   
    }

    private void VerticalMotion()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !IsJumping) 
        {
            rigid.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
            IsJumping = true;
            IsGrounded = false;
            StartCoroutine(JumpDelay());
        }

        // Ground Check
        if (GroundCheck) 
        {
            int layerMask = 1 << 6; // Ground Layer
            if (Physics.Raycast(rigid.transform.position, transform.TransformDirection(-Vector3.up), out RaycastHit hit, 0.5f, layerMask))
            {
                IsGrounded = true;
                IsJumping = false;                            
                f_AirTime = 0.0f;            
            } else {
                IsGrounded = false;
            }
        }
        

        if (!IsGrounded) {
            f_AirTime += Time.deltaTime;
        }
    }

    private IEnumerator JumpDelay() 
    {
        GroundCheck = false;
        yield return new WaitForSeconds(0.5f);
        GroundCheck = true;
    }

    private void Interactables()
    {
        // Run a spherical scan for all interactables within the players vicinity                
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, InteractionDistance);
        foreach (var hit in hitColliders) {
            if (hit.gameObject.tag == "Interactable") 
            {   
                // If no interactable has been set yet, set the first one we find
                if (targetInteractable == null) targetInteractable = hit.gameObject.GetComponent<Interactable>();
                else 
                {
                    // Get distance from player to interactable and compare with current target interactable
                    float dist_target = Vector3.Distance(transform.position, targetInteractable.gameObject.transform.position);
                    float dist_compare = Vector3.Distance(transform.position, hit.gameObject.transform.position);

                    // If the new target is closer than the old, replace it
                    if (dist_compare < dist_target) {
                        targetInteractable = hit.gameObject.GetComponent<Interactable>();
                    }
                }
            }
        }  

        // Interact with target interactable, if one exists
        if (targetInteractable != null && Input.GetKeyDown(KeyCode.E)) {
            targetInteractable.Interact();
        }
    }

    private void HorizontalMotion() 
    {
        // Get the Input Value for Movement along the X and Z axis
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Prepare a motion vector to used to modify the velocity
        Vector3 motionVector = Vector3.zero;

        // If there is input along the X or Z axis in either direction
        if (horizontal > 0.1f || vertical > 0.1f || horizontal < -0.1f || vertical < -0.1f) 
        {            
            // Set the motion vector to be relative to the input direction multiplied by the player speed and DeltaTime
            //motionVector = new Vector3(horizontal * RunSpeed * Time.deltaTime, 0, vertical * RunSpeed * Time.deltaTime);
            Vector3 forwardMotion = camera_pivot.transform.forward * vertical * RunSpeed * Time.deltaTime;
            Vector3 rightMotion = camera_pivot.transform.right * horizontal * RunSpeed * Time.deltaTime;
            motionVector = forwardMotion + rightMotion;
            motionVector.y = 0;
        } 
        
        // Otherwise, if there is no input along the X or Z axis
        else 
        {
            // Set the motion vector to a brake force to that will slow down the velocity
            if (rigid.velocity.magnitude > 0 || rigid.velocity.magnitude < 0) {
                Vector3 brakeVector = -rigid.velocity * BrakeSpeed;                
                motionVector = brakeVector;
                motionVector.y = 0;
            } 
        }                                          

        // Lastly, set the new velocity for the rigidbody equal to our resulting velocity.        
        rigid.velocity += motionVector;

        Vector3 xzVel = new Vector3(rigid.velocity.x, 0, rigid.velocity.z);
        Vector3 yVel = new Vector3(0, rigid.velocity.y, 0);

        xzVel = Vector3.ClampMagnitude(xzVel, MaxSpeed);
        yVel = Vector3.ClampMagnitude(yVel, MaxFallSpeed);        
        rigid.velocity = xzVel + yVel;

        // Clamp the Rigidbody velocity based on maximum speed
        // -- Need to figure out how to clamp based on input values.  With commented out code, it doesn't take account of camera orientation
        // float resultX = Mathf.Clamp(currentVelocity.x, -MaxSpeed/2, MaxSpeed/2);
        //float resultX = Mathf.Clamp(rigid.velocity.x, -MaxSpeed, MaxSpeed);        
        //float resultY = Mathf.Clamp(currentVelocity.z, (vertical < -0.1f) ? -MaxSpeed/2 : -MaxSpeed, (vertical < -0.1f) ? MaxSpeed/2 : MaxSpeed);
        //float resultY = Mathf.Clamp(rigid.velocity.z, -MaxSpeed, MaxSpeed);
        //rigid.velocity = new Vector3 (resultX, rigid.velocity.y, resultY);     

        // -- DEBUGGING VALUES -- 
        Velocity = rigid.velocity;
        Vector3 horizontalVelocity = Velocity;
        horizontalVelocity.y = 0;
        VelocityMagnitude = horizontalVelocity.magnitude;        
        CurrentSpeedRatio = VelocityMagnitude / MaxSpeed;
    }

    // To be used for negative angles
    private float ClampAngle(float angle, float from, float to) 
    {
        if (angle < 0f) angle = 360 + angle;
        if (angle > 180f) return Mathf.Max(angle, 360+from);
        return Mathf.Min(angle, to);
    }

    private void OnDrawGizmos() 
    {
        if (DebugInteractionRadius) Gizmos.DrawWireSphere(transform.position, InteractionDistance);             
    }
}
