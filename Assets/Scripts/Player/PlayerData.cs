using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{

    public float GroundCheckRadius = 0.15f;
    public float CoyoteTime = 0.1f;
    public float JumpBufferTime = 0.1f;
    public float LowJumpMultiplier = 2f;
    public float FallMultiplier = 2.5f;
    public float DodgeAmount = 5f;
    public float DodgeDuration = 0.5f;
    public float ParryInterval = 0.3f;
    public int Atk;
    public int MaxHp;
    public int MoveSpeed;
    public float BlinkDurationInterval = 0.16f;
    public float BlinkInterval = 1f;
    public int MaxJumpCount = 1;
    public int MaxAttackCount = 2;
    [Header("플레이어 이동속도")]
    public float moveSpeed = 5f;
    [Header("플레이어 점프 힘")]
    public float JumpPower = 10f;
    [Header("넉백")]
    public float KnockbackForceX = 5f;
    public float KnockbackForceY = 4f;
}
