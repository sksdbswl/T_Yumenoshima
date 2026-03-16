using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DecalSystem : MonoBehaviour
{
   [Header("데칼 설정")]
    public Material[] decalMaterials; 
    private int currentIndex = 0;

    [Header("프리뷰 오브젝트")]
    public DecalProjector previewProjector; 

    [Header("레이캐스트 설정")]
    public float distance = 100f;
    public LayerMask targetLayer; 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) SwitchDecal(0);
        if (Input.GetKeyDown(KeyCode.B)) SwitchDecal(1);
        if (Input.GetKeyDown(KeyCode.C)) SwitchDecal(2);

        ShowPreview();
    }

    void SwitchDecal(int index)
    {
        if (index >= decalMaterials.Length) return;
        
        currentIndex = index;
        if (previewProjector != null)
        {
            previewProjector.material = decalMaterials[currentIndex];
        }
        Debug.Log($"현재 선택된 데칼: {index + 1}번");
    }

    void ShowPreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        // 화면 중앙 (FPS 기준)
        //Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distance, targetLayer))
        {
            previewProjector.gameObject.SetActive(true);

            // 위치 이동
            previewProjector.transform.position = hit.point;

            // 회전: 벽면(Normal)의 반대 방향을 바라보게 설정
            previewProjector.transform.rotation = Quaternion.LookRotation(-hit.normal);
            
            if (Input.GetMouseButtonDown(0))
            {
                PlaceDecal(hit.point, previewProjector.transform.rotation);
            }
        }
        else
        {
            previewProjector.gameObject.SetActive(false);
        }
    }

    void PlaceDecal(Vector3 pos, Quaternion rot)
    {
        // 수학적으로 완전히 동일한 위치의 높이일 경우 겹침 현상을 보안하기 위해서
        Vector3 spawnPos = pos + (rot * Vector3.forward * -0.01f);
        
        GameObject newDecal = Instantiate(previewProjector.gameObject, spawnPos, rot);
        newDecal.SetActive(true);
    }
}
