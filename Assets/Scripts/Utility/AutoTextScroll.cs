using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace REIW
{
    public class AutoTextScroll : MonoBehaviour
    {
        [Header("Refs")]
        public TextMeshProUGUI text;
        public RectTransform ViewportRect;
        
        private float scrollSpeed = 10f; // 이동 속도
        private float tolerance = 0.1f; //엣지 판정 오차(월드좌표 기반)

        private RectTransform TextRect;
        private Vector2 startPos;
        private bool shouldScroll;

        private void Awake()
        {
            TextRect = text.rectTransform;

            // 스크롤용 텍스트는 한 줄, 고정 크기
            text.enableWordWrapping = false;
            text.enableAutoSizing   = false;

            Canvas.ForceUpdateCanvases();
            text.ForceMeshUpdate(true);

            UpdateWidth();
            UpdateShouldScroll();
            CacheStartPos();
        }

        private void OnEnable()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(TextRect);

            UpdateWidth();
            UpdateShouldScroll();
            CacheStartPos();

            //LocalizationManager.Singleton.OnLocaleChanged += Refresh;
        }

        private void OnDisable()
        {
            //LocalizationManager.Singleton.OnLocaleChanged -= Refresh;
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateWidth();
            UpdateShouldScroll();
            CacheStartPos();
        }

        private void UpdateWidth()
        {
            if (!text) return;

            // Canvas 최신 값으로 동기화
            Canvas.ForceUpdateCanvases();
            // 텍스트 내용이나 폰트가 바뀌었을 때 사이즈 다시 체크
            text.ForceMeshUpdate(true);

            // GetPreferredValues()를 텍스트가 완전히 표시되기 위해 필요한 크기를 계산
            // Mathf.Infinity: 가로폭 제한이 없다고 가정(한 줄로 전부 표시)
            // ViewportRect.rect.height: 세로는 현재 뷰포트 높이를 기준으로 계산
            // 결과로 pref.x=필요한 가로 폭, pref.y =필요한 세로 높이
            var pref = text.GetPreferredValues(Mathf.Infinity, ViewportRect.rect.height);
            
            // 현재 텍스트에 적용되어야할 가로 폭
            float preferredWidth = pref.x;

            // Text의 RectTransform의 가로 크기를 preferredWidth로 변경
            // SetSizeWithCurrentAnchors() 앵커 설정(왼쪽 고정 등)을 유지하면서 width만 변경
            TextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
        }

        /// <summary>
        /// 텍스트 시작위치 캐싱
        /// </summary>
        private void CacheStartPos()
        {
            // 전체 폭의 5% 앞에서 시작
            float offset = ViewportRect.rect.width * 0.05f; 
            if (shouldScroll)
                startPos = new Vector2(offset, TextRect.anchoredPosition.y);
            else
                startPos = new Vector2(0f, TextRect.anchoredPosition.y);

            TextRect.anchoredPosition = startPos;
        }

        /// <summary>
        /// 스크롤 사용여부 확인
        /// </summary>
        private float onEps  = 4f;
        private float offEps = 4f;

        private void UpdateShouldScroll()
        {
            //GetPreferredValues() : 주어진 폭/높이 제한 안에서 텍스트를 완전히 표시하는 데 필요한 크기 계산
            float viewportW = ViewportRect.rect.width;
            float preferredW = text.GetPreferredValues(Mathf.Infinity, ViewportRect.rect.height).x;
            
            shouldScroll = (viewportW + onEps) < preferredW;
        }

        private void Update()
        {
            var pos = TextRect.anchoredPosition;
            
            // 스크롤이 필요 없는 경우
            if (!shouldScroll)
            {
                 var anchorMax = TextRect.anchorMax;
                 anchorMax.x = 1f;
                
                 TextRect.anchorMax = anchorMax;
                 TextRect.pivot = new Vector2(0.5f, TextRect.pivot.y);
                
                return;
            }

            pos.x -= scrollSpeed * Time.deltaTime;
            TextRect.anchoredPosition = pos;
            
            if (IsRightEdgeTouching())
            {
                StartCoroutine(WaitAndReset());
            }
        }

        IEnumerator WaitAndReset()
        {
            yield return new WaitForSeconds(2f);

            float elapsed = 0f;
            float duration = 1f; // 1초 동안 이동
            Vector2 start = TextRect.anchoredPosition;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                TextRect.anchoredPosition = Vector2.Lerp(start, startPos, t);
                yield return null; // 다음 프레임까지 대기
            }
            
            TextRect.anchoredPosition = startPos; // 정확히 도착점 고정
        }
        
        bool IsRightEdgeTouching()
        {
            Vector3[] textCorners = new Vector3[4];
            Vector3[] viewportCorners = new Vector3[4];

            //왼쪽 아래, 왼쪽 위, 오른쪽 위, 오른쪽 아래 배열 저장
            TextRect.GetWorldCorners(textCorners);
            ViewportRect.GetWorldCorners(viewportCorners);

            // 오른쪽 위 가장자리
            float textRightX = textCorners[2].x;
            float viewportRightX = viewportCorners[2].x;

            //두 오른쪽 경계의 차이가 tolerance 이하(즉, 거의 같은 위치)면 true
            //혹은 텍스트의 오른쪽이 뷰포트보다 안쪽에 있을 때 (textRightX < viewportRightX) true
            //나머지 false
            return Mathf.Abs(textRightX - viewportRightX) <= tolerance
                   || textRightX < viewportRightX;
        }

        public void Refresh()
        {
            startPos.x = 0f;
            TextRect = text.rectTransform;
            
            // 스크롤용 텍스트는 한 줄, 고정 크기
            text.enableWordWrapping = false;
            text.enableAutoSizing   = false;

            // 앵커/피벗 초기화 
            var aMin = TextRect.anchorMin; 
            var aMax = TextRect.anchorMax; 
            aMin.x = 0f;
            aMax.x = 0f;
            TextRect.anchorMin = aMin;
            TextRect.anchorMax = aMax;
            TextRect.pivot = new Vector2(0f, TextRect.pivot.y);

            Canvas.ForceUpdateCanvases();
            text.ForceMeshUpdate(true);

            UpdateWidth();
            UpdateShouldScroll();
            CacheStartPos();
        }
    }
}
