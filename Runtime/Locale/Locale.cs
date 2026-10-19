using System.Collections.Generic;

namespace Air.UnityGameCore.Runtime
{
    public static class Locale
    {
        static Dictionary<string, string> table;

        public static void Use(IReadOnlyDictionary<string, string> entries)
        {
            table = entries == null ? null : new Dictionary<string, string>(entries);
        }

        public static string Lookup(string key)
        {
            if (string.IsNullOrEmpty(key) || table == null) return null;
            return table.TryGetValue(key, out var value) ? value : null;
        }

        public static string Line(string key, params (string name, string value)[] slots)
        {
            var text = Lookup(key);
            if (string.IsNullOrEmpty(text)) text = key ?? "";
            if (slots == null) return text;
            for (int i = 0; i < slots.Length; i++)
                text = text.Replace("{" + slots[i].name + "}", slots[i].value ?? "");
            return text;
        }
    }
}
