using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class MinimapBaker : MonoBehaviour
{
    [Header("굽는 데 사용할 카메라")]
    public Camera minimapCamera;

    [Header("카메라가 그릴 RenderTexture")]
    public RenderTexture renderTexture;

    [Header("결과를 붙일 UI Image (2D 미니맵 배경)")]
    public Image minimapImage;

    // 필요하다면 파일로도 저장할지 여부
    public bool saveToPng = false;
    public string fileName = "minimap.png";

    private void Start()
    {
            Bake();
    }
    
    public void Bake()
    {
        if (minimapCamera == null || renderTexture == null || minimapImage == null)
        {
            Debug.LogWarning("[MinimapBaker] 세팅이 안 됨");
            return;
        }

        // 카메라가 RT로 렌더하게 설정
        minimapCamera.targetTexture = renderTexture;

        // 한 프레임 렌더
        minimapCamera.Render();

        // RT → Texture2D 복사
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();

        RenderTexture.active = currentRT;

        // Texture2D → Sprite
        Sprite minimapSprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );

        minimapImage.sprite = minimapSprite;

        // 이제부터는 2D 이미지만 쓸 거면 카메라 끄거나 비활성화해도 됨
        minimapCamera.gameObject.SetActive(false);

        // (선택) PNG로 저장 – 에디터/빌드 둘 다 동작
        if (saveToPng)
        {
            byte[] png = tex.EncodeToPNG();
            string path = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllBytes(path, png);
            Debug.Log("[MinimapBaker] saved png to " + path);
        }
    }
}