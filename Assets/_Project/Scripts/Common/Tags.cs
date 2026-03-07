/// <summary>
/// Tag 标签常量
/// 需在 Project Settings -> Tags and Layers 中创建对应 Tag
/// </summary>
public static class Tags
{
    /// <summary>
    /// 靠近显示文本型
    /// </summary>
    public const string InteractableText = "InteractableText";

    /// <summary>
    /// 靠近显示按钮型，点击播放动画
    /// </summary>
    public const string InteractableButton = "InteractableButton";

    /// <summary>
    /// 复活点（Unity 自带 Tag）
    /// </summary>
    public const string Respawn = "Respawn";

    /// <summary>
    /// 玩家（Unity 自带 Tag）
    /// </summary>
    public const string Player = "Player";
}
