using UnityEngine;
using UnityEngine.UI;

namespace UI.Xml
{
    [ExecuteInEditMode]
    //[RequireComponent(typeof(Dropdown))]
    public class XmlLayoutDropdown : MonoBehaviour
    {
        public Image Arrow;
        //public Dropdown Dropdown;
        public Toggle ItemTemplate;
        public Scrollbar DropdownScrollbar;

        public string optionsDataSource;
    }
}
