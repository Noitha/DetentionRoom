namespace UI.Xml.CustomAttributes
{    
    public class CursorAttribute : CustomXmlAttribute
    {
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary attributes)
        {
            xmlElement.cursor = value.ToCursorInfo();
        }
    }
    
    public class CursorClickAttribute : CursorAttribute 
    {
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary attributes)
        {
            xmlElement.cursorClick = value.ToCursorInfo();
        }
    }
}
