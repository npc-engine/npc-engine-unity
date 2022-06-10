using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPCEngine.Components
{
    /// <summary>
    /// Simple collider trigger that sets location in PlayerCharacter
    /// 
    /// Requires Trigger collider.
    /// </summary>
    public class ColliderLocationTrigger : MonoBehaviour
    {

        /// <summary>
        /// Location to set on trigger enter.
        /// </summary>
        public Location location;

        /// <summary>
        /// Trigger
        /// </summary>
        public Collider locationCollider;

        // Check if player is in the collider
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                var character = PlayerCharacter.Instance;
                character.currentLocation = location;
            }
        }
    }

}
