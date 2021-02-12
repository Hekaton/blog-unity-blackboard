using UnityEngine;

public class Switch : MonoBehaviour, IInteractive
{
    private BlackboardController bc;
    [SerializeField] private string blackboardEventName;

    private void Start()
    {
        bc = GameObject.FindWithTag("BlackboardController").GetComponent<BlackboardController>();
    }

    public void Interact()
    {
        BoolVariable boolVariable = (BoolVariable)bc.GetBlackboardValue(blackboardEventName);
        boolVariable.value = !boolVariable.value;
        bc.TriggerEvent(blackboardEventName);
    }
}