using UnityEngine;
using System.Collections;

namespace UnityStandardAssets.Cameras
{
    public class TargetFieldOfView : AbstractTargetFollower
    {
        [SerializeField] private float m_FovAdjustTime = 1;
        [SerializeField] private float m_ZoomAmountMultiplier = 2;
        [SerializeField] private bool m_IncludeEffectsInSize = false;

        private float m_BoundSize;
        private float m_FovAdjustVelocity;
        private Camera m_Cam;
        private Transform m_LastTarget;

        protected override void Start()
        {
            base.Start();
            m_BoundSize = MaxBoundsExtent(m_Target, m_IncludeEffectsInSize);

            // get a reference to the actual camera component:
            m_Cam = GetComponentInChildren<Camera>();
        }

        protected override void FollowTarget(float deltaTime)
        {
            // calculate the correct field of view to fit the bounds size at the current distance
            float dist = (m_Target.position - transform.position).magnitude;
            float requiredFOV = Mathf.Atan2(m_BoundSize, dist) * Mathf.Rad2Deg * m_ZoomAmountMultiplier;

            m_Cam.fieldOfView = Mathf.SmoothDamp(m_Cam.fieldOfView, requiredFOV, ref m_FovAdjustVelocity, m_FovAdjustTime);
        }

        public override void SetTarget(Transform newTransform)
        {
            base.SetTarget(newTransform);
            m_BoundSize = MaxBoundsExtent(newTransform, m_IncludeEffectsInSize);
        }

        public static float MaxBoundsExtent(Transform obj, bool includeEffects)
        {
            // get the maximum bounds extent of object, including all child renderers,
            // but excluding particles and trails, for FOV zooming effect.

            var renderers = obj.GetComponentsInChildren<Renderer>();

            Bounds bounds = new Bounds();
            bool initBounds = false;
            foreach (Renderer r in renderers)
            {
                if (!((r is TrailRenderer) || (r is ParticleSystemRenderer)))
                {
                    if (!initBounds)
                    {
                        initBounds = true;
                        bounds = r.bounds;
                    }
                    else
                    {
                        bounds.Encapsulate(r.bounds);
                    }
                }
            }
            float max = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
            return max;
        }
    }
}
