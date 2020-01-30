namespace UI.Xml.CustomAttributes
{    
    public class OpacityAttribute: CustomXmlAttribute
    {
        public override bool UsesApplyMethod { get { return true; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            var parsedValue = float.Parse(value);

            xmlElement.DefaultOpacity = parsedValue;
            xmlElement.CanvasGroup.alpha = parsedValue;                        
        }

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
