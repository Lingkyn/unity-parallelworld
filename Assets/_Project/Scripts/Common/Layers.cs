/// <summary>
/// Layer 层名称常量
/// </summary>
public static class Layers
{
    /// <summary>
    /// 受光圈影响的对象层 (User Layer 6)
    /// </summary>
    public const string ApertureAffected = "ApertureAffected";

    /// <summary>
    /// 可交互物层 (User Layer 9)，统一筛选文本型与按钮型
    /// </summary>
    public const string Interactable = "Interactable";

    /// <summary>
    /// 死亡区域层，玩家进入即触发死亡
    /// </summary>
    public const string DeathZone = "DeathZone";
}
