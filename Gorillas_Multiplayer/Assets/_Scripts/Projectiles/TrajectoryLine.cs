using System.Collections.Generic;
using UnityEngine;

public class TrajectoryLine : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Transform _lineSpawnPoint;
    private float _gravity;
    [SerializeField] private int _maxNumPoints = 100;
    [SerializeField] private float _timeStep = 0.1f;
    private List<Vector3> _segmentsList = new();
    private Vector3[] _segments;
    private bool _trajectoryLineVisible = false;

    private void Start()
    {
        HideTrajectoryLine();
        _gravity = -Physics2D.gravity.y;
    }

    public void CalculateTrajectoryLine(float power, float angle, int direction, float forceMultiplier)
    {
        float totalTime = 0f;
        _segmentsList.Clear();

        power *= forceMultiplier;

        // set the start position of the line renderer
        Vector3 startPos = _lineSpawnPoint.position;
        Vector3 previousPos = startPos;
        bool pathComplete = false;
        Vector3 zenith = new(0f, -Mathf.Infinity, 0f);
        Vector3 newPos = Vector3.zero, rayDir;
        float rayDistance = 0f;
        RaycastHit2D[] hits;
        bool containsMask;

        for (int i = 0; i < _maxNumPoints; i++)
        {
            float angleRad = Mathf.Deg2Rad * angle;
            float vx = power * Mathf.Cos(angleRad);
            float vy = power * Mathf.Sin(angleRad);

            float x = vx * totalTime * direction;
            float y = vy * totalTime - 0.5f * _gravity * totalTime * totalTime;

            totalTime += _timeStep;
            newPos = startPos + new Vector3(x, y, 0f);
            rayDir = newPos - previousPos;
            rayDistance = Vector3.Distance(previousPos, newPos);
            hits = Physics2D.CircleCastAll(previousPos, 0.05f, rayDir, rayDistance);
            containsMask = false;

            // if the point is at a higher Y-value than currently saved, update it so we can use it as the highest point for the camera to track
            if (newPos.y > zenith.y)
            {
                zenith = newPos;
                // add the new zenith
                CameraManager.Instance.SetProjectileZenithRpc(zenith);
            }

            foreach (var hit in hits)
            {
                // if we hit a mask, we can keep going and ignore any ground or player hits
                if (hit.transform.CompareTag("ExplosionMask"))
                {
                    containsMask = true;
                    break;
                }
            }

            if (!containsMask)
            {
                foreach (var hit in hits)
                {
                    // if there is no mask, we can check for other hits
                    if (hit.transform.CompareTag("Ground") || hit.transform.CompareTag("Player"))
                    {
                        pathComplete = true;
                    }
                }
            }

            previousPos = newPos;
            _segmentsList.Add(newPos);

            // check to see if the line has gone below some depth
            if (newPos.y < -15f) pathComplete = true;

            if (pathComplete) break;
        }
        _segments = _segmentsList.ToArray();
    }

    public void DrawTrajectoryLine()
    {
        _trajectoryLineVisible = true;
        _lineRenderer.positionCount = _segments.Length;
        _lineRenderer.SetPositions(_segments);
    }

    public void HideTrajectoryLine()
    {
        if (_trajectoryLineVisible)
        {
            _lineRenderer.positionCount = 0;
            _trajectoryLineVisible = false;
        }
    }
}
