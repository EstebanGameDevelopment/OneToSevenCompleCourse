using System;
using UnityEngine;

#if UNITY_EDITOR
namespace Wave.Essence.Interaction.Mode.Editor
{
	[Serializable]
	public class InteractionModeManagerAsset : ScriptableObject
	{
		public bool autoAddManager = true;
		public bool addedManager = false;
	}
}
#endif
