using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TraversalPro
{
    /// <summary>
    /// This class is a workaround for an issue in Unity's TerrainCollider collision detection. When/if that issue gets
    /// resolved by Unity, this class will no longer be needed. The issue is that rigidbodies on a TerrainCollider
    /// sometimes miss contacts and fall through the collider then pop up a few frames later when contact resumes.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class TerrainCollisionCorrection : MonoBehaviour
    {
        Rigidbody rb;
        CapsuleCollider cap;
        IFreeFall freeFall;
        readonly HashSet<EntityId> contactIds = new();
        Vector3 priorVelocity;
        const float tolerance = .01f;

        void OnEnable()
        {
            rb = GetComponent<Rigidbody>();
            cap = GetComponent<CapsuleCollider>();
            freeFall = GetComponent<IFreeFall>();
            StartCoroutine(LateFixedUpdate());
        }

        void FixedUpdate()
        {
            contactIds.Clear();
            priorVelocity = rb.linearVelocity;
        }

        void OnCollisionStay(Collision other)
        {
            EntityId otherId = other.collider.GetEntityId();
            if (other.collider is TerrainCollider)
            {
                contactIds.Add(otherId);
            }
        }

        IEnumerator LateFixedUpdate()
        {
            WaitForFixedUpdate waitForFixedUpdate = new();
            while (isActiveAndEnabled)
            {
                yield return waitForFixedUpdate;
                Vector3 center = rb.position + cap.center;
                float cylinderHeight = cap.height - cap.radius - cap.radius;
                Vector3 origin = center + Vector3.up * (cylinderHeight * .5f);
                if (Physics.SphereCast(
                        origin, 
                        cap.radius - tolerance, 
                        Vector3.down, 
                        out RaycastHit hit,
                        cylinderHeight + tolerance))
                {
                    if (!contactIds.Contains(hit.colliderEntityId)
                        && hit.collider is TerrainCollider)
                    {
                        Vector3 vel = rb.linearVelocity;
                        Vector3 gravity = freeFall?.Gravity ?? Physics.gravity;
                        if (vel.y >= gravity.y * Time.deltaTime * 3)
                        {
                            rb.position += Vector3.up * (cylinderHeight + tolerance - hit.distance);
                            vel.y = Mathf.Max(priorVelocity.y, vel.y);
                            rb.SetVelocity(vel);
                        }
                    }
                }
            }
        }

        void OnValidate()
        {
            rb = GetComponent<Rigidbody>();
            rb.constraints |= RigidbodyConstraints.FreezeRotation;
            Transform parent = transform;
            while (parent)
            {
                parent.localScale = Vector3.one;
                parent = parent.parent;
            }

            cap = GetComponent<CapsuleCollider>();
            cap.direction = 1;
        }
    }
}