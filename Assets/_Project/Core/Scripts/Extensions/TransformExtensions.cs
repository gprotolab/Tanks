using UnityEngine;

namespace ANut.Core.Utils
{
    public static class TransformExtensions
    {
        public static Transform FindChildByNameRecursive(this Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            if (parent.name == childName)
            {
                return parent;
            }

            foreach (Transform child in parent)
            {
                var found = child.FindChildByNameRecursive(childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}