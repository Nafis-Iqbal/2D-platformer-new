using UnityEngine;

public class GrapplingRope : MonoBehaviour
{
    [Header("General References:")]
    public PlayerGrapplingGun playerGrapplingGun;
    public LineRenderer lineRenderer;

    [Header("General Settings:")]
    [SerializeField] private int precision = 40;
    [Range(0, 20)] [SerializeField] private float straightenLineSpeed = 5;

    [Header("Rope Animation Settings:")]
    public AnimationCurve ropeAnimationCurve;
    [Range(0.01f, 4)] [SerializeField] private float StartWaveSize = 2;
    float waveSize = 0;

    [Header("Rope Progression:")]
    public AnimationCurve ropeProgressionCurve;
    [SerializeField] [Range(1, 50)] private float ropeProgressionSpeed = 1;

    float moveTime = 0;

    [HideInInspector] public bool isGrappling = true;

    bool straightLine = true;

    private void OnEnable()
    {
        moveTime = 0;
        lineRenderer.positionCount = precision;
        waveSize = StartWaveSize;
        straightLine = false;

        LinePointsToFirePoint();

        lineRenderer.enabled = true;
    }

    private void OnDisable()
    {
        lineRenderer.enabled = false;
        isGrappling = false;
        playerGrapplingGun.PlayerReleasedRope();
    }

    private void LinePointsToFirePoint()
    {
        for (int i = 0; i < precision; i++)
        {
            lineRenderer.SetPosition(i, playerGrapplingGun.firePoint.position);
        }
    }

    private void Update()
    {
        moveTime += Time.deltaTime;
        DrawRope();
    }

    void DrawRope()
    {
        if (!straightLine)
        {
            if (lineRenderer.GetPosition(precision - 1).x == playerGrapplingGun.grapplePoint.x)
            {
                straightLine = true;
            }
            else
            {
                DrawRopeWaves();
            }
        }
        else
        {
            if (!isGrappling)
            {
                playerGrapplingGun.Grapple();
                isGrappling = true;
            }
            if (waveSize > 0)
            {
                waveSize -= Time.deltaTime * straightenLineSpeed;
                DrawRopeWaves();
            }
            else
            {
                waveSize = 0;

                if (lineRenderer.positionCount != 2) { lineRenderer.positionCount = 2; }

                DrawRopeNoWaves();
            }
        }
    }

    void DrawRopeWaves()
    {
        for (int i = 0; i < precision; i++)
        {
            float delta = (float)i / ((float)precision - 1f);
            Vector2 offset = Vector2.Perpendicular(playerGrapplingGun.grappleDistanceVector).normalized * ropeAnimationCurve.Evaluate(delta) * waveSize;
            Vector2 targetPosition = Vector2.Lerp(playerGrapplingGun.firePoint.position, playerGrapplingGun.grapplePoint, delta) + offset;
            Vector2 currentPosition = Vector2.Lerp(playerGrapplingGun.firePoint.position, targetPosition, ropeProgressionCurve.Evaluate(moveTime) * ropeProgressionSpeed);

            lineRenderer.SetPosition(i, currentPosition);
        }
    }

    void DrawRopeNoWaves()
    {
        lineRenderer.SetPosition(0, playerGrapplingGun.firePoint.position);
        lineRenderer.SetPosition(1, playerGrapplingGun.grapplePoint);
    }
}
