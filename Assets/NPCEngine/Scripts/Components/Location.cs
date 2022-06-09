using UnityEngine;

namespace NPCEngine.Components
{
    [CreateAssetMenu(fileName = "Location", menuName = "NPCEngine/Location")]
    public class Location: ScriptableObject
    {
        public string Name;
        
        [TextArea(3, 10)]
        public string Description;        
    }
}