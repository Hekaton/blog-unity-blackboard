using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(BlackboardController))]
public class BlackboardControllerEditor : Editor
{
    private bool showBlackboard = true;
    
    private GUIStyle deleteButtonStyle;
    private GUIStyle editableFoldoutStyle;
    private List<bool> groupFoldouts = new List<bool>();
    private string currentGroupKey;
    private List<string> currentGroup;
    private BlackboardController bbctrl;
    private Blackboard bb;
    private Blackboard.DictionaryOfStringAndListObj blackboardGroups;
    private List<ReorderableList> reorderableLists = new List<ReorderableList>();

    private void OnEnable()
    {
        bbctrl = (BlackboardController)target;
        RefreshBlackboard();
        
        Undo.undoRedoPerformed += delegate
        {
            if (bb == null) return;
            
            RefreshFoldoutsList();
            RefreshReordableLists();
        };
    }

    private void RefreshBlackboard()
    {
        bb = bbctrl.EditorGetBlackboard();

        if (bb == null) return;
        
        blackboardGroups = bb.GetGroupedBlackboard();
        
        RefreshFoldoutsList();
        RefreshReordableLists();
        
    }

    private void RefreshFoldoutsList()
    {
        groupFoldouts.Clear();
        
        // Initialize the foldouts
        for(int i = 0; i < blackboardGroups.Count; i++)
        {
            groupFoldouts.Add(true);
        }
    }

    private void RefreshReordableLists()
    {
        reorderableLists.Clear();
        
        // Initialize the reordable lists
        foreach (KeyValuePair<string,ListOfString> group in blackboardGroups)
        {
            ReorderableList entries = new ReorderableList(group.Value.list, typeof(List<string>), true, false, true, false);
            entries.drawHeaderCallback = DrawHeader;
            entries.drawElementCallback = DrawListItems;
            entries.onReorderCallbackWithDetails = OnReorder;
            entries.onAddCallback = AddEntry;
            
            reorderableLists.Add(entries);
        }
    }

