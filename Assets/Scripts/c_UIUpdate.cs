using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class c_UIUpdate : MonoBehaviour
{
    internal InputAction IA_Jump;
    internal InputAction IA_JumpJet;
    internal GameObject GO_UIText_Jump;
    internal GameObject GO_UIText_Jetpack;
    internal GameObject GO_UIText_Velocity;

    internal GameObject GO_Player;
    internal PlayerController c_PlayerController;
    string text_Jump;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        IA_Jump = InputSystem.actions.FindAction("Jump");
        IA_JumpJet = InputSystem.actions.FindAction("JumpJet");

        GO_UIText_Jump = transform.Find("UIText_Jump").gameObject;
        GO_UIText_Jetpack = transform.Find("UIText_Jetpack").gameObject;

        GO_Player = GameObject.Find("Player").gameObject;
        c_PlayerController = GO_Player.GetComponent<PlayerController>();
        GO_UIText_Velocity = transform.Find("UIText_Velocity").gameObject;

        StartCoroutine(DisplayMoveSpeed());
    }

    // Update is called once per frame
    void Update()
    {
        GO_UIText_Jump.GetComponent<TMP_Text>().enabled = IA_Jump.IsPressed();
        GO_UIText_Jetpack.GetComponent<TMP_Text>().enabled = IA_JumpJet.IsPressed();
    }

    IEnumerator DisplayMoveSpeed()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.05f);

            float moveSpeed = c_PlayerController.GetMoveSpeed();
            GO_UIText_Velocity.GetComponent<TMP_Text>().text = "Vel: " + string.Format("{0:#.00}", moveSpeed );
        }

        // yield return null;
    }
}
