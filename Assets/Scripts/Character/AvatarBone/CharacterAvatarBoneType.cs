namespace REIW
{
    public enum ReiwHumanBodyBones
    {
        None = 0,
        
        Root = 1,
        //===============================================================
        // REIW Avatar Socket Bones @Kim-jaejun
        //  * Equipment / Avatar 등 Attachment Socket 타입
        //===============================================================
        
        LeftHandWeapon,
        RightHandWeapon,
        
        //===============================================================
        // REIW Avatar Human Body Bones @Kim-jaejun
        //  * 본 구조 변경에 따른, 수정/대응을 최소화 하기 위해 일정 간극으로 띄워두었습니다.
        //===============================================================
        
        Hips = 100,                                     // Hips bone
            LeftUpperLeg,                               // Left Upper Leg bone
                LeftLowerLeg,                           // Left Knee bone
                    LeftFoot,                           // Left Ankle bone
                        LeftToes,                       // Left Toes bone
            RightUpperLeg,                              // Right Upper Leg bone
                RightLowerLeg,                          // Right Knee bone
                    RightFoot,                          // Right Ankle bone
                        RightToes,                      // Right Toes bone
                        
            Spine1 = 200,                               // first Spine bone
                Spine2,                 
                    Spine3,
                    SpineCm,
                        LeftBust,                       // Left Bust bone
                        RightBust,                      // Right Bust bone
                        Neck1,                          // Neck bone
                            Neck2,
                                NeckCm,
                                    Head = 250,         // Head bone
                                        LeftBrow,       // Left Brow bone
                                        RightBrow,      // Right Brow bone
                                        LeftEye,        // Left Eye bone
                                            LeftPupil,   // Left Pupil bone
                                            LeftEyeOut, // Left EyeOut bone
                                        RightEye,       // Right Eye bone
                                            RightPupil,  // Right Pupil bone
                                            RightEyeOut,// Right EyeOut bone
                                        Jaw,            // Jaw bone
                                        MouthCm,        // Mouth bone
                                
                        LeftShoulder = 300,             // Left Shoulder bone
                            LeftUpperArm,               // Left Upper Arm bone
                                LeftLowerArm,           // Left Elbow bone
                                    LeftHand,           // Left Wrist bone
                        RightShoulder,                  // Right Shoulder bone
                            RightUpperArm,              // Right Upper Arm bone
                                RightLowerArm,          // Right Elbow bone
                                    RightHand,          // Right Wrist bone
        
        LeftThumbProximal = 400,        // Left thumb 1st phalange : thumb
            LeftThumbIntermediate,      // Left thumb 2nd phalange : thumb
                LeftThumbDistal,        // Left thumb 3rd phalange : thumb
        LeftIndexProximal,              // Left index 1st phalange : index
            LeftIndexIntermediate,      // Left index 2nd phalange : index
                LeftIndexDistal,        // Left index 3rd phalange : index
        LeftMiddleProximal,             // Left middle 1st phalange : middle
            LeftMiddleIntermediate,     // Left middle 2nd phalange : middle
                LeftMiddleDistal,       // Left middle 3rd phalange : middle
        LeftRingProximal,               // Left ring 1st phalange : ring
            LeftRingIntermediate,       // Left ring 2nd phalange : ring
                LeftRingDistal,         // Left ring 3rd phalange : ring
        LeftLittleProximal,             // Left little 1st phalange : pinky
            LeftLittleIntermediate,     // Left little 2nd phalange : pinky
                LeftLittleDistal,       // Left little 3rd phalange : pinky
                
        RightThumbProximal = 500,       // Right thumb 1st phalange : thumb
            RightThumbIntermediate,     // Right thumb 2nd phalange : thumb
                RightThumbDistal,       // Right thumb 3rd phalange : thumb
        RightIndexProximal,             // Right index 1st phalange : index
            RightIndexIntermediate,     // Right index 2nd phalange : index
                RightIndexDistal,       // Right index 3rd phalange : index
        RightMiddleProximal,            // Right middle 1st phalange : middle
            RightMiddleIntermediate,    // Right middle 2nd phalange : middle
                RightMiddleDistal,      // Right middle 3rd phalange : middle
        RightRingProximal,              // Right ring 1st phalange : ring
            RightRingIntermediate,      // Right ring 2nd phalange : ring
                RightRingDistal,        // Right ring 3rd phalange : ring
        RightLittleProximal,            // Right little 1st phalange : pinky
            RightLittleIntermediate,    // Right little 2nd phalange : pinky
                RightLittleDistal,      // Right little 3rd phalange : pinky
        
        LastBone,                   // Last bone index delimiter
    }
}