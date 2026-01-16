using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间

public class CatBodyController : MonoBehaviour
{
    [Header("设置")]
    [Tooltip("把猫身上带有SkinnedMeshRenderer的物体拖到这里")]
    public SkinnedMeshRenderer catMeshRenderer;

    [Tooltip("BlendShape的名字，必须和Blender里起的一模一样")]
    public string shapeKeyName = "胖";

    // 内部记录BlendShape的索引号，比用字符串查找更快
    private int blendShapeIndex;

    void Start()
    {
        if (catMeshRenderer == null)
        {
            Debug.LogError("请在Inspector面板赋值 SkinnedMeshRenderer！");
            return;
        }

        // 1. 获取BlendShape的索引 (Index)
        // Unity是通过索引来控制变形的，而不是名字。名字只是为了方便我们查找。
        blendShapeIndex = catMeshRenderer.sharedMesh.GetBlendShapeIndex(shapeKeyName);

        if (blendShapeIndex == -1)
        {
            Debug.LogError($"在模型上找不到名为 '{shapeKeyName}' 的 BlendShape，请检查拼写！");
        }
    }

    /// <summary>
    /// 提供给 UI Slider 调用的公共方法
    /// </summary>
    /// <param name="value">Slider传来的值 (建议 Slider 范围设为 0 到 100)</param>
    public void OnFatSliderChanged(float value)
    {
        if (blendShapeIndex != -1)
        {
            // SetBlendShapeWeight 接受两个参数：索引 和 权重(0-100)
            catMeshRenderer.SetBlendShapeWeight(blendShapeIndex, value);
        }
    }
}