using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    private IInteractive target;

    public void Interact(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        if (target != null && !target.Equals(null)) // To make sure we have a target and it wasn't destroyed
        {
            target.Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IInteractive>() != null) target = other.GetComponent<Switch>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<IInteractive>() == target) target = null;
    }
}