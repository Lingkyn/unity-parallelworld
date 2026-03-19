using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// SpriteShadow 阴影投射的可配置参数
    /// </summary>
    [CreateAssetMenu(fileName = "SpriteShadowConfig", menuName = "ParallelWorld/Sprite Shadow Config")]
    public class SpriteShadowConfig : ScriptableObject
    {
        [Header("自动化")]
        [Tooltip("自动扫描子节点中的 SpotLight 作为光源（当未手动配置 lightEntries 时生效）")]
        public bool autoCollectSpotLightsInChildren = true;

        [Tooltip("自动创建缺失的 Shadow 节点")]
        public bool autoCreateShadowObject = true;

        [Tooltip("自动创建的 Shadow 节点名称前缀")]
        public string shadowObjectPrefix = "Shadow_";

        [Tooltip("自动创建 Shadow 节点的父级，空则挂到当前 Sprite 物体下")]
        public string shadowRootName = "ShadowRoot";

        [Header("阴影效果")]
        [Tooltip("阴影投射命中的墙体层")]
        public LayerMask wallMask;

        [Tooltip("阴影投射到墙面后的扩展距离")]
        public float shadowExpand = 0.8f;

        [Tooltip("阴影厚度（深度方向）")]
        public float shadowThickness = 0.8f;

        [Tooltip("投射射线的最大距离")]
        public float projectionRayDistance = 200f;

        [Header("碰撞")]
        [Tooltip("自动生成的阴影碰撞体是否为触发器")]
        public bool shadowColliderIsTrigger = false;

        [Header("渲染")]
        [Tooltip("若指定则用于阴影 MeshRenderer，否则回退到 Sprites/Default")]
        public Material shadowMaterial;

        [Tooltip("当未指定 shadowMaterial 时，使用该颜色")]
        public Color shadowColor = new Color(0f, 0f, 0f, 0.5f);

        [Header("性能")]
        [Tooltip("网格更新间隔（每隔 N 帧更新一次）")]
        public int meshUpdateInterval = 2;

        [Tooltip("碰撞体更新间隔（每隔 N 帧更新一次）")]
        public int colliderUpdateInterval = 4;
    }
}
