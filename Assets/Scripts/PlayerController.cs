using NUnit.Framework;
using System;
using System.Collections;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private c_PlayerInput PlayerInput;

    private Rigidbody this_Rigidbody;
    private GameObject this_CameraObject;

    private bool PlayerJump;

    private CharacterController this_CharController;

    #region TestContext States
    private bool TestContext_1;
    #endregion TestContext States

    [SerializeField]
    private float MoveSpeed;

    [SerializeField]
    private float HorizontalLookMultiplier;

    [SerializeField]
    private float VerticalLookMultiplier;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Chris Test
        print("Start");
        INIT_PlayerInput();
        INIT_PlayerObjectComponents();
    }

    #region Init Functions
    void INIT_PlayerInput()
    {
        PlayerInput = GameObject.Find("GameManager").GetComponent<c_PlayerInput>();

        TestContext_1 = false;
        ToggleCursorState(TestContext_1);
    }

    void INIT_PlayerObjectComponents()
    {
        this_Rigidbody = gameObject.GetComponent<Rigidbody>();

        this_CharController = gameObject.GetComponent<CharacterController>();

        this_CameraObject = gameObject.transform.Find("Main Camera").gameObject;
        cameraAngle = this_CameraObject.transform.localEulerAngles.x;
    }

    #endregion Init Functions

    bool QuitPressed;
    float QuitTimer = 0f;
    // Update is called once per frame
    void Update()
    {
        UpdateJump();
        UpdateJumpJet();
        UpdateLook();
        UpdateMovement();
        UpdateQuitButton();

        #region Quit Button
        if (QuitPressed)
        {
            print("Pressed");
            if (QuitTimer > 0f)
            {
                Application.Quit();
                print("Quit");
            }
            else
                QuitTimer = 0.2f;
        }

        if(QuitTimer > 0f)
        {
            QuitTimer -= Time.deltaTime;
            if (QuitTimer < 0f)
                QuitTimer = 0f;
        }
        #endregion Quit Button

        #region Mouse Cursor
        if(PlayerInput.TestContext_1())
        {
            TestContext_1 = !TestContext_1;
            ToggleCursorState(TestContext_1);
        }

        #endregion Mouse Cursor
    }

    #region Update Functions

    Vector2 v2_MovementVelocityPercentage;
    float MovementVelocityRate;
    Vector3 v3_LastFrameVelocity;
    void UpdateMovement()
    {
        Vector2 v2_InputVector = PlayerInput.GetInputVector();

        #region Update Velocity Direction Percentage
        if (!OnGround && JetpackActive)
            MovementVelocityRate = 3f * Time.deltaTime;
        else
            MovementVelocityRate = 10f * Time.deltaTime;

        v2_InputVector.Normalize();

        v2_MovementVelocityPercentage = MovementVelocityPerc(v2_MovementVelocityPercentage, v2_InputVector);

        #endregion Update Velocity Direction Percentage

        Vector3 v3_PlayerInput = new Vector3(v2_MovementVelocityPercentage.x, 0, v2_MovementVelocityPercentage.y);

        // Default on-ground player input velocity conversion
        Vector3 playerVel = gameObject.transform.rotation * v3_PlayerInput;
        
        if(!OnGround && JetpackActive)
            playerVel *= 0.65f;

        playerVel *= MoveSpeed;
        playerVel += yVel * Vector3.up;

        // OVERRIDE values if player isn't using Jetpack and isn't on the ground.
        if(!OnGround && !JetpackActive)
        {
            playerVel = v3_LastFrameVelocity;
            playerVel.y = yVel;
        }

        v3_LastFrameVelocity = playerVel;

        this_CharController.Move(playerVel * Time.deltaTime);
    }

    private float JumpHeight = 1.5f;
    private float Gravity = -9.81f * 3.5f;

    private float yVel;
    private bool OnGround;
    void UpdateJump()
    {
        RaycastHit _hit;
        int layerMask = LayerMask.GetMask("Terrain");
        CapsuleCollider _collider = gameObject.GetComponent<CapsuleCollider>();
        
        OnGround = Physics.SphereCast(gameObject.transform.position, _collider.radius - 0.05f, Vector3.down, out _hit, _collider.radius + 0.14f, layerMask);

        if (OnGround && !JetpackActive)
        {
            yVel = 0f;

            if (PlayerInput.GetJumpButton())
            {
                yVel = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                OnGround = false;
            }
        }
        else
        {
            if(!JetpackActive)
                yVel += Gravity * Time.deltaTime;
        }

        
    }

    bool JetpackActive = false;
    float JetpackMaxVertVelocity = 7f; // 4 Feels like Tribes 'Heavy' Velocity
    float JetpackArmorGravityInfluence = 10f;
    void UpdateJumpJet()
    {
        JumpJetButtonState state = PlayerInput.JumpJetState();

        switch (state)
        {
            case JumpJetButtonState.Pressed:
            case JumpJetButtonState.Held:
                
                JetpackActive = true;

                // Forcing initial velocity if player taps ground while using jetpack
                if (OnGround && yVel < 0f)
                    yVel = 0f;

                yVel += Gravity / JetpackArmorGravityInfluence * -1f * Time.deltaTime;

                if(yVel > JetpackMaxVertVelocity)
                    yVel = JetpackMaxVertVelocity;

                print("Vert Vel: " + (Gravity / JetpackArmorGravityInfluence * -1f * Time.deltaTime));
                break;
            case JumpJetButtonState.Released:
            case JumpJetButtonState.Off:
                JetpackActive = false;
                break;
            default:
                break;
        }
    }

    float cameraAngle;
    void UpdateLook()
    {
        Vector2 v2_LookVector = PlayerInput.GetLookVector();

        if (v2_LookVector == new Vector2())
            return;

        if(v2_LookVector.x != 0f)
        {
            Vector3 v3_PlayerDirection = this_CharController.transform.localEulerAngles;
            v3_PlayerDirection.y += v2_LookVector.x * HorizontalLookMultiplier;
            this_CharController.transform.localEulerAngles = v3_PlayerDirection;
        }
        
        if(v2_LookVector.y != 0f)
        {
            // print(v2_LookVector);
            cameraAngle -= v2_LookVector.y * VerticalLookMultiplier;
            cameraAngle = Mathf.Clamp(cameraAngle, -44f, 44f);
            this_CameraObject.transform.localEulerAngles = new Vector3(cameraAngle, 0f, 0f);
        }
    }

    void UpdateQuitButton()
    {
        QuitPressed = PlayerInput.QuitButton();
    }

    void ToggleCursorState(bool IsVisible)
    {
        if (!IsVisible)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;

            Cursor.visible = IsVisible;
    }

    #endregion Update Functions

    #region Physics Updates

    private void FixedUpdate()
    {
        
    }

    #endregion Physics Updates

    #region Math Functions


    Vector2 MovementVelocityPerc(Vector2 VelPercValue, Vector2 PlayerInputValue)
    {
        Vector2 output = new Vector2();

        output.x = MovementVelocityPerc(VelPercValue.x, PlayerInputValue.x);
        output.y = MovementVelocityPerc(VelPercValue.y, PlayerInputValue.y);

        return output;
    }
    float MovementVelocityPerc(float VelPercValue, float PlayerInputValue)
    {
        if (PlayerInputValue != VelPercValue)
        {
            if (PlayerInputValue < VelPercValue)
            {
                VelPercValue -= MovementVelocityRate;

                if (VelPercValue < PlayerInputValue)
                    VelPercValue = PlayerInputValue;
            }
            else
            {
                VelPercValue += MovementVelocityRate;

                if (VelPercValue > PlayerInputValue)
                    VelPercValue = PlayerInputValue;
            }
        }

        return VelPercValue;
    }

    #endregion Math Functions

    #region Collider Extensions

    public LayerMask obstacleLayers;
    public Color mtvColor = Color.yellow;
    public bool autoResolve = true;
    public bool smoothResolve = true;

    public event Action<Vector3> OnPenetrationStart;
    public event Action<Vector3> OnPenetrationStay;
    public event System.Action OnPenetrationEnd;

    Collider col;
    Vector3 lastCorrection;
    bool resolvingCollision;

    void ObjectClipStart()
    {
        col = GetComponent<Collider>();

        OnPenetrationStart += correction =>
        {
            float penetrationDepth = correction.magnitude;
        };
    }

    void ObjectClipUpdate()
    {
        bool colliding = col.GetPenetrationsInLater(obstacleLayers, out Vector3 correction);
        correction += correction.normalized * 0.001f;
        lastCorrection = colliding ? correction : Vector3.zero;

        if (colliding)
        {
            if (!resolvingCollision) OnPenetrationStart?.Invoke(correction);
            else OnPenetrationStay?.Invoke(correction);

            resolvingCollision = true;

            if (autoResolve)
            {
                Vector3 delta = smoothResolve
                    ? Vector3.Lerp(Vector3.zero, correction, 0.05f)
                    : correction;

                transform.position += delta;
            }

            Debug.Log($"Colliding, MTV = {correction.magnitude:F3}");
        }
        else
        {
            if (resolvingCollision) OnPenetrationEnd?.Invoke();
            resolvingCollision = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (col == null) col = GetComponent<Collider>();
        if (col == null) return;

        if (lastCorrection != Vector3.zero)
        {
            Vector3 start = col.bounds.center;
            Vector3 end = start + lastCorrection;
            Gizmos.color = mtvColor;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(end, 0.05f);
        }
    }

    #endregion
}
