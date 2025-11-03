using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace REIW
{
    [CreateAssetMenu(menuName = "ScriptableObject/CharacterCustomizingRangeConfigSO")]
    public class CharacterCustomizingRangeConfigSO : ScriptableObject
    {
        public CustomizingRangeConfig CustomizingRange;
    }
    
    
    [System.Serializable]
    public class CustomizingRangeConfig
    {
        [Space(15)]
        [Header("================ Face ================")]
        public Range SkinGloss               = new() { Min = 0f,      Max = 1.5f  };
        
        public Range FaceHorizontalScale     = new() { Min = -0.05f,  Max = 0.05f };
        public Range FaceVerticalScale       = new() { Min = -0.05f,  Max = 0.05f };
        
        public Range BrowHorizontalScale     = new() { Min = -0.05f,  Max = 0.05f };
        public Range BrowVerticalScale       = new() { Min = -0.05f,  Max = 0.05f };
        public Range BrowAngle               = new() { Min = -5f,     Max = 5f    };
        
        public Range EyeHorizontalScale      = new() { Min = -0.05f,  Max = 0.05f };
        public Range EyeVerticalScale        = new() { Min = -0.05f,  Max = 0.05f };
        public Range EyeHorizontalOffset     = new() { Min = -0.01f,  Max = 0.01f };
        public Range EyeVerticalOffset       = new() { Min = -0.01f,  Max = 0.01f };
        public Range EyelidOutOffset         = new() { Min = -0.01f,  Max = 0.01f };
        
        public Range IrisColorBlend          = new() { Min = 0f,      Max = 1f    };
        
        public Range LensHorizontalOffset    = new() { Min = -0.5f,   Max = 0.5f  };
        public Range LensVerticalOffset      = new() { Min = -0.5f,   Max = 0.5f  };
        public Range LensAlpha               = new() { Min = 0f,      Max = 1f    };
        
        public Range LipHorizontalScale      = new() { Min = -0.05f,  Max = 0.05f };
        public Range LipVerticalScale        = new() { Min = -0.05f,  Max = 0.05f };
        public Range LipHorizontalOffset     = new() { Min = -0.02f,  Max = 0.02f };
        public Range LipVerticalOffset       = new() { Min = -0.02f,  Max = 0.02f };
        
        public Range StickerHorizontalOffset = new() { Min = -0.2f,   Max = 0.2f  };
        public Range StickerVerticalOffset   = new() { Min = -0.2f,   Max = 0.2f  };
        public Range StickerSize             = new() { Min = 0.05f,   Max = 0.5f  };

        [Space(15)]
        [Header("================ Body ================")]
        public Range HeightScale             = new() { Min = -0.05f,  Max = 0.05f };
        
        public Range ShoulderOffset          = new() { Min = -0.02f,  Max = 0.02f };
        
        public Range NeckHorizontalScale     = new() { Min = -0.15f,  Max = 0.15f };
        public Range NeckVerticalOffset      = new() { Min = -0.015f, Max = 0.015f};
        
        public Range ChestScale              = new() { Min = -0.1f,   Max = 0.1f  };
        
        public Range WaistScale              = new() { Min = -0.1f,   Max = 0.1f  };
        
        public Range ArmHorizontalScale      = new() { Min = -0.1f,   Max = 0.1f  };
        public Range ArmVerticalScale        = new() { Min = -0.05f,  Max = 0.05f };
        
        public Range LegHorizontalScale      = new() { Min = -0.1f,   Max = 0.1f  };
        public Range LegVerticalScale        = new() { Min = -0.05f,  Max = 0.05f };
    }
    
    [System.Serializable]
    public struct Range
    {
        public float Min;
        public float Max;
    }
}
