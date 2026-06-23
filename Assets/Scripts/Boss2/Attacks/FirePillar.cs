using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;


public class FirePillar : MonoBehaviour, IDamageable
{
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private AudioClip exploseAudio;
    private int damage;

    private bool hasHit = false;

    private IObjectPool<FirePillar> objectPool;
    public IObjectPool<FirePillar> ObjectPool { set => objectPool = value; }

    public void Init(int baseAtk) => damage = Mathf.RoundToInt(baseAtk * damageMultiplier);

    public IDamageable.DamageInfo SetDamage() => new() { damage = damage, canParry = false };

    public void GetDamage(IDamageable.DamageInfo damageInfo) { }

    public void Setup()
    {
        hasHit = false;
        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particle.Play();
        SoundManager.Instance.PlaySFX(exploseAudio);
        WaitParticle().Forget();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        if (other.CompareTag("Player"))
        {
            other.GetComponent<IDamageable>()?.GetDamage(SetDamage());
            hasHit = true;
        }
    }

    private async UniTaskVoid WaitParticle()
    {
        await UniTask.WaitUntil(() => !particle.IsAlive());
        objectPool.Release(this);
    }
}
