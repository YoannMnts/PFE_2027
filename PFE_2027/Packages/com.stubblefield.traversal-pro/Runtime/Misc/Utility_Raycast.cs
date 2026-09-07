using System.Collections.Generic;
using UnityEngine;

namespace TraversalPro
{
    public static partial class Utility
    {
        internal static RaycastHit Capsulecast(
            in Capsule capsule,
            Vector3 direction,
            RaycastHit[] hits,
            float maxDistance,
            LayerMask layerMask,
            HashSet<EntityId> ignoredColliderIds,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            int hitCount = Physics.CapsuleCastNonAlloc(
                capsule.lower,
                capsule.upper,
                capsule.radius,
                direction,
                hits,
                maxDistance,
                layerMask,
                queryTriggerInteraction);
            RemoveInitialIntersectionHits(hits, ref hitCount, direction);
            RemoveIgnoredColliders(hits, ref hitCount, ignoredColliderIds);
            return NearestHit(hits, hitCount);
        }

        internal static (float staticFriction, float dynamicFriction) GetFriction(
            in Collider collider,
            float defaultStaticFriction = DefaultStaticFriction,
            float defaultDynamicFriction = DefaultStaticFriction)
        {
            if (collider)
            {
                var material = collider.sharedMaterial;
                if (material)
                {
                    return (Mathf.Clamp01(material.staticFriction), Mathf.Clamp01(material.dynamicFriction));
                }
                return (Mathf.Clamp01(defaultStaticFriction), Mathf.Clamp01(defaultDynamicFriction));
            }
            return (0, 0);
        }

        /// <summary>
        /// Checks if the RaycastHit is the result of a call to one of the shape-castAll or shape-castNonAlloc methods
        /// where it initially intersected some collider at the ray's origin. Note that only "all" and "nonAlloc"
        /// versions of spherecast, boxcast, and capsulecast create these kinds of RaycastHits.
        /// This does not happen for the ray version.
        /// </summary>
        internal static bool IsHitInitialIntersection(in this RaycastHit hit, Vector3 castDirection)
        {
            return hit.point == default
                   & hit.distance == 0
                   & hit.normal == -castDirection;
        }

        internal static bool IsValid(in this RaycastHit hit)
        {
            return hit.colliderEntityId != EntityId.None;
        }

        internal static RaycastHit NearestHit(RaycastHit[] hits, int count)
        {
            float min = float.PositiveInfinity;
            int indexOfNearestHit = -1;
            for (int i = 0; i < count; i++)
            {
                float distance = hits[i].distance;
                if (distance < min)
                {
                    min = distance;
                    indexOfNearestHit = i;
                }
            }

            return indexOfNearestHit >= 0 ? hits[indexOfNearestHit] : default;
        }

        internal static RaycastHit Raycast(
            Vector3 origin,
            Vector3 direction,
            RaycastHit[] hits,
            float maxDistance,
            LayerMask layerMask,
            HashSet<EntityId> ignoredColliderIds,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            int hitCount = Physics.RaycastNonAlloc(
                origin,
                direction,
                hits,
                maxDistance,
                layerMask,
                queryTriggerInteraction);
            RemoveInitialIntersectionHits(hits, ref hitCount, direction);
            RemoveIgnoredColliders(hits, ref hitCount, ignoredColliderIds);
            return NearestHit(hits, hitCount);
        }

        internal static void RemoveHitsWithZeroDistance(RaycastHit[] hits, ref int count)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                float distance = hits[i].distance;
                if (distance <= 0) RemoveAtSwapBack(hits, i, ref count);
            }
        }

        internal static void RemoveIgnoredColliders(Collider[] colliders, ref int count, HashSet<EntityId> ignoredColliderIds)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                if (ignoredColliderIds.Contains(colliders[i].GetEntityId()))
                {
                    RemoveAtSwapBack(colliders, i, ref count);
                }
            }
        }

        internal static void RemoveIgnoredColliders(RaycastHit[] hits, ref int count, HashSet<EntityId> ignoredColliderIds)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                if (ignoredColliderIds.Contains(hits[i].colliderEntityId))
                {
                    RemoveAtSwapBack(hits, i, ref count);
                }
            }
        }

        internal static void RemoveInitialIntersectionHits(RaycastHit[] hits, ref int count, Vector3 castDirection)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                if (hits[i].IsHitInitialIntersection(castDirection))
                {
                    RemoveAtSwapBack(hits, i, ref count);
                }
            }
        }

        internal static void RemoveRaycastHitsAfterCount(RaycastHit[] hits, int count)
        {
            for (int i = count; i < hits.Length; i++)
            {
                hits[i] = default;
            }
        }

        internal struct RaycastHitComparer : IComparer<RaycastHit>
        {
            public int Compare(RaycastHit a, RaycastHit b)
            {
                float aDistance = a.colliderEntityId != EntityId.None ? a.distance : float.MaxValue;
                float bDistance = b.colliderEntityId != EntityId.None ? b.distance : float.MaxValue;
                if (aDistance == bDistance) return 0;
                return aDistance > bDistance ? 1 : -1;
            }
        }

        internal static void SortByDistance(RaycastHit[] hits, int count)
        {
            Sort(hits, new RaycastHitComparer(), count);
        }

        internal static RaycastHit Spherecast(
            Sphere sphere,
            Vector3 direction,
            RaycastHit[] hits,
            float maxDistance,
            LayerMask layerMask,
            HashSet<EntityId> ignoredColliderIds,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            int hitCount = Physics.SphereCastNonAlloc(
                sphere.center,
                sphere.radius,
                direction,
                hits,
                maxDistance,
                layerMask,
                queryTriggerInteraction);
            RemoveInitialIntersectionHits(hits, ref hitCount, direction);
            RemoveIgnoredColliders(hits, ref hitCount, ignoredColliderIds);
            return NearestHit(hits, hitCount);
        }

        internal static bool TryGetSurfaceNormal(
            Vector3 direction,
            in RaycastHit hit,
            out Vector3 normal,
            float distance = .01f)
        {
            if (hit.IsValid())
            {
                Ray ray = new(hit.point, direction);
                ray.origin -= ray.direction * distance;
                if (hit.collider.Raycast(ray, out RaycastHit rayHit, distance * 2))
                {
                    normal = rayHit.normal;
                    return true;
                }
            }
            normal = default;
            return false;
        }

        internal static float TravelDistance(in RaycastHit hit, float maxDistance)
        {
            return hit.colliderEntityId == EntityId.None ? maxDistance : hit.distance;
        }
    }
}