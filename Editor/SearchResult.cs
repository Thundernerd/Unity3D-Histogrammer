using UnityEngine;

namespace TNRD.Histogrammer
{
    public class SearchResult
    {
        public readonly string Name;
        public readonly object Value;
        public readonly string Path;
        public readonly Component Context;

        public SearchResult(string name, object value, string path, Component context)
        {
            Name = name;
            Value = value ?? string.Empty;
            Path = path;
            Context = context;
        }
    }
}
