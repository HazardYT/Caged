using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Knife.HDRPOutline.Core;

public class OutlineScaling : MonoBehaviour
{
    [SerializeField] private OutlineObject mat;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private float minDistance = 1f;
    private Color ogColor;
    private void Awake() { ogColor = mat.Color; }
    private void Update()
    {
        Collider[] playerCheck = Physics.OverlapSphere(transform.position, maxDistance, playerLayer);

        if (playerCheck.Length > 0)
        {
            float closestDistance = maxDistance;
            foreach (Collider playerCollider in playerCheck)
            {
                float distance = Vector3.Distance(transform.position, playerCollider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                }
            }

            float alpha = Mathf.InverseLerp(maxDistance, minDistance, closestDistance);
            SetOutlineAlpha(alpha);
        }
        else
        {
            SetOutlineAlpha(0);
        }
    }

    private void SetOutlineAlpha(float alpha)
    {
        Color newColor = mat.Color;
        newColor.a = alpha;
        mat.Color = newColor;
    }
}