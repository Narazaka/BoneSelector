using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace Narazaka.Unity.BoneTools
{
    public class BoneSelector
    {
        public IList<BoneReference> BoneReferences { get; }
        public IList<Transform> AllReferences
        {
            get
            {
                if (AllReferencesCache == null)
                {
                    AllReferencesCache = BoneReferences.SelectMany(r => r.References).Distinct().ToList();
                }
                return AllReferencesCache;
            }
        }
        IList<Transform> AllReferencesCache;

        public BoneSelector(IList<BoneReference> boneReferences)
        {
            BoneReferences = boneReferences;
        }

        public IEnumerable<BoneReference> FilteredBoneReferences(IEnumerable<Transform> includes = null, IEnumerable<Transform> excludes = null)
        {
            var includesSet = includes == null ? null : new HashSet<Transform>(includes);
            var excludesSet = excludes == null ? null : new HashSet<Transform>(excludes);
            IEnumerable<BoneReference> boneReferences = BoneReferences;
            if (includesSet != null) boneReferences = boneReferences.Where(r => includesSet.Overlaps(r.References));
            if (excludesSet != null) boneReferences = boneReferences.Where(r => !excludesSet.Overlaps(r.References));
            return boneReferences;
        }
    }
}
