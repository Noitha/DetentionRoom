using System;

namespace UI.Xml
{
    [Serializable]
    public class AnimationDictionary : SerializableDictionary<string, XmlLayoutAnimation>
    {
        public AnimationDictionary()
        {
            _Comparer = StringComparer.OrdinalIgnoreCase;
        }
    }
}
