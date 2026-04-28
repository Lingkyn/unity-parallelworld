using UnityEngine;

public class moveattack : MonoBehaviour
{
    public enum MoveAxis
    {
        X = 0,
        Y = 1
    }

    [Header("Movement")]
    [SerializeField, Tooltip("运动轴")]
    private MoveAxis axis = MoveAxis.Y;
    [SerializeField, Tooltip("是否使用本地坐标")]
    private bool useLocalPosition = false;
    [SerializeField, Tooltip("移动速度")]
    private float speed = 1f;
    [SerializeField, Tooltip("最小值")]
    private float min = -1f;
    [SerializeField, Tooltip("最大值")]
    private float max = 1f;

    private float _baseValue;
    private bool _hasBase;

    // Update is called once per frame
    void Update()
    {
        if (!_hasBase)
        {
            _baseValue = useLocalPosition
                ? (axis == MoveAxis.X ? transform.localPosition.x : transform.localPosition.y)
                : (axis == MoveAxis.X ? transform.position.x : transform.position.y);
            _hasBase = true;
        }

        float range = Mathf.Max(Mathf.Abs(min), Mathf.Abs(max));
        float value = Mathf.PingPong(Time.time * speed, range * 2f) - range;
        float target = _baseValue + value;

        if (useLocalPosition)
        {
            Vector3 pos = transform.localPosition;
            if (axis == MoveAxis.X)
                pos.x = target;
            else
                pos.y = target;
            transform.localPosition = pos;
        }
        else
        {
            Vector3 pos = transform.position;
            if (axis == MoveAxis.X)
                pos.x = target;
            else
                pos.y = target;
            transform.position = pos;
        }
    }
}
