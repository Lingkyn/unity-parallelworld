using UnityEngine;

namespace ParallelWorld
{
    /// <summary>
    /// 相机震动：受击、冲刺等事件触发，提供偏移量供 CameraController 叠加
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        [Header("默认参数")]
        [SerializeField, Tooltip("默认震动强度")]
        private float defaultIntensity = 0.3f;

        [SerializeField, Tooltip("默认震动时长（秒）")]
        private float defaultDuration = 0.15f;

        private float _shakeIntensity;
        private float _shakeDuration;
        private float _shakeDurationTotal; // 震动开始时的总时长，用于衰减计算
        private float _seed;

        private void Awake()
        {
            _seed = Random.Range(0f, 1000f);
        }

        private void Update()
        {
            if (_shakeDuration > 0f)
            {
                _shakeDuration -= Time.deltaTime;
                if (_shakeDuration < 0f) _shakeDuration = 0f;
            }
        }

        /// <summary>
        /// 向场景中所有 CameraShake 触发震动（可从任意处调用，如受击时）
        /// </summary>
        public static void ShakeAll(float intensity = 0.3f, float duration = 0.15f)
        {
            foreach (var shake in FindObjectsByType<CameraShake>(FindObjectsSortMode.None))
                shake.Shake(intensity, duration);
        }

        /// <summary>
        /// 触发震动
        /// </summary>
        public void Shake(float intensity = -1f, float duration = -1f)
        {
            _shakeIntensity = intensity >= 0f ? intensity : defaultIntensity;
            _shakeDurationTotal = duration >= 0f ? duration : defaultDuration;
            _shakeDuration = _shakeDurationTotal;
        }

        /// <summary>
        /// 获取当前帧的震动偏移（供 CameraController 叠加）
        /// </summary>
        public Vector3 GetShakeOffset()
        {
            if (_shakeDuration <= 0f || _shakeDurationTotal <= 0f) return Vector3.zero;

            float t = 1f - (_shakeDuration / _shakeDurationTotal);
            float factor = _shakeIntensity * (1f - t * t); // 衰减曲线

            float x = (Mathf.PerlinNoise(_seed, Time.time * 50f) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(_seed + 1f, Time.time * 50f) - 0.5f) * 2f;

            return new Vector3(x * factor, y * factor, 0f);
        }
    }
}
