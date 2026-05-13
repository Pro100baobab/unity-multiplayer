using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementCSP : NetworkBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _gravity = -9.81f;

    private CharacterController _cc;
    private float _verticalVelocity;
    private PlayerNetwork _playerNetwork;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _playerNetwork = GetComponent<PlayerNetwork>();
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        TimeManager.OnTick += OnTick;
        TimeManager.OnPostTick += OnPostTick;
    }


    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        if (TimeManager != null)
        {
            TimeManager.OnTick -= OnTick;
            TimeManager.OnPostTick -= OnPostTick;
        }
    }

    private void OnTick()
    {
        if (_playerNetwork != null && !_playerNetwork.IsAlive.Value)
            return;

        PlayerMoveData md = default;

        if (IsOwner)
        {
            md.Horizontal = Input.GetAxisRaw("Horizontal");
            md.Vertical = Input.GetAxisRaw("Vertical");
        }

        Replicate(md);
    }

    private void OnPostTick()
    {
        CreateReconcile();
    }

    [Replicate]
    private void Replicate(PlayerMoveData md, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        Vector3 move = new Vector3(md.Horizontal, 0f, md.Vertical).normalized;
        move *= _speed;

        _verticalVelocity += _gravity * (float)TimeManager.TickDelta;
        move.y = _verticalVelocity;

        _cc.Move(move * (float)TimeManager.TickDelta);
    }

    public override void CreateReconcile()
    {
        PlayerReconcileData rd = new PlayerReconcileData
        {
            Position = transform.position,
            VerticalVelocity = _verticalVelocity
        };

        Reconcile(rd);
    }

    public void ResetState()
    {
        _verticalVelocity = 0f;
    }

    [Reconcile]
    private void Reconcile(PlayerReconcileData rd, Channel channel = Channel.Unreliable)
    {
        _cc.enabled = false;

        transform.position = rd.Position;
        _verticalVelocity = rd.VerticalVelocity;

        _cc.enabled = true;
    }
}