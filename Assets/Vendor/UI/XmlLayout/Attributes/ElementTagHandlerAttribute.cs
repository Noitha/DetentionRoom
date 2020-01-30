using System;

namespace UI.Xml
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ElementTagHandlerAttribute : Attribute
    {
        public string TagName;

        public ElementTagHandlerAttribute(string tagName)
        {
            TagName = tagName;
        }
    }    
}
