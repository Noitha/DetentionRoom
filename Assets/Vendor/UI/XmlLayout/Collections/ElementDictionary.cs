using System;

namespace UI.Xml
{   
    [Serializable]
    public class ElementDictionary : SerializableDictionary<string, XmlElement>
    {
        public ElementDictionary()
        {
            _Comparer = StringComparer.OrdinalIgnoreCase;
        }
    }     
}
