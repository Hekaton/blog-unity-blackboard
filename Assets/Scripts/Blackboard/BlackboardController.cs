using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
        
        LoadBlackboardState();
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

    private BlackboardStateSave SaveBlackboardState()
    {
        BlackboardStateSave save = new BlackboardStateSave();
        
        foreach (KeyValuePair<string, BlackboardVariable> entry in blackboard.AsList())
        {
            if (entry.Value == null || entry.Value.persistenceType == PersistenceType.AlwaysPersist) 
                continue;
            
            if (entry.Value.persistenceType == PersistenceType.SavedToFile)
                save.savedEntries.Add(new KeyValuePair<string, BVarSave>(entry.Key, entry.Value.CreateSave()));
            
            entry.Value.UndoChanges();
        }

        return save;
    }

    private void LoadBlackboardState()
    {
        if (File.Exists(Application.persistentDataPath + "/SaveFile"))
        {
            BlackboardStateSave save;
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/SaveFile", FileMode.Open);
                save = (BlackboardStateSave)bf.Deserialize(file);
                file.Close();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogError("You most likely need to delete the SaveFile file at '" + 
                               Application.persistentDataPath + "', start/stop the game and look at the exception " +
                               "thrown on playmode exit");
                throw;
            }
            
            foreach (KeyValuePair<string, BVarSave> entry in save.savedEntries)
            {
                if (!blackboard.KeyExists(entry.Key))
                {
                    Debug.LogWarning($"Key {entry.Key} does not exist for the current blackboard. Skipping.");
                    continue;
                }

                BlackboardVariable finalValue = blackboard.GetValue(entry.Key);
                if (entry.Value.GetType() != finalValue.GetSaveType())
                {
                    Debug.LogWarning($"Value {entry.Value.GetType()} does not match the value type of current " +
                                     $"blackboard's {entry.Key} key, which is {finalValue.GetType()}. Skipping.");
                    continue;
                }
                
                if(finalValue.persistenceType == PersistenceType.SavedToFile) finalValue.LoadFrom(entry.Value);
            }

            foreach (KeyValuePair<string,BlackboardVariable> entry in blackboard.AsList())
            {
                if (entry.Value != null && entry.Value.persistenceType != PersistenceType.SavedToFile) 
                    entry.Value.SnapshotState();
            }
        }
        else
        {
            Debug.Log("No saved blackboard data found.");
        }
    }

    private void OnDestroy()
    {
        // Prevents unnecessary saving if we're not destroying the original BlackboardController
        if (instance != this) return;
        
        BlackboardStateSave save = SaveBlackboardState();
        
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/SaveFile");
        bf.Serialize(file, save);
        file.Close();
    }
}