using System;

namespace DesktopPet.Save
{
    [Serializable]
    public class DesktopPetSaveData
    {
        public int saveVersion = 1;
        public WindowSettingsData window = new WindowSettingsData();
        public PetSettingsData pet = new PetSettingsData();
        public AudioSettingsData audio = new AudioSettingsData();
        public PrivacySettingsData privacy = new PrivacySettingsData();
    }

    [Serializable]
    public class WindowSettingsData
    {
        public bool alwaysOnTop = true;
        public bool borderless = true;
        public bool transparentBackground = true;
        public bool clickThrough;
        public bool allowDrag = true;
        public bool hasPosition;
        public int positionX;
        public int positionY;
    }

    [Serializable]
    public class PetSettingsData
    {
        public float scale = 0.55f;
    }

    [Serializable]
    public class AudioSettingsData
    {
        public float masterVolume = 1f;
        public float sfxVolume = 1f;
    }

    [Serializable]
    public class PrivacySettingsData
    {
        public bool microphoneEnabled;
    }
}
