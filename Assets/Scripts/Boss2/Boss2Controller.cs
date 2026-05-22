using System;
using System.Collections;
using Unity.Behavior;
using UnityEngine;

[RequireComponent(typeof(BehaviorGraphAgent))]
public class Boss2Controller : MonoBehaviour, IDamageable
{
    [Header("── 기본 ──")]
    [SerializeField] private float maxHP = 1000f;

    [Header("── 패턴 인터벌 ──")]
    [Tooltip("공격 후 다음 패턴까지 대기 시간")]
    [SerializeField] private float postAttackDelay = 1.5f;
    [Tooltip("패링 투사체 공격: 텔포 전 대기 (반사 투사체가 보스에 닿을 시간)")]
    [SerializeField] private float parryWindowDelay = 2f;

    [Header("── 텔레포트 ──")]
    [Tooltip("인덱스: 0=평지, 1=1층, 2=2층, 3=3층")]
    [SerializeField] private Transform[] floorLeftPositions;
    [SerializeField] private Transform[] floorCenterPositions;
    [SerializeField] private Transform[] floorRightPositions;
    [SerializeField] private float teleportDuration = 0.3f;

    [Header("── 패링 투사체 ──")]
    [SerializeField] private GameObject parriableProjectilePrefab;
    [SerializeField] private int projectileCount = 3;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float projectileInterval = 0.4f;

    [Header("── 불기둥 ──")]
    [SerializeField] private GameObject firePillarWarningPrefab;
    [SerializeField] private GameObject firePillarExplosionPrefab;
    [SerializeField] private int firePillarCount = 3;
    [SerializeField] private float firePillarWarningDuration = 2f;

