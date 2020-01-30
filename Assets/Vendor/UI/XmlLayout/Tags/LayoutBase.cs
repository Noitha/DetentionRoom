using UnityEngine;
using UnityEngine.UI;

namespace UI.Xml.Tags
{
    public abstract class LayoutBaseTagHandler : ElementTagHandler
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<HorizontalOrVerticalLayoutGroup>();
            }
        }
    }
}
