using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Blackboard", menuName = "Blackboard/Blackboard", order = 0)]
public class Blackboard : ScriptableObject
{
    #region Stuff for grouping dictionary entries into categories
    
    [Serializable] public class DictionaryOfStringAndListObj : SerializableDictionary<string, ListOfString> {}
    
    [SerializeField] private DictionaryOfStringAndListObj groupedBlackboardEntries = new DictionaryOfStringAndListObj();
    
    public DictionaryOfStringAndListObj GetGroupedBlackboard() => groupedBlackboardEntries;
    
    public void AddGroup()
    {
        bool success = false;
        int i = 0;
        while (!success)
        {
            string newKey = "newGroup" + i;
            if (!groupedBlackboardEntries.ContainsKey(newKey))
            {
                groupedBlackboardEntries.Add(newKey, new ListOfString());
                success = true;
            }
    
            i++;
        }
    }
    
    public void RenameGroup(string key, string newKey)
    {
        if (groupedBlackboardEntries.ContainsKey(newKey))
        {
            Debug.LogError($"A group named {newKey} already exists");
            return;
        }
        
        ListOfString groupContent = groupedBlackboardEntries[key];
        groupedBlackboardEntries.Add(newKey, groupContent);
        groupedBlackboardEntries.Remove(key);
    }
    
    public void RemoveGroup(string key)
    {
        // First delete the blackboard Entries that were in this group
        ListOfString entries = groupedBlackboardEntries[key];
        foreach (string entry in entries.list)
        {
            blackboardEntries.Remove(entry);
        }
        // Then delete the group
        groupedBlackboardEntries.Remove(key);
    }
    
    public void AddRow(string groupKey)
    {
        string newEntryKey = AddRow();
        groupedBlackboardEntries[groupKey].list.Add(newEntryKey);
    }
    
    public void RemoveRow(string groupKey, string entryKey)
    {
        List<string> groupEntries = groupedBlackboardEntries[groupKey].list;
        for (int i = 0; i < groupEntries.Count; i++)
        {
            if (groupEntries[i] == entryKey)
            {
                groupEntries.Remove(groupEntries[i]);
                break;
            }
        }
        
        blackboardEntries.Remove(entryKey);
    }
    
    public void RenameRow(string groupKey, string oldEntryKey, string newEntryKey)
    {
        RenameRow(oldEntryKey, newEntryKey);
        
        List<string> groupEntries = groupedBlackboardEntries[groupKey].list;
        for (int i = 0; i < groupEntries.Count; i++)
        {
            if (groupEntries[i] == oldEntryKey)
            {
                groupEntries[i] = newEntryKey;
                break;
            }
        }
    }
    
    #endregion

    #region UI Stuff

    /// <summary>
    /// Should only be used by the custom inspector, and to draw list items
    /// </summary>
    public Dictionary<string, BlackboardVariable> EditorGetDict() => blackboardEntries;
    
    public void UpdateValue(string key, BlackboardVariable value) => blackboardEntries[key] = value;
    
    private string AddRow()
    {
        int i = 0;
        while (true)
        {
            string newKey = "newKey" + i;
            if (!KeyExists(newKey))
            {
                blackboardEntries.Add(newKey, null);
                return newKey;
            }

            i++;
        }
    }

    private void AddRow(string key, BlackboardVariable value)
    {
        blackboardEntries.Add(key, value);
    }

    private void RenameRow(string oldKey, string newKey)
    {
        BlackboardVariable value = GetValue(oldKey);
        RemoveRow(oldKey);
        AddRow(newKey, value);
    }

    private void RemoveRow(string key)
    {
        blackboardEntries.Remove(key);
    }
    
    #endregion

    #region Initial blackboard functionalities

    [Serializable] public class DictionaryOfStringAndBV : SerializableDictionary<string, BlackboardVariable> {}

    [SerializeField] private DictionaryOfStringAndBV blackboardEntries = new DictionaryOfStringAndBV();

    ///<summary> Use this when iterating over the whole blackboard. To get a single blackboard entry,
    /// use GetBlackboardValue (when dealing with the blackboard controller) or GetValue instead. </summary>
    public List<KeyValuePair<string, BlackboardVariable>> AsList() => blackboardEntries.ToList();
    
    public bool KeyExists(string eventName) => blackboardEntries.ContainsKey(eventName);
    public BlackboardVariable GetValue(string eventName) => blackboardEntries[eventName];
    
    #endregion
}

[Serializable]
public class ListOfString
{
    [SerializeField] public List<string> list;

    public ListOfString() => list = new List<string>();

    public ListOfString(List<string> initialList) => list = initialList;
}