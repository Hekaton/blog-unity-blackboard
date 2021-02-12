using UnityEngine;

public abstract class BlackboardVariable : ScriptableObject
{
    public PersistenceType persistenceType;

    public abstract BVarSave CreateSave();
    public abstract void LoadFrom(BVarSave source);
    public abstract System.Type GetSaveType();
    public abstract void SnapshotState();
    public abstract void UndoChanges();
}

[System.Serializable]
public enum PersistenceType
{
    NeverPersist,
    AlwaysPersist,
    SavedToFile
}