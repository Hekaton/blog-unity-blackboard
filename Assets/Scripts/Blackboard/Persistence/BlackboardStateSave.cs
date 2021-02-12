using System;
using System.Collections.Generic;

[Serializable]
public class BlackboardStateSave
{
    public List<KeyValuePair<string, BVarSave>> savedEntries = 
        new List<KeyValuePair<string, BVarSave>>();
}