using UnityEngine;

namespace REIW
{
    
    //TEST Code
    public class OptionManager : SingletonBase<OptionManager>
    {
        
        [SerializeField]
        public _eOptionMagica _magicaCloth = _eOptionMagica.High;
        
        [SerializeField]
        public float MasterVolume =1;
        
        [SerializeField]
        public float BGMVolume =0.8f;
        
        [SerializeField]
        public float SfxVolume =0.8f;

        // 실제 감도 계수는 Min, Max 사이를 LookSensitivity 값(0~1)로 보간한 값
        [SerializeField]
        public float LookSensitivity = .5f;
        [SerializeField]
        public float MinLookSensitivity = .5f;
        [SerializeField]
        public float MaxLookSensitivity = 1.5f;
        
        public void LoadOption()
        {
            if (PlayerPrefs.HasKey("MagicaCloth"))
            {
                _magicaCloth = (_eOptionMagica)PlayerPrefs.GetInt("MagicaCloth");
            }
            
            if (PlayerPrefs.HasKey("MasterVolume"))
            {
                MasterVolume = PlayerPrefs.GetFloat("MasterVolume");
                //SoundManager.Singleton.SetMasterVolume(MasterVolume);
            }
            
            if (PlayerPrefs.HasKey("BGMVolume"))
            {
                BGMVolume = PlayerPrefs.GetFloat("BGMVolume");
                //SoundManager.Singleton.SetBgmVolume(BGMVolume);
            }
            
            if (PlayerPrefs.HasKey("SfxVolume"))
            {
                SfxVolume = PlayerPrefs.GetFloat("SfxVolume");
               // SoundManager.Singleton.SetSfxVolume(SfxVolume);
            }
            
            // 회전 감도
            if (PlayerPrefs.HasKey("LookSensitivity"))
            {
                LookSensitivity = PlayerPrefs.GetFloat("LookSensitivity");
            }
            if (PlayerPrefs.HasKey("MinLookSensitivity"))
            {
                MinLookSensitivity = PlayerPrefs.GetFloat("MinLookSensitivity");
            }
            if (PlayerPrefs.HasKey("MaxLookSensitivity"))
            {
                MaxLookSensitivity = PlayerPrefs.GetFloat("MaxLookSensitivity");
            }
        }

        public _eOptionMagica GetOptionMagica()
        {
            return _magicaCloth;
        }

        // public void SetOptionMagica(_eOptionMagica eOptionList)
        // {
        //     if (_magicaCloth == _eOptionMagica.Off && eOptionList != _eOptionMagica.Off)
        //     {
        //         foreach (var cloth in GameObject.FindObjectsOfType<MagicaCloth>())
        //             cloth.enabled = true;    
        //     }
        //     
        //     _magicaCloth = (_eOptionMagica)eOptionList;
        //     PlayerPrefs.SetInt("MagicaCloth", (int)_magicaCloth);
        //
        //     ArtConfigSO.MagicaClothConfig config = REIW.Main.Singleton.ArtConfig.High;
        //     switch (_magicaCloth)
        //     {
        //         case _eOptionMagica.High:
        //             config = REIW.Main.Singleton.ArtConfig.High;
        //             break;
        //         case _eOptionMagica.Medium:
        //             config = REIW.Main.Singleton.ArtConfig.Medium;
        //             break;
        //         case _eOptionMagica.Low:
        //             config = REIW.Main.Singleton.ArtConfig.Low;
        //             break;
        //         case _eOptionMagica.Off:
        //             config = REIW.Main.Singleton.ArtConfig.Off;
        //             DisableAllMagicaCloth();
        //             break;
        //     }
        //     MagicaManager.SetSimulationFrequency(config.SimulationFrequency);      
        //     MagicaManager.SetMaxSimulationCountPerFrame(config.MaxSimulationCountPerFrame);
        // }
        
        // public void DisableAllMagicaCloth()
        // {
        //     foreach (var cloth in GameObject.FindObjectsOfType<MagicaCloth>())
        //         cloth.enabled = false;
        // }


        public void SetVolume(eOptionList option, float volume)
        {
            switch (option)
            {
                case eOptionList.MasterVolume:
                    MasterVolume = volume;
                    break;
                case eOptionList.SfxVolume:
                    SfxVolume = volume;
                    break;
                case eOptionList.BGMVolume:
                    BGMVolume = volume;
                    break;
            }
            PlayerPrefs.SetFloat(option.ToString(), volume);
            //SoundManager.Singleton.SetVolume(option, volume);
        }

        public float GetVolume(eOptionList option)
        {
            switch (option)
            {
                case eOptionList.MasterVolume:
                    return MasterVolume;
                    break;
                case eOptionList.SfxVolume:
                    return SfxVolume;
                    break;
                case eOptionList.BGMVolume:
                    return BGMVolume;
                    break;
            }
            return 0;
        }
        
        public void SetLookSensitivity(eOptionList option, float value)
        {
            switch (option)
            {
                case eOptionList.LookSensitivity:
                    LookSensitivity = value;
                    break;
                case eOptionList.MinLookSensitivity:
                    MinLookSensitivity = value;
                    break;
                case eOptionList.MaxLookSensitivity:
                    MaxLookSensitivity = value;
                    break;
            }
            PlayerPrefs.SetFloat(option.ToString(), value);
            InputController.Singleton.SetLookSensitivity(Mathf.Lerp(MinLookSensitivity, MaxLookSensitivity, LookSensitivity));
        }
        
        public float GetLookSensitivity(eOptionList option)
        {
            switch (option)
            {
                case eOptionList.LookSensitivity:
                    return LookSensitivity;
                case eOptionList.MinLookSensitivity:
                    return MinLookSensitivity;
                case eOptionList.MaxLookSensitivity:
                    return MaxLookSensitivity;
            }
            return 0;
        }
    }

    public enum eOptionList
    {
        MasterVolume,
        BGMVolume,
        SfxVolume,
        MagicaCloth,
        LookSensitivity,
        MinLookSensitivity,
        MaxLookSensitivity,
    }

    public enum _eOptionMagica
    {
        High = 1,
        Medium = 2,
        Low = 3,
        Off =4,
    }
}
