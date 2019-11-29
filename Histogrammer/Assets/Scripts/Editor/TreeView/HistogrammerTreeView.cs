using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace TNRD.Histogrammer
{
    public class HistogrammerTreeView : TreeView
    {
        private List<TreeViewItem> items = new List<TreeViewItem>();

        private Dictionary<int, TreeViewItem> hashToTreeViewItem = new Dictionary<int, TreeViewItem>();

        private HistogrammerTreeView(TreeViewState state) : base(state)
        {
        }

        public HistogrammerTreeView(HistogrammerTreeViewState state, HistogrammerColumnHeader multiColumnHeader)
            : base(state, multiColumnHeader)
        {
            showAlternatingRowBackgrounds = true;
            showBorder = true;
        }

        public void Initialize(List<SearchResult> results)
        {
            items = new List<TreeViewItem>();

            int id = 0;
            foreach (SearchResult result in results)
            {
                string[] splits = result.Name.Split('/');

                int depth = -1;
                TreeViewItem parent = null;
                for (int i = 0; i < splits.Length; i++)
                {
                    string split = splits[i];
                    int hash = GetHash(i, splits.Length, result.Context);

                    TreeViewItem treeViewItem;
                    if (!hashToTreeViewItem.TryGetValue(hash, out treeViewItem))
                    {
                        treeViewItem = new HistogrammerTreeViewItem(result)
                        {
                            id = ++id,
                            depth = ++depth,
                            displayName = split,
                            icon = i == splits.Length - 1
                                ? (Texture2D) EditorGUIUtility.ObjectContent(null, typeof(MonoScript)).image
                                : (Texture2D) EditorGUIUtility.ObjectContent(null, typeof(GameObject)).image
                        };

                        hashToTreeViewItem[hash] = treeViewItem;

                        if (i == 0) // Only add top level to the list, others are children
                            items.Add(treeViewItem);
                    }
                    else
                    {
                        ++depth;
                    }

                    if (parent != null) 
                        parent.AddChild(treeViewItem);

                    parent = treeViewItem;
                }
            }

            Reload();
        }

        private int GetHash(int index, int total, Component context)
        {
            // Root/GameObject/Component
            // loop/total-1/total-2 

            if (index == total - 1) // context itself
                return context.GetHashCode();
            if (index == total - 2) // context's gameobject
                return context.transform.GetHashCode();

            int count = total - index - 2;
            for (int i = 0; i < count; i++)
            {
                context = context.transform.parent;
            }

            return context.GetHashCode();
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(0, -1, "Root");

            foreach (TreeViewItem item in items)
            {
                root.AddChild(item);
            }

            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); i++)
            {
                Rect rect = args.GetCellRect(i);
                TreeViewItem item = args.item;
                int column = args.GetColumn(i);
                CellGUI(rect, item, column, ref args);
            }
        }

        private void CellGUI(Rect rect, TreeViewItem item, int column, ref RowGUIArgs args)
        {
            HistogrammerTreeViewItem histogrammerTreeViewItem = (HistogrammerTreeViewItem) item;

            switch (column)
            {
                case 0:
                {
                    args.rowRect = rect;
                    base.RowGUI(args);
                    break;
                }
                case 1:
                {
                    if (item.depth == 0)
                    {
                        EditorGUI.LabelField(rect, histogrammerTreeViewItem.SearchResult.Path);
                    }

                    break;
                }
            }
        }

        protected override void SingleClickedItem(int id)
        {
            HistogrammerTreeViewItem item = FindItem(id);
            GameObject root = GetRootForSelection(item.SearchResult.Context);
            EditorGUIUtility.PingObject(root);
        }

        protected override void DoubleClickedItem(int id)
        {
            HistogrammerTreeViewItem item = FindItem(id);
            GameObject root = GetRootForSelection(item.SearchResult.Context);
            AssetDatabase.OpenAsset(root);
        }

        private HistogrammerTreeViewItem FindItem(int id)
        {
            IList<TreeViewItem> rows = FindRows(new List<int> {id});
            if (rows.Count != 1)
                return null;

            HistogrammerTreeViewItem item = (HistogrammerTreeViewItem) rows.First();
            return item;
        }

        private GameObject GetRootForSelection(Component context)
        {
            Transform root = context.transform;

            while (root.parent != null)
            {
                root = context.gameObject.transform.parent;
            }

            return root.gameObject;
        }
    }
}
