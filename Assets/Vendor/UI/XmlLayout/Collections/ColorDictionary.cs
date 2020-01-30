using UnityEngine;
using System;

namespace UI.Xml
{   
    [Serializable]
    public class ColorDictionary : SerializableDictionary<string, Color>
    {
        public ColorDictionary()
        {
            _Comparer = StringComparer.OrdinalIgnoreCase;
        }
    }    
}
