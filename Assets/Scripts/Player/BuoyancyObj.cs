using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace WaterSystem
{
    public class BuoyancyObj : MonoBehaviour
    {
        // --- Buoyancy Settings ---
        public BuoyancyType _buoyancyType;
        public float density;
        public float volume;
        public float voxelResolution = 0.51f;
        private Bounds _voxelBounds;
        public Vector3 centerOfMass = Vector3.zero;
        public float waterLevelOffset = 0f;

        private const float Dampner = 0.005f;
        private const float WaterDensity = 1000;

        private float _baseDrag;
        private float _baseAngularDrag;
        private int _guid;
        private float3 _localArchimedesForce;

        private Vector3[] _voxels;
        private NativeArray<float3> _samplePoints;
        [NonSerialized] public float3[] Heights;
        private float3[] _normals;
        private float3[] _velocity;
        [SerializeField] Collider[] colliders;
        private Rigidbody _rb;
        private DebugDrawing[] _debugInfo;
        [NonSerialized] public float PercentSubmerged;

        // Set to true by PickupScript when this object is held.
        // Suspends all buoyancy forces and position overrides while true.
        [NonSerialized] public bool isHeld = false;

        // --- Dynamic Water Tracking ---
        [Header("Water Settings")]
        public Transform waterTransform;
        public float waterRiseSpeed = 1f;
        private float currentWaterHeight;

        [ContextMenu("Initialize")]
        private void Init()
        {
            _voxels = null;

            switch (_buoyancyType)
            {
                case BuoyancyType.NonPhysical:
                    SetupVoxels();
                    SetupData();
                    break;
                case BuoyancyType.NonPhysicalVoxel:
                    SetupColliders();
                    SetupVoxels();
                    SetupData();
                    break;
                case BuoyancyType.Physical:
                    SetupVoxels();
                    SetupData();
                    SetupPhysical();
                    break;
                case BuoyancyType.PhysicalVoxel:
                    SetupColliders();
                    SetupVoxels();
                    SetupData();
                    SetupPhysical();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetupVoxels()
        {
            if (_buoyancyType == BuoyancyType.NonPhysicalVoxel || _buoyancyType == BuoyancyType.PhysicalVoxel)
            {
                SliceIntoVoxels();
            }
            else
            {
                _voxels = new Vector3[1];
                _voxels[0] = centerOfMass;
            }
        }

        private void SetupData()
        {
            _debugInfo = new DebugDrawing[_voxels.Length];
            Heights = new float3[_voxels.Length];
            _normals = new float3[_voxels.Length];
            _samplePoints = new NativeArray<float3>(_voxels.Length, Allocator.Persistent);
        }

        private void OnEnable()
        {
            _guid = gameObject.GetInstanceID();
            Init();
            LocalToWorldConversion();
        }

        private void SetupColliders()
        {
            colliders = GetComponentsInChildren<Collider>();
            if (colliders.Length != 0) return;

            colliders = new Collider[1];
            colliders[0] = gameObject.AddComponent<BoxCollider>();
            Debug.LogError($"Buoyancy:Object \"{name}\" had no Collider. BoxCollider has been added.");
        }

        private void Update()
        {
            // Buoyancy fully suspended while object is held by player
            if (isHeld) return;

#if STATIC_EVERYTHING
            var dt = 0.0f;
#else
            var dt = Time.deltaTime;
#endif

            if (waterTransform != null)
            {
                float targetHeight = waterTransform.position.y + waterLevelOffset;
                currentWaterHeight = Mathf.Lerp(currentWaterHeight, targetHeight, waterRiseSpeed * dt);
            }

            switch (_buoyancyType)
            {
                case BuoyancyType.NonPhysical:
                    {
                        var t = transform;
                        if (waterTransform != null)
                        {
                            float objectBottomY = t.position.y - _voxelBounds.extents.y;
                            if (objectBottomY < currentWaterHeight)
                            {
                                var vec = t.position;
                                vec.y = Mathf.Max(vec.y, currentWaterHeight);
                                t.position = vec;
                                t.up = Vector3.Slerp(t.up, _normals[0], dt);
                            }
                        }
                        break;
                    }
                case BuoyancyType.NonPhysicalVoxel:
                    break;
                case BuoyancyType.Physical:
                case BuoyancyType.PhysicalVoxel:
                    LocalToWorldJob.CompleteJob(_guid);
                    GetVelocityPoints();
                    if (waterTransform != null)
                    {
                        GerstnerWavesJobs.UpdateSamplePoints(ref _samplePoints, _guid);
                        GerstnerWavesJobs.GetData(_guid, ref Heights, ref _normals);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void FixedUpdate()
        {
            // Buoyancy forces suspended while object is held by player
            if (isHeld) return;

            if (waterTransform == null) return;

            float waterHeight = currentWaterHeight;

            float objectBottomY = transform.position.y - _voxelBounds.extents.y;
            if (objectBottomY >= waterHeight) return;

            float submergedAmount = 0f;

            switch (_buoyancyType)
            {
                case BuoyancyType.PhysicalVoxel:
                    LocalToWorldJob.CompleteJob(_guid);
                    Physics.autoSyncTransforms = false;

                    for (int i = 0; i < _voxels.Length; i++)
                        BuoyancyForce(_samplePoints[i], _velocity[i], waterHeight, ref submergedAmount, ref _debugInfo[i]);

                    Physics.SyncTransforms();
                    Physics.autoSyncTransforms = true;
                    UpdateDrag(submergedAmount);
                    break;

                case BuoyancyType.Physical:
                    BuoyancyForce(Vector3.zero, _velocity[0], waterHeight, ref submergedAmount, ref _debugInfo[0]);
                    UpdateDrag(submergedAmount);
                    break;

                default:
                    break;
            }
        }

        private void LateUpdate() => LocalToWorldConversion();

        private void OnDestroy() => CleanUp();

        private void CleanUp()
        {
            if (_buoyancyType == BuoyancyType.Physical || _buoyancyType == BuoyancyType.PhysicalVoxel)
                LocalToWorldJob.Cleanup(_guid);
            else
                _samplePoints.Dispose();
        }

        private void LocalToWorldConversion()
        {
            if (_buoyancyType != BuoyancyType.Physical && _buoyancyType != BuoyancyType.PhysicalVoxel) return;

            var transformMatrix = transform.localToWorldMatrix;
            LocalToWorldJob.ScheduleJob(_guid, transformMatrix);
        }

        private void BuoyancyForce(Vector3 position, float3 velocity, float waterHeight, ref float submergedAmount, ref DebugDrawing debug)
        {
            debug.Position = position;
            debug.WaterHeight = waterHeight;
            debug.Force = Vector3.zero;

            if (!(position.y - voxelResolution < waterHeight)) return;

            var k = math.clamp(waterHeight - (position.y - voxelResolution), 0f, 1f);
            submergedAmount += k / _voxels.Length;

            var localDampingForce = Dampner * _rb.mass * -velocity;
            var force = localDampingForce + math.sqrt(k) * _localArchimedesForce;
            _rb.AddForceAtPosition(force, position);

            debug.Force = force;
        }

        private void UpdateDrag(float submergedAmount)
        {
            PercentSubmerged = math.lerp(PercentSubmerged, submergedAmount, 0.25f);
            _rb.drag = _baseDrag + _baseDrag * (PercentSubmerged * 10f);
            _rb.angularDrag = _baseAngularDrag + PercentSubmerged * 0.5f;
        }

        private void GetVelocityPoints()
        {
            for (int i = 0; i < _voxels.Length; i++)
                _velocity[i] = _rb.GetPointVelocity(_samplePoints[i]);
        }

        private void SliceIntoVoxels()
        {
            var t = transform;
            var rot = t.rotation;
            var pos = t.position;
            var size = t.localScale;
            t.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            t.localScale = Vector3.one;

            _voxels = null;
            var points = new List<Vector3>();

            var rawBounds = VoxelBounds();
            _voxelBounds = rawBounds;
            _voxelBounds.size = RoundVector(rawBounds.size, voxelResolution);

            for (float ix = -_voxelBounds.extents.x; ix < _voxelBounds.extents.x; ix += voxelResolution)
                for (float iy = -_voxelBounds.extents.y; iy < _voxelBounds.extents.y; iy += voxelResolution)
                    for (float iz = -_voxelBounds.extents.z; iz < _voxelBounds.extents.z; iz += voxelResolution)
                    {
                        var x = (voxelResolution * 0.5f) + ix;
                        var y = (voxelResolution * 0.5f) + iy;
                        var z = (voxelResolution * 0.5f) + iz;

                        var p = new Vector3(x, y, z) + _voxelBounds.center;

                        bool inside = false;
                        foreach (var c in colliders)
                        {
                            if (PointIsInsideCollider(c, p))
                                inside = true;
                        }

                        if (inside) points.Add(p);
                    }

            _voxels = points.ToArray();
            t.SetPositionAndRotation(pos, rot);
            t.localScale = size;

            float voxelVolume = Mathf.Pow(voxelResolution, 3f) * _voxels.Length;
            float rawVolume = rawBounds.size.x * rawBounds.size.y * rawBounds.size.z;
            volume = Mathf.Min(rawVolume, voxelVolume);
            density = gameObject.GetComponent<Rigidbody>().mass / volume;
        }

        private Bounds VoxelBounds()
        {
            var bounds = new Bounds();
            foreach (var c in colliders) bounds.Encapsulate(c.bounds);
            return bounds;
        }

        private static Vector3 RoundVector(Vector3 vec, float rounding) =>
            new Vector3(Mathf.Ceil(vec.x / rounding) * rounding,
                        Mathf.Ceil(vec.y / rounding) * rounding,
                        Mathf.Ceil(vec.z / rounding) * rounding);

        private bool PointIsInsideCollider(Collider c, Vector3 p) =>
            Vector3.Distance(Physics.ClosestPoint(p, c, Vector3.zero, Quaternion.identity), p) < 0.01f;

        private void SetupPhysical()
        {
            if (!TryGetComponent(out _rb))
            {
                _rb = gameObject.AddComponent<Rigidbody>();
                Debug.LogError($"Buoyancy:Object \"{name}\" had no Rigidbody. Rigidbody has been added.");
            }

            _rb.centerOfMass = centerOfMass + _voxelBounds.center;
            _baseDrag = _rb.drag;
            _baseAngularDrag = _rb.angularDrag;

            _velocity = new float3[_voxels.Length];
            float archimedesForceMagnitude = WaterDensity * Mathf.Abs(Physics.gravity.y) * volume;
            _localArchimedesForce = new float3(0, archimedesForceMagnitude, 0) / _voxels.Length;

            LocalToWorldJob.SetupJob(_guid, _voxels, ref _samplePoints);
        }

        private void OnDrawGizmosSelected()
        {
            const float gizmoSize = 0.05f;
            var t = transform;
            var matrix = Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);

            if (_voxels != null)
            {
                Gizmos.color = Color.yellow;
                foreach (var p in _voxels) Gizmos.DrawCube(p, new Vector3(gizmoSize, gizmoSize, gizmoSize));
            }

            Gizmos.matrix = matrix;
            if (voxelResolution >= 0.1f)
            {
                Gizmos.DrawWireCube(_voxelBounds.center, _voxelBounds.size);
                Vector3 center = _voxelBounds.center;
                float y = center.y - _voxelBounds.extents.y;
                for (float x = -_voxelBounds.extents.x; x < _voxelBounds.extents.x; x += voxelResolution)
                    Gizmos.DrawLine(new Vector3(x, y, -_voxelBounds.extents.z + center.z),
                                    new Vector3(x, y, _voxelBounds.extents.z + center.z));
                for (float z = -_voxelBounds.extents.z; z < _voxelBounds.extents.z; z += voxelResolution)
                    Gizmos.DrawLine(new Vector3(-_voxelBounds.extents.x, y, z + center.z),
                                    new Vector3(_voxelBounds.extents.x, y, z + center.z));
            }
            else
                _voxelBounds = VoxelBounds();

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_voxelBounds.center + centerOfMass, 0.2f);

            Gizmos.matrix = Matrix4x4.identity;

            if (_debugInfo != null)
            {
                foreach (DebugDrawing debug in _debugInfo)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawCube(debug.Position, new Vector3(gizmoSize, gizmoSize, gizmoSize));
                    var water = debug.Position;
                    water.y = debug.WaterHeight;
                    Gizmos.DrawLine(debug.Position, water);
                    Gizmos.DrawSphere(water, gizmoSize * 4f);

                    if (_buoyancyType == BuoyancyType.Physical || _buoyancyType == BuoyancyType.PhysicalVoxel)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawRay(debug.Position, debug.Force / _rb.mass);
                    }
                }
            }
        }

        private struct DebugDrawing
        {
            public Vector3 Force;
            public Vector3 Position;
            public float WaterHeight;
        }

        public enum BuoyancyType
        {
            NonPhysical,
            NonPhysicalVoxel,
            Physical,
            PhysicalVoxel
        }
    }
}