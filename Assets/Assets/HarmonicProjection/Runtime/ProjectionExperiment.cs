using System;
using UnityEngine;

namespace HarmonicProjection
{
    /// One phase drives circular motion and SHM. All coordinates are root-local.
    public sealed class ProjectionExperiment : MonoBehaviour
    {
        public const float CenterY = 0.32f, MotorX = -0.35f, SpringX = 0.07f;
        public const float AnchorY = 0.69f, ScreenX = 0.34f, ShadowX = 0.331f, BobRadius = 0.02f;
        [Header("Motion: SI units; apply edits on reset")]
        [Range(0.02f, 0.14f)] public float amplitude = 0.11f;
        [Range(0.05f, 2f)] public float frequencyHz = 0.8f;
        public float initialPhaseDegrees = 135f;
        [Range(0f, 2f)] public float playbackSpeed = 1f;
        public bool playOnAwake = true;
        public bool showRays = true;
        public bool showOrbit = true;
        [Header("Prefab references")]
        public Transform arm, rotatingTip, bob, shadow, movingLabel, upperTick, lowerTick;
        public MeshFilter spring;
        public LineRenderer orbit, projectionRay;
        public GameObject incidentRays;
        public float Displacement { get; private set; }
        public float Velocity { get; private set; }
        public float Acceleration { get; private set; }
        public float PhaseRadians { get { return (float)phase; } }
        public bool IsPlaying { get; private set; }
        public float ActiveAmplitude { get { return activeAmplitude; } }
        public float ActiveFrequencyHz { get { return activeFrequency; } }
        public float ProjectionError { get { return rotatingTip && bob ? Mathf.Abs(rotatingTip.localPosition.y - bob.localPosition.y) : 0f; } }
        double phase;
        float activeAmplitude, activeFrequency;
        Mesh runtimeMesh;
        SpringTube tube;
        readonly Vector3[] orbitPoints = new Vector3[97];

        void Awake()
        {
            if (!spring || !arm || !rotatingTip || !bob || !shadow || !orbit || !projectionRay)
            { Debug.LogError("Missing references: use the generated ProjectionExperiment prefab.", this); enabled = false; return; }
            runtimeMesh = new Mesh { name = "Projection spring runtime" };
            runtimeMesh.MarkDynamic();
            spring.sharedMesh = runtimeMesh;
            tube = new SpringTube();
            IsPlaying = playOnAwake;
            ResetSimulation();
        }
        void Update()
        {
            if (IsPlaying) phase = (phase + 2.0 * Math.PI * activeFrequency * Time.deltaTime * Mathf.Max(0f, playbackSpeed)) % (2.0 * Math.PI);
            ApplyPose(runtimeMesh);
        }
        public void Play() { IsPlaying = true; }
        public void Pause() { IsPlaying = false; }
        public void ResetSimulation()
        {
            activeAmplitude = Mathf.Clamp(amplitude, 0.02f, 0.14f);
            activeFrequency = Mathf.Clamp(frequencyHz, 0.05f, 2f);
            phase = initialPhaseDegrees * Math.PI / 180.0;
            UpdateOrbitAndTicks();
            ApplyPose(runtimeMesh);
        }
        public void SetAmplitude(float value) { amplitude = value; ResetSimulation(); }
        public void SetFrequency(float value) { frequencyHz = value; ResetSimulation(); }
        public void SetRaysVisible(bool value) { showRays = value; }
        public void SetOrbitVisible(bool value) { showOrbit = value; }
        public void SeekPhaseDegrees(float degrees)
        { phase = degrees * Math.PI / 180.0; ApplyPose(runtimeMesh); }

        void UpdateOrbitAndTicks()
        {
            if (orbit)
            {
                for (int i = 0; i < orbitPoints.Length; i++)
                {
                    float a = i * 2f * Mathf.PI / (orbitPoints.Length - 1);
                    orbitPoints[i] = new Vector3(MotorX + activeAmplitude * Mathf.Sin(a), CenterY + activeAmplitude * Mathf.Cos(a), 0.006f);
                }
                orbit.positionCount = orbitPoints.Length;
                orbit.SetPositions(orbitPoints);
            }
            if (upperTick) upperTick.localPosition = new Vector3(ShadowX, CenterY + activeAmplitude, 0f);
            if (lowerTick) lowerTick.localPosition = new Vector3(ShadowX, CenterY - activeAmplitude, 0f);
        }
        void ApplyPose(Mesh mesh)
        {
            if (!bob || !rotatingTip || !arm || !shadow) return;
            float w = 2f * Mathf.PI * activeFrequency;
            Displacement = activeAmplitude * (float)Math.Cos(phase);
            Velocity = -activeAmplitude * w * (float)Math.Sin(phase);
            Acceleration = -w * w * Displacement;
            Vector3 center = new Vector3(MotorX, CenterY, 0f);
            Vector3 tip = center + new Vector3(activeAmplitude * (float)Math.Sin(phase), Displacement, 0f);
            rotatingTip.localPosition = tip;
            Vector3 direction = tip - center;
            arm.localPosition = (tip + center) * 0.5f;
            arm.localRotation = Quaternion.FromToRotation(Vector3.up, direction);
            arm.localScale = new Vector3(0.008f, direction.magnitude * 0.5f, 0.008f);
            bob.localPosition = new Vector3(SpringX, CenterY + Displacement, 0f);
            shadow.localPosition = new Vector3(ShadowX, bob.localPosition.y, 0f);
            if (movingLabel) movingLabel.localPosition = shadow.localPosition + new Vector3(-0.001f, 0f, -0.035f);
            if (mesh != null)
            {
                if (tube == null) tube = new SpringTube();
                tube.Update(mesh, AnchorY - bob.localPosition.y - BobRadius, 12, 20, 8, 0.025f, 0.002f);
            }
            if (projectionRay)
            {
                projectionRay.enabled = showRays;
                projectionRay.SetPosition(0, tip);
                projectionRay.SetPosition(1, new Vector3(ShadowX, tip.y, 0f));
            }
            if (incidentRays) incidentRays.SetActive(showRays);
            if (orbit) orbit.enabled = showOrbit;
        }
        public Mesh CreatePreviewMesh()
        {
            ResetSimulation();
            Mesh mesh = new Mesh { name = "Projection spring preview" };
            ApplyPose(mesh);
            return mesh;
        }
        void OnDestroy() { if (runtimeMesh) Destroy(runtimeMesh); }
    }
}
