using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

[RequireComponent(typeof(BehaviorGraphAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Boss2Controller : MonoBehaviour, IDamageable
{
    private static readonly int ActingHash       = Animator.StringToHash("Acting");
    private static readonly int LaserHash        = Animator.StringToHash("Laser");
    private static readonly int BigAttackHash    = Animator.StringToHash("BigAttack");
    private static readonly int FireBallHash     = Animator.StringToHash("FireBall");
    private static readonly int SpreadFireBallHash = Animator.StringToHash("SpreadFireBall");
    private static readonly int FireWallHash     = Animator.StringToHash("FireWall");
    private static readonly int DeathHash        = Animator.StringToHash("Death");
    private static readonly int StunHash         = Animator.StringToHash("Stun");

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
    [SerializeField] private GameObject parriableProjectilePrefab; // ParriableProjectile 컴포넌트 포함 프리팹
    [SerializeField] private int   projectileCount    = 3;
    [SerializeField] private float projectileSpeed    = 8f;
    [SerializeField] private float projectileInterval = 0.4f;

    [Header("── 불기둥 ──")]
    [SerializeField] private GameObject firePillarWarningPrefab;
    [SerializeField] private GameObject firePillarExplosionPrefab; // FirePillar 컴포넌트 포함 프리팹
    [SerializeField] private int   firePillarCount           = 3;
    [SerializeField] private float firePillarWarningDuration = 2f;
    [SerializeField] private float firePillarGroundY         = 0f;
    [SerializeField] private float firePillarRangeXMin       = -8f;
    [SerializeField] private float firePillarRangeXMax       = 8f;

    [Header("── 탄막 ──")]
    [SerializeField] private GameObject bulletPrefab; // BossBullet 컴포넌트 포함 프리팹
    [SerializeField] private int   bulletCurtainCount = 12;
    [SerializeField] private float bulletSpeed        = 6f;
    [SerializeField] private float bulletLifetime     = 5f;

    [Header("── 층 레이저 (2페이즈) ──")]
    [SerializeField] private GameObject laserPrefab; // FloorLaser 컴포넌트 포함 프리팹
    [SerializeField] private Transform[] floorLaserLeftPositions;
    [SerializeField] private Transform[] floorLaserRightPositions;
    [SerializeField] private float laserWarningDuration = 1.5f;
    [SerializeField] private float laserActiveDuration  = 2f;

    [Header("── 그로기 패턴 (2페이즈) ──")]
    [SerializeField] private GameObject groggyPartPrefab; // GroggyPart 컴포넌트 포함 프리팹
    [SerializeField] private Transform[] groggyPartSpawnPoints;
    [SerializeField] private Transform   groggyCenterPosition;
    [SerializeField] private float groggyTimeLimit       = 15f;
    [SerializeField] private float groggyDuration        = 3f;
    [SerializeField] private float groggyHPRecoveryAmount = 200f;

    [Header("── 시각 효과 ──")]
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Color phase2Color = new(1f, 0.3f, 0.3f);

    [Header("── 피격 효과 ──")]
    [SerializeField] private float hitFlashDuration = 0.08f;
    [SerializeField] private float hitStopDuration  = 0.04f;

    [Header("── 사망 연출 ──")]
    [SerializeField] private float deathStopDuration  = 0.3f;
    [SerializeField] private float deathSlowScale     = 0.2f;
    [SerializeField] private float deathSlowDuration  = 1.0f;

    public UnityEvent OnBossDead;

    public bool  IsActing  { get; private set; } = false;
    public bool  IsGroggy  { get; private set; } = false;
    public float CurrentHP { get; private set; }
    public float MaxHP     => maxHP;
    public BossData Data   => data;
    public bool IsDead     => CurrentHP <= 0f;
    public bool IsPhase2   => CurrentHP <= maxHP * 0.5f;
    public float HPPercent => CurrentHP / maxHP;

    private BehaviorGraphAgent behaviorAgent;
    private bool phase2Triggered = false;
    private int  currentFloor    = 0;
    private int  currentSide     = 1;
    private Vector3[,] teleportPositions;

    private int  groggyPartsTotal     = 0;
    private int  groggyPartsDestroyed = 0;
    private List<GroggyPart>  spawnedGroggyParts = new();
    // 보스 사망 시 씬에 남은 풀 오브젝트 정리용 (반환되면 inactive이므로 Destroy 안 함)
    private List<GameObject> activePoolObjects = new();

    private bool fireSignalReceived = false;
    private GameObject player;
    private Animator animator;

    [SerializeField] private BossData    data;
    [SerializeField] private GameObject  parryWarning;
    [SerializeField] private GameObject  avoidWarning;
    [SerializeField] private Transform   lookAtZone;
    [SerializeField] private AudioClip   parryFireballClip;
    [SerializeField] private AudioClip   fireballClip;
    [SerializeField] private AudioClip   ChargeClip;
    [SerializeField] private AudioClip   fireWallClip;
    [SerializeField] private AudioClip   hitClip;
    public Transform LookAtZone => lookAtZone;
    private int maxHP;

    private IObjectPool<BossBullet>          bulletPool;
    private IObjectPool<FirePillar>          PillarPool;
    private IObjectPool<GameObject>          PillarWarningPool;
    private IObjectPool<FloorLaser>          LaserPool;
    private IObjectPool<GroggyPart>          PartPool;
    private IObjectPool<ParriableProjectile> ParryPool;

    private BossBullet          CreateBullet()  { var o = Instantiate(bulletPrefab)               .GetComponent<BossBullet>();          return o; }
    private GameObject          CreateWarning() { return Instantiate(firePillarWarningPrefab); }
    private FirePillar          CreatePillar()  { var o = Instantiate(firePillarExplosionPrefab) .GetComponent<FirePillar>();           o.ObjectPool = PillarPool; return o; }
    private FloorLaser          CreateLaser()   { var o = Instantiate(laserPrefab)               .GetComponent<FloorLaser>();           o.ObjectPool = LaserPool;  return o; }
    private GroggyPart          CreatePart()    { var o = Instantiate(groggyPartPrefab)          .GetComponent<GroggyPart>();           o.ObjectPool = PartPool;   return o; }
    private ParriableProjectile CreateParry()   { var o = Instantiate(parriableProjectilePrefab) .GetComponent<ParriableProjectile>();  o.ObjectPool = ParryPool;  return o; }

    private void OnGet(BossBullet          p) => p.gameObject.SetActive(true);
    private void OnGet(GameObject          p) => p.SetActive(true);
    private void OnGet(FirePillar          p) => p.gameObject.SetActive(true);
    private void OnGet(FloorLaser          p) => p.gameObject.SetActive(true);
    private void OnGet(GroggyPart          p) => p.gameObject.SetActive(true);
    private void OnGet(ParriableProjectile p) => p.gameObject.SetActive(true);

    private void OnRelease(BossBullet          p) => p.gameObject.SetActive(false);
    private void OnRelease(GameObject          p) => p.SetActive(false);
    private void OnRelease(FirePillar          p) => p.gameObject.SetActive(false);
    private void OnRelease(FloorLaser          p) => p.gameObject.SetActive(false);
    private void OnRelease(GroggyPart          p) => p.gameObject.SetActive(false);
    private void OnRelease(ParriableProjectile p) => p.gameObject.SetActive(false);

    private void OnDestroyPooledObject(BossBullet          p) => Destroy(p.gameObject);
    private void OnDestroyPooledObject(GameObject          p) => Destroy(p);
    private void OnDestroyPooledObject(FirePillar          p) => Destroy(p.gameObject);
    private void OnDestroyPooledObject(FloorLaser          p) => Destroy(p.gameObject);
    private void OnDestroyPooledObject(GroggyPart          p) => Destroy(p.gameObject);
    private void OnDestroyPooledObject(ParriableProjectile p) => Destroy(p.gameObject);


    private void Awake()
    {
        player    = GameObject.FindGameObjectWithTag("Player");
        maxHP     = data.Hp;
        CurrentHP = maxHP;
        behaviorAgent  = GetComponent<BehaviorGraphAgent>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator       = GetComponent<Animator>();
        parryWarning.SetActive(false);
        avoidWarning.SetActive(false);
        SetupTeleportPositions();

        bulletPool        = new ObjectPool<BossBullet>         (CreateBullet,  OnGet, OnRelease, OnDestroyPooledObject);
        PillarPool        = new ObjectPool<FirePillar>          (CreatePillar,  OnGet, OnRelease, OnDestroyPooledObject);
        PillarWarningPool = new ObjectPool<GameObject>          (CreateWarning, OnGet, OnRelease, OnDestroyPooledObject);
        LaserPool         = new ObjectPool<FloorLaser>          (CreateLaser,   OnGet, OnRelease, OnDestroyPooledObject);
        PartPool          = new ObjectPool<GroggyPart>          (CreatePart,    OnGet, OnRelease, OnDestroyPooledObject);
        ParryPool         = new ObjectPool<ParriableProjectile> (CreateParry,   OnGet, OnRelease, OnDestroyPooledObject);
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
            float diff   = player.transform.position.x - transform.position.x;
            float facing = transform.localScale.x;
            float absX   = Mathf.Abs(facing);

            if      (facing > 0f && diff < -0.5f)
                transform.localScale = new Vector3(-absX, transform.localScale.y, transform.localScale.z);
            else if (facing < 0f && diff >  0.5f)
                transform.localScale = new Vector3( absX, transform.localScale.y, transform.localScale.z);
        }

        if (!phase2Triggered && IsPhase2 && !IsDead)
        {
            phase2Triggered = true;
            OnPhase2Start();
        }
    }

    public IDamageable.DamageInfo SetDamage() => default;
    public void GetDamage(IDamageable.DamageInfo damageInfo) => TakeDamage(damageInfo.damage);

    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        SoundManager.Instance.PlaySFX(hitClip);
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
            if (spriteRenderer) spriteRenderer.enabled = true;
            Time.timeScale = 1f;
            StartCoroutine(DeathEffectCoroutine());
        }
    }

    public void HealHP(float amount) => CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);

    private void OnPhase2Start()
    {
        behaviorAgent.BlackboardReference.SetVariableValue("IsPhase2", true);
        if (spriteRenderer) spriteRenderer.color = phase2Color;
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
        yield return null;
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Death"))
            yield return null;
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        OnBossDead?.Invoke();
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    private void DestroyAllSpawnedObjects()
    {
        foreach (var obj in activePoolObjects)
            if (obj != null) Destroy(obj);
        activePoolObjects.Clear();

        foreach (var part in spawnedGroggyParts)
            if (part != null) Destroy(part.gameObject);
        spawnedGroggyParts.Clear();

        bulletPool?.Clear();
        PillarPool?.Clear();
        PillarWarningPool?.Clear();
        LaserPool?.Clear();
        PartPool?.Clear();
        ParryPool?.Clear();
    }

    private void SetupTeleportPositions()
    {
        int floorCount = 4;
        teleportPositions = new Vector3[floorCount, 3];

        for (int i = 0; i < floorCount; i++)
        {
            if (i < floorLeftPositions.Length   && floorLeftPositions[i])
                teleportPositions[i, 0] = floorLeftPositions[i].position;
            if (i < floorCenterPositions.Length && floorCenterPositions[i])
                teleportPositions[i, 1] = floorCenterPositions[i].position;
            if (i < floorRightPositions.Length  && floorRightPositions[i])
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

    // ── 공격 패턴 ─────────────────────────────────────────────

    public IEnumerator AttackParriableProjectile(Action<bool> callback)
    {
        IsActing = true;
        var playerTf = player?.transform;

        fireSignalReceived = false;
        animator.Play(FireBallHash);
        yield return new WaitUntil(() => fireSignalReceived);

        for (int i = 0; i < projectileCount; i++)
        {
            if (parriableProjectilePrefab && playerTf)
            {
                Vector3 dir    = (playerTf.position - transform.position).normalized;
                float   spread = UnityEngine.Random.Range(-10f, 10f) * Mathf.Deg2Rad;
                Vector2 fd = new Vector2(
                    dir.x * Mathf.Cos(spread) - dir.y * Mathf.Sin(spread),
                    dir.x * Mathf.Sin(spread) + dir.y * Mathf.Cos(spread)).normalized;

                var go = ParryPool.Get();
                go.transform.position = transform.position;
                activePoolObjects.Add(go.gameObject);

                if (go.TryGetComponent<Rigidbody2D>(out var rb)) rb.linearVelocity = fd * projectileSpeed;
                go.Initialize(this, data.atk);

                float returnDelay = projectileInterval * (projectileCount - i) + parryWindowDelay + 1f;
                StartCoroutine(ReturnParry(go, returnDelay));
            }
            yield return new WaitForSeconds(projectileInterval);
        }

        yield return new WaitForSeconds(0.3f + parryWindowDelay);
        yield return StartCoroutine(TeleportToRandomPosition());
        yield return new WaitForSeconds(postAttackDelay);

        IsActing = false;
        callback?.Invoke(true);
    }

    private IEnumerator ReturnParry(ParriableProjectile go, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (go != null && go.gameObject.activeSelf)
            ParryPool.Release(go);
    }

    public IEnumerator AttackFirePillar(Action<bool> callback)
    {
        IsActing = true;

        fireSignalReceived = false;
        animator.Play(FireWallHash);
        yield return new WaitUntil(() => fireSignalReceived);

        if (firePillarWarningPrefab)
        {
            for (int i = 0; i < firePillarCount; i++)
            {
                Vector3 spawnPos = new Vector3(
                    UnityEngine.Random.Range(firePillarRangeXMin, firePillarRangeXMax),
                    firePillarGroundY, 0f);

                var warning = PillarWarningPool.Get();
                warning.transform.position = spawnPos;
                activePoolObjects.Add(warning);
                StartCoroutine(FirePillarExplode(spawnPos, warning, firePillarWarningDuration));
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
        PillarWarningPool.Release(warning);

        if (firePillarExplosionPrefab)
        {
            var explosion = PillarPool.Get();
            explosion.transform.position = pos;
            activePoolObjects.Add(explosion.gameObject);
            explosion.Init(data.atk);
            explosion.Setup(); // Start()는 첫 생성 때만 실행 → 명시 호출
        }
    }

    public IEnumerator AttackBulletCurtain(Action<bool> callback)
    {
        IsActing = true;

        fireSignalReceived = false;
        animator.Play(SpreadFireBallHash);
        yield return new WaitUntil(() => fireSignalReceived);

        if (bulletPrefab)
        {
            float step = 360f / bulletCurtainCount;
            for (int i = 0; i < bulletCurtainCount; i++)
            {
                float   angle = i * step * Mathf.Deg2Rad;
                Vector3 dir   = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

                var bullet = bulletPool.Get();
                bullet.transform.position = transform.position;
                activePoolObjects.Add(bullet.gameObject);

                if (bullet.TryGetComponent<Rigidbody2D>(out var rb)) rb.linearVelocity = dir * bulletSpeed;
                bullet.Init(data.atk);

                StartCoroutine(ReturnBullet(bullet, bulletLifetime));
            }
        }

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(TeleportToRandomPosition());
        yield return new WaitForSeconds(postAttackDelay);
        IsActing = false;
        callback?.Invoke(true);
    }

    // 수명 만료 — 이미 자체 반환(비활성)이면 스킵
    private IEnumerator ReturnBullet(BossBullet bullet, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        if (bullet != null && bullet.gameObject.activeSelf)
            bulletPool.Release(bullet);
    }

    public IEnumerator AttackFloorLaser(Action<bool> callback)
    {
        IsActing = true;

        bool       bossOnLeft  = transform.position.x <= 0f;
        Transform[] spawnPoints = bossOnLeft ? floorLaserLeftPositions : floorLaserRightPositions;

        animator.SetBool(ActingHash, IsActing);
        animator.Play(LaserHash);

        if (laserPrefab && spawnPoints is { Length: > 0 })
        {
            int[]       order  = ShuffledOrder(spawnPoints.Length);
            var         lasers = new FloorLaser[spawnPoints.Length];

            for (int i = 0; i < order.Length; i++)
            {
                int idx = order[i];
                if (!spawnPoints[idx]) continue;

                float   halfLen  = laserPrefab.transform.localScale.x * 0.4f;
                float   offsetX  = bossOnLeft ? halfLen : -halfLen;
                Vector3 spawnPos = new(spawnPoints[idx].position.x + offsetX,
                                       spawnPoints[idx].position.y, 0f);

                var go = LaserPool.Get();
                go.transform.position = spawnPos;
                activePoolObjects.Add(go.gameObject);
                go.Init(data.atk);
                go.StartWarning();
                lasers[i] = go;

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

        groggyPartsTotal     = groggyPartSpawnPoints?.Length ?? 0;
        groggyPartsDestroyed = 0;
        spawnedGroggyParts.Clear();

        if (groggyPartPrefab && groggyPartSpawnPoints is { Length: > 0 })
        {
            foreach (var pt in groggyPartSpawnPoints)
            {
                if (!pt) continue;
                var part = PartPool.Get();
                part.transform.position = pt.position;
                activePoolObjects.Add(part.gameObject);
                spawnedGroggyParts.Add(part);
                part.Initialize(this);
            }
        }

        float timer = 0f;
        SoundManager.Instance.PlaySFXLoop(ChargeClip);
        while (timer < groggyTimeLimit && groggyPartsDestroyed < groggyPartsTotal)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        SoundManager.Instance.StopSFXLoop();

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
                if (part != null && part.gameObject.activeSelf) PartPool.Release(part);
            spawnedGroggyParts.Clear();
            HealHP(groggyHPRecoveryAmount);
        }

        IsActing = false;
        animator.SetBool(ActingHash, IsActing);
        yield return StartCoroutine(TeleportToRandomPosition());
        yield return new WaitForSeconds(postAttackDelay);
        callback?.Invoke(true);
    }

    public void OnFireSignal() => fireSignalReceived = true;

    public void OnGroggyPartDestroyed() => groggyPartsDestroyed++;

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
        float elapsed = 0f;
        while (elapsed < deathSlowDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1f;
        OnDeath();
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

    private void DestroyIt()        => Destroy(gameObject);
    private void EnableWarning()    => parryWarning.SetActive(true);
    private void DisableWarning()   => parryWarning.SetActive(false);
    private void EnableAvoidWarning()  => avoidWarning.SetActive(true);
    private void DisableAvoidWarning() => avoidWarning.SetActive(false);

    public void OnGameOver()
        => behaviorAgent.BlackboardReference.SetVariableValue("IsGameOver", true);

    private void PlayParryFireballSound() => SoundManager.Instance.PlaySFX(parryFireballClip);
    private void PlayFireballSound()      => SoundManager.Instance.PlaySFX(fireballClip);
    private void PlayFireWallSound()      => SoundManager.Instance.PlaySFX(fireWallClip);
}
