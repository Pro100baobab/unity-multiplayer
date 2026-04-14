using FishNet.Object;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float _speed = 18f;
    [SerializeField] private int _damage = 20;

    private void Update()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!base.IsServerInitialized) return;

        var target = other.GetComponent<PlayerNetwork>();
        if (target == null) return;
        if (target.OwnerId == base.OwnerId) return;

        // Сообщаем цели, кто её атаковал
        target.SetLastAttacker(base.Owner);

        target.HP.Value = Mathf.Max(0, target.HP.Value - _damage);
        base.Despawn(gameObject);
    }
}