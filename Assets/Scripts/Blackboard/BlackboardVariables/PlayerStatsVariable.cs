using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New PlayerStatsVariable", menuName = "Blackboard/Variables/PlayerStats", order = 0)]
public class PlayerStatsVariable : BlackboardVariable
{
    public Vector3 playerPosition;
    private Vector3 playerPositionSnapshot;

    public override BVarSave CreateSave()
    {
        return new BVSPlayerStatsVariable(playerPosition);
    }

    public override void LoadFrom(BVarSave source)
    {
        BVSPlayerStatsVariable sourceAsBVS = (BVSPlayerStatsVariable) source;
        playerPosition = new Vector3(sourceAsBVS.x, sourceAsBVS.y, sourceAsBVS.z);
    }

    public override Type GetSaveType()
    {
        return typeof(BVSPlayerStatsVariable);
    }

    public override void SnapshotState()
    {
        playerPositionSnapshot = playerPosition;
    }

    public override void UndoChanges()
    {
        playerPosition = playerPositionSnapshot;
    }
}