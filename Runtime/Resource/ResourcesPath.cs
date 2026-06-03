namespace Air.UnityGameCore.Runtime.Resource
{
    /// <summary>Helpers for Unity <c>Resources.Load</c> paths (no file extension).</summary>
    public static class ResourcesPath
    {
        public static string Normalize(string path) =>
            string.IsNullOrWhiteSpace(path)
                ? path
                : path.Replace('\\', '/').Trim('/');
    }
}