    [Header("── 탄막 ──")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int bulletCurtainCount = 12;
    [SerializeField] private float bulletSpeed = 6f;

    [Header("── 층 레이저 (2페이즈) ──")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private Transform[] floorLaserPositions;
    [SerializeField] private float laserWarningDuration = 1f;
    [SerializeField] private float laserActiveDuration = 2f;

    [Header("── 그로기 패턴 (2페이즈) ──")]
    [SerializeField] private GameObject groggyPartPrefab;
    [SerializeField] private Transform[] groggyPartSpawnPoints;
    [SerializeField] private float groggyTimeLimit = 15f;
    [SerializeField] private float groggyDuration = 3f;
    [SerializeField] private float groggyHPRecoveryAmount = 200f;

    [Header("── 시각 효과 ──")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color phase2Color = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private ParticleSystem phase2Particles;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip phase2SFX;

    public bool IsActing { get; private set; } = false;
    public bool IsGroggy { get; private set; } = false;
    public float CurrentHP { get; private set; }
    public float MaxHP => maxHP;
    public bool IsDead => CurrentHP <= 0f;
    public bool IsPhase2 => CurrentHP <= maxHP * 0.5f;

    private BehaviorGraphAgent behaviorAgent;
    private bool phase2Triggered = false;
    private int currentFloor = 0;
    private int currentSide = 1;
    private Vector3[,] teleportPositions;

    private int groggyPartsTotal = 0;
    private int groggyPartsDestroyed = 0;
    private void Awake()
    {
        CurrentHP = maxHP;
        behaviorAgent = GetComponent<BehaviorGraphAgent>();
        SetupTeleportPositions();
    }

    private void Update()
    {
        if (!phase2Triggered && IsPhase2 && !IsDead)
        {
            phase2Triggered = true;
            OnPhase2Start();
        }
    }

    public IDamageable.DamageInfo SetDamage() => default;

    public void GetDamage(IDamageable.DamageInfo damageInfo)
    {
        TakeDamage(damageInfo.damage);
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        CurrentHP = Mathf.Max(0f, CurrentHP - damage);

        if (IsDead)
        {
            behaviorAgent.BlackboardReference.SetVariableValue("IsDead", true);
            OnDeath();
        }
    }

    public void HealHP(float amount)
    {
        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
    }

    private void OnPhase2Start()
    {
        Debug.Log("[Boss2] ★ 2페이즈 전환 ★");

        behaviorAgent.BlackboardReference.SetVariableValue("IsPhase2", true);

        if (spriteRenderer) spriteRenderer.color = phase2Color;
        if (phase2Particles) phase2Particles.Play();
        if (audioSource && phase2SFX) audioSource.PlayOneShot(phase2SFX);
    }

    private void OnDeath()
    {
        Debug.Log("[Boss2] 사망");
        behaviorAgent.BlackboardReference.SetVariableValue("IsDead", true);
        StopAllCoroutines();
    }

    private void SetupTeleportPositions()
    {
        int floorCount = 4;
        teleportPositions = new Vector3[floorCount, 3];

        for (int i = 0; i < floorCount; i++)
        {
            if (i < floorLeftPositions.Length && floorLeftPositions[i])
                teleportPositions[i, 0] = floorLeftPositions[i].position;
            if (i < floorCenterPositions.Length && floorCenterPositions[i])
                teleportPositions[i, 1] = floorCenterPositions[i].position;
            if (i < floorRightPositions.Length && floorRightPositions[i])
                teleportPositions[i, 2] = floorRightPositions[i].position;
        }
    }

    private IEnumerator TeleportToRandomPosition()
    {
        int newFloor = UnityEngine.Random.Range(0, 4);
        int newSide = UnityEngine.Random.Range(0, 3);
        if (newFloor == currentFloor && newSide == currentSide)
            newSide = (newSide + 1) % 3;

        Vector3 target = teleportPositions[newFloor, newSide];

        if (spriteRenderer)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(teleportDuration * 0.5f);
            transform.position = target;
            yield return new WaitForSeconds(teleportDuration * 0.5f);
            spriteRenderer.enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(teleportDuration * 0.5f);
            transform.position = target;
            yield return new WaitForSeconds(teleportDuration * 0.5f);
        }

        currentFloor = newFloor;
        currentSide = newSide;
    }

    public IEnumerator AttackParriableProjectile(Action<bool> callback)
    {
        IsActing = true;

        var playerTf = GameObject.FindGameObjectWithTag("Player")?.transform;

        for (int i = 0; i < projectileCount; i++)
        {
            if (parriableProjectilePrefab && playerTf)
            {
                Vector3 dir = (playerTf.position - transform.position).normalized;
                float spread = UnityEngine.Random.Range(-10f, 10f) * Mathf.Deg2Rad;
                Vector2 fd = new Vector2(
                    dir.x * Mathf.Cos(spread) - dir.y * Mathf.Sin(spread),
                    dir.x * Mathf.Sin(spread) + dir.y * Mathf.Cos(spread)).normalized;

                var go = Instantiate(parriableProjectilePrefab, transform.position, Quaternion.identity);
                if (go.TryGetComponent<Rigidbody2D>(out var rb)) rb.linearVelocity = fd * projectileSpeed;
                if (go.TryGetComponent<ParriableProjectile>(out var pp)) pp.Initialize(this);
            }
            yield return new WaitForSeconds(projectileInterval);
        }

        yield return new WaitForSeconds(0.3f);
        yield return new WaitForSeconds(parryWindowDelay);
        yield return StartCoroutine(TeleportToRandomPosition());
        yield return new WaitForSeconds(postAttackDelay);

        IsActing = false;
        callback?.Invoke(true);
    }

    public IEnumerator AttackFirePillar(Action<bool> callback)
    {
        IsActing = true;
        Debug.Log("[Boss2] 불기둥 공격");

        var playerTf = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (firePillarWarningPrefab && playerTf)
        {
            for (int i = 0; i < firePillarCount; i++)
            {
                Vector3 spawnPos = playerTf.position + new Vector3(
                    UnityEngine.Random.Range(-3f, 3f), 0f, 0f);

                var warning = Instantiate(firePillarWarningPrefab, spawnPos, Quaternion.identity);
                float delay = firePillarWarningDuration - 0.25f * i;
                StartCoroutine(FirePillarExplode(spawnPos, warning, Mathf.Max(0.5f, delay)));

                yield return new WaitForSeconds(0.25f);
            }
            yield return new WaitForSeconds(firePillarWarningDuration + 0.5f);
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }

        yield return StartCoroutine(TeleportToRandomPosition());
        yield return new WaitForSeconds(postAttackDelay);
        IsActing = false;
        callback?.Invoke(true);
    }

    private IEnumerator FirePillarExplode(Vector3 pos, GameObject warning, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (warning) Destroy(warning);
        if (firePillarExplosionPrefab)
            Destroy(Instantiate(firePillarExplosionPrefab, pos, Quaternion.identity), 1.5f);
    }

    public IEnumerator AttackBulletCurtain(Action<bool> callback)
    {
        IsActing = true;
        Debug.Log("[Boss2] 탄막 공격");

        if (bulletPrefab)
        {
            float step = 360f / bulletCurtainCount;
            for (int i = 0; i < bulletCurtainCount; i++)
            {
                float angle = i * step * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                var bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                if (bullet.TryGetComponent<Rigidbody2D>(out var rb)) rb.linearVelocity = dir * bulletSpeed;
                Destroy(bullet, 5f);
            }
        }

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(TeleportToRandomPosition());
        yield return new WaitForSeconds(postAttackDelay);
        IsActing = false;
        callback?.Invoke(true);
    }

    public IEnumerator AttackFloorLaser(Action<bool> callback)
    {
        IsActing = true;
        Debug.Log("[Boss2] 층 레이저 공격");

        if (laserPrefab && floorLaserPositions is { Length: > 0 })
        {
            int[] order = ShuffledOrder(floorLaserPositions.Length);
            foreach (int idx in order)
            {
                if (!floorLaserPositions[idx]) continue;
                var laser = Instantiate(laserPrefab, floorLaserPositions[idx].position, Quaternion.identity);
                if (laser.TryGetComponent<FloorLaser>(out var fl))
                    fl.Initialize(laserWarningDuration, laserActiveDuration);
                yield return new WaitForSeconds(0.3f);
            }
            yield return new WaitForSeconds(laserActiveDuration + 0.5f);
        }
        else
        {
            yield return new WaitForSeconds(3f);
        }

        yield return StartCoroutine(TeleportToRandomPosition());
        yield return new WaitForSeconds(postAttackDelay);
        IsActing = false;
        callback?.Invoke(true);
    }

    public IEnumerator AttackGroggyPattern(Action<bool> callback)
    {
        IsActing = true;
        Debug.Log("[Boss2] 그로기 패턴! 파츠를 파괴하세요!");

        groggyPartsTotal = groggyPartSpawnPoints?.Length ?? 0;
        groggyPartsDestroyed = 0;

        if (groggyPartPrefab && groggyPartSpawnPoints is { Length: > 0 })
        {
            foreach (var pt in groggyPartSpawnPoints)
            {
                if (!pt) continue;
                var part = Instantiate(groggyPartPrefab, pt.position, Quaternion.identity);
                if (part.TryGetComponent<GroggyPart>(out var gp)) gp.Initialize(this);
            }
        }

        float timer = 0f;
        while (timer < groggyTimeLimit && groggyPartsDestroyed < groggyPartsTotal)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (groggyPartsDestroyed >= groggyPartsTotal)
        {
            Debug.Log("[Boss2] 파츠 파괴 성공 → 그로기!");
            IsGroggy = true;
            yield return new WaitForSeconds(groggyDuration);
            IsGroggy = false;
        }
        else
        {
            Debug.Log("[Boss2] 파츠 파괴 실패 → HP 회복");
            HealHP(groggyHPRecoveryAmount);
        }

        yield return StartCoroutine(TeleportToRandomPosition());
        yield return new WaitForSeconds(postAttackDelay);
        IsActing = false;
        callback?.Invoke(true);
    }

    public void OnGroggyPartDestroyed()
    {
        groggyPartsDestroyed++;
        Debug.Log($"[Boss2] 파츠 {groggyPartsDestroyed}/{groggyPartsTotal} 파괴");
    }

    private int[] ShuffledOrder(int count)
    {
        var order = new int[count];
        for (int i = 0; i < count; i++) order[i] = i;
        for (int i = count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }
        return order;
    }

    public float HPPercent => CurrentHP / maxHP;
}
