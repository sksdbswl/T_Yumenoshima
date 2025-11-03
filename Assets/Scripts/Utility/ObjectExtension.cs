using UnityEngine;

namespace REIW
{
	public static class ObjectExtensions
	{
		public static T GetorAddComponent<T>(this GameObject obj) where T : Component
		{
			return obj.TryGetComponent<T>(out var c) ? c : obj.AddComponent<T>();
		}
	}
}