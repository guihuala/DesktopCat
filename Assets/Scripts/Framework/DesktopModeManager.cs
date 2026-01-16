using UnityEngine;
using UnityEngine.EventSystems;

public class DesktopModeManager : MonoBehaviour
{
    [Header("设置")]
    [Tooltip("每秒检测几次鼠标位置，不用太高，节省性能")]
    public float checkInterval = 0.1f; 
    
    // 物理射线检测（检测3D猫猫）
    public LayerMask interactableLayer; 

    private float _timer;
    private bool _isCurrentlyClickable = true;

    void Start()
    {
        // 1. 限制帧率，防止桌宠占用显卡 100%
        Application.targetFrameRate = 60;
        
        // 2. 初始化窗口透明
        WindowUtils.SetupWindow();
    }

    void Update()
    {
        // 只有打包出来的 EXE 才跑这套逻辑，编辑器里跑没意义
#if !UNITY_EDITOR
        _timer += Time.deltaTime;
        if (_timer >= checkInterval)
        {
            _timer = 0;
            UpdateWindowClickState();
        }
#endif
    }

    /// <summary>
    /// 核心逻辑：智能判断当前是否需要让出鼠标控制权
    /// </summary>
    void UpdateWindowClickState()
    {
        bool shouldBeClickable = false;

        // --- 判定 1: 鼠标是否悬停在 UI 上? ---
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // 这里有个坑：如果你的全屏是一个透明的 Panel，这里也会判定为 True。
            // 解决方法：给那个透明 Panel 的 Image 组件加上 Raycast Padding 或者 alphaHitTestMinimumThreshold
            shouldBeClickable = true; 
        }

        // --- 判定 2: 鼠标是否悬停在 3D/2D 物体（猫猫）上? ---
        // 只有当 UI 没捕获时才检测物体
        if (!shouldBeClickable)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactableLayer))
            {
                shouldBeClickable = true;
            }
        }

        // --- 只有状态改变时才调用 Windows API，节省性能 ---
        if (_isCurrentlyClickable != shouldBeClickable)
        {
            _isCurrentlyClickable = shouldBeClickable;
            WindowUtils.SetClickable(_isCurrentlyClickable);
        }
    }
}