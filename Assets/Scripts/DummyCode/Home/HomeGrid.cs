using UnityEngine;

namespace REIW.LoneGarden
{
    public class HomeGrid : MonoBehaviour
    {
        [Header("Grid (Plane 10x10 중심 기준)")]
        [SerializeField] int width = 10;     // 셀 개수 (X)
        [SerializeField] int height = 10;    // 셀 개수 (Z)
        [SerializeField] float cellSize = 1f;
        [SerializeField] Transform origin;   // 보통 Ground(Plane) Transform을 넣어주세요

        [Header("Optional")]
        [SerializeField] LayerMask groundMask;
        [SerializeField] Material allowedMat, blockedMat;

        private bool[,] _buildable;

        // 공개 프로퍼티
        public int Width  => width;
        public int Height => height;
        public float CellSize => cellSize;
        
        void Awake()
        {
            _buildable = new bool[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    _buildable[x, y] = true;
        }

        public bool IsInside(int x, int y)
            => x >= 0 && y >= 0 && x < width && y < height;

        public bool IsBuildableCell(int x, int y)
            => IsInside(x, y) && _buildable[x, y];

        // ───────────────────────────────────────────────────────────────────
        // Plane(10x10) "중앙 피벗" 기준으로 코너 원점 계산 후 사용
        // ───────────────────────────────────────────────────────────────────

        // 그리드 전체 크기
        private Vector2 TotalSize() => new Vector2(width * cellSize, height * cellSize);

        // Plane(또는 origin) 중앙에서 왼쪽-아래 코너로 이동한 "코너 원점"
        private Vector3 CornerOrigin()
        {
            var o = origin != null ? origin : transform; // fallback
            var total = TotalSize();
            return o.position
                 - o.right   * (total.x * 0.5f)
                 - o.forward * (total.y * 0.5f);
        }

        // 셀 (x,y)의 "센터" 월드 좌표
        public Vector3 GridToWorld(int x, int y, float yLevel = 0f)
        {
            var o = origin != null ? origin : transform;
            var corner = CornerOrigin();
            var center = corner
                       + o.right   * ((x + 0.5f) * cellSize)
                       + o.forward * ((y + 0.5f) * cellSize);
            center.y = yLevel != 0f ? yLevel : center.y;
            return center;
        }

        // 월드 좌표 → 그리드 인덱스 (가까운 셀)
        public bool WorldToGrid(Vector3 world, out int gx, out int gy)
        {
            var o = origin != null ? origin : transform;
            var corner = CornerOrigin();

            // origin의 평면 축 기준 좌표로 투영
            Vector3 to = world - corner;
            float lx = Vector3.Dot(to, o.right);
            float lz = Vector3.Dot(to, o.forward);

            gx = Mathf.FloorToInt(lx / cellSize);
            gy = Mathf.FloorToInt(lz / cellSize);
            return IsInside(gx, gy);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (width <= 0 || height <= 0) return;

            var o = origin != null ? origin : transform;

            // 회전/위치 적용해 그리기
            Matrix4x4 prev = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(o.position, o.rotation, Vector3.one);

            float halfW = (width  * cellSize) * 0.5f;
            float halfH = (height * cellSize) * 0.5f;
            Vector3 start = new Vector3(-halfW, 0.01f, -halfH); // 중앙 기준 시작점

            var cellBox = new Vector3(cellSize, 0.02f, cellSize);

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Vector3 center = start + new Vector3((x + 0.5f) * cellSize, 0f, (y + 0.5f) * cellSize);
                Gizmos.color = (_buildable != null && _buildable[x, y]) ? Color.green : Color.red;
                Gizmos.DrawWireCube(center, cellBox);
            }

            Gizmos.matrix = prev;
        }
#endif
    }
}
