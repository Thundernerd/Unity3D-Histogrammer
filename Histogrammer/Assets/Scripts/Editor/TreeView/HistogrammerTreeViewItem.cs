using UnityEditor.IMGUI.Controls;

namespace TNRD.Histogrammer
{
    public class HistogrammerTreeViewItem : TreeViewItem
    {
        public readonly SearchResult SearchResult;

        public HistogrammerTreeViewItem(SearchResult searchResult)
        {
            SearchResult = searchResult;
        }
    }
}
