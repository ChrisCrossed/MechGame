using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class c_UIUpdate : MonoBehaviour
{
    internal InputAction IA_Jump;
    internal InputAction IA_JumpJet;
    internal GameObject GO_UIText_Jump;
    internal GameObject GO_UIText_Jetpack;

    string text_Jump;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        IA_Jump = InputSystem.actions.FindAction("Jump");
        IA_JumpJet = InputSystem.actions.FindAction("JumpJet");

        GO_UIText_Jump = transform.Find("UIText_Jump").gameObject;
        GO_UIText_Jetpack = transform.Find("UIText_Jetpack").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        GO_UIText_Jump.GetComponent<TMP_Text>().enabled = IA_Jump.IsPressed();
        GO_UIText_Jetpack.GetComponent<TMP_Text>().enabled = IA_JumpJet.IsPressed();
    }
}
