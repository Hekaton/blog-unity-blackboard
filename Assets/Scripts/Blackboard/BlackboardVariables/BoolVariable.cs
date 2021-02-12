using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New BoolVariable", menuName = "Blackboard/Variables/Bool", order = 0)]
public class BoolVariable : BlackboardVariable
{
    public bool value;
    private bool valueSnapshot;

    public override BVarSave CreateSave()
    {
        return new BVSBoolVariable(value);
    }

    public override void LoadFrom(BVarSave source)
    {
        value = ((BVSBoolVariable) source).value;
    }

    public override Type GetSaveType()
    {
        return typeof(BVSBoolVariable);
    }

    public override void SnapshotState()
    {
        valueSnapshot = value;
    }

    public override void UndoChanges()
    {
        value = valueSnapshot;
    }
}