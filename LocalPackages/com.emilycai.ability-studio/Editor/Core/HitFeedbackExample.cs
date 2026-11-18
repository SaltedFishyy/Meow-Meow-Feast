using System.IO;
using AbilityStudio.Model;
using UnityEditor;
using UnityEngine;
namespace AbilityStudio.Editor
{
    /// <summary>Creates a self-contained feedback sample without replacing existing skills or resources.</summary>
    public static class HitFeedbackExample
    {
        private const string Folder = "Assets/_Sandbox/HitFeedback2D";
        /// <summary>Creates or opens a sample with a generated visual and short audio clip.</summary>
        [MenuItem("Tools/技能工坊/创建命中反馈示例")]
        public static void Open()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            SkillBoardWindow.OpenSkill(Create());
        }
        /// <summary>Returns the existing sample or creates it once, including persistent media resources.</summary>
        public static SkillDefinition Create()
        {
            EnsureFolder(Folder);
            string path = Folder + "/HitFeedback.asset";
            var skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>(path);
            if (skill == null)
            {
                skill = ScriptableObject.CreateInstance<SkillDefinition>();
                skill.Id = "hit_feedback"; skill.DisplayName = "命中反馈示例"; skill.IntegratedAttacks = true;
                var attack = new ConeAttack { Range = 12, AngleDegrees = 120 };
                attack.Settings.BaseDamage = 1; attack.Settings.Cooldown = .5f;
                skill.AddAttack(attack);
                skill.SetBinding(SkillDefinition.CoreLayoutId, attack.ModuleId, BindingRules.Trigger, BindingRules.Normal);
                var effects = new HitFeedbackEffect[] { new CameraShakeFeedback(), new HitVfxFeedback { Prefab = Visual(), Lifetime = .18f, Scale = .5f, Offset = Vector3.back * .2f }, new HitAudioFeedback { Clip = Sound(), Volume = .35f, MinInterval = .08f }, new RecoilFeedback() };
                skill.SetBoardPosition(SkillDefinition.CoreLayoutId, new Vector2(0, 160));
                skill.SetBoardPosition(attack.ModuleId, new Vector2(420, 160));
                for (int i = 0; i < effects.Length; i++)
                {
                    skill.AddEffect(effects[i]); skill.SetBinding(attack.ModuleId, effects[i].ModuleId, BindingRules.Targets);
                    skill.SetBoardPosition(effects[i].ModuleId, new Vector2(920, i * 400));
                }
                SkillAssetCommands.Create(skill, path);
            }
            return skill;
        }
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent); AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
        private static GameObject Visual()
        {
            string path = Folder + "/Impact.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;
            var material = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/Impact.mat");
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
                material = new Material(shader); material.color = new Color(1, .65f, .15f);
                AssetDatabase.CreateAsset(material, Folder + "/Impact.mat");
            }
            var instance = GameObject.CreatePrimitive(PrimitiveType.Quad);
            try
            {
                Object.DestroyImmediate(instance.GetComponent<Collider>());
                instance.GetComponent<Renderer>().sharedMaterial = material;
                return PrefabUtility.SaveAsPrefabAsset(instance, path);
            }
            finally { Object.DestroyImmediate(instance); }
        }
        private static AudioClip Sound()
        {
            string path = Folder + "/Impact.wav";
            if (!File.Exists(path))
            {
                const int rate = 22050, count = 2205;
                using (var writer = new BinaryWriter(File.Create(path)))
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + count * 2);
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
                    writer.Write((short)1); writer.Write((short)1); writer.Write(rate); writer.Write(rate * 2);
                    writer.Write((short)2); writer.Write((short)16); writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(count * 2);
                    for (int i = 0; i < count; i++)
                    { float t = (float)i / rate; writer.Write((short)(Mathf.Sin(2 * Mathf.PI * 180 * t) * Mathf.Exp(-60 * t) * 20000)); }
                }
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
    }
}
