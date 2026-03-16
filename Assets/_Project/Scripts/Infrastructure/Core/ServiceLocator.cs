using System;
using System.Collections.Generic;

/// <summary>
/// 静态服务定位器：用于注册与解析单例服务，避免场景内分散使用 FindObjectOfType。
/// 由各服务在 Awake 中 Register，需要方通过 Get 解析；单测或场景卸载时可调用 Clear。
/// </summary>
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public static void Register<T>(T instance)
    {
        if (instance == null) return;
        _services[typeof(T)] = instance;
    }

    public static T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var obj))
            return obj as T;
        return null;
    }

    public static void Clear()
    {
        _services.Clear();
    }
}
