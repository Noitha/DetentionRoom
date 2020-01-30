namespace UI.Xml.CustomAttributes
{
    public class ScaleAttribute : CustomXmlAttribute
    {
        public override bool UsesApplyMethod { get { return true; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary attributes)
        {
            xmlElement.rectTransform.localScale = value.ToVector3();
        }

        public override eAttributeGroup AttributeGroup
        {
            get
            {
                return eAttributeGroup.RectTransform;
            }
        }

        public override string DefaultValue
        {
            get
            {
                return "1 1 1";
            }
        }
    }
}
