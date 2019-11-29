using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TNRD.Histogrammer
{
    public class HistogrammerEditorWindow : EditorWindow, ISerializationCallbackReceiver
    {
        [MenuItem("Window/TNRD/Histogrammer")]
        private static void Open()
        {
            HistogrammerEditorWindow wnd = GetWindow<HistogrammerEditorWindow>(false, "Histogrammer", true);
            wnd.minSize = new Vector2(320, 320);
            wnd.Show();
        }

        private static GUIContent[] DEFAULT_POPUP_CONTENT = {new GUIContent("None")};

        [SerializeField] private MonoScript monoScript;
        [SerializeField] private Vector2 scrollPosition;

        private Type scriptType;
        private List<FieldInfo> filteredFields = new List<FieldInfo>();
        private GUIContent[] popupContent = DEFAULT_POPUP_CONTENT;
        private int selectedIndex;
        private FieldInfo SelectedField { get { return filteredFields[selectedIndex]; } }

        private GUIStyle headerStyle;

        private Rect actualBoxRect;

        private Dictionary<object, List<SearchResult>> valueToSearchResults =
            new Dictionary<object, List<SearchResult>>();

        private Dictionary<object, bool> valueToFoldout =
            new Dictionary<object, bool>();

        private Dictionary<object, TreeViewData> valueToTreeViewData =
            new Dictionary<object, TreeViewData>();

        private int totalSearchResults;

        private void OnEnable()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleLeft
                };
            }
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            monoScript = (MonoScript) EditorGUILayout.ObjectField("Script", monoScript, typeof(MonoScript), false);
            if (EditorGUI.EndChangeCheck())
            {
                LoadType();
                CreatePopupContent();
            }

            EditorGUI.BeginDisabledGroup(ShouldDisableFieldPopup());
            selectedIndex = EditorGUILayout.Popup(new GUIContent("Field"), selectedIndex, popupContent);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(ShouldDisableSearchButton());
            if (GUILayout.Button("Search"))
            {
                Search();
            }

            EditorGUI.EndDisabledGroup();

            OnResultsGUI();
        }

        private void LoadType()
        {
            filteredFields.Clear();

            if (monoScript == null)
                return;

            scriptType = monoScript.GetClass();

            LoadFields();
        }

        private void LoadFields()
        {
            List<FieldInfo> fields = TypeUtility.GetFieldsUpTo(scriptType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                typeof(MonoBehaviour));

            foreach (FieldInfo fieldInfo in fields)
            {
                if (fieldInfo.IsPublic)
                {
                    if (TypeUtility.HasCustomAttribute<NonSerializedAttribute>(fieldInfo))
                        continue;

                    filteredFields.Add(fieldInfo);
                    continue;
                }

                if (!TypeUtility.HasCustomAttribute<SerializeField>(fieldInfo))
                    continue;

                filteredFields.Add(fieldInfo);
            }

            filteredFields = filteredFields
                .OrderBy(x => x.DeclaringType.Name)
                .ThenBy(x => x.Name)
                .ToList();
        }

        private void CreatePopupContent()
        {
            if (monoScript == null || filteredFields.Count == 0)
            {
                popupContent = new[] {new GUIContent("None")};
                selectedIndex = 0;
                return;
            }

            popupContent = new GUIContent[filteredFields.Count];

            for (int i = 0; i < filteredFields.Count; i++)
            {
                FieldInfo field = filteredFields[i];
                string name = string.Format("{0}/{1}",
                    ObjectNames.NicifyVariableName(field.DeclaringType.Name),
                    ObjectNames.NicifyVariableName(field.Name));
                GUIContent content = new GUIContent(name);
                popupContent[i] = content;
            }
        }

        private bool ShouldDisableFieldPopup()
        {
            if (monoScript == null)
                return true;

            return false;
        }

        private bool ShouldDisableSearchButton()
        {
            if (monoScript == null)
                return true;

            if (filteredFields.Count == 0)
                return true;

            return false;
        }

        private void Search()
        {
            SlimClear();
            SearchPrefabs();
            CreateTreeViews();

            totalSearchResults = valueToSearchResults.Sum(x => x.Value.Count);
        }

        private void SearchPrefabs()
        {
            string[] paths = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);

            FieldInfo fieldInfo = SelectedField;

            try
            {
                float step = 1f / paths.Length;
                float progress = 0f;

                foreach (string path in paths)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Hold on...", "Scanning prefabs", progress))
                    {
                        SlimClear();
                        break;
                    }

                    string trimmedPath = path.Replace(Application.dataPath, string.Empty);
                    string relativePath = string.Format("Assets{0}", trimmedPath);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(relativePath);
                    Component[] components = prefab.GetComponentsInChildren(scriptType);

                    foreach (Component component in components)
                    {
                        object value = fieldInfo.GetValue(component);
                        string name = GetPath(component, prefab);
                        SearchResult result = new SearchResult(name, value, relativePath, component);
                        AddSearchResult(result);
                    }

                    progress += step;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private string GetPath(Component context, GameObject upperLimit)
        {
            string path = context.GetType().Name;

            Transform transform = context.transform;

            while (transform != null)
            {
                path = string.Format("{0}/{1}", transform.name, path);
                if (transform.gameObject == upperLimit)
                    break;
                transform = transform.parent;
            }

            return path;
        }

        private void AddSearchResult(SearchResult result)
        {
            if (!valueToSearchResults.ContainsKey(result.Value))
            {
                valueToSearchResults.Add(result.Value, new List<SearchResult>());
                valueToFoldout.Add(result.Value, false);
            }

            valueToSearchResults[result.Value].Add(result);
        }

        private void CreateTreeViews()
        {
            foreach (KeyValuePair<object, List<SearchResult>> result in valueToSearchResults)
            {
                HistogrammerColumnHeaderState columnHeaderState = HistogrammerColumnHeaderState.Create();
                HistogrammerTreeViewState treeViewState = new HistogrammerTreeViewState();
                TreeViewData treeViewData = TreeViewData.Create(treeViewState, columnHeaderState, result.Value);
                valueToTreeViewData.Add(result.Key, treeViewData);
            }
        }

        private void OnResultsGUI()
        {
            EditorGUILayout.Space();

            Rect headerRect = EditorGUILayout.GetControlRect(false, 20f, GUILayout.ExpandWidth(true));
            headerRect.x = 0;
            headerRect.width = position.size.x;
            GUI.Box(headerRect, string.Empty, EditorStyles.toolbar);
            headerRect.xMin += 5;
            EditorGUI.LabelField(headerRect, "Results", headerStyle);

            Rect boxRect =
                EditorGUILayout.GetControlRect(false, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            boxRect.x = 0;
            boxRect.y -= EditorGUIUtility.standardVerticalSpacing + 2;
            boxRect.width = position.size.x;
            boxRect.height += EditorGUIUtility.standardVerticalSpacing + 2;
            GUI.Box(boxRect, string.Empty);

            if (Event.current.type == EventType.Repaint)
                actualBoxRect = boxRect;

            if (valueToSearchResults.Count == 0)
            {
                EditorGUI.LabelField(actualBoxRect, "No results available");
                return;
            }

            GUILayout.BeginArea(actualBoxRect);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (KeyValuePair<object, List<SearchResult>> keyValuePair in valueToSearchResults)
            {
                OnResultGUI(keyValuePair.Key, keyValuePair.Value);
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void OnResultGUI(object value, List<SearchResult> results)
        {
            Rect rect = EditorGUILayout.GetControlRect(false);
            float count = results.Count;
            EditorGUI.ProgressBar(rect, count / totalSearchResults,
                string.Format("{0} ({1}/{2})", ValueToString(value), count, totalSearchResults));

            if (IsMouseDown(rect))
            {
                valueToFoldout[value] = !valueToFoldout[value];
                Repaint();
                GUIUtility.ExitGUI();
            }

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            OnFoldoutGUI(value);
        }

        private void OnFoldoutGUI(object value)
        {
            if (!valueToFoldout[value])
                return;

            TreeViewData treeViewData = valueToTreeViewData[value];
            Rect rect = EditorGUILayout.GetControlRect(false, treeViewData.TreeView.totalHeight);
            treeViewData.TreeView.OnGUI(rect);
        }

        private string ValueToString(object value)
        {
            if (value == null)
                return "NULL";

            Type valueType = value.GetType();
            if (valueType == typeof(string))
            {
                if (string.IsNullOrEmpty((string) value))
                    return "NULL";
            }
            else if (valueType == typeof(LayerMask))
            {
                return LayerMask.LayerToName(((LayerMask) value).value);
            }

            if (TypeUtility.IsEnumerable(value))
            {
                IEnumerable enumerable = (IEnumerable) value;
                string[] stringified = enumerable.Cast<object>()
                    .Select(ValueToString)
                    .ToArray();
                return string.Format("[{0}]", string.Join(", ", stringified));
            }

            return value.ToString();
        }

        private bool IsMouseDown(Rect rect)
        {
            Event evt = Event.current;
            return evt.type == EventType.MouseDown && evt.button == 0 && rect.Contains(evt.mousePosition);
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            FullReset();
        }

        private void FullReset()
        {
            monoScript = null;
            selectedIndex = 0;
            popupContent = DEFAULT_POPUP_CONTENT;
            SlimClear();
        }

        private void SlimClear()
        {
            scrollPosition = Vector2.zero;
            valueToFoldout.Clear();
            valueToSearchResults.Clear();
            valueToTreeViewData.Clear();
        }
    }
}
