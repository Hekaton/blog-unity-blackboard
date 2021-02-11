using UnityEngine;

public class Switch : MonoBehaviour, IInteractive
{
    private bool isOn;
    
    public void Toggle()
    {
        isOn = !isOn;
        Debug.Log("switch is now " + isOn);
    }

    public void Interact()
    {
        Toggle();
    }
}