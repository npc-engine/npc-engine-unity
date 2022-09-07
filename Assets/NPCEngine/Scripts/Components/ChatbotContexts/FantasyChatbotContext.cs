using System;
using System.Collections.Generic;

namespace NPCEngine.Components
{
    /// <summary>
    /// Text generation context for LIGHT dataset format (2 personas and location)
    /// </summary>
    [Serializable()]
    public class FantasyChatbotContext
    {
        // Default context from config.yml
        // "location_name": "",
        // "location": "",
        // "name": "",
        // "persona": "",
        // "other_name": "",
        // "other_persona": "",
        // "history": [
        //   {"speaker": "", "line": ""}, 
        //   {"speaker": "", "line": ""}
        // ]
        public string location_name;
        public string location;
        public string name;
        public string persona;
        public string other_name;
        public string other_persona;

        public List<ChatLine> history;

    }

    [Serializable()]
    public class ChatLine
    {
        public string speaker;
        public string line;
    }

  
}