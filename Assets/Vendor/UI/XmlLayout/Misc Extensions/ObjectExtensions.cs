using UnityEngine;

namespace UI.Xml
{
    public static class ObjectExtensions
    {
        public static string ConvertToAttributeString(this object o)
        {
            if (o is UnityEngine.Object)
            {
                var unityObject = (UnityEngine.Object)o;

                if (XmlLayoutResourceDatabase.instance.IsResource(unityObject))
                {                    
                    return XmlLayoutResourceDatabase.instance.GetResourcePath(unityObject);
                }
                else
                {
                    if (o is Sprite
                     || o is Font
                     || o is AudioClip)
                    {
                        try
                        {
                            return unityObject.name;
                        } catch { }
                    }
                }
            }
            
            return o.ToString();
        }
    }
}
