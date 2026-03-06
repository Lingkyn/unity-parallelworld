using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// SpriteShadow 阴影投射的可配置参数
    /// </summary>
    [CreateAssetMenu(fileName = "SpriteShadowConfig", menuName = "ParallelWorld/Sprite Shadow Config")]
    public class SpriteShadowConfig : ScriptableObject
    {
        [Header("阴影效果")]
        [Tooltip("阴影投射到墙面后的扩展距离")]
        public float shadowExpand = 0.8f;

        [Tooltip("阴影厚度（深度方向）")]
        public float shadowThickness = 0.8f;

        [Header("性能")]
        [Tooltip("网格更新间隔（每隔 N 帧更新一次）")]
        public int meshUpdateInterval = 2;

        [Tooltip("碰撞体更新间隔（每隔 N 帧更新一次）")]
        public int colliderUpdateInterval = 4;
    }
}
