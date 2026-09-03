using System;
using System.Collections.Generic;

namespace DesktopPet.Save
{
    [Serializable]
    public class DesktopPetSaveData
    {
        public int saveVersion = 3;
        public WindowSettingsData window = new WindowSettingsData();
        public PetSettingsData pet = new PetSettingsData();
        public AudioSettingsData audio = new AudioSettingsData();
        public PrivacySettingsData privacy = new PrivacySettingsData();
        public AppearanceSettingsData appearance = new AppearanceSettingsData();
        public OnlineRewardSaveData onlineReward = new OnlineRewardSaveData();
        public FurnitureInventorySaveData furnitureInventory = new FurnitureInventorySaveData();
        public FurniturePlacementSaveData furniturePlacement = new FurniturePlacementSaveData();
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
        public bool hasRuntimeStats;
        public float energy = 70f;
        public float hunger = 20f;
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

    [Serializable]
    public class AppearanceSettingsData
    {
        public int dayNightMode;
        public bool hasChosenPet;
        public int furStyle;
    }

    [Serializable]
    public class OnlineRewardSaveData
    {
        public double elapsedSeconds;
        public int pendingRewards;
    }

    [Serializable]
    public class FurnitureItemSaveData
    {
        public string furnitureId;
        public int totalOwned;
        public int placedCount;
    }

    [Serializable]
    public class FurnitureInventorySaveData
    {
        public List<FurnitureItemSaveData> items = new List<FurnitureItemSaveData>();
        public List<string> discoveredIds = new List<string>();
    }

    [Serializable]
    public class FurniturePlacementItemSaveData
    {
        public int anchorType;
        public string furnitureId;
    }

    [Serializable]
    public class FurniturePlacementSaveData
    {
        public List<FurniturePlacementItemSaveData> items = new List<FurniturePlacementItemSaveData>();
    }
}
