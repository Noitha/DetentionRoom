using UnityEngine;
using System;

namespace UI.Xml.CustomAttributes
{    
    public class WidthAttribute: SizeAttribute
    {
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {            
            var elementTransform = xmlElement.rectTransform;

            //var alignment = xmlElement.HasAttribute("rectAlignment") ? GetRectAlignment(xmlElement.GetAttribute("rectAlignment")) : RectAlignment.MiddleCenter;
            var alignment = RectAlignment.MiddleCenter;

            if (xmlElement.HasAttribute("rectAlignment"))
            {
                alignment = GetRectAlignment(xmlElement.GetAttribute("rectAlignment"));
            }
            else if (elementAttributes.ContainsKey("rectAlignment"))
            {
                alignment = GetRectAlignment(elementAttributes.GetValue("rectAlignment"));
            }

            if (elementAttributes.ContainsKey("position"))
            {
                elementTransform.position = elementAttributes["position"].ToVector2();
            }

            var position = elementTransform.position;

            var width = float.Parse(value.Replace("%", String.Empty));

            var originalHeight = elementTransform.rect.height;

            if (value.Contains("%"))
            {
                // Use a percentage-based width value
                elementTransform.sizeDelta = Vector2.zero;

                var workingWidth = width / 100f;

                var vector = ApplyAlignment(new Vector2(workingWidth, 0), alignment);                

                elementTransform.anchorMin = new Vector2(vector.x, elementTransform.anchorMin.y);
                elementTransform.anchorMax = new Vector2(vector.x + workingWidth, elementTransform.anchorMax.y);                
            }
            else
            {
                // Use a fixed width value
                var alignmentStruct = GetAlignmentStruct(width, 0, position, alignment);
                
                elementTransform.anchorMin = new Vector2(alignmentStruct.AnchorMin.x, elementTransform.anchorMin.y);
                elementTransform.anchorMax = new Vector2(alignmentStruct.AnchorMax.x, elementTransform.anchorMax.y);

                elementTransform.pivot = new Vector2(alignmentStruct.Pivot.x, elementTransform.pivot.y);

                elementTransform.sizeDelta = new Vector2(width, elementTransform.sizeDelta.y);                
            }

            // preserve height
            elementTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalHeight);
        }
    }
}
