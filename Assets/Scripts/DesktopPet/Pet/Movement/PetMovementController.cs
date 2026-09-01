using DesktopPet.Config;
using UnityEngine;

namespace DesktopPet.Pet.Movement
{
    public sealed class PetMovementController : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private Transform bedPoint;
        [SerializeField] private Transform foodPoint;
        [SerializeField] private Transform toiletPoint;
        [SerializeField] private Transform cameraPoint;
        private PetTuningConfig tuning;
        private Vector3 target;
        private float deadline;
        private Renderer[] petRenderers;

        public bool IsMoving { get; private set; }
        public Vector3 Target => target;
        public Transform BedPoint => bedPoint;
        public Transform FoodPoint => foodPoint;
        public Transform ToiletPoint => toiletPoint;

        public void Initialize(PetTuningConfig config)
        {
            tuning = config;
            petRenderers = GetComponentsInChildren<Renderer>();
            if (foodPoint == null) foodPoint = FindScenePoint("FoodBowlPoint");
            if (toiletPoint == null) toiletPoint = FindScenePoint("ToiletPoint");
            KeepPetInsideCamera();
        }

        private static Transform FindScenePoint(string objectName)
        {
            var point = GameObject.Find(objectName);
            return point != null ? point.transform : null;
        }

        public bool MoveToRandomPoint()
        {
            if (waypoints != null && waypoints.Length > 0)
            {
                for (var attempt = 0; attempt < waypoints.Length; attempt++)
                {
                    var point = waypoints[Random.Range(0, waypoints.Length)];
                    if (point != null && Vector3.Distance(transform.position, point.position) > 0.2f)
                        return MoveTo(point.position);
                }
            }

            var offset = Random.insideUnitCircle * tuning.wanderRadius;
            return MoveTo(transform.position + new Vector3(offset.x, 0f, offset.y));
        }

        public bool MoveToBed() => bedPoint != null && MoveTo(bedPoint.position);
        public bool MoveToFood() => MoveTo(foodPoint != null ? foodPoint.position : transform.position + transform.forward * 0.5f);
        public bool MoveToCamera()
        {
            if (cameraPoint != null) return MoveTo(cameraPoint.position);
            var camera = Camera.main;
            if (camera == null) return false;
            var point = camera.transform.position + camera.transform.forward * 1.2f;
            point.y = transform.position.y;
            return MoveTo(point);
        }

        public bool MoveTo(Vector3 worldPosition)
        {
            target = worldPosition;
            target.y = transform.position.y;
            target = ConstrainToRoom(target);
            target = ConstrainToCamera(target);
            deadline = Time.time + tuning.movementTimeout;
            IsMoving = true;
            return true;
        }

        public void Stop() => IsMoving = false;

        private void Update()
        {
            if (tuning == null) return;
            KeepPetInsideCamera();
            if (!IsMoving) return;
            var delta = target - transform.position;
            delta.y = 0f;
            if (delta.magnitude <= tuning.arrivalDistance || Time.time >= deadline)
            {
                IsMoving = false;
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, target, tuning.walkSpeed * Time.deltaTime);
            if (delta.sqrMagnitude > 0.0001f)
            {
                var desired = Quaternion.LookRotation(delta.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, tuning.turnSpeed * Time.deltaTime);
            }
        }

        private void KeepPetInsideCamera()
        {
            var safePosition = ConstrainToCamera(transform.position);
            safePosition.y = transform.position.y;
            if ((safePosition - transform.position).sqrMagnitude > 0.000001f)
                transform.position = safePosition;
        }

        private Vector3 ConstrainToCamera(Vector3 desiredPosition)
        {
            var camera = Camera.main;
            if (camera == null || petRenderers == null || petRenderers.Length == 0) return desiredPosition;

            var bounds = petRenderers[0].bounds;
            for (var i = 1; i < petRenderers.Length; i++)
                if (petRenderers[i] != null && petRenderers[i].enabled) bounds.Encapsulate(petRenderers[i].bounds);

            var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            for (var x = -1; x <= 1; x += 2)
            for (var y = -1; y <= 1; y += 2)
            for (var z = -1; z <= 1; z += 2)
            {
                var corner = bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z));
                var viewport = camera.WorldToViewportPoint(corner);
                if (viewport.z <= 0f) return transform.position;
                min = Vector2.Min(min, viewport);
                max = Vector2.Max(max, viewport);
            }

            var currentCenter = camera.WorldToViewportPoint(bounds.center);
            var projectedTargetCenter = camera.WorldToViewportPoint(bounds.center + desiredPosition - transform.position);
            if (projectedTargetCenter.z <= 0f) return transform.position;
            var leftPadding = currentCenter.x - min.x + tuning.screenSafeMargin;
            var rightPadding = max.x - currentCenter.x + tuning.screenSafeMargin;
            var bottomPadding = currentCenter.y - min.y + tuning.screenSafeMargin;
            var topPadding = max.y - currentCenter.y + tuning.screenSafeMargin;
            projectedTargetCenter.x = Mathf.Clamp(projectedTargetCenter.x, leftPadding, 1f - rightPadding);
            projectedTargetCenter.y = Mathf.Clamp(projectedTargetCenter.y, bottomPadding, 1f - topPadding);

            var constrainedCenter = camera.ViewportToWorldPoint(projectedTargetCenter);
            var centerOffset = bounds.center - transform.position;
            var result = constrainedCenter - centerOffset;
            result.y = desiredPosition.y;
            return result;
        }

        private Vector3 ConstrainToRoom(Vector3 position)
        {
            if (!tuning.constrainToRoom) return position;
            position.x = Mathf.Clamp(position.x,
                tuning.roomCenter.x - tuning.roomHalfExtents.x,
                tuning.roomCenter.x + tuning.roomHalfExtents.x);
            position.z = Mathf.Clamp(position.z,
                tuning.roomCenter.y - tuning.roomHalfExtents.y,
                tuning.roomCenter.y + tuning.roomHalfExtents.y);
            return position;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(target, 0.08f);
        }
#endif
    }
}
