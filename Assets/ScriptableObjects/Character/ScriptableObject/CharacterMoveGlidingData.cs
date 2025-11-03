using UnityEngine;

namespace REIW
{
	[CreateAssetMenu(fileName = "CharacterMoveGlidingData", menuName = "ScriptableObject/CharacterMoveGlidingData")]
	public class CharacterMoveGlidingData : ScriptableObject
	{
		[Header("Condition -------------------------------------")]
		[SerializeField]
		private LayerMask _groundLayer;
		[SerializeField]
		private float _checkConditionHeight = 1f; // 최소 높이

		[Header("Movement -------------------------------------")]
		[SerializeField]
		private float _maxMoveSpeed = 10f;
		[SerializeField]
		private float _gravity = 1f;
		[SerializeField]
		private float _risePower = 30f;
		[SerializeField]
		private float _riseMax = 5f;

		[SerializeField, Range(0.03f, 1.0f)]
		public float PlaySmoothTime = 0.08f;

		[Header("Rotation -------------------------------------")]
		[SerializeField]
		private float _rotationSharpness = 5.0f;

		[Header("Velocity -------------------------------------")]
		// GlidingData 등에 추가(또는 이 스크립트에 SerializeField로)
		[SerializeField]
		private bool _input = false;
		[SerializeField, Range(0f, 1.3f)]
		private float _descentStartDelay = 1;
		[SerializeField]
		float _accel = 20f; // 가속 (m/s^2)
		[SerializeField]
		float _decel = 25f; // 감속 (m/s^2)
		[SerializeField]
		float _maxLateralAccel = 12f; // 최대 측가속 한계 a_lat_max (m/s^2) ← 클수록 더 잘 꺾임
		[SerializeField]
		float _lateralFriction = 6f; // 옆미끄럼 감쇠 강도 (1/s)
		[SerializeField]
		float _lateralBrake = 25f; // 옆 성분을 0으로 끌어당기는 제동 m/s^2
		[SerializeField]
		float _maxSlipAngleDeg = 8f; // 허용되는 최대 슬립 각
		[SerializeField]
		float _lowSpeedSnap = 6.0f; // 이 속도 이하면 방향 스냅
		[SerializeField, Range(0f, 1f)]
		float _autoForwardSpeedRatio = 0.1f; // MaxMoveSpeed의 몇 %로 전진할지(1=풀속도)

		[Header("Camera -------------------------------------")]
		[SerializeField]
		public Vector3 CameraEventOffset = new Vector3(0f, 1f, 0f);

		// [SerializeField]
		// public int GlideObjectID = 0;

		// condition
		public LayerMask GroundLayer => _groundLayer;
		public float CheckConditionHeight => _checkConditionHeight;
		// movement
		public float MaxMoveSpeed => _maxMoveSpeed;
		public float Gravity => _gravity;
		public float RisePower => _risePower;
		public float RiseMax => _riseMax;
		// rotation
		public float RotationSharpness => _rotationSharpness;
		// velocity
		public float DescentStartDelay => _descentStartDelay;
		public float Accel => _accel;
		public float Decel => _decel;
		public float MaxLateralAccel => _maxLateralAccel;
		public float LateralFriction => _lateralFriction;
		public float LateralBrake => _lateralBrake;
		public float MaxSlipAngleDeg => _maxSlipAngleDeg;
		public float LowSpeedSnap => _lowSpeedSnap;
		public float AutoForwardSpeedRatio => _autoForwardSpeedRatio;

		[Header("Blend / Air -------------------------------------")]
		[SerializeField, Tooltip("이 이상이면 공중으로 간주")]
		public float airborneGate = 0.20f;
		[Header("IK			 -------------------------------------")]
		public float IK_Sharpness = 10;
		public float IK_POWER  = 0.35f;
		public float footBackMax = 0.12f;
		public float footUpMax = 0.1f;
		public float footSideMax = 0.03f;
	}
}
