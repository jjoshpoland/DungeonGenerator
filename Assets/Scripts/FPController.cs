using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPController : MonoBehaviour
{
    public float MoveSpeed = 4f;
    public float SprintModifier = 2f;
    public float Acceleration = 10f;
    public float RotationSpeed = .12f;

    public float CameraPitchMax = 70f;
    public float CameraPitchMin = -30f;
    public float MaxCameraDistance = 8f;
    public float StartingCameraDistance = 4f;

    public float JumpHeight = 1.2f;
    public float JumpCooldown = 0.5f;
    public float Gravity = -9.81f;
    public float GroundedCheckOffset = -0.14f;
    public float GroundedRadius = 0.28f; //should match character controller radius
    public float Falltimeout = 0.15f;
    public LayerMask GroundLayers;

    public GameObject CameraTarget;

    public bool ThirdPerson = true;

    float speed;
    bool sprint;
    float targetRotation;
    float rotationVelocity;
    float verticalVelocity;
    bool grounded;
    Vector2 move;
    Vector2 look;
    CharacterController cc;

    float camTargetYaw;
    float camTargetPitch;

    float fallTimeoutDelta;
    float jumpTimeoutDelta;
    bool jump;
    float terminalVelocity = 53f;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
    }
    // Start is called before the first frame update
    void Start()
    {
        camTargetYaw = CameraTarget.transform.rotation.eulerAngles.y;

        jumpTimeoutDelta = JumpCooldown;
        fallTimeoutDelta = Falltimeout;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        JumpAndGravity();
        GroundCheck();
        ProcessMove();
    }

    private void LateUpdate()
    {
        CameraRotation();   
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            sprint = true;
        }
        else if(context.canceled)
        {
            sprint = false;
        }
    }

    public void Fire(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            Ray ray = Camera.main.ScreenPointToRay(Pointer.current.position.value);
            if(Physics.Raycast(ray, out RaycastHit hit, 2f))
            {
                if(hit.collider.TryGetComponent<IInteractable>(out IInteractable thing))
                {
                    thing.Interact();
                }
            }
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void Look(InputAction.CallbackContext context)
    {
        look = context.ReadValue<Vector2>();

        look *= .5f;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            jump = true;
        }
        else if (context.canceled)
        {
            jump = false;
        }
    }


    private void ProcessMove()
    {
        float targetSpeed = MoveSpeed * (sprint ? SprintModifier : 1);
        if (move == Vector2.zero) targetSpeed = 0;

        float currentMovementVelocity = new Vector3(cc.velocity.x, 0, cc.velocity.z).magnitude;

        if(currentMovementVelocity < targetSpeed - .1f || currentMovementVelocity > targetSpeed + .1f)
        {
            speed = Mathf.Lerp(currentMovementVelocity, targetSpeed, Time.deltaTime * Acceleration);

        }
        else
        {
            speed = targetSpeed;
        }

        Vector3 inputDirection = new Vector3(move.x, 0, move.y).normalized;

        if(move != Vector2.zero)
        {
            inputDirection = transform.right * move.x + transform.forward * move.y;
        }

        cc.Move(inputDirection.normalized * (speed * Time.deltaTime) + new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime);

        //animate moving
    }

    private void CameraRotation()
    {
        if (look.sqrMagnitude >= .01f)
        {
            camTargetPitch += -look.y * RotationSpeed;
            rotationVelocity = look.x * RotationSpeed;

            camTargetPitch = ClampAngle(camTargetPitch, -90f, 90f);

            CameraTarget.transform.localRotation = Quaternion.Euler(camTargetPitch, 0f, 0f);

            transform.Rotate(Vector3.up * rotationVelocity);
        }
    }

    private void GroundCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedCheckOffset, transform.position.z);
        grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

        //update animator grounded
    }

    private void JumpAndGravity()
    {
        if(grounded)
        {
            fallTimeoutDelta = Falltimeout;

            //stop animating any falling

            //velocity shouldn't continue to grow negatively if grounded
            if(verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if(jump && jumpTimeoutDelta <= 0f)
            {
                float height = new Vector3(cc.velocity.x, 0, cc.velocity.z).magnitude > 0 ? JumpHeight / 2f : JumpHeight;
                verticalVelocity = Mathf.Sqrt(height * -2f * Gravity);

                //animate jumping
            }

            if(jumpTimeoutDelta >= 0f)
            {
                jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            jumpTimeoutDelta = JumpCooldown;

            if(fallTimeoutDelta >= 0f)
            {
                fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                //animate falling
            }

            jump = false;
        }

        if(verticalVelocity < terminalVelocity)
        {
            verticalVelocity += Gravity * Time.deltaTime;
        }
    }


    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnApplicationFocus(bool focus)
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
}
