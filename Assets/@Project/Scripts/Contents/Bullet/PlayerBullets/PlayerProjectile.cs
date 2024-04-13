using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile : Bullet
{    
    [SerializeField] GameObject _hitEffectPrefab;
    [SerializeField] TrailRenderer _trailRenderers;
    [SerializeField] protected Define.BulletType _bulletType;
    [SerializeField] protected LayerMask _damagableLayer;

    protected Rigidbody _rigid;
    protected float _speed;
    protected float _damage;

    protected bool _isSplash;

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody>();
    }

    public virtual void Setup(float speed, float damage, bool splash, Vector3 groundTargetPos, Transform target = null)
    {
        _speed = speed;
        _damage = damage;
        _isSplash = splash;
        if (_bulletType != Define.BulletType.Missile)
            _trailRenderers.Clear();        
    }

    private void OnTriggerEnter(Collider other)
    {        
        if ((_damagableLayer & (1 << other.gameObject.layer)) != 0)
        {
            StopAllCoroutines();

            GameObject hitEffect = Instantiate(_hitEffectPrefab);
            hitEffect.transform.position = transform.position;
            hitEffect.transform.rotation = transform.rotation;
            
            if (_isSplash)
            {
                RaycastHit[] hits = Physics.SphereCastAll(transform.position, 5/*범위지정*/, Vector3.up, 0, _damagableLayer);

                foreach (var hit in hits) // 범위에 들어간 적은 데미지 부여
                {
                    if (hit.transform.TryGetComponent(out Entity entity))
                        entity.GetDamaged(_damage);
                }
            }
            else
            {
                if (other.TryGetComponent(out Entity entity))
                    entity.GetDamaged(_damage);
            }

            gameObject.SetActive(false);
        }        
    }

    private void OnDisable()
    {
        ObjectPooler.ReturnToPool(gameObject); // 한 객체에 한번만
        CancelInvoke();
    }
}
