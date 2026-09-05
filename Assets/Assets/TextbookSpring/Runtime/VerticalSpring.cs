using System;
using UnityEngine;

namespace TextbookSpring
{
    // Local +Y is upward. The root is the fixed attachment, in metres.
    public sealed class VerticalSpring : MonoBehaviour
    {
        [Header("References (created by the editor tool)")]
        public MeshFilter spring;
        public Transform bob;
        [Header("Physics: metres, kilograms, seconds")]
        [Min(0.01f)] public float mass = 0.2f;
        [Min(0.1f)] public float stiffness = 10f;
        [Min(0.1f)] public float naturalLength = 0.28f;
        [Min(0f)] public float amplitude = 0.07f;
        public float phaseDegrees = 180f;
        [Range(0f, 3f)] public float playbackSpeed = 1f;
        public bool playOnAwake = true;
        [Header("Geometry (metres)")]
        [Range(4, 24)] public int turns = 12;
        [Range(12, 32)] public int samplesPerTurn = 20;
        [Range(6, 12)] public int tubeSides = 8;
        [Min(0.01f)] public float coilRadius = 0.028f;
        [Min(0.001f)] public float wireRadius = 0.0022f;
        [Min(0.01f)] public float bobRadius = 0.026f;
        public const float Gravity = 9.81f;
        public float Displacement { get; private set; }
        public float Velocity { get; private set; }
        public float Acceleration { get; private set; }
        public float Period { get { return 2f * Mathf.PI * Mathf.Sqrt(mass / stiffness); } }
        public float EquilibriumLength { get { return naturalLength + mass * Gravity / stiffness; } }
        public float EffectiveAmplitude { get; private set; }
        public bool IsPlaying { get; private set; }
        double time;
        Mesh ownedMesh;
        SpringTube geometry;

        void Awake()
        {
            if (!spring || !bob) { Debug.LogError("Create the complete prefab from Tools > Textbook Spring.", this); enabled = false; return; }
            ownedMesh = new Mesh { name = "Spring runtime mesh" };
            ownedMesh.MarkDynamic();
            spring.sharedMesh = ownedMesh;
            geometry = new SpringTube();
            IsPlaying = playOnAwake;
            Evaluate();
        }
        void Update()
        {
            if (IsPlaying) time += Time.deltaTime * Mathf.Max(0f, playbackSpeed);
            Evaluate();
        }
        public void Play() { IsPlaying = true; }
        public void Pause() { IsPlaying = false; }
        public void ResetSimulation() { time = 0; Evaluate(); }
        // Use this for UI changes. Changes intentionally restart the phase.
        public void SetParameters(float newMass, float newStiffness, float newAmplitude)
        {
            mass = newMass; stiffness = newStiffness; amplitude = newAmplitude;
            ResetSimulation();
        }
        void Sanitize()
        {
            mass = Mathf.Clamp(mass, 0.01f, 10f);
            stiffness = Mathf.Clamp(stiffness, 0.1f, 1000f);
            naturalLength = Mathf.Clamp(naturalLength, 0.1f, 2f);
            turns = Mathf.Clamp(turns, 4, 24);
            samplesPerTurn = Mathf.Clamp(samplesPerTurn, 12, 32);
            tubeSides = Mathf.Clamp(tubeSides, 6, 12);
            coilRadius = Mathf.Clamp(coilRadius, 0.01f, 0.1f);
            wireRadius = Mathf.Clamp(wireRadius, 0.001f, coilRadius * 0.15f);
            bobRadius = Mathf.Clamp(bobRadius, 0.01f, 0.15f);
            // Conservative clearance: prevent adjacent turns overlapping.
            float minLength = 2f * SpringTube.Lead + turns * wireRadius * 2.5f;
            naturalLength = Mathf.Max(naturalLength, minLength);
            EffectiveAmplitude = Mathf.Clamp(amplitude, 0f, Mathf.Max(0f, EquilibriumLength - minLength));
        }
        void Evaluate()
        {
            if (!spring || !bob) return;
            Sanitize();
            double omega = Math.Sqrt(stiffness / mass);
            double phase = omega * time + phaseDegrees * Math.PI / 180.0;
            Displacement = EffectiveAmplitude * (float)Math.Cos(phase);
            Velocity = -EffectiveAmplitude * (float)(omega * Math.Sin(phase));
            Acceleration = -(stiffness / mass) * Displacement;
            float length = EquilibriumLength - Displacement;
            bob.localPosition = new Vector3(0f, -length - bobRadius, 0f);
            bob.localScale = Vector3.one * (2f * bobRadius);
            if (ownedMesh != null) geometry.Update(ownedMesh, length, turns, samplesPerTurn, tubeSides, coilRadius, wireRadius);
        }
        // Called only by the builder; returned mesh must be saved as an asset.
        public Mesh CreatePreviewMesh()
        {
            time = 0;
            Evaluate();
            Mesh preview = new Mesh { name = "Spring preview" };
            new SpringTube().Update(preview, EquilibriumLength - Displacement, turns, samplesPerTurn, tubeSides, coilRadius, wireRadius);
            spring.sharedMesh = preview;
            return preview;
        }
        void OnDestroy() { if (ownedMesh != null) Destroy(ownedMesh); }
    }
}
