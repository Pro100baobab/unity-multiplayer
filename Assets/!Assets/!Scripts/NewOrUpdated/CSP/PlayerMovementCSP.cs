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
        base.TimeManager.OnTick += OnTick;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        if (base.TimeManager != null)
            base.TimeManager.OnTick -= OnTick;
    }

    private void OnTick()
    {
        if (_playerNetwork != null && !_playerNetwork.IsAlive.Value)
            return;

        if (base.IsOwner)
        {
            PlayerMoveData moveData = new PlayerMoveData
            {
                Horizontal = Input.GetAxisRaw("Horizontal"),
                Vertical = Input.GetAxisRaw("Vertical")
            };
            Replicate(moveData);
        }
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

    [Replicate]
    private void Replicate(PlayerMoveData md, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        Vector3 move = new Vector3(md.Horizontal, 0f, md.Vertical).normalized;
        move *= _speed;

        _verticalVelocity += _gravity * (float)base.TimeManager.TickDelta;
        move.y = _verticalVelocity;

        _cc.Move(move * (float)base.TimeManager.TickDelta);

        if (_cc.isGrounded)
            _verticalVelocity = 0f;
    }

    [Reconcile]
    private void Reconcile(PlayerReconcileData rd, Channel channel = Channel.Unreliable)
    {
        // Если в консоли клиента появятся эти сообщения при движении другого игрока, значит, данные приходят, но не применяются. 
        Debug.Log($"[Reconcile] Applying position {rd.Position} for object {gameObject.name}, IsOwner={IsOwner}");

        transform.position = rd.Position;
        _verticalVelocity = rd.VerticalVelocity;
    }
}