using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace TNRD.Histogrammer
{
    public class HistogrammerColumnHeaderState : MultiColumnHeaderState
    {
        private HistogrammerColumnHeaderState(Column[] columns) : base(columns)
        {
        }

        public static HistogrammerColumnHeaderState Create()
        {
            return new HistogrammerColumnHeaderState(
                new[]
                {
                    new Column()
                    {
                        autoResize = true,
                        canSort = false,
                        headerContent = new GUIContent("Name"),
                        width = 100,
                    },
                    new Column()
                    {
                        autoResize = true,
                        canSort = false,
                        headerContent = new GUIContent("Path"),
                        width = 200,
                    },
                });
        }
    }
}
