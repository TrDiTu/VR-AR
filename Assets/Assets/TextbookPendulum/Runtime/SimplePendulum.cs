using System;
using System.Collections.Generic;
using UnityEngine;

namespace TextbookPendulum
{
    /// Small-angle SHM in alpha and arc displacement s, with exact circular geometry.
    public sealed class SimplePendulum : MonoBehaviour
    {
        [Header("Small-angle model: apply parameters on reset")]
        [Range(0.2f, 1.5f)] public float length = 0.5f;
        [Range(0f, 10f)] public float amplitudeDegrees = 10f;
        [Range(0.1f, 25f)] public float gravity = 9.81f;
        public float initialPhaseDegrees = 0f;
        [Range(0f, 3f)] public float playbackSpeed = 1f;
        public bool playOnAwake = true;
        public bool showAnnotations = true;
        [Header("Generated prefab references")]
        public Transform bob, cord;
        public MeshFilter guides;
        public GameObject annotations;
        public LineRenderer angleArc, displacementArc;
        public Transform angleLabel, lengthLabel, displacementLabel, equilibriumLabel, zeroPotentialLabel;
        public const float BobRadius = 0.022f;
        public float AngleRadians { get; private set; }
        public float AngleDegrees { get { return AngleRadians * Mathf.Rad2Deg; } }
        public float ArcDisplacement { get { return activeLength * AngleRadians; } }
        public float AngularVelocity { get; private set; }
        public float TangentialVelocity { get { return activeLength * AngularVelocity; } }
        public float TangentialAcceleration { get { return -activeGravity * AngleRadians; } }
        public float ActiveLength { get { return activeLength; } }
        public float Period { get { return 2f * Mathf.PI * Mathf.Sqrt(activeLength / activeGravity); } }
        public bool IsPlaying { get; private set; }
        float activeLength = 0.5f, activeGravity = 9.81f, activeAmplitude;
        double phase;
        Mesh ownedGuides;
        readonly Vector3[] anglePoints = new Vector3[25];
        readonly Vector3[] displacementPoints = new Vector3[25];

