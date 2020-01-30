using UnityEngine;
using System;

namespace UI.Xml
{   
    [Serializable]
    public class MaterialDictionary : SerializableDictionary<string, Material>
    {
        public MaterialDictionary()
        {
            _Comparer = StringComparer.OrdinalIgnoreCase;
        }
    }    
}
