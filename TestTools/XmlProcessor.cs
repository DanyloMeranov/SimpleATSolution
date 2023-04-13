using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

namespace TestTools
{
    public static class XmlProcessor
    {
        private static IEnumerable<Dictionary<string, string>> GetStringValues(XPathNavigator navigator, string xpath)
        {
            var sb = new List<Dictionary<string, string>>();
            var blockNodesIterator = navigator.Select($"//{xpath}");
            while (blockNodesIterator.MoveNext())
            {
                var dicValues = new Dictionary<string, string>();
                var elementsXml = new XmlDocument();
                elementsXml.LoadXml(blockNodesIterator.Current.OuterXml);
                var elementsNavi = elementsXml.CreateNavigator();
                var elementNodesIterator = elementsNavi.Select($"//{xpath}/*");
                while (elementNodesIterator.MoveNext())
                {
                    if (dicValues.ContainsKey(elementNodesIterator.Current.Name))
                        dicValues[elementNodesIterator.Current.Name] = elementNodesIterator.Current.Value;
                    else
                        dicValues.Add(elementNodesIterator.Current.Name, elementNodesIterator.Current.Value);
                }
                sb.Add(dicValues);
            }
            return sb.ToArray();
        }

        public static Dictionary<string, string>[] ReadData(string fileName, string sectionName)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(fileName);
                var navigator = doc.CreateNavigator();

                var sections = GetStringValues(navigator, sectionName);
                return sections.ToArray();
            }
            catch (XmlException)
            {
            }
            return null;
        }

    }
}
