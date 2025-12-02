using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInputObject PlayerInput;
    private PlayerInputObject PlayerInput_OLD;

    private Rigidbody this_Rigidbody;

    private bool PlayerJump;

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
        PlayerInput = new PlayerInputObject();

        PlayerInput.Init();
    }

    void INIT_PlayerObjectComponents()
    {
        this_Rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    #endregion Init Functions

    // Update is called once per frame
    void Update()
    {
        // UPDATE_Input();
        TempInput();
    }

    #region Update Functions

    private PlayerInputObject UPDATE_Input()
    {
        PlayerInput_OLD = PlayerInput;

        PlayerInput.InputUpdate();

        #region Jump Evaluation
        PlayerJump = false;

        if (PlayerInput.Jump && !PlayerInput_OLD.Jump)
            PlayerJump = true;
        #endregion Jump Evaluation

        print(PlayerInput.MoveVector);

        return PlayerInput;
    }

    #endregion Update Functions

    #region Physics Updates

    Rigidbody rigidbody_OLD;
    private void FixedUpdate()
    {
        rigidbody_OLD = this_Rigidbody;

        FIXEDUPDATE_PlayerMovement();
    }
    float Acceleration = 450f;
    float ReverseAcceleration = 375f;
    Vector3 v3_OldVelocity;
    int frames = 0;
    private void FIXEDUPDATE_PlayerMovement()
    {
        Vector3 v3_OldVelocity = rigidbody_OLD.linearVelocity;
        Vector3 v3_OldVelocity_Normalized = v3_OldVelocity.normalized;
        Vector3 v3_NewVelocity = v3_OldVelocity;

        print("Input: " + v2_Input);

        if(v2_Input.x != 0f)
        {
            if(v2_Input.x > 0f)
            {
                // Opposite sign of input gets most velocity change
                if (v3_OldVelocity.x < 0f)
                {
                    v3_NewVelocity.x += Time.fixedDeltaTime * Acceleration;
                }
                else
                {
                    v3_NewVelocity.x += Time.fixedDeltaTime * ReverseAcceleration;
                }
            }
            else if(v2_Input.x < 0f)
            {
                // Opposite sign of input gets most velocity change
                if(v3_OldVelocity.x > 0f)
                {
                    v3_NewVelocity.x -= Time.fixedDeltaTime * Acceleration;
                }
                else
                {
                    v3_NewVelocity.x -= Time.fixedDeltaTime * ReverseAcceleration;
                }
            }
        }
        else
        {
            if (v3_NewVelocity.x < 0f)
            {
                v3_NewVelocity.x += Time.fixedDeltaTime * ReverseAcceleration;
                if (v3_NewVelocity.x > 0f)
                    v3_NewVelocity.x = 0f;
            }
            else
            {
                v3_NewVelocity.x -= Time.fixedDeltaTime * ReverseAcceleration;
                if (v3_NewVelocity.x < 0f)
                    v3_NewVelocity.x = 0f;
            }
        }

        // Percent
        v3_NewVelocity.x = Mathf.Clamp(v3_NewVelocity.x, -100f, 100f);

        float perc = 100f / Mathf.Abs(v3_NewVelocity.x);
        float sign = Mathf.Sign(v3_NewVelocity.x);

        this_Rigidbody.AddForce( new Vector3(500f * (sign * perc), 0f, 0f) );

        v3_OldVelocity = v3_NewVelocity;
        /*
        if (PlayerInput.MoveVector.Y == 1f)
        {
            print("True");
            this_Rigidbody.linearVelocity = gameObject.transform.rotation * new Vector3(0f, 5f, 0f);
        }
        */
    }


    #endregion Physics Updates

    #region TEMP INPUT

    Vector2 v2_Input;
    bool JumpButton;
    bool JumpButton_OLD;
    bool JumpPressed;
    void TempInput()
    {
        #region Input
        v2_Input = new Vector2();
        bool leftPressed = Input.GetKey(KeyCode.A);
        bool rightPressed = Input.GetKey(KeyCode.D);
        bool upPressed = Input.GetKey(KeyCode.W);
        bool downPressed = Input.GetKey(KeyCode.S);

        if (leftPressed && !rightPressed)
            v2_Input.x = -1f;
        else if(rightPressed && !leftPressed)
            v2_Input.x = 1f;

        if(upPressed && !downPressed)
            v2_Input.y = 1f;
        else if(downPressed && !upPressed)
            v2_Input.y = -1f;

        v2_Input.Normalize();
        #endregion Input

        #region Jump
        JumpButton_OLD = JumpButton;
        JumpButton = false;
        if (Input.GetKey(KeyCode.Space))
            JumpButton = true;

        JumpPressed = JumpButton && !JumpButton_OLD;
        #endregion Jump
    }
    #endregion

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
