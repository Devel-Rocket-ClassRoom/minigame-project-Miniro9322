using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Events;
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
    [SerializeField] private float firePillarGroundY = 0f;
    [SerializeField] private float firePillarRangeXMin = -8f;
    [SerializeField] private float firePillarRangeXMax = 8f;

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
    [SerializeField] private float hitFlashDuration = 0.08f;
    [SerializeField] private float hitStopDuration = 0.04f;

    [Header("── 사망 연출 ──")]
    [SerializeField] private float deathStopDuration = 0.3f;
    [SerializeField] private float deathSlowScale = 0.2f;
    [SerializeField] private float deathSlowDuration = 1.0f;
    [SerializeField] private ParticleSystem deathParticle;

    public UnityEvent OnBossDead;

    public bool IsActing { get; private set; } = false;
    public bool IsGroggy { get; private set; } = false;
    public float CurrentHP { get; private set; }
    public float MaxHP => maxHP;
    public BossData Data => data;
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
    [SerializeField] private BossData data;
    [SerializeField] private GameObject parryWarning;
    [SerializeField] private GameObject avoidWarning;
    [SerializeField] private Transform lookAtZone;
    public Transform LookAtZone => lookAtZone;
    private int maxHP;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        maxHP = data.Hp;
        CurrentHP = maxHP;
        behaviorAgent = GetComponent<BehaviorGraphAgent>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        parryWarning.SetActive(false);
        avoidWarning.SetActive(false);
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
            float facing = transform.localScale.x; // 양수 = 오른쪽, 음수 = 왼쪽

            // 현재 바라보는 방향 반대쪽으로 0.5 이상 벗어났을 때만 전환
            // Abs로 원래 크기 유지 (하드코딩 1f 대신)
            float absX = Mathf.Abs(facing);
            if (facing > 0f && diff < -0.5f)
                transform.localScale = new Vector3(-absX, transform.localScale.y, transform.localScale.z);
            else if (facing < 0f && diff > 0.5f)
                transform.localScale = new Vector3(absX, transform.localScale.y, transform.localScale.z);
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
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        CurrentHP = Mathf.Max(0f, CurrentHP - damage);

        if (!IsDead)
        {
            StartCoroutine(HitFlashCoroutine());
            StartCoroutine(HitStopCoroutine());
        }
        else
        {
            behaviorAgent.BlackboardReference.SetVariableValue("IsDead", true);
            StopAllCoroutines();
            if (spriteRenderer) spriteRenderer.enabled = true;  // 텔레포트 중 사망 시 스프라이트 복원
            Time.timeScale = 1f;  // 혹시 히트스탑 중이었다면 복원
            StartCoroutine(DeathEffectCoroutine());
        }
    }

    public void HealHP(float amount)
    {
        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
    }

    private void OnPhase2Start()
    {

        behaviorAgent.BlackboardReference.SetVariableValue("IsPhase2", true);

        if (spriteRenderer) spriteRenderer.color = phase2Color;
        if (phase2Particles) phase2Particles.Play();
        if (audioSource && phase2SFX) audioSource.PlayOneShot(phase2SFX);
    }

    private void OnDeath()
    {
        behaviorAgent.BlackboardReference.SetVariableValue("IsDead", true);
        StopAllCoroutines();
        DestroyAllSpawnedObjects();
        animator.Play(DeathHash);
        StartCoroutine(WaitForDeathAnimation());
    }

    private IEnumerator WaitForDeathAnimation()
    {
        yield return null;  // 애니메이션 시작 대기

        // Death 상태에 들어올 때까지 대기
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Death"))
            yield return null;

        // 재생 완료까지 대기
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        OnBossDead?.Invoke();

        yield return new WaitForSeconds(0.5f);  // 클리어 화면 뜰 시간 확보
        Destroy(gameObject);
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

    private IEnumerator DoTeleport(Vector3 target)
    {
        if (spriteRenderer) spriteRenderer.enabled = false;
        yield return new WaitForSeconds(teleportDuration * 0.5f);
        transform.position = target;
        yield return new WaitForSeconds(teleportDuration * 0.5f);
        if (spriteRenderer) spriteRenderer.enabled = true;
    }

    private IEnumerator TeleportToPosition(Vector3 target)
    {
        yield return StartCoroutine(DoTeleport(target));
    }

    private IEnumerator TeleportToRandomPosition()
    {
        var candidates = new List<(int floor, int side)>();
        for (int f = 0; f < 4; f++)
            for (int s = 0; s < 3; s++)
                if (teleportPositions[f, s] != Vector3.zero)
                    candidates.Add((f, s));

        candidates.RemoveAll(c => c.floor == currentFloor && c.side == currentSide);

        if (candidates.Count == 0) yield break;

        var pick = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        yield return StartCoroutine(DoTeleport(teleportPositions[pick.floor, pick.side]));

        currentFloor = pick.floor;
        currentSide  = pick.side;
    }

    public IEnumerator AttackParriableProjectile(Action<bool> callback)
    {
        IsActing = true;

        var playerTf = player?.transform;

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
                if (go.TryGetComponent<ParriableProjectile>(out var pp)) pp.Initialize(this, data.atk);
            }
            yield return new WaitForSeconds(projectileInterval);
        }

        yield return new WaitForSeconds(0.3f + parryWindowDelay);
        yield return StartCoroutine(TeleportToRandomPosition());
        yield return new WaitForSeconds(postAttackDelay);

        IsActing = false;
        callback?.Invoke(true);
    }

    public IEnumerator AttackFirePillar(Action<bool> callback)
    {
        IsActing = true;

        var playerTf = player?.transform;

        fireSignalReceived = false;
        animator.Play(FireWallHash);

        // 애니메이션 이벤트 OnFireSignal() 대기
        yield return new WaitUntil(() => fireSignalReceived);

        if (firePillarWarningPrefab)
        {
            // 모든 경고 표시를 동시에 생성
            for (int i = 0; i < firePillarCount; i++)
            {
                Vector3 spawnPos = new Vector3(
                    UnityEngine.Random.Range(firePillarRangeXMin, firePillarRangeXMax),
                    firePillarGroundY, 0f);

                var warning = Instantiate(firePillarWarningPrefab, spawnPos, Quaternion.identity);
                spawnedObjects.Add(warning);
                StartCoroutine(FirePillarExplode(spawnPos, warning, firePillarWarningDuration));
            }

            // 경고 + 폭발 시간만큼 대기
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
        {
            var explosion = Instantiate(firePillarExplosionPrefab, pos, Quaternion.identity);
            if (explosion.TryGetComponent<FirePillar>(out var fp)) fp.Init(data.atk);
        }
    }

    public IEnumerator AttackBulletCurtain(Action<bool> callback)
    {
        IsActing = true;

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
                if (bullet.TryGetComponent<BossBullet>(out var bb)) bb.Init(data.atk);
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
                    fl.Init(data.atk);
                    fl.StartWarning();
                    lasers[i] = fl;
                }
                yield return new WaitForSeconds(0.5f);
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
            IsGroggy = true;
            animator.SetBool(StunHash, IsGroggy);
            animator.Play(StunHash);
            yield return new WaitForSeconds(groggyDuration);
            IsGroggy = false;
            animator.SetBool(StunHash, IsGroggy);
        }
        else
        {
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
    }

    private IEnumerator HitFlashCoroutine()
    {
        if (!spriteRenderer) yield break;
        Color baseColor = IsPhase2 ? phase2Color : Color.white;
        spriteRenderer.color = Color.black;
        yield return new WaitForSecondsRealtime(hitFlashDuration);
        spriteRenderer.color = baseColor;
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

    private IEnumerator DeathEffectCoroutine()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(deathStopDuration);

        Time.timeScale = deathSlowScale;
        if (deathParticle) deathParticle.Play();

        float elapsed = 0f;
        while (elapsed < deathSlowDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1f;
        OnDeath();  // 연출 완료 후 사망 처리
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

    private void EnableWarning()
    {
        parryWarning.SetActive(true);
    }

    private void DisableWarning()
    {
        parryWarning.SetActive(false);
    }
    private void EnableAvoidWarning()
    {
        avoidWarning.SetActive(true);
    }

    private void DisableAvoidWarning()
    {
        avoidWarning.SetActive(false);
    }

    public void OnGameOver()
    {
        behaviorAgent.BlackboardReference.SetVariableValue("IsGameOver", true);
    }
}