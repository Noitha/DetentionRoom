#if TEXTMESHPRO_PRESENT
namespace UI.Xml.Tags
{    
    public class TooltipUseTextMeshProAttribute : CustomXmlAttribute
    {
        public override eAttributeGroup AttributeGroup
        {
            get
            {
                return eAttributeGroup.Tooltip;
            }
        }

        public override string ValueDataType
        {
            get
            {                
                return "xs:boolean";
            }
        }

        public override bool UsesApplyMethod { get { return false; } }
    }

}
#endif
