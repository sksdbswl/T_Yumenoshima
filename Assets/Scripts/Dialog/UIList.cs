//===========================================================
// # ReadMe
//  - 각 UI들의 enum 형 이름은 prefab 이름과 같아야 한다.
//  - "enum Type name" and "UI Prefab name" are same context.
//  - Popup 과 Panel을 구분하는 방법 예시
//   > Game 에서 ESC 키를 눌러서 UI가 닫히는 경우에는 Popup으로,
//   > 그렇지 않은 경우에는 Panel로 구분한다.
//===========================================================

public enum UIList
{
    POPUP_START,
        
    BuilderUI,
        
    POPUP_MAX, // Popup Max
    PANEL_START, // Panel Start
        
    TestUI,
    
    PANEL_MAX,
}