using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

/// <summary>
/// StateMachineBehaviour that waits for a random exit time, then transitions 
/// to a random state with a random duration.
/// Attach this to the state you want to transition FROM.
/// Destination states are auto-populated from connected transitions in the editor.
/// </summary>
public class RandomStateBehaviour : StateMachineBehaviour
{
    [Header("Random State Selection")]
    [Tooltip("Auto-populated from connected transitions. You can also edit manually.")]
    public string[] destinationStates;

    [Header("Random Exit Time")]
    [Tooltip("Minimum time (in seconds) to stay in this state before transitioning")]
    public float minExitTime = 1f;
    
    [Tooltip("Maximum time (in seconds) to stay in this state before transitioning")]
    public float maxExitTime = 3f;

    [Header("Random Transition Duration")]
    [Tooltip("Minimum crossfade duration in seconds")]
    public float minTransitionDuration = 0.1f;
    
    [Tooltip("Maximum crossfade duration in seconds")]
    public float maxTransitionDuration = 0.5f;

    [Header("Options")]
    [Tooltip("Use fixed time (seconds) instead of normalized time for crossfade")]
    public bool useFixedTime = true;

    private Animator cachedAnimator;
    private int cachedLayerIndex;
    private float timeInState;
    private float targetExitTime;
    private bool hasTriggeredTransition;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        cachedAnimator = animator;
        cachedLayerIndex = layerIndex;
        timeInState = 0f;
        targetExitTime = Random.Range(minExitTime, maxExitTime);
        hasTriggeredTransition = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (hasTriggeredTransition)
            return;

        timeInState += Time.deltaTime;

        if (timeInState >= targetExitTime)
        {
            TriggerRandomTransition();
        }
    }

    /// <summary>
    /// Triggers a crossfade to a random destination state with a random duration.
    /// </summary>
    public void TriggerRandomTransition()
    {
        if (hasTriggeredTransition)
            return;

        if (cachedAnimator == null || destinationStates == null || destinationStates.Length == 0)
        {
            Debug.LogWarning("RandomStateBehaviour: No animator or destination states configured.");
            return;
        }

        hasTriggeredTransition = true;

        // Pick random state
        string targetState = destinationStates[Random.Range(0, destinationStates.Length)];
        
        // Pick random duration
        float duration = Random.Range(minTransitionDuration, maxTransitionDuration);

        // Perform crossfade
        if (useFixedTime)
        {
            cachedAnimator.CrossFadeInFixedTime(targetState, duration, cachedLayerIndex);
        }
        else
        {
            cachedAnimator.CrossFade(targetState, duration, cachedLayerIndex);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RandomStateBehaviour))]
public class RandomStateBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Refresh Destinations from Transitions"))
        {
            RefreshDestinations((RandomStateBehaviour)target, true);
        }
    }

    public static void RefreshDestinations(RandomStateBehaviour behaviour, bool logResults = false)
    {
        // Find which state this behaviour is attached to
        string assetPath = AssetDatabase.GetAssetPath(behaviour);
        if (string.IsNullOrEmpty(assetPath))
            return;

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
        if (controller == null)
            return;

        // Search all layers and states for this behaviour
        List<string> destinations = new List<string>();
        
        foreach (var layer in controller.layers)
        {
            FindDestinationsInStateMachine(layer.stateMachine, behaviour, destinations);
        }

        if (destinations.Count > 0)
        {
            behaviour.destinationStates = destinations.ToArray();
            EditorUtility.SetDirty(behaviour);
            
            if (logResults)
                Debug.Log($"RandomStateBehaviour: Found {destinations.Count} destination(s): {string.Join(", ", destinations)}");
        }
        else if (logResults)
        {
            Debug.LogWarning("RandomStateBehaviour: No transitions found from the state this behaviour is attached to.");
        }
    }

    private static void FindDestinationsInStateMachine(AnimatorStateMachine stateMachine, RandomStateBehaviour behaviour, List<string> destinations)
    {
        foreach (var childState in stateMachine.states)
        {
            var state = childState.state;
            
            // Check if this state has our behaviour
            foreach (var b in state.behaviours)
            {
                if (b == behaviour)
                {
                    // Found the state, now get all its transitions
                    foreach (var transition in state.transitions)
                    {
                        if (transition.destinationState != null)
                        {
                            string destName = transition.destinationState.name;
                            if (!destinations.Contains(destName))
                            {
                                destinations.Add(destName);
                            }
                        }
                    }
                    return;
                }
            }
        }

        // Recurse into sub-state machines
        foreach (var childStateMachine in stateMachine.stateMachines)
        {
            FindDestinationsInStateMachine(childStateMachine.stateMachine, behaviour, destinations);
        }
    }
}

/// <summary>
/// Automatically refreshes all RandomStateBehaviour destinations on compile and play mode enter.
/// </summary>
[InitializeOnLoad]
public static class RandomStateBehaviourAutoRefresh
{
    static RandomStateBehaviourAutoRefresh()
    {
        // Refresh on compile/reload
        RefreshAllBehaviours();
        
        // Refresh when entering play mode
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            RefreshAllBehaviours();
        }
    }

    private static void RefreshAllBehaviours()
    {
        // Find all animator controllers in the project
        string[] guids = AssetDatabase.FindAssets("t:AnimatorController");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            
            if (controller == null)
                continue;

            foreach (var layer in controller.layers)
            {
                RefreshBehavioursInStateMachine(layer.stateMachine);
            }
        }
        
        AssetDatabase.SaveAssets();
    }

    private static void RefreshBehavioursInStateMachine(AnimatorStateMachine stateMachine)
    {
        foreach (var childState in stateMachine.states)
        {
            foreach (var behaviour in childState.state.behaviours)
            {
                if (behaviour is RandomStateBehaviour rsb)
                {
                    RandomStateBehaviourEditor.RefreshDestinations(rsb, false);
                }
            }
        }

        // Recurse into sub-state machines
        foreach (var childStateMachine in stateMachine.stateMachines)
        {
            RefreshBehavioursInStateMachine(childStateMachine.stateMachine);
        }
    }
}
#endif
