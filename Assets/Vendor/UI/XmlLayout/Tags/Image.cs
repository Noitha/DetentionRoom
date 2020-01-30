using UnityEngine;
using UnityEngine.UI;

namespace UI.Xml.Tags
{
    public class ImageTagHandler : ElementTagHandler
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<Image>();
            }
        }

        public override void SetValue(string newValue, bool fireEventHandlers = true)
        {
            currentXmlElement.SetAndApplyAttribute("image", newValue);                        
        }
    }
}
