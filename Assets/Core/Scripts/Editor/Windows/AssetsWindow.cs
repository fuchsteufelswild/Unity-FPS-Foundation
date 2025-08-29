using Codice.Client.BaseCommands;
using Toolbox.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Nexora.Editor
{
    /// <summary>
    /// A window to inspect and modify available assets(<see cref="ScriptableObject"/>).
    /// There is a <see cref="TreeView"/> sidebar on the left-side to easy navigation supported with keyboard
    /// that halps navigate through different types of asset types. 
    /// </summary>
    /// <remarks>
    /// Main content renders are controlled by <see cref="IWindow"/> classes.
    /// </remarks>
    public sealed class AssetsWindow : TreeBasedWindow<AssetsWindow>
    {
        private const float SidebarWidth = 150f;

        public override string WindowTitle => "Game Assets";

        protected override string TreeViewStateSessionKey { get; } = "Nexora.AssetsWindowsTreeViewState";

        private TreeViewSidebarDrawer _treeViewDrawer;

        private GUILayoutOption[] ContentAreaLayoutOptions { get; } =
        {
            GUILayout.ExpandWidth(true),
            GUILayout.ExpandHeight(true)
        };

        private GUILayoutOption[] SidebarLayoutOptions { get; } =
        {
            GUILayout.Width(SidebarWidth)
        };

        [MenuItem("Nexora/Assets Window")]
        private static void ShowAssetsWindow() => GetOrCreateWindow();

        protected override void InitializeComponents()
        {
            base.InitializeComponents();
            _treeViewDrawer = 
                new TreeViewSidebarDrawer(_windowTreeView, SidebarWidth, SidebarLayoutOptions, ContentAreaLayoutOptions);
        }

        private void OnGUI()
        {
            using(new GUILayout.HorizontalScope())
            {
                _treeViewDrawer?.Draw();
                DrawMainContent();
            }
        }

        private void DrawMainContent()
        {
            using (new GUILayout.VerticalScope())
            {
                _windowTreeView?.SelectedWindowViewItem?.WindowView?.RenderViewContent();
            }
        }

        /// <summary>
        /// Draws the <see cref="TreeView"/> as a sidebar on the left side of the window,
        /// with a search field above.
        /// </summary>
        private class TreeViewSidebarDrawer
        {
            private const float DefaultSpaceBetweenRows = 2f;

            private readonly WindowTreeView _windowTreeView;
            private readonly float _sidebarWidth = 250f;
            private readonly GUILayoutOption[] _sidebarLayoutOptions = new GUILayoutOption[0];
            private readonly GUILayoutOption[] _contentAreaLayoutOptions = new GUILayoutOption[0];

            private readonly SearchField _searchField;

            public TreeViewSidebarDrawer(
                WindowTreeView windowTreeView, 
                float sidebarWidth, 
                GUILayoutOption[] sidebarLayoutOptions,
                GUILayoutOption[] contentAreaLayoutOptions)
            {
                _windowTreeView = windowTreeView;
                _sidebarWidth = sidebarWidth;
                _sidebarLayoutOptions = sidebarLayoutOptions;
                _contentAreaLayoutOptions = contentAreaLayoutOptions;
                _searchField = new SearchField();
            }

            public void Draw()
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox, _sidebarLayoutOptions))
                {
                    GUILayout.Space(DefaultSpaceBetweenRows);
                    DrawTreeViewSearchField();
                    HandleNavigationInput();
                    DrawTreeView();
                }
            }

            private void DrawTreeViewSearchField()
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(DefaultSpaceBetweenRows);
                    _windowTreeView.searchString = _searchField.OnToolbarGUI(_windowTreeView.searchString);
                }
            }

            private void HandleNavigationInput()
            {
                var currentEvent = Event.current;
                if (currentEvent.type == EventType.KeyDown && currentEvent.control)
                {
                    switch (currentEvent.keyCode)
                    {
                        case KeyCode.LeftArrow:
                            FocusOnSidebar(currentEvent);
                            break;
                        case KeyCode.RightArrow:
                            FocusOnMainContent(currentEvent);
                            break;
                    }
                }
            }

            private void FocusOnSidebar(Event evt)
            {
                if (_windowTreeView.HasFocus() == false)
                {
                    _windowTreeView.SetFocus();
                    evt.Use();
                }
            }

            private void FocusOnMainContent(Event evt)
            {
                var selectedView = _windowTreeView.SelectedWindowViewItem?.WindowView;
                if (selectedView != null && selectedView.HasInputFocus() == false)
                {
                    selectedView.RequestInputFocus();
                    evt.Use();
                }
            }

            private void DrawTreeView()
            {
                var treeRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, _contentAreaLayoutOptions);
                _windowTreeView.OnGUI(treeRect);
            }
        }

    }
}
