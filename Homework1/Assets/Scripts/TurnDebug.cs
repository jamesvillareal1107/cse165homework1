using UnityEngine;
using UnityEngine.InputSystem;

public class TurnInputDebug : MonoBehaviour
{
    public InputActionReference turnAction;

    private void OnEnable()
    {
        if (turnAction != null)
            turnAction.action.Enable();
    }

    private void Update()
    {
        if (turnAction == null) return;

        Vector2 v = turnAction.action.ReadValue<Vector2>();
        Debug.Log("Right turn input: " + v);
    }
}