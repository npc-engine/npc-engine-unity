using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPCEngine.Components
{
    /// <summary>
    /// Simple collider trigger that sets location in PlayerCharacter
    /// </summary>
    public class ColliderLocationTrigger : MonoBehaviour
    {

        public Location location;

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
