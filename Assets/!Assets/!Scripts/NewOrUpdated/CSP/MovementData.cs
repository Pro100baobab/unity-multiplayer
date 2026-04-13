using FishNet.Object.Prediction;
using UnityEngine;


public struct PlayerMoveData : IReplicateData
{
    public float Horizontal;
    public float Vertical;

    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}

public struct PlayerReconcileData : IReconcileData
{
    public Vector3 Position;
    public float VerticalVelocity;

    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}