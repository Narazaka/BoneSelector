using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;

namespace Narazaka.Unity.BoneTools
{
    public class BoneReference
    {
        const string EditorOnlyTag = "EditorOnly";
        static Regex EndBoneRe = new Regex("end$", RegexOptions.IgnoreCase);

        public static List<BoneReference> Make(Transform root, bool detectExtraChild = false)
        {
            var boneHierarchy = new Dictionary<Transform, BoneReference>();
            var renderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var renderer in renderers)
            {
                if (renderer.bones == null || renderer.sharedMesh == null) continue;
                var weights = renderer.sharedMesh.GetAllBoneWeights();
                var boneIndexes = weights.Where(w => w.weight > 0).Select(w => w.boneIndex).ToArray();
                foreach (var boneIndex in boneIndexes)
                {
                    var bone = renderer.bones[boneIndex];
                    if (bone == null) continue;
                    if (boneHierarchy.TryGetValue(bone, out var info))
                    {
                        info.References.Add(renderer.transform);
                    }
                    else
                    {
                        boneHierarchy.Add(bone, new BoneReference { Bone = bone, References = new HashSet<Transform> { renderer.transform } });
                    }
                }
            }

            foreach (var bone in boneHierarchy.Keys)
            {
                var info = boneHierarchy[bone];
                if (detectExtraChild)
                {
                    for (var i = 0; i < bone.childCount; i++)
                    {
                        var child = bone.GetChild(i);
                        if (!boneHierarchy.ContainsKey(child))
                        {
                            info.HasExtraChild = true;
                            break;
                        }
                    }
                }
                var parent = bone.parent;
                while (parent != root)
                {
                    info.Parents.Add(parent);
                    parent = parent.parent;
                }
                info.Parents.Reverse();
            }

            return boneHierarchy.Values.ToList();
        }

        public Transform Bone;
        public List<Transform> Parents = new List<Transform>();
        public HashSet<Transform> References = new HashSet<Transform>();
        public bool HasExtraChild = false;

        public bool ReferencesAllEditorOnly { get => References.All(t => t.CompareTag(EditorOnlyTag)); }
        public bool IsEnd { get => EndBoneRe.IsMatch(Bone.name); }
        public string BonePath { get => $"{BoneParentPath}/{Bone.name}"; }
        public string BoneParentPath { get => string.Join("/", Parents.Select(b => b.name)); }
    }
}
