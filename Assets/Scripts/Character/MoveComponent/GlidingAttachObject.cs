using UnityEngine;

namespace REIW
{
    public class GlidingAttachObject : CacheMonoBehaviour
    {
//        private Material _material;
        public eKnownEffect Trail_Jump = eKnownEffect.FX_M_Gliding_Trail_Jump_05;
        public eKnownEffect Mount_Summon = eKnownEffect.FX_M_Gliding_MountSummon_05;
        public eKnownEffect Trail_Loop = eKnownEffect.FX_M_Gliding_Trail_05;

        public eKnownSfxSound Trail_Loop_Sound = eKnownSfxSound.SE_Glide_Common;
        
        public Transform Fx_Trail_JumpTransform = null; // 소환, jump
        public Transform Fx_Mount_SummonTransform = null; // 소환, jump
        
        public Transform Fx_Trail_1 = null;  // 트래일1
        public Transform Fx_Trail_2 = null;  // 트래일2
        
        public System.Action DetachAction;
        public System.Action AttachAction;

        private float _time = float.MinValue;
        
        private void Awake()
        {
//            _material = GetComponentInChildren<SkinnedMeshRenderer>().material;
        }

        private void Start()
        {
            AttachAction.Invoke();
        }

        public void DetachObject()
        {
            DetachAction?.Invoke();
            MyTransform.SetParent(null);
            _time = 0.5f;
        }

        private void LateUpdate()
        {
            if (_time < 0)
                return;
            
            _time -= Time.deltaTime;

            if (_time <= 0)
                OnFinish();
        }

        public void OnFinish()
        {
            if (MyGameObject == null)
                return;

            Destroy(MyGameObject);
        }
    }
}
