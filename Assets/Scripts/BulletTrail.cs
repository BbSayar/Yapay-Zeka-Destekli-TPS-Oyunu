using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BulletTrail : MonoBehaviour
{
    private LineRenderer line;

    public float fadeDuration = 0.5f;
    private float fadeTimer;

    private Color startColor;
    private Color endColor;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        fadeTimer = fadeDuration;

        startColor = line.startColor;
        endColor = line.endColor;
    }

    public void SetPoints(Vector3 startPoint, Vector3 endPoint)
    {
        line.SetPosition(0, startPoint);
        line.SetPosition(1, endPoint);
    }

    void Update()
    {
        fadeTimer -= Time.deltaTime;

        float alpha = Mathf.Clamp01(fadeTimer / fadeDuration);

        line.startColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
        line.endColor = new Color(endColor.r, endColor.g, endColor.b, alpha);

        if (fadeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}