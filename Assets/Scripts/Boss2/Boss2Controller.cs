using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
[RequireComponent(typeof(BehaviorGraphAgent))]

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Boss2Controller : MonoBehaviour, IDamageable
{
    private static readonly int ActingHash = Animator.StringToHash("Acting");
    private static readonly int LaserHash = Animator.StringToHash("Laser");
    private static readonly int BigAttackHash = Animator.StringToHash("BigAttack");
    private static readonly int FireBallHash = Animator.StringToHash("FireBall");
    private static readonly int SpreadFireBallHash = Animator.StringToHash("SpreadFireBall");
    private static readonly int FireWallHash = Animator.StringToHash("FireWall");
    private static readonly int DeathHash = Animator.StringToHash("Death");
    private static readonly int StunHash = Animator.StringToHash("Stun");
    [Header("── 기본 ──")]
    [SerializeField] private float maxHP = 1000f;

    [Header("── 스폰 ──")]
    [Tooltip("시작 층 (0=평지, 1=1층, 2=2층, 3=3층)")]
    [SerializeField] private int spawnFloor = 0;
    [Tooltip("시작 위치 (0=왼쪽, 1=중앙, 2=오른쪽)")]
    [SerializeField] private int spawnSide = 1;
    [Tooltip("체크하면 왼쪽/중앙/오른쪽 중 랜덤 스폰")]
    [SerializeField] private bool randomSpawn = false;

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
    [SerializeField] private Transform[] floorLaserLeftPositions;
    [SerializeField] private Transform[] floorLaserRightPositions;
    [SerializeField] private float laserWarningDuration = 1.5f;
    [SerializeField] private float laserActiveDuration = 2f;

    [Header("── 그로기 패턴 (2페이즈) ──")]
    [SerializeField] private GameObject groggyPartPrefab;
    [SerializeField] private Transform[] groggyPartSpawnPoints;
    [SerializeField] private Transform groggyCenterPosition;
    [SerializeField] private float groggyTimeLimit = 15f;
    [SerializeField] private float groggyDuration = 3f;
    [SerializeField] private float groggyHPRecoveryAmount = 200f;

    [Header("── 시각 효과 ──")]
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Color phase2Color = new(1f, 0.3f, 0.3f);
    [SerializeField] private ParticleSystem phase2Particles;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip phase2SFX;

    [Header("── 피격 효과 ──")]
    [Tooltip("피격 시 흰 플래시 유지 시간 (초)")]
    [SerializeField] private float hitFlashDuration = 0.08f;
    [Tooltip("피격 히트스탑 지속 시간 (초, unscaled)")]
    [SerializeField] private float hitStopDuration = 0.04f;

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
    private List<GameObject> spawnedGroggyParts = new();
    private List<GameObject> spawnedObjects = new();
    private bool fireSignalReceived = false;
    private GameObject player;
    private Animator animator;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        CurrentHP = maxHP;
        behaviorAgent = GetComponent<BehaviorGraphAgent>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        SetupTeleportPositions();
    }

    private void Start()
    {
        int floor = Mathf.Clamp(spawnFloor, 0, 3);
        int side  = randomSpawn ? UnityEngine.Random.Range(0, 3) : Mathf.Clamp(spawnSide, 0, 2);

        Vector3 spawnPos = teleportPositions[floor, side];
        if (spawnPos != Vector3.zero)
        {
            transform.position = spawnPos;
            currentFloor = floor;
            currentSide  = side;
        }
    }

    private void Update()
    {
        if (IsDead) return;

        if (player != null)
        {
            float diff = player.transform.position.x - transform.position.x;
            float facing = transform.localScale.x; // 1 = 오른쪽, -1 = 왼쪽

            // 현재 바라보는 방향 반대쪽으로 0.5 이상 벗어났을 때만 전환
            if (facing > 0f && diff < -0.5f)
                transform.localScale = new Vector3(-1f, 1f, 1f);
            else if (facing < 0f && diff > 0.5f)
                transform.localScale = new Vector3(1f, 1f, 1f);
        }

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
        if (!IsDead)
        {
            StartCoroutine(HitFlashCoroutine());
            StartCoroutine(HitStopCoroutine());
        }
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
        animator.Play(DeathHash);
        StopAllCoroutines();
        DestroyAllSpawnedObjects();
    }

    private void DestroyAllSpawnedObjects()
    {
        foreach (var obj in spawnedObjects)
            if (obj) Destroy(obj);
        spawnedObjects.Clear();

        foreach (var obj in spawnedGroggyParts)
            if (obj) Destroy(obj);
        spawnedGroggyParts.Clear();
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

    private IEnumerator TeleportToPosition(Vector3 target)
    {
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

        fireSignalReceived = false;
        animator.Play(FireBallHash);

        // 애니메이션 이벤트 OnFireSignal() 대기
        yield return new WaitUntil(() => fireSignalReceived);

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
                spawnedObjects.Add(go);
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

        fireSignalReceived = false;
        animator.Play(FireWallHash);

        // 애니메이션 이벤트 OnFireSignal() 대기
        yield return new WaitUntil(() => fireSignalReceived);

        if (firePillarWarningPrefab && playerTf)
        {
            for (int i = 0; i < firePillarCount; i++)
            {
                Vector3 spawnPos = playerTf.position + new Vector3(
                    UnityEngine.Random.Range(-3f, 3f), 0f, 0f);

                var warning = Instantiate(firePillarWarningPrefab, spawnPos, Quaternion.identity);
                spawnedObjects.Add(warning);
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
            Instantiate(firePillarExplosionPrefab, pos, Quaternion.identity);
    }

    public IEnumerator AttackBulletCurtain(Action<bool> callback)
    {
        IsActing = true;
        Debug.Log("[Boss2] 탄막 공격");

        fireSignalReceived = false;
        animator.Play(SpreadFireBallHash);

        // 애니메이션 이벤트 OnFireSignal() 대기
        yield return new WaitUntil(() => fireSignalReceived);

        if (bulletPrefab)
        {
            float step = 360f / bulletCurtainCount;
            for (int i = 0; i < bulletCurtainCount; i++)
            {
                float angle = i * step * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                var bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                spawnedObjects.Add(bullet);
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

        bool bossOnLeft = transform.position.x <= 0f;
        Transform[] spawnPoints = bossOnLeft ? floorLaserLeftPositions : floorLaserRightPositions;

        animator.SetBool(ActingHash, IsActing);
        animator.Play(LaserHash);

        if (laserPrefab && spawnPoints is { Length: > 0 })
        {
            int[] order = ShuffledOrder(spawnPoints.Length);
            var lasers = new FloorLaser[spawnPoints.Length];

            for (int i = 0; i < order.Length; i++)
            {
                int idx = order[i];
                if (!spawnPoints[idx]) continue;

                float laserHalfLength = laserPrefab.transform.localScale.x * 0.4f;
                float offsetX = bossOnLeft ? laserHalfLength : -laserHalfLength;
                Vector3 spawnPos = new(spawnPoints[idx].position.x + offsetX, spawnPoints[idx].position.y, 0f);
                var go = Instantiate(laserPrefab, spawnPos, Quaternion.identity);
                spawnedObjects.Add(go);
                if (go.TryGetComponent<FloorLaser>(out var fl))
                {
                    fl.StartWarning();
                    lasers[i] = fl;
                }
                yield return new WaitForSeconds(0.3f);
            }

            yield return new WaitForSeconds(laserWarningDuration);

            foreach (var laser in lasers)
            {
                if (laser == null) continue;
                laser.Activate(laserActiveDuration);
                yield return new WaitForSeconds(laserActiveDuration + 0.2f);
            }
        }
        else
        {
            yield return new WaitForSeconds(3f);
        }

        IsActing = false;
        animator.SetBool(ActingHash, IsActing);

        yield return StartCoroutine(TeleportToRandomPosition());
        yield return new WaitForSeconds(postAttackDelay);
        
        callback?.Invoke(true);
    }

    public IEnumerator AttackGroggyPattern(Action<bool> callback)
    {
        IsActing = true;
        animator.SetBool(ActingHash, IsActing);
        Debug.Log("[Boss2] 그로기 패턴! 파츠를 파괴하세요!");

        if (groggyCenterPosition != null)
            yield return StartCoroutine(TeleportToPosition(groggyCenterPosition.position));

        animator.Play(BigAttackHash);

        groggyPartsTotal = groggyPartSpawnPoints?.Length ?? 0;
        groggyPartsDestroyed = 0;
        spawnedGroggyParts.Clear();

        if (groggyPartPrefab && groggyPartSpawnPoints is { Length: > 0 })
        {
            foreach (var pt in groggyPartSpawnPoints)
            {
                if (!pt) continue;
                var part = Instantiate(groggyPartPrefab, pt.position, Quaternion.identity);
                spawnedGroggyParts.Add(part);
                spawnedObjects.Add(part);
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
            animator.SetBool(StunHash, IsGroggy);
            animator.Play(StunHash);
            yield return new WaitForSeconds(groggyDuration);
            IsGroggy = false;
            animator.SetBool(StunHash, IsGroggy);
        }
        else
        {
            Debug.Log("[Boss2] 파츠 파괴 실패 → HP 회복");
            foreach (var part in spawnedGroggyParts)
                if (part) Destroy(part);
            spawnedGroggyParts.Clear();
            HealHP(groggyHPRecoveryAmount);
        }

        IsActing = false;
        animator.SetBool(ActingHash, IsActing);

        yield return StartCoroutine(TeleportToRandomPosition());
        yield return new WaitForSeconds(postAttackDelay);
        
        callback?.Invoke(true);
    }

    /// <summary>애니메이션 이벤트에서 호출 — 발사 타이밍 신호</summary>
    public void OnFireSignal() => fireSignalReceived = true;

    public void OnGroggyPartDestroyed()
    {
        groggyPartsDestroyed++;
        Debug.Log($"[Boss2] 파츠 {groggyPartsDestroyed}/{groggyPartsTotal} 파괴");
    }

    private IEnumerator HitFlashCoroutine()
    {
        if (!spriteRenderer) yield break;

        Color current = spriteRenderer.color;
        spriteRenderer.color = Color.black;
        yield return new WaitForSecondsRealtime(hitFlashDuration);
        spriteRenderer.color = current;
    }

    private IEnumerator HitStopCoroutine()
    {
        Time.timeScale = 0.05f;
        float elapsed = 0f;
        while (elapsed < hitStopDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        Time.timeScale = 1f;
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

    private void DestroyIt()
    {
        Destroy(gameObject);
    }
}
