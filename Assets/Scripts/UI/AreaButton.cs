using Buildings.BuildingArea;
using PlayerInput;
using UnityEngine;
using UnityEngine.UI;

public class AreaButton : MonoBehaviour
{
    private BuildingAreaInputReceiver _areaInput;
    [SerializeField] private AreaType areaType;
    [SerializeField] private Image image;
    [SerializeField] private Color activatedColor;
    [SerializeField] private Color disabledColor;
    
    private void Start()
    {
        _areaInput = PlayerController.Instance.GetComponent<BuildingAreaInputReceiver>();
        _areaInput.RegisterForAreaTypeChanged(OnAreaTypeChanged);
    }

    private void OnEnable()
    {
        if (_areaInput != null) _areaInput.RegisterForAreaTypeChanged(OnAreaTypeChanged);
    }

    private void OnDisable()
    {
        _areaInput.UnregisterForAreaTypeChanged(OnAreaTypeChanged);
    }

    public void SetAreaType()
    {
        //Reset to default if already Selected
        _areaInput.AreaType = areaType;
    }

    private void OnAreaTypeChanged(AreaType type)
    {
        if (areaType == type)
        {
            image.color = activatedColor;
        }
        else
        {
            image.color = disabledColor;
        }
    }
}