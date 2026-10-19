using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using Luban;
using UnityEngine;

namespace Air.UnityGameCore.Runtime
{
    public static class LubanData
    {
        public static string LoadText(string resourcePath)
        {
            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null || string.IsNullOrEmpty(asset.text))
                throw new InvalidOperationException("Missing Luban table: " + resourcePath);
            return asset.text;
        }

        public static ByteBuf LoadBuffer(string resourcePath)
        {
            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
                throw new InvalidOperationException("Missing Luban table: " + resourcePath);
            return new ByteBuf(asset.bytes);
        }

        public static XElement Root(string xml, string name)
        {
            var root = XDocument.Parse(xml).Root;
            if (root == null || root.Name.LocalName != name)
                throw new FormatException("Luban table root must be " + name + ".");
            return root;
        }

        public static string Attr(XElement element, string name)
        {
            var value = element.Attribute(name);
            if (value == null || string.IsNullOrEmpty(value.Value))
                throw new FormatException("Missing Luban attribute " + name + ".");
            return value.Value;
        }

        public static int IntAttr(XElement element, string name)
        {
            if (!int.TryParse(Attr(element, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
                throw new FormatException("Luban attribute " + name + " must be an integer.");
            return number;
        }

        public static bool BoolAttr(XElement element, string name)
        {
            var text = Attr(element, name);
            if (text == "true") return true;
            if (text == "false") return false;
            throw new FormatException("Luban attribute " + name + " must be true or false.");
        }

        public static List<string> Texts(XElement parent, string childName)
        {
            var list = new List<string>();
            foreach (var child in parent.Elements(childName))
            {
                var text = child.Value.Trim();
                if (text.Length == 0)
                    throw new FormatException("Luban element " + childName + " is empty.");
                list.Add(text);
            }
            return list;
        }

        public static string Format(object value) => value == null ? "" : StringUtil.ToStr(value);
    }
}
