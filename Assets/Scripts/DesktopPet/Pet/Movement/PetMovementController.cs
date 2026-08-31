using DesktopPet.Config;
using UnityEngine;

namespace DesktopPet.Pet.Movement
{
    public sealed class PetMovementController : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private Transform bedPoint;
        [SerializeField] private Transform foodPoint;
        [SerializeField] private Transform cameraPoint;
        private PetTuningConfig tuning;
        private Vector3 target;
        private float deadline;

        public bool IsMoving { get; private set; }
        public Vector3 Target => target;
        public Transform BedPoint => bedPoint;

        public void Initialize(PetTuningConfig config) => tuning = config;

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
            deadline = Time.time + tuning.movementTimeout;
            IsMoving = true;
            return true;
        }

        public void Stop() => IsMoving = false;

        private void Update()
        {
            if (!IsMoving || tuning == null) return;
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
    }
}
