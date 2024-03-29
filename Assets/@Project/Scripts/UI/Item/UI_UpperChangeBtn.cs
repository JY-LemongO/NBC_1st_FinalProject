using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_UpperChangeBtn : UI_Item, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] TextMeshProUGUI _partText;

    public int currentIndex;
    public static int IndexOfUpperPart = 0;

    private UpperPart _currentUpper;

    protected override void Init()
    {
        base.Init();

        currentIndex = IndexOfUpperPart;
        ++IndexOfUpperPart;

        Button button = GetComponent<Button>();
        button.onClick.AddListener(ChangePart);

        _currentUpper = Managers.Module.GetPartOfIndex<UpperPart>(currentIndex);
        _partText.text = _currentUpper.upperSO.display_Name;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_parentUI._sidePopup == null)
        {
            UI_PartsStatus info = Managers.UI.ShowPopupUI<UI_PartsStatus>();
            _parentUI.SetSidePopup(info);
            return;
        }

        _parentUI._sidePopup.gameObject.SetActive(true);
        UI_UpperSelector selector = _parentUI as UI_UpperSelector;
        selector.DisPlayNextPartSpecText(_currentUpper);
        Managers.Module.CallInfoChange(_currentUpper.upperSO.display_Description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _parentUI._sidePopup.gameObject.SetActive(false);
    }

    private void ChangePart()
    {
        Managers.Module.ChangeUpperPart(currentIndex);
        Managers.Module.CallUpperPartChange(_currentUpper);
    }
}
