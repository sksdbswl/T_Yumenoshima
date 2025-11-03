using UnityEngine;

namespace REIW.LoneGarden
{
    public class BuildGrid : MonoBehaviour
    {
        [SerializeField] int width = 20;
        [SerializeField] int height = 20;
        [SerializeField] float cellSize = 1f;
        [SerializeField] LayerMask groundMask;
        [SerializeField] Material allowedMat, blockedMat;

        // true = 지을 수 있음
        private bool[,] _buildable;

        public float CellSize => cellSize;

        void Awake()
        {
            _buildable = new bool[width, height];

            // 예시: 전부 가능으로 시작 후 특정 영역 막기
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _buildable[x, y] = true;

            // 중앙 4칸 막기 같은 규칙을 추가해도 됨
        }

        public bool IsInside(int x, int y)
            => x >= 0 && y >= 0 && x < width && y < height;

        public bool IsBuildableCell(int x, int y)
            => IsInside(x, y) && _buildable[x, y];

        public Vector3 GridToWorld(int x, int y, float yLevel = 0f)
            => new Vector3(x * cellSize, yLevel, y * cellSize);

        public bool WorldToGrid(Vector3 world, out int gx, out int gy)
        {
            gx = Mathf.RoundToInt(world.x / cellSize);
            gy = Mathf.RoundToInt(world.z / cellSize);
            return IsInside(gx, gy);
        }

        // 시각화(선택): 기초 그리드 라인과 빌드 가능/불가 표시
        void OnDrawGizmosSelected()
        {
            if (_buildable == null) return;
            for (int x = 0; x < _buildable.GetLength(0); x++)
            for (int y = 0; y < _buildable.GetLength(1); y++)
            {
                Gizmos.color = _buildable[x, y] ? Color.green : Color.red;
                var pos = GridToWorld(x, y);
                Gizmos.DrawWireCube(pos + Vector3.up * 0.01f, new Vector3(cellSize, 0.02f, cellSize));
            }
        }
    }
}