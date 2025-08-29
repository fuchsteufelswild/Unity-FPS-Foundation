using System;
using System.Collections.Generic;
using UnityEngine;
using Nexora.InputSystem;

namespace Nexora.UI
{
    public sealed class ActivePanelStack
    {
        public const int UndefinedLayer = -1;
        public const int MaxLayerCount = 16;

        private static readonly ActivePanelStack _instance = new();
        private readonly Dictionary<int, List<PanelBase>> _panelStacksByLayer = new();

        public static event Action<int, PanelBase> LayerTopPanelChanged;

#if UNITY_EDITOR
        /// <summary>
        /// To prevent invalid state of the <see langword="static"/> class.
        /// Clears the '<see cref="_panelStacksByLayer"/>' to automatically clear the list
        /// in the Editor.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void EditorReload()
        {
            LayerTopPanelChanged = null;
            _instance._panelStacksByLayer.Clear();
        }
#endif

        /// <summary>
        /// Registers the <paramref name="panel"/> into the registry and updates the layer stack.
        /// </summary>
        /// <returns>If operation was successful.</returns>
        public static bool AddPanel(PanelBase panel, PanelShowOptions options = null)
        {
            // We don't register panels that cannot use cancel input
            // and not having a layer
            if(_instance.ShouldUseLayerStacking(panel) == false)
            {
                return true;
            }

            return _instance.AddPanelToLayerStack(panel, options ?? PanelShowOptions.Default);
        }

        /// <summary>
        /// Only classes with valid layers, and/or the ones that are can be closed by
        /// cancel input can be used for layer stacking.
        /// </summary>
        private bool ShouldUseLayerStacking(PanelBase panel)
        {
            int layer = panel.PanelLayer;
            return layer < MaxLayerCount && (layer != UndefinedLayer || panel.HandlesCancelInput);
        }

        /// <summary>
        /// Adds <paramref name="panel"/> to the <paramref name="panelList"/> and if the
        /// <paramref name="panel"/> is valid hides the top panel of that layer.
        /// </summary>
        /// <returns>If operation is successful (Panel is not already the top panel).</returns>
        private bool AddPanelToLayerStack(PanelBase panel, PanelShowOptions options)
        {
            List<PanelBase> panelList = GetOrCreatePanelList(panel);

            var topPanel = panelList.Last();
            if (topPanel == panel)
            {
                return false;
            }

            if (ShouldHideTopPanel())
            {
                topPanel.SetVisibility(false);
            }

            if (panel.HandlesCancelInput)
            {
                InputModule.Instance.AddCancelAction(panel.Deactivate);
            }

            panelList.MoveToEnd(panel);

            return true;

            bool ShouldHideTopPanel()
            {
                return panel.IsModal
                    && topPanel != null
                    && options.ShowOverCurrent == false;
            }
        }

        /// <summary>
        /// Gets or create panel list for the given <paramref name="layer"/>.
        /// </summary>
        private List<PanelBase> GetOrCreatePanelList(PanelBase panel)
        {
            int clampedLayer = GetClampedLayer();

            if (_panelStacksByLayer.TryGetValue(clampedLayer, out List<PanelBase> panelList))
            {
                return panelList;
            }

            return _panelStacksByLayer[clampedLayer] = new List<PanelBase>();

            // We treat -1 as 0, to be able to use its canceling input capabilities
            // Note: We don't see them as a valid panel when popping from stack,
            // they are just standing there but have no responsibility in the stack
            int GetClampedLayer()
            {
                int layer = panel.PanelLayer;
                return Mathf.Clamp(layer, 0, MaxLayerCount - 1);
            }
        }

        /// <summary>
        /// Unregisters <paramref name="panel"/> from the registry and updates the layer stack.
        /// </summary>
        /// <returns>
        /// If operation was succesful.
        /// </returns>
        public static bool RemovePanel(PanelBase panel, PanelHideOptions options = null)
        {
            if(panel == null)
            {
                return false;
            }

            _instance.RemovePanelFromStack(panel, options ?? PanelHideOptions.Default);
            return true;
        }

        private void RemovePanelFromStack(PanelBase panel, PanelHideOptions options)
        {
            List<PanelBase> panelList = GetOrCreatePanelList(panel);

            if(panelList.Contains(panel) == false)
            {
                return;
            }

            RemovePanelFromList(panel, panelList, options);
        }

        private void RemovePanelFromList(PanelBase panel, List<PanelBase> panelList, PanelHideOptions options)
        {
            panelList.Remove(panel);

            if(panel.HandlesCancelInput)
            {
                InputModule.Instance.RemoveCancelAction(panel.Deactivate);
            }

            if(panel.IsModal && options.HideStackBelow == false)
            {
                if(panelList.TryGetLast(out var newTopPanel))
                {
                    newTopPanel.SetVisibility(true);
                }

                LayerTopPanelChanged?.Invoke(panel.PanelLayer, newTopPanel);
            }
        }
    }
}