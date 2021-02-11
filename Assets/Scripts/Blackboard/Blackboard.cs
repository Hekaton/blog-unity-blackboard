﻿using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Blackboard", menuName = "Blackboard/Blackboard", order = 0)]
public class Blackboard : ScriptableObject
{
    public int rowsCount;
    
    [Serializable] public class DictionaryOfStringAndBV : SerializableDictionary<string, BlackboardVariable> {}

    [SerializeField] private DictionaryOfStringAndBV blackboardEntries = new DictionaryOfStringAndBV();
    
    public bool KeyExists(string eventName) => blackboardEntries.ContainsKey(eventName);
    public BlackboardVariable GetValue(string eventName) => blackboardEntries[eventName];

    private void OnValidate()
    {
        // Resize the blackboard, if need be
        int countDifference = rowsCount - blackboardEntries.Count;
        
        if (countDifference > 0)
        {
            int pairsAdded = 0;
            int i = 0;
            while (pairsAdded < countDifference)
            {
                string newKey = "newKey" + i;
                if (!KeyExists(newKey))
                {
                    blackboardEntries.Add(newKey, null);
                    pairsAdded++;
                }

                i++;
            }
        }
        else if (countDifference < 0)
        {
            for (int i = 0; i < Math.Abs(countDifference); i++)
            {
                blackboardEntries.Remove(blackboardEntries.Keys.Last());
            }
        }
    }
}