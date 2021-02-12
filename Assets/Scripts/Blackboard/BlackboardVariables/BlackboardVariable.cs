using UnityEngine;

public abstract class BlackboardVariable : ScriptableObject
{
    public bool shouldChangesPersist;
    
    public abstract void SnapshotState();
    public abstract void UndoChanges();
}