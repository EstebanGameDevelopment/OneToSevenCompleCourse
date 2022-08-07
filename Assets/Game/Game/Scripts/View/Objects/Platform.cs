using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YourVRExperience.Game
{
    public class Platform : MonoBehaviour
    {
        private List<Player> m_players = new List<Player>();

        void Start()
        {
            this.gameObject.tag = GameController.TAG_FLOOR;
        }

        void OnCollisionEnter(Collision _collision)
        {
            Player player = _collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                if (!m_players.Contains(player))
                {
                    player.transform.parent = this.transform;
                    player.UseRigidBody = false;
                    m_players.Add(player);
                }
            }
        }

        void OnCollisionExit(Collision _collision)
        {
            Player player = _collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                if (m_players.Remove(player))
                {
                    player.transform.parent = null;
                    player.UseRigidBody = true;
                }
            }
        }
    }
}