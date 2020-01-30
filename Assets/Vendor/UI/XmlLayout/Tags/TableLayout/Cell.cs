
using UnityEngine;

namespace UI.Xml.Tags
{
    public class CellTagHandler : ElementTagHandler
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<UI.Tables.TableCell>();
            }
        }

        public override string prefabPath
        {
            get
            {
                return "Prefabs/TableLayout/Cell";
            }
        }
    }
}

