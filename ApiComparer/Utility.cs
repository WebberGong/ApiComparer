using System;
using ApiComparer.Model.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiComparer
{
    public class Utility
    {
        public static JArray TryParseToJArray(string text)
        {
            try
            {
                return JArray.FromObject(JsonConvert.DeserializeObject(text));
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Try parse to json array failed.\r\n" + ex.Message);
                return null;
            }
        }

        public static JObject TryParseToJObject(string text)
        {
            try
            {
                return JObject.FromObject(JsonConvert.DeserializeObject(text));
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Try parse to json object failed.\r\n" + ex.Message);
                return null;
            }
        }

        public static object TryParseToObject(string text, Action<JObject> parseSuccessfully = null,
            Action<string> parseFailed = null)
        {
            var jObj = TryParseToJObject(text);
            if (jObj == null)
            {
                var trimedText = Trim(text);
                parseFailed?.Invoke(trimedText);
                return trimedText;
            }
            parseSuccessfully?.Invoke(jObj);

            return jObj;
        }

        public static object TryParseToObject(string text, States parent, Action<StateNode> parseSuccessfully = null,
            Action<string> parseFailed = null)
        {
            text = text.Trim();
            var jObj = TryParseToJObject(text);
            if (jObj == null)
            {
                var trimedText = Trim(text);
                parseFailed?.Invoke(trimedText);
                return trimedText;
            }
            var node = new StateNode(parent, null, new KeyValueState<string, string>(parent, "Content", string.Empty),
                JTokenType.Object, false);
            BuildStateNode(node, jObj);
            parseSuccessfully?.Invoke(node);

            return node;
        }

        public static void BuildStateNode(StateNode node, JToken jToken)
        {
            if (jToken == null)
                return;

            switch (jToken.Type)
            {
                case JTokenType.Object:
                    var objectNode = AddNode(node, jToken.Type.ToString(), jToken.Type);
                    using (var enumerator = jToken.Children().GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                            BuildStateNode(objectNode, enumerator.Current);
                    }
                    break;
                case JTokenType.Array:
                    var arrayNode = AddNode(node, jToken.Type.ToString(), jToken.Type);
                    BuildStateNode(arrayNode, jToken.First);
                    break;
                case JTokenType.Property:
                    var jProperty = jToken as JProperty;
                    if (jProperty != null)
                        if (jProperty.Value.HasValues)
                        {
                            var propertyNode = AddNode(node, jProperty.Name, jProperty.Value.Type);
                            BuildStateNode(propertyNode, jProperty.Value);
                        }
                        else
                        {
                            var value = jProperty.Value.ToString();
                            if (jProperty.Value.Type == JTokenType.Float && !value.Contains("."))
                            {
                                value += ".0";
                            }
                            AddLeafNode(node, jProperty.Name, value, jProperty.Value.Type);
                        }
                    break;
                default:
                    var jValue = jToken as JValue;
                    if (jValue != null)
                        AddLeafNode(node, jValue.Type.ToString(), jValue.Value.ToString(), jValue.Type);
                    else
                        throw new ArgumentException("Unhandled json type");
                    break;
            }
        }

        private static StateNode AddNode(StateNode parentNode, string key, JTokenType type)
        {
            var childNode = new StateNode(parentNode, parentNode,
                new KeyValueState<string, string>(parentNode, key, string.Empty), type, false);
            parentNode.ChildNodes.Add(childNode);
            return childNode;
        }

        private static StateNode AddLeafNode(StateNode parentNode, string key, string value, JTokenType type)
        {
            var childNode = new StateNode(parentNode, parentNode,
                new KeyValueState<string, string>(parentNode, key, value.Trim()), type, true);
            parentNode.ChildNodes.Add(childNode);
            return childNode;
        }

        public static string Trim(string text)
        {
            try
            {
                return text.Trim('\r', '\n', ' ');
            }
            catch
            {
                throw;
            }
        }
    }
}