    public override void OnInspectorGUI()
    {
        deleteButtonStyle = new GUIStyle(GUI.skin.button);
        deleteButtonStyle.normal.textColor = Color.red;
        deleteButtonStyle.stretchWidth = false;
        
        editableFoldoutStyle = new GUIStyle(EditorStyles.foldout);
        editableFoldoutStyle.stretchWidth = false;

        // Blackboard selector/foldout
        GUILayout.BeginHorizontal();

        showBlackboard = EditorGUILayout.Foldout(showBlackboard, "Blackboard", true);
        
        Blackboard newBB = (Blackboard)EditorGUILayout.ObjectField(
            bbctrl.EditorGetBlackboard(), typeof(Blackboard), false);

        if (newBB != bb)
        {
            Undo.RecordObject(bbctrl, "Update blackboardController's blackboard");
            bbctrl.EditorSetBlackboard(newBB);
            RefreshBlackboard();
            EditorUtility.SetDirty(bbctrl);
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(20);

        if (!showBlackboard || bb == null) return;

        
        int j = 0;

        // Since elements in a foreach loop are immutable, we have to store changes in temporary variables
        string groupToRename = null;
        string newGroupName = null;
        string groupToRemove = null;
        
        foreach (KeyValuePair<string, ListOfString> group in blackboardGroups)
        {
            GUILayout.BeginHorizontal();
            
            string textInGroupNameField = EditorGUILayout.DelayedTextField(group.Key);
            Rect foldoutRect = GUILayoutUtility.GetLastRect();
            groupFoldouts[j] = EditorGUI.Foldout(foldoutRect, groupFoldouts[j], "", false);
            if (groupFoldouts[j] == false)
            {
                GUILayout.EndHorizontal();
                GUILayout.Space(20);
                j++;
                continue;
            }

            if (textInGroupNameField != group.Key)
            {
                groupToRename = group.Key;
                newGroupName = textInGroupNameField;
            }
            
            if (GUILayout.Button("X", deleteButtonStyle))
            {
                groupToRemove = group.Key;
            }
            
            GUILayout.EndHorizontal();

            currentGroupKey = group.Key;
            currentGroup = group.Value.list;
            
            reorderableLists[j].DoLayoutList();
            GUILayout.Space(20);
        
            j++;
        }

        currentGroupKey = null;
        currentGroup = null;

        if (groupToRename != null)
        {
            Undo.RecordObject(bb, "Rename blackboard group");
            
            bb.RenameGroup(groupToRename, newGroupName);
            RefreshFoldoutsList();
            RefreshReordableLists();
            EditorUtility.SetDirty(bb);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        if (groupToRemove != null)
        {
            Undo.RecordObject(bb, "Remove blackboard group");
            bb.RemoveGroup(groupToRemove);
            RefreshFoldoutsList();
            RefreshReordableLists();
            EditorUtility.SetDirty(bb);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        if (GUILayout.Button("Add Group"))
        {
            Undo.RecordObject(bb, "Add blackboard group");
            bb.AddGroup();
            RefreshFoldoutsList();
            RefreshReordableLists();
            EditorUtility.SetDirty(bb);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

    private void DrawHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "Key");
        EditorGUI.LabelField(
            new Rect(rect.x + rect.width / 2 - 40, rect.y, 200, EditorGUIUtility.singleLineHeight),
            "Value");
        EditorGUI.LabelField(
            new Rect(rect.x + rect.width - 95, rect.y, 200, EditorGUIUtility.singleLineHeight),
            "Event");
        EditorGUI.LabelField(
            new Rect(rect.x + rect.width - 55, rect.y, 200, EditorGUIUtility.singleLineHeight),
            "Persist");
    }

    private void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        float spacing = 2;
        float trigBtnWidth = 50;
        float persistTogWidth = 20;
        float delBtnWidth = 20;

        string entryKey = currentGroup[index];
        BlackboardVariable entryValue = bb.EditorGetDict()[entryKey];
        
        string textInKeyField = EditorGUI.DelayedTextField(
            new Rect(rect.x, rect.y, 
                (rect.width - spacing - trigBtnWidth - persistTogWidth - spacing - delBtnWidth) / 2, 
                EditorGUIUtility.singleLineHeight), entryKey);
        
        BlackboardVariable objectInValueField = (BlackboardVariable) EditorGUI.ObjectField(
            new Rect(
                rect.x + (rect.width - spacing - trigBtnWidth - persistTogWidth - spacing - delBtnWidth) / 2 + spacing,
                rect.y, (rect.width - spacing - trigBtnWidth - persistTogWidth - spacing - delBtnWidth) / 2,
                EditorGUIUtility.singleLineHeight), entryValue, typeof(BlackboardVariable), false);
        
        bool triggerButton = GUI.Button(
            new Rect(rect.x + rect.width - trigBtnWidth - persistTogWidth - spacing - delBtnWidth, rect.y,
                trigBtnWidth, EditorGUIUtility.singleLineHeight), "Trigger");
        
        PersistenceType persistenceType = PersistenceType.NeverPersist;
        
        if (entryValue != null)
        {
            persistenceType = (PersistenceType) EditorGUI.EnumPopup(
                new Rect(rect.x + rect.width - persistTogWidth - delBtnWidth, rect.y, persistTogWidth,
                    EditorGUIUtility.singleLineHeight), entryValue.persistenceType);
        }

        bool deletebutton = GUI.Button(new Rect(rect.x + rect.width - delBtnWidth, rect.y, delBtnWidth, 
            EditorGUIUtility.singleLineHeight), "X", deleteButtonStyle);
        
        if (entryKey != textInKeyField) RenameEntry(entryKey, textInKeyField);
        
        if (entryValue != objectInValueField) UpdateEntryValue(entryKey, objectInValueField);
        
        if (triggerButton) bbctrl.TriggerEvent(entryKey);

        if (entryValue != null && entryValue.persistenceType != persistenceType)
        {
            Undo.RecordObject(entryValue, "Set blackboard variable's persistence type");
            entryValue.persistenceType = persistenceType;
            EditorUtility.SetDirty(entryValue);
        }
        
        if (deletebutton) RemoveEntry(entryKey);
    }

    private void OnReorder(ReorderableList list, int oldIndex, int newIndex)
    {
        EditorUtility.SetDirty(bb);
    }

    private void AddEntry(ReorderableList list)
    {
        Undo.RecordObject(bb, "Add blackboard entry");
        bb.AddRow(currentGroupKey);
        EditorUtility.SetDirty(bb);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private void RenameEntry(string oldKey, string newKey)
    {
        Undo.RecordObject(bb, "Update blackboard entry");
        if (newKey != null) bb.RenameRow(currentGroupKey, oldKey, newKey);
        EditorUtility.SetDirty(bb);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private void UpdateEntryValue(string key, BlackboardVariable value)
    {
        Undo.RecordObject(bb, "Update blackboard entry");
        bb.UpdateValue(key, value);
        EditorUtility.SetDirty(bb);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private void RemoveEntry(string key)
    {
        Undo.RecordObject(bb, "Remove blackboard entry");
        bb.RemoveRow(currentGroupKey, key);
        EditorUtility.SetDirty(bb);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
}