using System;
using UnityEngine.UI;

namespace UI.Xml.CustomAttributes
{
    public class OutlineAttribute : CustomXmlAttribute
    {
        public override bool UsesApplyMethod { get { return true; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            var elementTransform = xmlElement.rectTransform;

            if (value.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                var _outline = elementTransform.GetComponent<Outline>();
                if (_outline != null) _outline.enabled = false;

                return;
            }
            if (!elementAttributes.ContainsKey("outline")) return;

            var outlineColor = elementAttributes["outline"].ToColor(xmlElement.xmlLayoutInstance);

            var outline = elementTransform.GetComponent<Outline>();
            if (outline == null)
            {
                outline = elementTransform.gameObject.AddComponent<Outline>();
            }

            outline.enabled = true;
            outline.effectColor = outlineColor;

            if (elementAttributes.ContainsKey("outlinesize"))
            {
                outline.effectDistance = elementAttributes["outlinesize"].ToVector2();
            }
        }

        public override string ValueDataType
        {
            get
            {
                return "color";
            }
        }

        public override string DefaultValue
        {
            get
            {
                return "None";
            }
        }
    }

    public class OutlineSizeAttribute : OutlineAttribute
    {
        public override string ValueDataType
        {
            get
            {
                return "xs:float";
            }
        }

        public override string DefaultValue
        {
            get
            {
                return "1";
            }
        }
    }
}
