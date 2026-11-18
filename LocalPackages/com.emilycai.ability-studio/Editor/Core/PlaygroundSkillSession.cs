using AbilityStudio.Playground;
using AbilityStudio.Model;
using UnityEditor;
using UnityEngine;
namespace AbilityStudio.Editor
{
    /// <summary>Transfers the selected skill to the running preview without modifying scene asset references.</summary>
    [InitializeOnLoad]
    public static class PlaygroundSkillSession
    {
        private const string Key = "AbilityStudio.PlaygroundRequestedSkill";
        static PlaygroundSkillSession() { EditorApplication.playModeStateChanged += OnState; EditorApplication.update += GuardInput; }
        /// <summary>Requests a saved skill for the next Play transition, or switches the active arena.</summary>
        public static void Request(SkillDefinition skill)
        {
            SessionState.SetString(Key, AssetDatabase.GetAssetPath(skill));
            if (EditorApplication.isPlaying) Apply();
        }
        private static void OnState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode) EditorApplication.delayCall += Apply;
            if (state == PlayModeStateChange.EnteredEditMode) SessionState.EraseString(Key);
        }
        private static void GuardInput()
        {
            if (Application.isBatchMode || !EditorApplication.isPlaying) return;
            var player = Object.FindAnyObjectByType<PlaygroundPlayer>();
            if (player == null) return;
            player.EditorInputBlocked = EditorWindow.focusedWindow == null || EditorWindow.focusedWindow.GetType().Name != "GameView";
            if (player.EditorInputBlocked) player.CancelCharge();
        }
        private static void Apply()
        {
            if (!EditorApplication.isPlaying) return;
            var path = SessionState.GetString(Key, "");
            if (string.IsNullOrEmpty(path)) return;
            SessionState.EraseString(Key);
            var skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>(path);
            var arena = Object.FindAnyObjectByType<PlaygroundArena>();
            if (skill == null || arena == null) return;
            arena.ResetArena(); arena.Player.Configure(skill, arena.Runtime, Camera.main);
        }
    }
}
