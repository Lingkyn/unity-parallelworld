using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CamSwitch : MonoBehaviour
{
    [Header("Cameras")]
    public GameObject cameraA;
    public GameObject cameraB;

    [Header("Behavior")]
    public bool startWithA = true;
    public bool switchOnEnter = true;
    public bool switchOnExit = false;
    public GameObject targetPlayer;
    public bool resetPrioritiesOnExit = false;

    private Component cameraAComponent;
    private Component cameraBComponent;
    private int initialPriorityA;
    private int initialPriorityB;
    private bool hasInitialPriorities;

    private void Start()
    {
        ResolveCameraComponents();
        LoadExistingPriorities();
        ApplyInitialState();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsTarget(other))
        {
            return;
        }

        Debug.Log($"CamSwitch: target entered trigger ({other.name})", this);

        if (switchOnEnter)
        {
            SetActiveCamera(!startWithA);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsTarget(other))
        {
            return;
        }

        if (switchOnExit)
        {
            SetActiveCamera(startWithA);
        }

        if (resetPrioritiesOnExit)
        {
            ResetPriorityValues();
        }
    }

    private bool IsTarget(Collider other)
    {
        if (targetPlayer == null)
        {
            return true;
        }

        return other.gameObject == targetPlayer || other.transform.root.gameObject == targetPlayer;
    }

    private void ApplyInitialState()
    {
        SetActiveCamera(startWithA);
    }

    [ContextMenu("Reset Priority Values")]
    public void ResetPriorityValues()
    {
        if (!hasInitialPriorities)
        {
            ResolveCameraComponents();
            LoadExistingPriorities();
        }

        if (!hasInitialPriorities)
        {
            return;
        }

        SetCameraPriority(cameraAComponent, initialPriorityA);
        SetCameraPriority(cameraBComponent, initialPriorityB);
    }


    private void SetActiveCamera(bool useA)
    {
        if (cameraAComponent == null || cameraBComponent == null)
        {
            return;
        }

        if (!hasInitialPriorities)
        {
            return;
        }

        SetCameraPriority(cameraAComponent, useA ? initialPriorityA : initialPriorityB);
        SetCameraPriority(cameraBComponent, useA ? initialPriorityB : initialPriorityA);

        var activeCamera = useA ? cameraA : cameraB;
        if (activeCamera != null)
        {
            Debug.Log($"CamSwitch: active camera is {activeCamera.name}", this);
        }
    }

    private void SetCameraPriority(Component cam, int targetValue)
    {
        if (cam == null)
        {
            return;
        }

        var camType = cam.GetType();
        var priorityProperty = camType.GetProperty("Priority");

        if (priorityProperty != null && priorityProperty.CanWrite)
        {
            if (priorityProperty.PropertyType == typeof(int))
            {
                priorityProperty.SetValue(cam, targetValue, null);
                return;
            }

            var valueProperty = priorityProperty.PropertyType.GetProperty("Value");
            if (valueProperty != null && valueProperty.CanWrite && valueProperty.PropertyType == typeof(int))
            {
                var priorityStruct = priorityProperty.GetValue(cam, null);
                valueProperty.SetValue(priorityStruct, targetValue, null);
                priorityProperty.SetValue(cam, priorityStruct, null);
                return;
            }
        }

        var priorityField = camType.GetField("Priority") ?? camType.GetField("m_Priority");
        if (priorityField != null && priorityField.FieldType == typeof(int))
        {
            priorityField.SetValue(cam, targetValue);
            return;
        }

        if (priorityField != null)
        {
            var valueProperty = priorityField.FieldType.GetProperty("Value");
            if (valueProperty != null && valueProperty.CanWrite && valueProperty.PropertyType == typeof(int))
            {
                var priorityStruct = priorityField.GetValue(cam);
                valueProperty.SetValue(priorityStruct, targetValue, null);
                priorityField.SetValue(cam, priorityStruct);
                return;
            }
        }

        Debug.LogWarning($"CamSwitch: cannot set priority on {camType.Name}.", cam);
    }

    private void ResolveCameraComponents()
    {
        cameraAComponent = ResolvePriorityComponent(cameraA);
        cameraBComponent = ResolvePriorityComponent(cameraB);

        if (cameraA != null && cameraAComponent == null)
        {
            Debug.LogWarning("CamSwitch: cameraA has no component with Priority.", cameraA);
        }

        if (cameraB != null && cameraBComponent == null)
        {
            Debug.LogWarning("CamSwitch: cameraB has no component with Priority.", cameraB);
        }
    }

    private Component ResolvePriorityComponent(GameObject obj)
    {
        if (obj == null)
        {
            return null;
        }

        var components = obj.GetComponents<Component>();
        foreach (var comp in components)
        {
            if (comp == null)
            {
                continue;
            }

            if (HasPriority(comp))
            {
                return comp;
            }
        }

        return null;
    }

    private bool HasPriority(Component cam)
    {
        var camType = cam.GetType();
        var priorityProperty = camType.GetProperty("Priority");
        if (priorityProperty != null)
        {
            if (priorityProperty.PropertyType == typeof(int))
            {
                return true;
            }

            var valueProperty = priorityProperty.PropertyType.GetProperty("Value");
            if (valueProperty != null && valueProperty.PropertyType == typeof(int))
            {
                return true;
            }
        }

        var priorityField = camType.GetField("Priority") ?? camType.GetField("m_Priority");
        if (priorityField != null)
        {
            if (priorityField.FieldType == typeof(int))
            {
                return true;
            }

            var valueProperty = priorityField.FieldType.GetProperty("Value");
            if (valueProperty != null && valueProperty.PropertyType == typeof(int))
            {
                return true;
            }
        }

        return false;
    }

    private void LoadExistingPriorities()
    {
        if (cameraAComponent == null || cameraBComponent == null)
        {
            return;
        }

        if (!TryGetPriority(cameraAComponent, out var priorityA) || !TryGetPriority(cameraBComponent, out var priorityB))
        {
            return;
        }

        initialPriorityA = priorityA;
        initialPriorityB = priorityB;
        hasInitialPriorities = true;

        if (initialPriorityA == initialPriorityB)
        {
            Debug.LogWarning("CamSwitch: both cameras have the same Priority.", this);
        }
    }

    private bool TryGetPriority(Component cam, out int value)
    {
        value = 0;
        if (cam == null)
        {
            return false;
        }

        var camType = cam.GetType();
        var priorityProperty = camType.GetProperty("Priority");
        if (priorityProperty != null)
        {
            if (priorityProperty.PropertyType == typeof(int))
            {
                value = (int)priorityProperty.GetValue(cam, null);
                return true;
            }

            var valueProperty = priorityProperty.PropertyType.GetProperty("Value");
            if (valueProperty != null && valueProperty.PropertyType == typeof(int))
            {
                var priorityStruct = priorityProperty.GetValue(cam, null);
                value = (int)valueProperty.GetValue(priorityStruct, null);
                return true;
            }
        }

        var priorityField = camType.GetField("Priority") ?? camType.GetField("m_Priority");
        if (priorityField != null && priorityField.FieldType == typeof(int))
        {
            value = (int)priorityField.GetValue(cam);
            return true;
        }

        if (priorityField != null)
        {
            var valueProperty = priorityField.FieldType.GetProperty("Value");
            if (valueProperty != null && valueProperty.PropertyType == typeof(int))
            {
                var priorityStruct = priorityField.GetValue(cam);
                value = (int)valueProperty.GetValue(priorityStruct, null);
                return true;
            }
        }

        return false;
    }
}
