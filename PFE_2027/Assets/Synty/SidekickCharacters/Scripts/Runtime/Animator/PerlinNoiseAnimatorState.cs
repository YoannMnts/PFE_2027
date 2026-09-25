using UnityEngine;
using System;

/// <summary>
/// StateMachineBehaviour that applies Perlin noise to animator parameters while in a state.
/// Useful for adding subtle variation to idle animations, breathing effects, procedural motion, etc.
/// </summary>
public class PerlinNoiseAnimatorState : StateMachineBehaviour
{
    [Serializable]
    public class NoiseParameter
    {
        [Tooltip("Name of the animator parameter to affect")]
        public string parameterName;
        
        [Tooltip("Type of the animator parameter")]
        public ParameterType parameterType = ParameterType.Float;
        
        [Header("Speed")]
        [Tooltip("Minimum speed of the noise variation")]
        public float speedMin = 0.5f;
        
        [Tooltip("Maximum speed of the noise variation")]
        public float speedMax = 1.5f;
        
        [Header("Amplitude")]
        [Tooltip("Minimum amplitude/strength of the noise effect")]
        public float amplitudeMin = 0.5f;
        
        [Tooltip("Maximum amplitude/strength of the noise effect")]
        public float amplitudeMax = 1f;
        
        [Header("Randomization")]
        [Tooltip("How often to pick new random speed/amplitude values (seconds)")]
        public float randomizeInterval = 3f;
        
        [Tooltip("How quickly to lerp to new random values (0 = instant, higher = smoother)")]
        public float smoothing = 2f;
        
        [Header("Base Settings")]
        [Tooltip("Base value to add noise to (noise oscillates around this)")]
        public float baseValue = 0f;
        
        [Tooltip("Offset in the noise sample space (use different values per parameter for variation)")]
        public float noiseOffset = 0f;
        
        [Tooltip("Secondary noise offset for 2D Perlin sampling")]
        public float noiseOffsetY = 0f;
        
        [Tooltip("Remap noise from -1 to 1 range to 0 to 1 range")]
        public bool remapToPositive = false;
        
        [Tooltip("Use unscaled time (ignores Time.timeScale)")]
        public bool useUnscaledTime = false;
        
        // Runtime state
        [NonSerialized] public int parameterHash;
        [NonSerialized] public float timeAccumulator;
        [NonSerialized] public float randomizeTimer;
        [NonSerialized] public float currentSpeed;
        [NonSerialized] public float targetSpeed;
        [NonSerialized] public float currentAmplitude;
        [NonSerialized] public float targetAmplitude;
    }
    
    public enum ParameterType
    {
        Float,
        Int,
        Bool
    }
    
    [Header("Noise Parameters")]
    [Tooltip("List of animator parameters to apply noise to")]
    public NoiseParameter[] parameters;
    
    [Header("Global Settings")]
    [Tooltip("Global speed multiplier applied to all parameters")]
    public float globalSpeedMultiplier = 1f;
    
    [Tooltip("Global amplitude multiplier applied to all parameters")]
    public float globalAmplitudeMultiplier = 1f;
    
    [Tooltip("Smoothly blend noise in/out based on state normalized time")]
    public bool blendWithState = false;
    
    [Tooltip("Blend in duration (normalized time 0-1)")]
    [Range(0f, 0.5f)]
    public float blendInDuration = 0.1f;
    
    [Tooltip("Blend out duration (normalized time 0-1)")]
    [Range(0f, 0.5f)]
    public float blendOutDuration = 0.1f;
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (parameters == null) return;
        
        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            
            // Cache parameter hash
            param.parameterHash = Animator.StringToHash(param.parameterName);
            
            // Initialize time accumulator with offset for variation
            param.timeAccumulator = param.noiseOffset;
            
            // Initialize with random values immediately
            param.currentSpeed = UnityEngine.Random.Range(param.speedMin, param.speedMax);
            param.targetSpeed = param.currentSpeed;
            param.currentAmplitude = UnityEngine.Random.Range(param.amplitudeMin, param.amplitudeMax);
            param.targetAmplitude = param.currentAmplitude;
            
            // Randomize the initial timer so parameters don't all change at once
            param.randomizeTimer = UnityEngine.Random.Range(0f, param.randomizeInterval);
        }
    }
    
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (parameters == null || parameters.Length == 0) return;
        
        // Calculate blend factor if enabled
        float blendFactor = 1f;
        if (blendWithState)
        {
            float normalizedTime = stateInfo.normalizedTime % 1f;
            
            if (normalizedTime < blendInDuration && blendInDuration > 0f)
            {
                blendFactor = normalizedTime / blendInDuration;
            }
            else if (normalizedTime > (1f - blendOutDuration) && blendOutDuration > 0f)
            {
                blendFactor = (1f - normalizedTime) / blendOutDuration;
            }
            
            blendFactor = Mathf.Clamp01(blendFactor);
        }
        
        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            
            float deltaTime = param.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            
            // Update randomization timer
            param.randomizeTimer -= deltaTime;
            if (param.randomizeTimer <= 0f)
            {
                param.randomizeTimer = param.randomizeInterval;
                param.targetSpeed = UnityEngine.Random.Range(param.speedMin, param.speedMax);
                param.targetAmplitude = UnityEngine.Random.Range(param.amplitudeMin, param.amplitudeMax);
            }
            
            // Smoothly interpolate current values toward targets
            if (param.smoothing > 0f)
            {
                float lerpSpeed = deltaTime * param.smoothing;
                param.currentSpeed = Mathf.Lerp(param.currentSpeed, param.targetSpeed, lerpSpeed);
                param.currentAmplitude = Mathf.Lerp(param.currentAmplitude, param.targetAmplitude, lerpSpeed);
            }
            else
            {
                param.currentSpeed = param.targetSpeed;
                param.currentAmplitude = param.targetAmplitude;
            }
            
            // Accumulate time using current (smoothed) speed
            param.timeAccumulator += deltaTime * param.currentSpeed * globalSpeedMultiplier;
            
            // Sample Perlin noise (returns 0-1, remap to -1 to 1)
            float noiseValue = Mathf.PerlinNoise(param.timeAccumulator, param.noiseOffsetY);
            noiseValue = (noiseValue * 2f) - 1f;
            
            if (param.remapToPositive)
            {
                noiseValue = (noiseValue + 1f) * 0.5f;
            }
            
            // Apply current (smoothed) amplitude and blend
            float finalValue = param.baseValue + (noiseValue * param.currentAmplitude * globalAmplitudeMultiplier * blendFactor);
            
            // Apply to animator parameter
            switch (param.parameterType)
            {
                case ParameterType.Float:
                    animator.SetFloat(param.parameterHash, finalValue);
                    break;
                    
                case ParameterType.Int:
                    animator.SetInteger(param.parameterHash, Mathf.RoundToInt(finalValue));
                    break;
                    
                case ParameterType.Bool:
                    animator.SetBool(param.parameterHash, finalValue > 0.5f);
                    break;
            }
        }
    }
    
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Optionally reset parameters to base values on exit
        // Uncomment if you want this behavior:
        /*
        if (parameters == null) return;
        
        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            switch (param.parameterType)
            {
                case ParameterType.Float:
                    animator.SetFloat(param.parameterHash, param.baseValue);
                    break;
                case ParameterType.Int:
                    animator.SetInteger(param.parameterHash, Mathf.RoundToInt(param.baseValue));
                    break;
                case ParameterType.Bool:
                    animator.SetBool(param.parameterHash, param.baseValue > 0.5f);
                    break;
            }
        }
        */
    }
}
