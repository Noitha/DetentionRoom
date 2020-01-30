
using UnityEngine;

namespace UI.Xml.Tags
{
    public class RowTagHandler : ElementTagHandler
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<UI.Tables.TableRow>();
            }
        }

        public override string prefabPath
        {
            get
            {
                return "Prefabs/TableLayout/Row";
            }
        }
    }
}

