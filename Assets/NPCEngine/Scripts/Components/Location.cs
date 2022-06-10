using UnityEngine;

namespace NPCEngine.Components
{
    /// <summary>
    /// Location natural language description.
    /// </summary>
    [CreateAssetMenu(fileName = "Location", menuName = "NPCEngine/Location")]
    public class Location: ScriptableObject
    {
        public string Name;
        
        [TextArea(3, 10)]
        public string Description;        
    }
}