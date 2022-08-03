using System.Collections;
using System.Linq;
using NPCEngine.Components;
using NPCEngine.Utility;
using UnityEngine;
using UnityEngine.UI;

public class ButtonDisableEnable : MonoBehaviour
{
    private Button _button;
    public string serviceName;
    void Start()
    {
        _button = gameObject.GetComponent<Button>();
        _button.interactable = false;
        
        CoroutineUtility.StartCoroutine(EnableButtonWhenServiceRunning(), 
            this, "EnableButtonWhenServiceRunning");
    }

    public IEnumerator EnableButtonWhenServiceRunning()
    {
        Debug.Log("Entering EnableButtonWhenServiceRunning");
        while (true)
        {
            
            var services = NPCEngineManager.Instance.Services;
            if (services != null && services.Count(serviceMetadata => serviceMetadata.service == serviceName) > 0)
            {
                _button.interactable = true;
                yield return null;
            }
            else
            {
                yield return CoroutineUtility.WaitForSeconds(1f);
            }
        }
    }
}
