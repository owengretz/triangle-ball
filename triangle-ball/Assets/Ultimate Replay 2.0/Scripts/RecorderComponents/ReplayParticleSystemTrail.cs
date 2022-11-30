using System;
using UltimateReplay.Serializers;
using UnityEngine;

namespace UltimateReplay
{    
    /// <summary>
    /// A replay component which can be used to record and replay the Unity ParticleSystem.
    /// </summary>
    [ReplaySerializer(typeof(ReplayParticleSystemSerializer))]
    public class ReplayParticleSystemTrail : ReplayRecordableBehaviour
    {
        // Types
        /// <summary>
        /// Replay flags used to determine which component features are enabled.
        /// </summary>
        [Flags]
        public enum ReplayParticleSystemFlags
        {
            /// <summary>
            /// No features are enabled.
            /// </summary>
            None = 0,
            /// <summary>
            /// Interpolate the supported particle system values such as time offset for smoother playback.
            /// </summary>
            Interpolate = 1 << 1,
        }

        // Private
        private static ReplayParticleSystemSerializer sharedSerializer = new ReplayParticleSystemSerializer();

        private ParticleSystem.Particle[] lastParticles = null;
        private ParticleSystem.Particle[] particles = null;
        private ParticleSystem.Particle[] updateParticles = null;

        private float lastTime = 0;
        private float targetTime = 0;
        private uint randomSeed = 0;

        // Public
        /// <summary>
        /// The Unity particle system that will be recorded and also used for playback.
        /// </summary>
        public ParticleSystem observedParticleSystem = null;
        /// <summary>
        /// The <see cref="ReplayParticleSystemFlags"/> to specify which features are enabled.
        /// </summary>
        [HideInInspector]
        public ReplayParticleSystemFlags updateFlags = ReplayParticleSystemFlags.Interpolate;

        public bool recordParticlePosition = false;

        // Properties
        private ParticleSystem.Particle[] Particles
        {
            get
            {
                if (observedParticleSystem != null && particles == null)
                {
                    particles = new ParticleSystem.Particle[observedParticleSystem.main.maxParticles];
                    lastParticles = new ParticleSystem.Particle[observedParticleSystem.main.maxParticles];
                    updateParticles = new ParticleSystem.Particle[observedParticleSystem.main.maxParticles];
                }

                return particles;
            }
        }

        // Methods
        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Start()
        {
            if (observedParticleSystem == null)
                Debug.LogWarningFormat("Replay particle system '{0}' will not record or replay because the observed particle system has not been assigned", this);

            observedParticleSystem.GetParticles(Particles);
            observedParticleSystem.GetParticles(updateParticles);
        }

        /// <summary>
        /// Called by Unity editor.
        /// </summary>
        public override void Reset()
        {
            // Call base method
            base.Reset();

            // Try to auto-find particle component
            if (observedParticleSystem == null)
                observedParticleSystem = GetComponent<ParticleSystem>();
        }

        /// <summary>
        ///  Called by the replay system when persistent data should be reset.
        /// </summary>
        public override void OnReplayReset()
        {
            lastTime = targetTime;
        }

        /// <summary>
        /// Called by the replay system during playback mode.
        /// </summary>
        /// <param name="replayTime">The <see cref="ReplayTime"/> for the assocaited playback operation</param>
        public override void OnReplayUpdate(ReplayTime replayTime)
        {
            // Check for no component
            if (observedParticleSystem == null)
                return;

            float time = targetTime;

            // Check for interpolate
            if ((updateFlags & ReplayParticleSystemFlags.Interpolate) != 0)
            {
                // Interpolate the time value
                time = Mathf.Lerp(lastTime, targetTime, replayTime.Delta);
            }

            // Reset particle system and set seed for deterministic simulation
            observedParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            observedParticleSystem.randomSeed = randomSeed;            

            // Set simulation time
            if(time < observedParticleSystem.main.duration)
                observedParticleSystem.Simulate(time, true, true);

            // Interpolate particles
            for(int i = 0; i < Particles.Length; i++)
            {
                updateParticles[i].position = Vector3.Lerp(lastParticles[i].position, particles[i].position, replayTime.Delta);
            }

            observedParticleSystem.SetParticles(Particles);
        }

        /// <summary>
        /// Called by the replay system when the component should serialize its recorded data.
        /// </summary>
        /// <param name="state">The state object to write to</param>
        public override void OnReplaySerialize(ReplayState state)
        {
            // Check for no component
            if (observedParticleSystem == null)
                return;

            // Set serialize values
            sharedSerializer.RandomSeed = observedParticleSystem.randomSeed;
            sharedSerializer.SimulationTime = observedParticleSystem.time;

            // Run serializer
            sharedSerializer.OnReplaySerialize(state);


            if (recordParticlePosition == true && Particles != null)
            {
                // Record particles
                int amount = observedParticleSystem.GetParticles(Particles);

                state.Write(amount);

                for (int i = 0; i < amount; i++)
                {
                    state.Write(Particles[i].position);
                    state.Write(Particles[i].rotation3D);
                    state.Write(Particles[i].GetCurrentSize3D(observedParticleSystem));
                }
            }
            else
            {
                // No particles
                state.Write(0);
            }
        }

        /// <summary>
        /// Called by the replay system when the component should deserialize previously recorded data.
        /// </summary>
        /// <param name="state">The state object to read from</param>
        public override void OnReplayDeserialize(ReplayState state)
        {
            // Check for no component
            if (observedParticleSystem == null)
                return;

            // Reset 
            OnReplayReset();

            // Run serializer
            sharedSerializer.OnReplayDeserialize(state);

            // Set serialize values
            randomSeed = sharedSerializer.RandomSeed;
            targetTime = sharedSerializer.SimulationTime;



            // Read particles
            int amount = state.ReadInt32();

            if (amount > 0 && Particles != null)
            {
                // Copy old array
                Array.Copy(particles, lastParticles, particles.Length);

                for (int i = 0; i < amount; i++)
                {
                    particles[i].position = state.ReadVec3();
                    particles[i].rotation3D = state.ReadVec3();
                    particles[i].startSize3D = state.ReadVec3();
                }
            }
        }
    }
}
