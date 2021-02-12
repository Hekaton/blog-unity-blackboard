using UnityEngine;

[CreateAssetMenu(fileName = "New BoolVariable", menuName = "Blackboard/Variables/Bool", order = 0)]
public class BoolVariable : BlackboardVariable
{
    public bool value;
    private bool valueSnapshot;

    public override void SnapshotState()
    {
        valueSnapshot = value;
    }

    public override void UndoChanges()
    {
        value = valueSnapshot;
    }
}