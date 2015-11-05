using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XmlProcess
{
    [XmlRoot("Params")]
    public class EnvironmentSettings
    {
        [XmlElement("Param")]
        public List<Parameter> Parameters { get; set; }

        public static EnvironmentSettings LoadXml(string xml)
        {
            var serializer = new XmlSerializer(typeof(EnvironmentSettings));
            return (EnvironmentSettings)serializer.Deserialize(new StringReader(xml));
        }
        public Dictionary<string, string> GetByTarget(string target)
        {
            var dict = new Dictionary<string, string>();
            foreach(var item in Parameters.Select(p => new { name = p.Name, value = p.Values.FirstOrDefault(v => v.Target == target)}))
            {
                dict.Add(item.name, (item.value ?? new Value { Content = "" }).Content);
            }
            return dict;
        }
    }
    public class Parameter
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlElement("Value")]
        public List<Value> Values { get; set; }
    }
    public class Value
    {
        [XmlAttribute("target")]
        public string Target { get; set; }

        [XmlText]
        public string Content { get; set; }
    }
}