        void Awake()
        {
            if (!bob || !cord || !guides || !annotations || !angleArc || !displacementArc)
            { Debug.LogError("Use the prefab generated from Tools > Textbook Pendulum.", this); enabled = false; return; }
            ownedGuides = new Mesh { name = "Pendulum guide mesh runtime" };
            guides.sharedMesh = ownedGuides;
            IsPlaying = playOnAwake;
            ResetSimulation();
        }
        void Update()
        {
            if (IsPlaying)
                phase = (phase + Math.Sqrt(activeGravity / activeLength) * Time.deltaTime * Mathf.Max(0f, playbackSpeed)) % (2.0 * Math.PI);
            ApplyPose();
        }
        public void Play() { IsPlaying = true; }
        public void Pause() { IsPlaying = false; }
        public void SetAnnotationsVisible(bool value) { showAnnotations = value; ApplyPose(); }
        public void SetLength(float value) { length = value; ResetSimulation(); }
        public void SetAmplitudeDegrees(float value) { amplitudeDegrees = value; ResetSimulation(); }
        public void SetGravity(float value) { gravity = value; ResetSimulation(); }
        public void SeekPhaseDegrees(float value) { phase = value * Math.PI / 180.0; ApplyPose(); }
        public void ResetSimulation()
        {
            activeLength = Mathf.Clamp(length, 0.2f, 1.5f);
            activeGravity = Mathf.Clamp(gravity, 0.1f, 25f);
            activeAmplitude = Mathf.Clamp(amplitudeDegrees, 0f, 10f) * Mathf.Deg2Rad;
            phase = initialPhaseDegrees * Math.PI / 180.0;
            if (ownedGuides) BuildGuides(ownedGuides);
            if (equilibriumLabel) equilibriumLabel.localPosition = new Vector3(0f, -activeLength - 0.045f, -0.006f);
            if (zeroPotentialLabel) zeroPotentialLabel.localPosition = new Vector3(0f, -activeLength - 0.08f, -0.006f);
            ApplyPose();
        }
        void ApplyPose()
        {
            if (!bob || !cord) return;
            double w = Math.Sqrt(activeGravity / activeLength);
            AngleRadians = activeAmplitude * (float)Math.Cos(phase);
            AngularVelocity = -activeAmplitude * (float)(w * Math.Sin(phase));
            Vector3 direction = new Vector3(Mathf.Sin(AngleRadians), -Mathf.Cos(AngleRadians), 0f);
            bob.localPosition = activeLength * direction;
            bob.localScale = Vector3.one * (2f * BobRadius);
            // l is pivot-to-centre distance. Visible cord ends at the sphere surface.
            float visibleLength = activeLength - BobRadius;
            cord.localPosition = direction * (visibleLength * 0.5f);
            cord.localRotation = Quaternion.FromToRotation(Vector3.up, direction);
            cord.localScale = new Vector3(0.0024f, visibleLength * 0.5f, 0.0024f);
            if (annotations) annotations.SetActive(showAnnotations);
            float arcRadius = activeLength * 0.26f;
            for (int i = 0; i < anglePoints.Length; i++)
            {
                float a = AngleRadians * i / (anglePoints.Length - 1);
                Vector3 d = new Vector3(Mathf.Sin(a), -Mathf.Cos(a), 0f);
                anglePoints[i] = d * arcRadius + new Vector3(0f, 0f, -0.003f);
                displacementPoints[i] = d * activeLength + new Vector3(0f, 0f, -BobRadius - 0.002f);
            }
            bool displaced = Mathf.Abs(AngleRadians) > 0.0002f;
            if (angleArc) { angleArc.enabled = displaced; angleArc.SetPositions(anglePoints); }
            if (displacementArc) { displacementArc.enabled = displaced; displacementArc.SetPositions(displacementPoints); }
            float side = AngleRadians < 0f ? -1f : 1f;
            if (angleLabel)
            {
                angleLabel.gameObject.SetActive(displaced);
                angleLabel.localPosition = new Vector3(side * (arcRadius * Mathf.Sin(Mathf.Abs(AngleRadians)) + 0.021f), -arcRadius, -0.005f);
            }
            if (lengthLabel) lengthLabel.localPosition = direction * (activeLength * 0.56f) + new Vector3(side * 0.027f, 0f, -0.005f);
            if (displacementLabel)
            {
                displacementLabel.gameObject.SetActive(displaced);
                displacementLabel.localPosition = new Vector3(activeLength * Mathf.Sin(AngleRadians * 0.5f) + side * 0.015f, -activeLength - 0.02f, -BobRadius - 0.006f);
            }
        }
        public Mesh CreatePreviewMesh()
        {
            ResetSimulation();
            Mesh result = new Mesh { name = "Pendulum guides preview" };
            BuildGuides(result);
            return result;
        }
        void BuildGuides(Mesh mesh)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();
            // All dashes share one mesh / renderer. Double sided for AR viewpoints.
            int count = Mathf.CeilToInt(activeLength / 0.022f);
            for (int i = 0; i < count; i++)
            {
                float a = activeLength * i / count;
                float b = activeLength * (i + 0.52f) / count;
                Dash(vertices, indices, new Vector3(0f, -a, 0.004f), new Vector3(0f, -b, 0.004f));
            }
            float extent = Mathf.Max(activeAmplitude, 5f * Mathf.Deg2Rad) + 4f * Mathf.Deg2Rad;
            for (int i = 0; i < 20; i++)
            {
                float a = Mathf.Lerp(-extent, extent, i / 20f);
                float b = Mathf.Lerp(-extent, extent, (i + 0.55f) / 20f);
                Dash(vertices, indices, new Vector3(activeLength * Mathf.Sin(a), -activeLength * Mathf.Cos(a), 0.004f),
                     new Vector3(activeLength * Mathf.Sin(b), -activeLength * Mathf.Cos(b), 0.004f));
            }
            mesh.Clear(); mesh.SetVertices(vertices); mesh.SetTriangles(indices, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
        }
        static void Dash(List<Vector3> vertices, List<int> indices, Vector3 a, Vector3 b)
        {
            Vector3 side = Vector3.Cross((b-a).normalized, Vector3.forward) * 0.0007f;
            int n = vertices.Count;
            vertices.Add(a-side); vertices.Add(a+side); vertices.Add(b+side); vertices.Add(b-side);
            indices.Add(n); indices.Add(n+1); indices.Add(n+2); indices.Add(n); indices.Add(n+2); indices.Add(n+3);
            indices.Add(n+2); indices.Add(n+1); indices.Add(n); indices.Add(n+3); indices.Add(n+2); indices.Add(n);
        }
        void OnDestroy() { if (ownedGuides) Destroy(ownedGuides); }
    }
}
