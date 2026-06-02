using System;
using System.Collections.Generic;
using Air.GameCore.Serialization;

namespace Air.UnityGameCore.Runtime.Serialization
{
    /// <summary>
    /// Host-provided JSON (e.g. <see cref="NewtonsoftJsonSerializer"/>).
    /// Assign <see cref="Instance"/> before connector HTTP or CLI JSON helpers run.
    /// </summary>
    public static class JsonSerialization
    {
        public static IJsonSerializer Instance { get; set; }

        public static string Serialize(object value) =>
            Require().Serialize(value);

        public static Dictionary<string, object> ParseObject(string json) =>
            Require().ParseObject(json);

        public static object Deserialize(string json) =>
            Require().Deserialize(json);

        static IJsonSerializer Require()
        {
            if (Instance == null)
                throw new InvalidOperationException(
                    "JsonSerialization.Instance is not set. Call JsonSerializationBootstrap.EnsureRegistered() or assign NewtonsoftJsonSerializer.Default.");
            return Instance;
        }
    }
}
