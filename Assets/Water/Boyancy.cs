using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Buoyancy2D : MonoBehaviour
{
    [Header("Water")]
    [SerializeField] private string waterLayerName = "Fluid";
    [SerializeField] private float probeRadius = 0.06f;      // 探测半径：水粒子稀就调大
    [SerializeField] private int samplePoints = 8;           // 6~12 比较稳

    [Header("Buoyancy")]
    [SerializeField] private float buoyancyMultiplier = 1.2f; // 1=刚好悬浮，>1 会浮起
    [SerializeField] private float maxUpForce = 500f;

    [Header("Water Damping")]
    [SerializeField] private float linearDampingInWater = 2.5f;
    [SerializeField] private float angularDampingInWater = 1.5f;

    private Rigidbody2D rb;
    private Collider2D col;
    private int waterLayer;
    private int waterMask;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        ResolveWaterLayer();
    }

    private void FixedUpdate()
    {
        Bounds b = col.bounds;

        int hitCount = 0;
        Vector2 hitPointSum = Vector2.zero;

        // 在底部 1/3 区域横向采样
        float y = b.min.y + b.size.y * 0.25f;

        for (int i = 0; i < samplePoints; i++)
        {
            float t = (samplePoints == 1) ? 0.5f : (float)i / (samplePoints - 1);
            Vector2 p = new Vector2(Mathf.Lerp(b.min.x, b.max.x, t), y);

            // 只要附近有水粒子，就算浸没
            if (Physics2D.OverlapCircle(p, probeRadius, waterMask) != null)
            {
                hitCount++;
                hitPointSum += p;
            }
        }

        if (hitCount == 0) return;

        float submerged01 = (float)hitCount / samplePoints;
        Vector2 forcePoint = hitPointSum / hitCount;

        // 需要抵消的重量
        float weight = rb.mass * (-Physics2D.gravity.y) * rb.gravityScale;

        // 浮力：重量 * 浸没比例 * 倍数
        float upForce = weight * submerged01 * buoyancyMultiplier;
        upForce = Mathf.Min(upForce, maxUpForce);

        rb.AddForceAtPosition(Vector2.up * upForce, forcePoint, ForceMode2D.Force);

        // 水中阻尼，防止抖动/乱翻
        rb.AddForce(-rb.linearVelocity * linearDampingInWater * submerged01, ForceMode2D.Force);
        rb.AddTorque(-rb.angularVelocity * angularDampingInWater * submerged01, ForceMode2D.Force);
    }

    private void ResolveWaterLayer()
    {
        waterLayer = LayerMask.NameToLayer(waterLayerName);
        if (waterLayer < 0) waterLayer = LayerMask.NameToLayer("Fluid");
        if (waterLayer < 0) waterLayer = LayerMask.NameToLayer("Water");

        if (waterLayer >= 0)
        {
            waterMask = 1 << waterLayer;
            return;
        }

        // Fallback to all layers so buoyancy still works when layer names are misconfigured.
        waterMask = Physics2D.AllLayers;
        Debug.LogWarning($"{nameof(Buoyancy2D)} on {name}: water layer not found. Using all layers for overlap checks.");
    }
}
