
namespace XmlProcess
{
    public class Token
    {
        public string Value { get; set; }
        public int Row { get; set; }
        public Token(string value, int row)
        {
            Value = value;
            Row = row;
        }
        public Token(char value, int row)
        {
            Value = value.ToString();
            Row = row;
        }
        public override string ToString()
        {
            return string.Format("{0} ({1})", Value, Row);
        }
    }
}
