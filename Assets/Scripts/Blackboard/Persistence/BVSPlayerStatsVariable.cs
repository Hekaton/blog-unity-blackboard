using UnityEngine;

[System.Serializable]
public class BVSPlayerStatsVariable : BVarSave
{
    public float x;
    public float y;
    public float z;

    public BVSPlayerStatsVariable(Vector3 playerPosition)
    {
        x = playerPosition.x;
        y = playerPosition.y;
        z = playerPosition.z;
    }
}