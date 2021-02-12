using System;
using System.Collections.Generic;
using UnityEngine;

public class BlackboardController : MonoBehaviour
{
    // We only ever want to have one controller at a time, so we'll make this a singleton
    private static BlackboardController instance;
    
    [SerializeField] private Blackboard blackboard;

    // Actions allow us to execute a series of functions when invoked
    private Action<BlackboardVariable> tempEventHolder;
    private Dictionary<string, Action<BlackboardVariable>> blackboardEvents =
        new Dictionary<string, Action<BlackboardVariable>>();

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        {
            Destroy(gameObject);
        }
    }
    
    public BlackboardVariable GetBlackboardValue(string key)
    {
        if (!blackboard.KeyExists(key))
        {
            Debug.LogError($"key {key} does not exist for this blackboard");
            return null;
        }

        return blackboard.GetValue(key);
    }

    public void StartListening(string eventName, Action<BlackboardVariable> listener)
    {
        if (!blackboard.KeyExists(eventName))
        {
            Debug.LogError($"key {eventName} does not exist for this blackboard");
            return;
        }

        tempEventHolder = null;
        // If we already have the specific key and its value in the dictionary
        // don't re-add the pair into dictionary
        if (blackboardEvents.TryGetValue(eventName, out tempEventHolder))
        {
            tempEventHolder += listener;
            // Because TryGetValue returned the found delegate to tempEventHolder as value type
            // Any change on the variable tempEventHolder is local
            // could not affect the delegate stored back in the dictionary
            // we need to Copy the newly aggregated delegate back into the dictionary.
            blackboardEvents[eventName] = tempEventHolder;
        }
        else
        {
            tempEventHolder += listener;
            blackboardEvents.Add(eventName, tempEventHolder);
        }
    }

    public void StopListening(string eventName, Action<BlackboardVariable> listener)
    {
        if (!blackboard.KeyExists(eventName))
        {
            Debug.LogError($"key {eventName} does not exist for this blackboard");
            return;
        }

        tempEventHolder = null;
        if (blackboardEvents.TryGetValue(eventName, out tempEventHolder))
        {
            tempEventHolder -= listener;
            // Because TryGetValue returned the found delegate to tempEventHolder as value type
            // Any change on the variable tempEventHolder is local
            // could not affect the delegate stored back in the dictionary
            // we need to Copy the newly deducted delegate back into the dictionary.
            blackboardEvents[eventName] = tempEventHolder;
        }
    }

    public void TriggerEvent(string eventName)
    {
        if (!blackboard.KeyExists(eventName))
        {
            Debug.LogError($"key {eventName} does not exist for this blackboard");
            return;
        }

        tempEventHolder = null;
        if (blackboardEvents.TryGetValue(eventName, out tempEventHolder))
        {
            tempEventHolder?.Invoke(blackboard.GetValue(eventName));
        }
    }
}