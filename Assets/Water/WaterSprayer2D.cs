using System.Collections.Generic;
using UnityEngine;

public class WaterSprayer2D : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject waterParticlePrefab;
    [SerializeField] private Transform emitPoint;
    [SerializeField] private float emitRate = 80f;
    [SerializeField] private float initialRadius = 0.04f;
    [SerializeField] private float lifeTime = 8f;

    [Header("Shoot")]
    [SerializeField] private float shootForce = 14f;
    [SerializeField] private float spreadAngle = 7f;
    [SerializeField, Range(0f, 1f)] private float inheritPlayerVelocity = 0.4f;

    [Header("Particle Physics")]
    [SerializeField] private float particleMass = 0.02f;
    [SerializeField] private float particleDrag = 0.35f;
    [SerializeField] private float particleAngularDrag = 0.05f;
    [SerializeField] private float particleGravityScale = 1f;
    [SerializeField] private float maxSpeed = 22f;

    [Header("Fluid Feel")]
    [SerializeField] private float neighborRadius = 0.2f;
    [SerializeField] private float restDistance = 0.08f;
    [SerializeField] private float pressure = 6.5f;
    [SerializeField] private float cohesion = 2.6f;
    [SerializeField] private float viscosity = 1.2f;
    [SerializeField] private float maxFluidForce = 35f;
    [SerializeField, Range(4, 24)] private int maxNeighbors = 12;

    [Header("Performance")]
    [SerializeField] private int maxAliveParticles = 900;
    [SerializeField] private int prewarmCount = 200;
    [SerializeField, Range(10, 60)] private int solverFPS = 30;
    [SerializeField] private bool disableWaterSelfCollision = true;
    [SerializeField] private string waterLayerName = "Fluid";

    private readonly List<WaterParticleFluid2D> activeParticles = new List<WaterParticleFluid2D>(1024);
    private readonly Stack<WaterParticleFluid2D> pooledParticles = new Stack<WaterParticleFluid2D>(1024);

    private Camera cam;
    private Rigidbody2D playerRb;
    private float spawnAccumulator;
    private float solverAccumulator;
    private int totalCreated;
    private int recycleCursor;
    private int waterLayer = -1;

    private static PhysicsMaterial2D cachedWaterMaterial;

    private void Awake()
    {
        cam = Camera.main;
        playerRb = GetComponent<Rigidbody2D>();
        if (emitPoint == null) emitPoint = transform;

        waterLayer = LayerMask.NameToLayer(waterLayerName);
        if (waterLayer < 0) waterLayer = LayerMask.NameToLayer("Fluid");
        if (waterLayer < 0) waterLayer = LayerMask.NameToLayer("Water");
        if (waterLayer >= 0 && disableWaterSelfCollision)
        {
            Physics2D.IgnoreLayerCollision(waterLayer, waterLayer, true);
        }

        int prewarm = Mathf.Clamp(prewarmCount, 0, maxAliveParticles);
        for (int i = 0; i < prewarm; i++)
        {
            WaterParticleFluid2D particle = CreateParticleInstance();
            if (particle == null) break;
            particle.Deactivate();
            pooledParticles.Push(particle);
        }
    }

    private void Update()
    {
        if (waterParticlePrefab == null) return;

        if (Input.GetMouseButton(0))
        {
            spawnAccumulator += emitRate * Time.deltaTime;
            while (spawnAccumulator >= 1f)
            {
                SpawnOneParticle();
                spawnAccumulator -= 1f;
            }
        }
        else
        {
            spawnAccumulator = 0f;
        }
    }

    private void FixedUpdate()
    {
        float now = Time.time;
        for (int i = activeParticles.Count - 1; i >= 0; i--)
        {
            WaterParticleFluid2D particle = activeParticles[i];
            if (!particle.IsExpired(now)) continue;

            particle.Deactivate();
            pooledParticles.Push(particle);
            RemoveActiveAtSwapBack(i);
        }

        float step = 1f / Mathf.Max(10, solverFPS);
        solverAccumulator += Time.fixedDeltaTime;
        while (solverAccumulator >= step)
        {
            WaterParticleFluid2D.RunGlobalSolver(step);
            solverAccumulator -= step;
        }
    }

    private void SpawnOneParticle()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        WaterParticleFluid2D particle = AcquireParticle();
        if (particle == null) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 baseDir = (Vector2)mouseWorld - (Vector2)emitPoint.position;
        if (baseDir.sqrMagnitude < 0.0001f) baseDir = Vector2.right;
        baseDir.Normalize();

        float angleOffset = Random.Range(-spreadAngle, spreadAngle);
        Vector2 sprayDir = Quaternion.Euler(0f, 0f, angleOffset) * baseDir;

        Vector2 spawnPos = (Vector2)emitPoint.position + sprayDir * initialRadius;
        Vector2 inheritedVel = playerRb != null ? playerRb.linearVelocity * inheritPlayerVelocity : Vector2.zero;
        Vector2 initialVel = sprayDir * shootForce + inheritedVel;

        var settings = new WaterParticleFluid2D.FluidSettings(
            Mathf.Max(0.06f, neighborRadius),
            Mathf.Max(0.02f, restDistance),
            Mathf.Max(0f, pressure),
            Mathf.Max(0f, cohesion),
            Mathf.Max(0f, viscosity),
            Mathf.Max(1f, maxFluidForce),
            Mathf.Clamp(maxNeighbors, 4, 24),
            Mathf.Max(2f, maxSpeed));

        particle.Activate(
            spawnPos,
            initialVel,
            lifeTime,
            settings,
            initialRadius,
            particleMass,
            particleDrag,
            particleAngularDrag,
            particleGravityScale,
            BuildOrGetWaterMaterial(),
            waterLayer);
    }

    private WaterParticleFluid2D AcquireParticle()
    {
        if (pooledParticles.Count > 0)
        {
            WaterParticleFluid2D pooled = pooledParticles.Pop();
            activeParticles.Add(pooled);
            return pooled;
        }

        if (totalCreated < maxAliveParticles)
        {
            WaterParticleFluid2D created = CreateParticleInstance();
            if (created == null) return null;
            activeParticles.Add(created);
            return created;
        }

        if (activeParticles.Count == 0) return null;

        int idx = recycleCursor % activeParticles.Count;
        recycleCursor++;
        return activeParticles[idx];
    }

    private WaterParticleFluid2D CreateParticleInstance()
    {
        GameObject go = Instantiate(waterParticlePrefab, transform.position, Quaternion.identity);
        go.name = $"WaterParticle_{totalCreated}";

        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        if (rb == null) rb = go.AddComponent<Rigidbody2D>();

        CircleCollider2D col = go.GetComponent<CircleCollider2D>();
        if (col == null) col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = false;

        WaterParticleFluid2D fluid = go.GetComponent<WaterParticleFluid2D>();
        if (fluid == null) fluid = go.AddComponent<WaterParticleFluid2D>();

        totalCreated++;
        return fluid;
    }

    private void RemoveActiveAtSwapBack(int idx)
    {
        int last = activeParticles.Count - 1;
        activeParticles[idx] = activeParticles[last];
        activeParticles.RemoveAt(last);
    }

    private static PhysicsMaterial2D BuildOrGetWaterMaterial()
    {
        if (cachedWaterMaterial != null) return cachedWaterMaterial;

        cachedWaterMaterial = new PhysicsMaterial2D("RuntimeWater")
        {
            friction = 0.015f,
            bounciness = 0f
        };
        return cachedWaterMaterial;
    }
}

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class WaterParticleFluid2D : MonoBehaviour
{
    public readonly struct FluidSettings
    {
        public readonly float Radius;
        public readonly float RadiusSqr;
        public readonly float RestDistance;
        public readonly float Pressure;
        public readonly float Cohesion;
        public readonly float Viscosity;
        public readonly float MaxForce;
        public readonly int MaxNeighbors;
        public readonly float MaxSpeed;

        public FluidSettings(float radius, float restDistance, float pressure, float cohesion, float viscosity, float maxForce, int maxNeighbors, float maxSpeed)
        {
            Radius = radius;
            RadiusSqr = radius * radius;
            RestDistance = restDistance;
            Pressure = pressure;
            Cohesion = cohesion;
            Viscosity = viscosity;
            MaxForce = maxForce;
            MaxNeighbors = maxNeighbors;
            MaxSpeed = maxSpeed;
        }
    }

    private static readonly List<WaterParticleFluid2D> Registry = new List<WaterParticleFluid2D>(2048);
    private static int[] bucketHead = new int[4096];
    private const int BucketMask = 4095;
    private static int lastSolveFrame = -1;

    private Rigidbody2D rb;
    private CircleCollider2D col;
    private float expireAt;
    private bool activeParticle;
    private int registryIndex = -1;

    private FluidSettings settings;
    private Vector2 cachedPos;
    private Vector2 cachedVel;
    private int cellX;
    private int cellY;
    private int nextInBucket;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
    }

    public void Activate(
        Vector2 position,
        Vector2 velocity,
        float lifeTime,
        FluidSettings settings,
        float visualRadius,
        float mass,
        float drag,
        float angularDrag,
        float gravityScale,
        PhysicsMaterial2D material,
        int layer)
    {
        this.settings = settings;
        expireAt = Time.time + Mathf.Max(0.05f, lifeTime);

        if (layer >= 0) gameObject.layer = layer;

        transform.position = position;
        gameObject.SetActive(true);

        col.radius = Mathf.Max(0.008f, visualRadius * 0.45f);
        col.sharedMaterial = material;

        rb.simulated = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.mass = mass;
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
        rb.gravityScale = gravityScale;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        rb.interpolation = RigidbodyInterpolation2D.None;
        rb.linearVelocity = velocity;
        rb.angularVelocity = 0f;

        LimitSpeed();

        if (!activeParticle)
        {
            Register();
            activeParticle = true;
        }
    }

    public void Deactivate()
    {
        if (activeParticle)
        {
            Unregister();
            activeParticle = false;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;
        gameObject.SetActive(false);
    }

    public bool IsExpired(float now)
    {
        return activeParticle && now >= expireAt;
    }

    private void Register()
    {
        registryIndex = Registry.Count;
        Registry.Add(this);
    }

    private void Unregister()
    {
        int last = Registry.Count - 1;
        if (registryIndex < 0 || last < 0) return;

        WaterParticleFluid2D lastItem = Registry[last];
        Registry[registryIndex] = lastItem;
        lastItem.registryIndex = registryIndex;

        Registry.RemoveAt(last);
        registryIndex = -1;
    }

    public static void RunGlobalSolver(float dt)
    {
        if (lastSolveFrame == Time.frameCount) return;
        lastSolveFrame = Time.frameCount;

        int count = Registry.Count;
        if (count <= 1) return;

        for (int i = 0; i < bucketHead.Length; i++) bucketHead[i] = -1;

        float cellSize = Registry[0].settings.Radius;
        float invCellSize = 1f / Mathf.Max(0.05f, cellSize);

        for (int i = 0; i < count; i++)
        {
            WaterParticleFluid2D p = Registry[i];
            p.cachedPos = p.rb.position;
            p.cachedVel = p.rb.linearVelocity;
            p.cellX = Mathf.FloorToInt(p.cachedPos.x * invCellSize);
            p.cellY = Mathf.FloorToInt(p.cachedPos.y * invCellSize);

            int h = Hash(p.cellX, p.cellY);
            p.nextInBucket = bucketHead[h];
            bucketHead[h] = i;
        }

        for (int i = 0; i < count; i++)
        {
            WaterParticleFluid2D p = Registry[i];
            Vector2 pos = p.cachedPos;
            Vector2 vel = p.cachedVel;

            Vector2 center = Vector2.zero;
            Vector2 avgVel = Vector2.zero;
            Vector2 pressureDir = Vector2.zero;
            int neighbors = 0;

            for (int ox = -1; ox <= 1; ox++)
            {
                int qx = p.cellX + ox;
                for (int oy = -1; oy <= 1; oy++)
                {
                    int qy = p.cellY + oy;
                    int h = Hash(qx, qy);

                    int idx = bucketHead[h];
                    while (idx >= 0)
                    {
                        WaterParticleFluid2D other = Registry[idx];
                        idx = other.nextInBucket;

                        if (other == p) continue;
                        if (other.cellX != qx || other.cellY != qy) continue;

                        Vector2 delta = other.cachedPos - pos;
                        float sqr = delta.sqrMagnitude;
                        if (sqr < 0.000001f || sqr > p.settings.RadiusSqr) continue;

                        float dist = Mathf.Sqrt(sqr);
                        center += other.cachedPos;
                        avgVel += other.cachedVel;

                        if (dist < p.settings.RestDistance)
                        {
                            float t = 1f - (dist / p.settings.RestDistance);
                            pressureDir -= (delta / dist) * t;
                        }

                        neighbors++;
                        if (neighbors >= p.settings.MaxNeighbors) break;
                    }

                    if (neighbors >= p.settings.MaxNeighbors) break;
                }

                if (neighbors >= p.settings.MaxNeighbors) break;
            }

            if (neighbors == 0) continue;

            center /= neighbors;
            avgVel /= neighbors;

            Vector2 cohesionForce = (center - pos) * p.settings.Cohesion;
            Vector2 viscosityForce = (avgVel - vel) * p.settings.Viscosity;
            Vector2 pressureForce = pressureDir * p.settings.Pressure;

            Vector2 force = cohesionForce + viscosityForce + pressureForce;
            if (force.sqrMagnitude > p.settings.MaxForce * p.settings.MaxForce)
            {
                force = force.normalized * p.settings.MaxForce;
            }

            p.rb.AddForce(force, ForceMode2D.Force);
            p.LimitSpeed();
        }
    }

    private void LimitSpeed()
    {
        float max = settings.MaxSpeed;
        if (max <= 0f) return;

        Vector2 v = rb.linearVelocity;
        float sqr = v.sqrMagnitude;
        if (sqr <= max * max) return;

        rb.linearVelocity = v.normalized * max;
    }

    private static int Hash(int x, int y)
    {
        unchecked
        {
            int h = x * 73856093 ^ y * 19349663;
            return h & BucketMask;
        }
    }
}




