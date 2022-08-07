using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YourVRExperience.Utils;

namespace YourVRExperience.Game
{
	public class AreaVisionDetection : MonoBehaviour
	{
		public delegate void VisionDetectionEvent(GameObject _objectDetected);
		public delegate void VisionLostEvent(GameObject _objectDetected);

		public event VisionDetectionEvent DetectionEvent;
		public event VisionLostEvent LostEvent;

		public void DispatchVisionDetectionEvent(GameObject _objectDetected)
		{
			if (DetectionEvent != null)
				DetectionEvent(_objectDetected);
		}

		public void DispatchVisionLostEvent(GameObject _objectDetected)
		{
			if (LostEvent != null)
				LostEvent(_objectDetected);
		}

		public enum TargetCharacters { PLAYER = 0, ENEMY, NPC }

		public Vision Vision;

		private GameObject m_planeAreaVisionDetection;
		private int m_checkRadiusInstances = 10;

		private GameObject m_currentDetection = null;

		void Start()
		{
			CreateAreaVisionDetection();

			SystemEventController.Instance.DispatchSystemEvent(SystemEventGameController.EVENT_AREAVISIONDETECTED_HAS_STARTED, this);
		}

		void OnDestroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.DispatchSystemEvent(SystemEventGameController.EVENT_AREAVISIONDETECTED_HAS_BEEN_DESTROYED, this);
		}

		public void CreateAreaVisionDetection()
		{
			if (Vision.UseBehavior)
			{
				this.gameObject.transform.rotation = Quaternion.identity;
				if (m_planeAreaVisionDetection != null)
				{
					GameObject.Destroy(m_planeAreaVisionDetection);
				}
				m_planeAreaVisionDetection = GameObject.CreatePrimitive(PrimitiveType.Plane);
				m_planeAreaVisionDetection.GetComponent<MeshCollider>().enabled = false;
				m_planeAreaVisionDetection.AddComponent<PlaneFromPoly>();
				m_planeAreaVisionDetection.transform.parent = this.transform;

				Utilities.DrawAreaVision(this.gameObject.transform.position, Vision.Orientation, m_planeAreaVisionDetection, m_checkRadiusInstances, Vision.DetectionDistance, Vision.DetectionAngle, Vision.Material, Vision.HeightToFloor);
			}
		}

		public Vision CopyVision()
		{
			return Vision.Clone();
		}

		public void SetVision(Vision _vision)
		{
			Vision.Set(_vision);
		}

		public void StopAreaDetection()
		{
			if (m_planeAreaVisionDetection != null)
			{
				GameObject.Destroy(m_planeAreaVisionDetection);
			}
			Vision.UseBehavior = false;
		}

		public void ChangeDistanceArea(float _distanceAreaDetection)
		{
			Vision.DetectionDistance = _distanceAreaDetection;
			CreateAreaVisionDetection();
		}


		public void UpdateLogic()
		{
			if (Vision.UseBehavior == false) return;

			float angle = (Mathf.Atan2(this.transform.forward.x, -this.transform.forward.z) * Mathf.Rad2Deg) - Vision.Orientation;

			switch (Vision.Target)
			{
				case TargetCharacters.PLAYER:
					for (int i = 0; i < GameController.Instance.Players.Count; i++)
					{
						IPlayer player = GameController.Instance.Players[i];
						float heightDistance = Mathf.Abs(this.gameObject.transform.position.y - player.GetGameObject().transform.position.y);
						if (heightDistance < Vision.HeightDetection)
						{
							if (Utilities.IsInsideCone(this.gameObject, angle, player.GetGameObject(), Vision.DetectionDistance, Vision.DetectionAngle) > 0)
							{
								if (m_currentDetection == null)
								{
									Debug.Log("<color=red>PLAYER DETECTED!!!</color>");
									m_currentDetection = player.GetGameObject();
									DispatchVisionDetectionEvent(player.GetGameObject());
								}
							}
							else
							{
								if (m_currentDetection != null)
								{
									if (m_currentDetection == player.GetGameObject())
									{
										m_currentDetection = null;
										DispatchVisionLostEvent(player.GetGameObject());
									}
								}
							}
						}
					}
					break;

				case TargetCharacters.ENEMY:
					if (LevelController.Instance != null)
					{
						for (int i = 0; i < LevelController.Instance.Enemies.Count; i++)
						{
							float heightDistance = Mathf.Abs(this.gameObject.transform.position.y - LevelController.Instance.Enemies[i].gameObject.transform.position.y);
							if (heightDistance < Vision.HeightDetection)
							{
								if (Utilities.IsInsideCone(this.gameObject, angle, LevelController.Instance.Enemies[i].gameObject, Vision.DetectionDistance, Vision.DetectionAngle) > 0)
								{
									if (m_currentDetection == null)
									{
										Debug.Log("<color=red>ENEMY DETECTED!!!</color>");
										m_currentDetection = LevelController.Instance.Enemies[i].gameObject;
										DispatchVisionDetectionEvent(LevelController.Instance.Enemies[i].gameObject);
									}
								}
								else
								{
									if (m_currentDetection != null)
									{
										if (m_currentDetection == LevelController.Instance.Enemies[i])
										{
											m_currentDetection = null;
											DispatchVisionLostEvent(LevelController.Instance.Enemies[i].gameObject);
										}
									}
								}
							}
						}
					}
					break;

				case TargetCharacters.NPC:
					if (LevelController.Instance != null)
					{
						for (int i = 0; i < LevelController.Instance.NPCs.Count; i++)
						{
							float heightDistance = Mathf.Abs(this.gameObject.transform.position.y - LevelController.Instance.NPCs[i].gameObject.transform.position.y);
							if (heightDistance < Vision.HeightDetection)
							{
								if (Utilities.IsInsideCone(this.gameObject, angle, LevelController.Instance.NPCs[i].gameObject, Vision.DetectionDistance, Vision.DetectionAngle) > 0)
								{
									if (m_currentDetection == null)
									{
										Debug.Log("<color=red>NPC DETECTED!!!</color>");
										m_currentDetection = LevelController.Instance.NPCs[i].gameObject;
										DispatchVisionDetectionEvent(LevelController.Instance.NPCs[i].gameObject);
									}
								}
								else
								{
									if (m_currentDetection != null)
									{
										if (m_currentDetection == LevelController.Instance.NPCs[i])
										{
											m_currentDetection = null;
											DispatchVisionLostEvent(LevelController.Instance.NPCs[i].gameObject);
										}
									}
								}
							}
						}
					}
					break;
			}
		}
	}
}