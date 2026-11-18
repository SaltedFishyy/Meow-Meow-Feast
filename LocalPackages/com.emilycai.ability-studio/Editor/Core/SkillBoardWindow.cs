using System;
using System.Linq;
using AbilityStudio.Model;
using AbilityStudio.Playground;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbilityStudio.Editor
{
    /// <summary>Hosts the composition board and the standalone Playground workflow.</summary>
    public sealed class SkillBoardWindow : EditorWindow
    {
        [SerializeField] private SkillDefinition _skill;
        private IBoardView _board;
        private VisualElement _canvas;
        private HelpBox _status;
        private IVisualElementScheduledItem _statusWatch;
        /// <summary>Opens the selected skill or the shared prototype asset.</summary>
        [MenuItem("Tools/技能工坊/技能画板")]
        public static void Open() => OpenSkill(Selection.activeObject as SkillDefinition ??
            AssetDatabase.LoadAssetAtPath<SkillDefinition>(PrototypeSceneCommands.SkillPath));
        /// <summary>Opens a specific shared asset without copying its data.</summary>
        public static void OpenSkill(SkillDefinition skill)
        {
            var window = GetWindow<SkillBoardWindow>("技能画板", true, typeof(SceneView));
            window._skill = skill;
            window.CreateGUI();
            window.Show();
        }
        /// <summary>Recreates UI after domain reload, retaining the selected asset.</summary>
        public void CreateGUI()
        {
            titleContent = new GUIContent("技能画板");
            _board?.Dispose();
            _statusWatch?.Pause();
            rootVisualElement.Clear();
            minSize = new Vector2(780, 460);
            var toolbar = new Toolbar();
            var asset = new ObjectField { objectType = typeof(SkillDefinition), allowSceneObjects = false, value = _skill };
            asset.style.width = 240;
            asset.RegisterValueChangedCallback(evt => { _skill = evt.newValue as SkillDefinition; _board?.Bind(_skill); PlaygroundWindow.FollowSkill(_skill); });
            toolbar.Add(asset);
            toolbar.Add(new ToolbarButton(() => { if (_skill != null) SkillAssetCommands.Save(_skill); }) { text = "保存技能" });
            toolbar.Add(new ToolbarButton(() => _board?.FrameAllModules()) { text = "显示全部" });
            toolbar.Add(new ToolbarButton(() => _board?.DeleteSelectedModules()) { text = "删除选中" });
            toolbar.Add(new ToolbarButton(() => PlaygroundWindow.OpenSkill(_skill)) { text = "独立 Playground 预览" });
            toolbar.Add(new ToolbarButton(Playground) { text = "进入测试场景" });
            toolbar.Add(new ToolbarButton(() => { Selection.activeObject = _skill; EditorGUIUtility.PingObject(_skill); }) { text = "查看技能" });
            rootVisualElement.Add(toolbar);
            rootVisualElement.Add(new HelpBox("修改参数后按回车，下次施法生效。效果必须连接目标，命中响应必须连接投射物。点击“保存技能”保留修改。", HelpBoxMessageType.Info));
            _status = new HelpBox("", HelpBoxMessageType.Info);
            rootVisualElement.Add(_status);
            var workspace = new VisualElement(); workspace.style.flexDirection = FlexDirection.Row; workspace.style.flexGrow = 1;
            workspace.Add(new ModuleLibraryView(() => _skill));
            _canvas = new VisualElement(); _canvas.style.flexGrow = 1; _canvas.style.minWidth = 0;
            workspace.Add(_canvas); rootVisualElement.Add(workspace);
            var type = TypeCache.GetTypesDerivedFrom<IBoardView>().FirstOrDefault(t => !t.IsAbstract && !t.IsInterface);
            _board = type == null ? null : (IBoardView)Activator.CreateInstance(type);
            if (_board == null) _canvas.Add(new HelpBox("技能画板暂时不可用。", HelpBoxMessageType.Error));
            else { _canvas.Add(_board.Element); _board.Bind(_skill); }
            _statusWatch = rootVisualElement.schedule.Execute(UpdateStatus).Every(250);
        }
        private void UpdateStatus()
        {
            if (_status == null) return;
            var issues = SkillValidator.Validate(_skill);
            _status.text = issues.Count > 0 ? string.Join("\n", issues) : "就绪 · 共用技能资产 · 支持撤销与重做";
            _status.messageType = issues.Count > 0 ? HelpBoxMessageType.Warning : HelpBoxMessageType.Info;
        }
        private void Playground()
        {
            if (_skill == null || string.IsNullOrEmpty(AssetDatabase.GetAssetPath(_skill)))
            {
                ShowNotification(new GUIContent("请选择已保存的技能资产。")); return;
            }
            if (!EditorApplication.isPlaying)
            {
                if (!PrototypeSceneCommands.TryOpenPlayground()) return;
                PlaygroundSkillSession.Request(_skill);
                EditorApplication.isPlaying = true;
            }
            else
            {
                if (UnityEngine.Object.FindAnyObjectByType<PlaygroundArena>() == null)
                { ShowNotification(new GUIContent("请先停止运行，再打开测试场景。")); return; }
                PlaygroundSkillSession.Request(_skill);
            }
            EditorApplication.ExecuteMenuItem("Window/General/Game");
        }
        private void OnDisable() { _statusWatch?.Pause(); _board?.Dispose(); _board = null; }
    }
}


