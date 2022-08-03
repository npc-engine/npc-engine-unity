using System.Collections;
using System.Linq;
using NPCEngine.API;
using NPCEngine.Components;
using NPCEngine.Utility;
using UnityEngine;
using UnityEngine.UI;

public class ButtonDisableEnable : MonoBehaviour
{
    private Button button;
    public string serviceName;
    void Start()
    {
        button = gameObject.GetComponent<Button>();
        button.interactable = false;
        
        CoroutineUtility.StartCoroutine(EnableButtonWhenServiceRunning(), 
            this, "EnableButtonWhenServiceRunning");
    }

    public IEnumerator EnableButtonWhenServiceRunning()
    {
        while (true)
        {
            var manager = NPCEngineManager.Instance;
            var services = manager.Services;
            if (services != null && services.Any())
            {
                var statuses = manager.ServiceStatuses;
                for (int i = 0; i < statuses.Count; i++)
                {
                    if (services[i].service == serviceName && statuses[i] == ServiceStatus.RUNNING)
                    {
                        button.interactable = true;
                        yield return null;
                    }
                }
            }
            yield return CoroutineUtility.WaitForSeconds(1f);
        }
    }
}
