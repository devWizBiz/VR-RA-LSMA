using UnityEngine;
using UnityEngine.InputSystem;

public class InputTest : MonoBehaviour
{
    public InputActionProperty ActionValueTest;
    public InputActionProperty ActionButtonTest;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       float value = ActionValueTest.action.ReadValue<float>();
       Debug.Log("The value is "+ value);

       bool button = ActionButtonTest.action.IsPressed();
       Debug.Log("The button is pressed "+ button);
    }
}
