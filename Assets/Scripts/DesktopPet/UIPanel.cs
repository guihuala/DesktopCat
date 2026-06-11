using UnityEngine;

namespace DesktopPet
{
    public class UIPanel : MonoBehaviour
    {
        [SerializeField] private string panelId = "panel";
        [SerializeField] private bool closeOthersWhenOpened = true;
        [SerializeField] private bool blockClickThrough = true;
        [SerializeField] private KeyCode hotkey = KeyCode.None;

        public string PanelId => panelId;
        public bool CloseOthersWhenOpened => closeOthersWhenOpened;
        public bool BlockClickThrough => blockClickThrough;
        public KeyCode Hotkey => hotkey;
        public bool IsOpen => gameObject.activeSelf;

        public virtual void Open()
        {
            gameObject.SetActive(true);
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);
        }

        public void Toggle()
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }
    }
}
