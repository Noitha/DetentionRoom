using UnityEngine.UI;

namespace UI.Xml.CustomAttributes
{        
    public class SelectedAttribute: CustomXmlAttribute
    {
        public override bool UsesApplyMethod { get { return true; } }
 
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            if (value.ToBoolean())
            {
                var selectable = xmlElement.GetComponent<Selectable>();
                if (selectable != null)
                {                    
                    XmlLayoutTimer.AtEndOfFrame(() => selectable.Select(), xmlElement);
                }
            }
        }

        public override string ValueDataType
        {
            get
            {
                return "xs:boolean";
            }
        }

        public override string DefaultValue
        {
            get
            {
                return "false";
            }
        }
    }
}
