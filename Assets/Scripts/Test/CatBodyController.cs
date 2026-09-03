using System;
using UnityEngine;

public class CatBodyController : MonoBehaviour
{
    [Header("设置")]
    [Tooltip("把猫身上带有SkinnedMeshRenderer的物体拖到这里")]
    public SkinnedMeshRenderer catMeshRenderer;

    [Tooltip("BlendShape的名字，必须和Blender里起的一模一样")]
    public string shapeKeyName = "fat";

    // 内部记录BlendShape的索引号，比用字符串查找更快
    private int blendShapeIndex;

    private void Awake()
    {
        if (catMeshRenderer == null)
        {
            catMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        }

        if (catMeshRenderer == null || catMeshRenderer.sharedMesh == null)
        {
            Debug.LogError("猫咪模型上找不到 SkinnedMeshRenderer。", this);
            blendShapeIndex = -1;
            return;
        }

        // 1. 获取BlendShape的索引 (Index)
        // Unity是通过索引来控制变形的，而不是名字。名字只是为了方便我们查找。
        blendShapeIndex = catMeshRenderer.sharedMesh.GetBlendShapeIndex(shapeKeyName);

        if (blendShapeIndex < 0)
        {
            for (var i = 0; i < catMeshRenderer.sharedMesh.blendShapeCount; i++)
            {
                var candidate = catMeshRenderer.sharedMesh.GetBlendShapeName(i);
                if (candidate.IndexOf("fat", StringComparison.OrdinalIgnoreCase) < 0) continue;
                blendShapeIndex = i;
                shapeKeyName = candidate;
                break;
            }
        }

        if (blendShapeIndex == -1)
        {
            Debug.LogError($"在猫咪模型上找不到名为 '{shapeKeyName}' 的 BlendShape，请检查导出设置。", this);
        }
    }

    /// <summary>
    /// 提供给 UI Slider 调用的公共方法
    /// </summary>
    /// <param name="value">Slider传来的值 (建议 Slider 范围设为 0 到 100)</param>
    public void OnFatSliderChanged(float value)
    {
        if (blendShapeIndex >= 0 && catMeshRenderer != null)
        {
            // SetBlendShapeWeight 接受两个参数：索引 和 权重(0-100)
            catMeshRenderer.SetBlendShapeWeight(blendShapeIndex, value);
        }
    }
}
