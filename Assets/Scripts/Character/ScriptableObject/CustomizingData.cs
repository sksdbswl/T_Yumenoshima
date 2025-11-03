using System;
using System.Collections.Generic;
using UnityEngine;

namespace REIW
{
    [CreateAssetMenu(fileName = "CustomizingData", menuName = "ScriptableObject/CustomizingData")]
    public class CustomizingData : ScriptableObject
    {
        public List<CustomizingTable> items;
    }
    [Serializable]
    public class CustomizingTable
    {
        public string ViewName;
        public int Index;
        public EnumRace Race;
        public EnumGender Gender;
        public ePresetType PresetType;
        public int Sequence;
        public string IconName;
        public string DataName;
    }
    
    public readonly struct CustomizingTableKey : IEquatable<CustomizingTableKey>
    {
        public ePresetType PresetType { get; }
        public EnumRace Race { get; }
        public EnumGender Gender { get; }
        public CustomizingTableKey(ePresetType presetType, EnumRace race, EnumGender gender) => (PresetType, Race, Gender) = (presetType, race, gender);
        public bool Equals(CustomizingTableKey other) => Race == other.Race && Gender == other.Gender && PresetType == other.PresetType;
        public override bool Equals(object obj) => obj is CustomizingTableKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine((int)Race, (int)Gender, (int)PresetType);
    }
    
    public enum ePresetType
    {
        QuickPreset    = 1,
        
        // Hair_Start = 1000
        Hair           = 1000,
        Beard          = 1010,
        
        // Face_Start  = 2000
        Face           = 2000,
        Eye            = 2010,
        Iris           = 2020,
        Lens           = 2030,
        Lip            = 2040,
        
        // Maekup_Start= 2000
        CheekMakeup    = 3010,
        EyeMakeup      = 3020,
        LipMakeup      = 3030,
        SpotMakeup     = 3040,
        
        // Tattoo_Start= 4000
        TattooLeftArm  = 4010,
        TattooRightArm = 4020,
        TattooLeftLeg  = 4030,
        TattooRightLeg = 4040,
    }
}

