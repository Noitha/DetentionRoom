using UnityEngine;
using UnityEngine.UI;

namespace UI.Xml
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Slider))]
    public class XmlLayoutSlider : MonoBehaviour
    {
        public Image Background;
        public Slider Slider;
        public Image Fill;
    }
}
