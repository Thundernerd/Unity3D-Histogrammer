using System.Collections.Generic;

namespace TNRD.Histogrammer
{
    public class TreeViewData
    {
        public readonly HistogrammerTreeView TreeView;
        public readonly HistogrammerTreeViewState TreeViewState;
        public readonly HistogrammerColumnHeader ColumnHeader;
        public readonly HistogrammerColumnHeaderState ColumnHeaderState;

        private TreeViewData(HistogrammerTreeViewState treeViewState, HistogrammerColumnHeaderState columnHeaderState)
        {
            TreeViewState = treeViewState;
            ColumnHeaderState = columnHeaderState;
            ColumnHeader = new HistogrammerColumnHeader(columnHeaderState);
            TreeView = new HistogrammerTreeView(treeViewState, ColumnHeader);
        }

        public static TreeViewData Create(HistogrammerTreeViewState treeViewState,
            HistogrammerColumnHeaderState columnHeaderState, List<SearchResult> results)
        {
            TreeViewData treeViewData = new TreeViewData(treeViewState, columnHeaderState);
            treeViewData.TreeView.Initialize(results);
            return treeViewData;
        }
    }
}
