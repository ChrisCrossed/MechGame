using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private c_PlayerInput PlayerInput;

    private Rigidbody this_Rigidbody;

    private bool PlayerJump;

    private CharacterController this_CharController;

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
    }

    void INIT_PlayerObjectComponents()
    {
        this_Rigidbody = gameObject.GetComponent<Rigidbody>();

        this_CharController = gameObject.GetComponent<CharacterController>();
    }

    #endregion Init Functions

    // Update is called once per frame
    void Update()
    {
        UpdateMovement();
    }

    #region Update Functions

    void UpdateMovement()
    {
        Vector2 v2_InputVector = PlayerInput.GetInputVector();
        if (v2_InputVector.magnitude < 0.25f)
            return;

        Vector3 v3_PlayerInput = new Vector3(v2_InputVector.x, 0, v2_InputVector.y);

        Vector3 playerVel = gameObject.transform.rotation * v3_PlayerInput;
        playerVel *= Time.deltaTime;

        this_CharController.Move(playerVel);
    }

    #endregion Update Functions

    #region Physics Updates

    private void FixedUpdate()
    {
        
    }

    #endregion Physics Updates


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
