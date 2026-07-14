using System.Collections.Generic; // Sử dụng thư viện cho các cấu trúc dữ liệu như List, HashSet, Dictionary
using UnityEngine; // Sử dụng thư viện Unity để dùng Vector2, Vector2Int, Physics2D, Mathf
using PolarBond.Core; // Nạp không gian tên chứa các class cốt lõi của game
using PolarBond.Entities; // Nạp không gian tên chứa các thực thể (PlayerEntity, MagnetEntity, v.v.)
using PolarBond.Managers; // Nạp không gian tên chứa các trình quản lý (GridManager)

namespace PolarBond.Logic // Định nghĩa không gian tên chứa logic chính của trò chơi
{
    public class MagneticPhysicsEngine // Lớp công cụ vật lý từ tính, xử lý các tương tác của nam châm
    {
        private GridManager gridManager; // Biến cục bộ để giữ tham chiếu tới GridManager nhằm quản lý vị trí trên lưới

        // Caches for Zero Allocation
        private HashSet<GridEntity> tentativeMovingCache = new HashSet<GridEntity>();
        private HashSet<GridEntity> frontierCache = new HashSet<GridEntity>();
        private List<GridEntity> currentMovingCache = new List<GridEntity>();
        private HashSet<GridEntity> reachableCache = new HashSet<GridEntity>();
        private Queue<GridEntity> queueCache = new Queue<GridEntity>();
        private List<GridEntity> toRemoveCache = new List<GridEntity>();

        private Dictionary<MagnetEntity, Vector2Int> netForcesCache = new Dictionary<MagnetEntity, Vector2Int>();
        private List<GridEntity> entitiesCache = new List<GridEntity>();
        private List<(MagnetEntity mag, Direction dir)> repulsionsCache = new List<(MagnetEntity, Direction)>();
        private HashSet<GridEntity> alreadyMovedCache = new HashSet<GridEntity>();
        
        private HashSet<GridEntity> visitedCache = new HashSet<GridEntity>();
        private HashSet<GridEntity> emptyTentativeCache = new HashSet<GridEntity>();

        private static readonly Vector2Int[] AdjacentDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        public MagneticPhysicsEngine(GridManager gridManager) // Hàm khởi tạo của lớp, nhận vào một GridManager
        {
            this.gridManager = gridManager; // Gán gridManager vào biến cục bộ để sử dụng sau này
        }

        // Bước 1: Di chuyển cơ học (Mechanical Movement) - Do người chơi đẩy
        public bool TryMovePlayer(PlayerEntity player, Direction dir, List<MergedBlock> allBlocks) // Hàm thử di chuyển người chơi theo 1 hướng nhất định
        {
            // Kiểm tra xem có lấy được danh sách các vật thể cần di chuyển không (bao gồm cả nam châm bị đẩy)
            if (TryGetMovingEntities(player, dir, out HashSet<GridEntity> movingEntities))
            {
                Vector2Int offset = dir.ToVector2Int(); // Chuyển đổi hướng di chuyển (Direction) thành 1 vector 2D (ví dụ: sang phải là (1,0))
                
                foreach (var e in movingEntities) // Duyệt qua tất cả các vật thể sẽ bị di chuyển
                {
                    gridManager.RemoveEntity(e.Position); // Xóa vật thể khỏi vị trí cũ trên lưới grid
                }
                foreach (var e in movingEntities) // Duyệt lại danh sách các vật thể để cập nhật vị trí
                {
                    e.Position += offset; // Tăng vị trí của vật thể thêm 1 ô theo hướng di chuyển
                    gridManager.AddEntity(e); // Đăng ký lại vật thể vào lưới grid ở vị trí mới
                }
                
                return true; // Trả về true, báo hiệu di chuyển thành công
            }
            return false; // Nếu không tìm được danh sách vật thể di chuyển hợp lệ (bị kẹt), trả về false
        }

        private static RaycastHit2D[] hitBuffer = new RaycastHit2D[20]; // Buffer tĩnh để dùng lại nhiều lần không xả rác

        private bool IsBlockedByWall(Vector2Int from, Vector2Int to) // Hàm kiểm tra xem đường đi từ ô 'from' đến ô 'to' có bị cản bởi tường không
        {
            GridEntity entity = gridManager.GetEntityAt(to); // Lấy thông tin vật thể tại vị trí đích (to) trên grid
            if (entity != null && entity.Type == EntityType.Wall) return true; // Nếu tại đích có 1 vật thể và nó là tường (Wall), thì bị chặn (return true)

            // Quy đổi tọa độ grid sang tọa độ thực trong Unity world (cộng thêm 0.5 để raycast từ tâm ô)
            Vector2 startWorld = new Vector2(from.x + 0.5f, from.y + 0.5f); 
            Vector2 endWorld = new Vector2(to.x + 0.5f, to.y + 0.5f);
            
            // Dùng LinecastNonAlloc để không tạo rác (mảng mới) mỗi lần gọi
            int hitCount = Physics2D.LinecastNonAlloc(startWorld, endWorld, hitBuffer);
            for (int i = 0; i < hitCount; i++) // Chỉ duyệt qua các tia trúng đích thật sự
            {
                var hit = hitBuffer[i];
                if (hit.collider != null && !hit.collider.isTrigger) // Nếu có va chạm và vật đó không phải là trigger (nghĩa là vật cản cứng)
                {
                    // Lấy component EntityView từ cha của đối tượng bị va chạm
                    var view = hit.collider.GetComponentInParent<PolarBond.Views.EntityView>(); 
                    // Nếu không có EntityView hoặc thực thể đó thuộc loại Tường (Wall)
                    if (view == null || view.LogicEntity?.Type == EntityType.Wall) 
                    {
                        return true; // Kết luận là bị chặn bởi tường vật lý
                    }
                }
            }
            return false; // Không có tường chặn, đường đi thông thoáng
        }

        // Hàm kiểm tra xem một nam châm bị đẩy lùi (bởi từ trường) có thể di chuyển không, kiểm tra đệ quy
        private bool CanRepelledMagnetMove(MagnetEntity mag, Vector2Int repelDir, int pushingMass, HashSet<GridEntity> tentativeMoving, HashSet<GridEntity> visited)
        {
            if (visited.Contains(mag)) return false; // Nếu nam châm này đã được duyệt qua (để tránh lặp vô hạn), trả về false
            
            List<MagnetEntity> blockMagnets = new List<MagnetEntity>(); // Danh sách chứa tất cả nam châm trong cùng 1 khối (đã dính vào nhau)
            if (mag.CurrentBlock != null) // Nếu nam châm thuộc về một khối đã gộp
            {
                foreach (var m in mag.CurrentBlock.Magnets) // Duyệt qua từng nam châm con trong khối
                {
                    if (!tentativeMoving.Contains(m)) // Nếu nam châm này chưa nằm trong nhóm "đang tính toán di chuyển"
                    {
                        blockMagnets.Add(m); // Thêm nó vào danh sách khối cần bị đẩy
                        visited.Add(m); // Đánh dấu là đã duyệt qua
                    }
                }
            }
            else // Nếu nam châm đứng lẻ loi (không thuộc khối nào)
            {
                blockMagnets.Add(mag); // Thêm chính nó vào danh sách
                visited.Add(mag); // Đánh dấu nó đã duyệt qua
            }

            int targetMass = blockMagnets.Count > 0 ? blockMagnets.Count : 1; // Khối lượng cần đẩy chính là số lượng nam châm trong khối
            if (pushingMass < targetMass) return false; // Nếu lực đẩy (khối lượng đẩy) nhỏ hơn khối lượng của khối, thì không thể đẩy được
            
            foreach (var m in blockMagnets) // Duyệt qua tất cả các nam châm trong khối bị đẩy
            {
                Vector2Int nextPos = m.Position + repelDir; // Tính toán vị trí tiếp theo mà nam châm này sẽ tới
                if (IsBlockedByWall(m.Position, nextPos)) return false; // Nếu hướng đi bị kẹt tường, toàn bộ khối không thể di chuyển
                
                GridEntity inWay = gridManager.GetEntityAt(nextPos); // Lấy vật cản tại vị trí đích
                // Nếu có vật cản và nó không phải là một phần của chính khối đang xét
                if (inWay != null && !(inWay is MagnetEntity mw && blockMagnets.Contains(mw)))
                {
                    if (inWay.Type == EntityType.Wall) return false; // Vật cản là tường, kẹt lại
                    // Nếu vật cản là người chơi, nhưng người chơi không nằm trong nhóm đang tính di chuyển, thì kẹt lại
                    if (inWay.Type == EntityType.Player && !tentativeMoving.Contains(inWay)) return false;
                    
                    if (inWay.Type == EntityType.Magnet) // Nếu vật cản là một nam châm khác
                    {
                        MagnetEntity magInWay = (MagnetEntity)inWay; // Ép kiểu sang MagnetEntity
                        // Nếu nam châm cản đường có cùng cực (đẩy nhau) và chưa nằm trong nhóm di chuyển dự kiến
                        if (magInWay.Polarity == m.Polarity && !tentativeMoving.Contains(magInWay))
                        {
                            // Kiểm tra đệ quy xem nam châm cản đường đó có thể bị đẩy tiếp không
                            if (!CanRepelledMagnetMove(magInWay, repelDir, pushingMass, tentativeMoving, visited))
                            {
                                return false; // Nếu khối cản đường không nhúc nhích được, thì khối hiện tại cũng bị kẹt
                            }
                        }
                    }
                }
            }
            
            foreach (var m in blockMagnets) // Sau khi kiểm tra vật cản trực tiếp, kiểm tra ma sát/lực đẩy từ tính ở 2 bên
            {
                Vector2Int nextPos = m.Position + repelDir; // Vị trí dự tính tiếp theo
                Vector2Int[] adj = AdjacentDirections; // 4 ô xung quanh
                foreach (var d in adj) // Duyệt qua 4 ô kề cạnh
                {
                    GridEntity adjEntity = gridManager.GetEntityAt(nextPos + d); // Lấy vật kề cạnh ở vị trí mới
                    // Nếu có nam châm kề cạnh và nó chưa tham gia vào nhóm di chuyển dự kiến
                    if (adjEntity != null && adjEntity is MagnetEntity adjMag && !tentativeMoving.Contains(adjMag))
                    {
                        if (adjMag.Polarity == m.Polarity) // Nếu hai nam châm cùng cực (đẩy nhau)
                        {
                            if (visited.Contains(adjMag)) continue; // Bỏ qua nếu đã xét nam châm này
                            Vector2Int nextRepelDir = adjMag.Position - nextPos; // Hướng mà nam châm bên cạnh sẽ bị đẩy dạt ra
                            // Kiểm tra đệ quy xem nam châm bên cạnh có thể bị trượt đi với lực đẩy bằng 1 (ma sát trượt) hay không
                            if (!CanRepelledMagnetMove(adjMag, nextRepelDir, 1, tentativeMoving, visited))
                            {
                                return false; // Nếu nam châm kề cạnh không trượt ra được, nó sẽ tạo ra lực cản từ tính chặn khối này lại
                            }
                        }
                    }
                }
            }
            return true; // Khối có thể di chuyển an toàn
        }

        // Kiểm tra xem từ vị trí tương lai (nextPos), cục đẩy có thể bị dội ngược theo hướng bounceDir không
        private bool CanSimulateBounce(MagnetEntity currMag, Vector2Int nextPos, Vector2Int bounceDir, HashSet<GridEntity> tentativeMoving, Vector2Int moveDir)
        {
            Vector2Int bounceDest = nextPos + bounceDir;
            if (IsBlockedByWall(nextPos, bounceDest)) return false;

            GridEntity obstacle = null;
            foreach (var e in gridManager.GetAllEntities())
            {
                Vector2Int futurePos = tentativeMoving.Contains(e) ? e.Position + moveDir : e.Position;
                if (futurePos == bounceDest)
                {
                    obstacle = e;
                    break;
                }
            }

            if (obstacle != null)
            {
                if (obstacle is MagnetEntity obsMag && obsMag.Polarity == currMag.Polarity)
                {
                    Vector2Int nextBounceDest = bounceDest + bounceDir;
                    if (IsBlockedByWall(bounceDest, nextBounceDest)) return false;
                    
                    GridEntity nextObstacle = null;
                    foreach (var e in gridManager.GetAllEntities())
                    {
                        Vector2Int futurePos = tentativeMoving.Contains(e) ? e.Position + moveDir : e.Position;
                        if (futurePos == nextBounceDest)
                        {
                            nextObstacle = e;
                            break;
                        }
                    }
                    if (nextObstacle != null) return false;
                    return true;
                }
                return false; 
            }
            return true;
        }

        // Hàm chính để gom tất cả các vật thể sẽ di chuyển cùng nhau khi có lực tác động
        private bool TryGetMovingEntities(GridEntity startEntity, Direction dir, out HashSet<GridEntity> finalMoving)
        {
            tentativeMovingCache.Clear();
            frontierCache.Clear();
            HashSet<GridEntity> tentativeMoving = tentativeMovingCache; // Tập các vật thể dự kiến sẽ di chuyển
            HashSet<GridEntity> frontier = frontierCache; // Hàng đợi chứa các vật thể cần kiểm tra lan truyền
            frontier.Add(startEntity); // Đưa vật thể gốc (như người chơi) vào hàng đợi
            
            // 1. Xây dựng tập di chuyển dự kiến bằng cách kiểm tra khối lượng và đẩy trực tiếp
            while(frontier.Count > 0) // Lặp chừng nào còn vật cần xử lý
            {
                foreach(var f in frontier) tentativeMoving.Add(f); // Thêm các vật từ hàng đợi vào tập dự kiến
                frontier.Clear(); // Xóa hàng đợi để chuẩn bị nạp vật mới
                
                bool dragModified = true; // Biến cờ theo dõi xem có vật nào bị kéo theo không
                while(dragModified) // Lặp để kéo tất cả các nam châm cùng một khối (bonded)
                {
                    dragModified = false; // Đặt lại cờ
                    currentMovingCache.Clear();
                    foreach (var e in tentativeMoving) currentMovingCache.Add(e);
                    List<GridEntity> currentMoving = currentMovingCache; // Sao chép danh sách hiện tại
                    foreach(var e in currentMoving) // Duyệt qua từng vật đang dự kiến di chuyển
                    {
                        if (e is MagnetEntity m && m.CurrentBlock != null) // Nếu là nam châm và nó nằm trong một khối hợp nhất
                        {
                            foreach(var bonded in m.CurrentBlock.Magnets) // Duyệt qua các nam châm anh em trong cùng khối
                            {
                                if (!tentativeMoving.Contains(bonded)) // Nếu nam châm anh em chưa nằm trong tập dự kiến
                                {
                                    tentativeMoving.Add(bonded); // Thêm nó vào tập dự kiến
                                    dragModified = true; // Đánh dấu là có sự thay đổi để vòng while quét lại
                                }
                            }
                        }
                    }
                }
                
                int totalMass = 0; // Tính tổng khối lượng (sức đẩy) của nhóm hiện tại
                bool hasPlayer = false; // Đánh dấu xem nhóm này có chứa người chơi không
                foreach (var e in tentativeMoving) // Duyệt qua các vật trong nhóm
                {
                    if (e.Type == EntityType.Player) hasPlayer = true; // Xác nhận có người chơi
                    if (e.Type == EntityType.Magnet) totalMass++; // Mỗi nam châm tăng sức đẩy thêm 1 đơn vị
                }
                if (hasPlayer) totalMass += 3; // Nếu có mặt người chơi, cộng thêm 3 đơn vị sức đẩy mạnh mẽ
                
                foreach (var e in tentativeMoving) // Kiểm tra các vật nằm chắn trên đường đi
                {
                    Vector2Int nextPos = e.Position + dir.ToVector2Int(); // Tính tọa độ ô phía trước
                    GridEntity inWay = gridManager.GetEntityAt(nextPos); // Lấy vật cản phía trước
                    
                    // Nếu có vật cản, và nó chưa nằm trong nhóm dự kiến hay hàng đợi
                    if (inWay != null && !tentativeMoving.Contains(inWay) && !frontier.Contains(inWay))
                    {
                        if (inWay.Type == EntityType.Magnet) // Nếu vật cản là nam châm
                        {
                            MagnetEntity magInWay = (MagnetEntity)inWay;
                            // Nếu vật điểu đẩy cũng là nam châm, cùng cực với vật cản, VÀ có người chơi đẩy
                            if (e is MagnetEntity pusherMag && pusherMag.Polarity == magInWay.Polarity && hasPlayer)
                            {
                                // Cùng cực thì không thể đẩy vật lý nếu người chơi tác động trực tiếp (quy tắc game)
                            }
                            else
                            {
                                frontier.Add(inWay); // Thêm vật cản vào hàng đợi để tiếp tục tính toán đẩy đi
                            }
                        }
                    }
                    // Kiểm tra tác động của lực từ trường từ xa
                    if (e is MagnetEntity eMag) // Nếu vật đang di chuyển là nam châm
                    {
                        Vector2Int magFieldPos = nextPos + dir.ToVector2Int(); // Tính tọa độ từ trường phía trước 1 ô
                        GridEntity inMagField = gridManager.GetEntityAt(magFieldPos); // Lấy vật thể ở ô từ trường đó
                        // Nếu ô đó có nam châm và chưa thuộc tập dự kiến di chuyển hay hàng đợi
                        if (inMagField != null && inMagField is MagnetEntity fieldMag && !tentativeMoving.Contains(fieldMag) && !frontier.Contains(fieldMag))
                        {
                            if (eMag.Polarity == fieldMag.Polarity) // Nếu hai nam châm cùng cực (sẽ đẩy nhau từ xa)
                            {
                                int targetMass = 1; // Khối lượng nam châm bị đẩy khởi điểm là 1
                                if (fieldMag.CurrentBlock != null) // Nếu nam châm bị đẩy thuộc về một khối
                                {
                                    targetMass = 0; // Đặt khối lượng về 0 để tính lại
                                    foreach (var m in fieldMag.CurrentBlock.Magnets) // Duyệt qua các nam châm trong khối bị đẩy
                                    {
                                        if (!tentativeMoving.Contains(m)) targetMass++; // Chỉ đếm các nam châm chưa di chuyển
                                    }
                                    if (targetMass == 0) targetMass = 1; // Đảm bảo khối lượng mục tiêu tối thiểu là 1
                                }
                                
                                if (totalMass >= targetMass) // Nếu sức đẩy lớn hơn hoặc bằng khối lượng vật bị đẩy
                                {
                                    frontier.Add(fieldMag); // Đưa khối bị đẩy từ xa vào hàng đợi di chuyển
                                }
                            }
                        }
                    }
                }
            }
            // 2. Lược bỏ (Prune) các vật thể không thể di chuyển (do kẹt tường, khối lượng yếu, hoặc từ trường cản)
            bool pruned = true; // Biến cờ cho biết có vật nào vừa bị loại ra không
            while (pruned) // Vòng lặp cắt tỉa
            {
                pruned = false; // Đặt lại cờ
                currentMovingCache.Clear();
                foreach (var e in tentativeMoving) currentMovingCache.Add(e);
                List<GridEntity> currentSet = currentMovingCache; // Sao chép tập dự kiến
                
                int totalMass = 0; // Tính lại sức đẩy
                bool hasPlayer = false; // Kiểm tra lại người chơi
                foreach (var e in currentSet) // Duyệt các vật thể
                {
                    if (e.Type == EntityType.Player) hasPlayer = true; // Đánh dấu người chơi
                    if (e.Type == EntityType.Magnet) totalMass++; // Tính khối lượng
                }
                if (hasPlayer) totalMass += 3; // Người chơi cung cấp thêm sức đẩy (ảo) để ép qua từ trường

                foreach (var curr in currentSet) // Kiểm tra khả năng di chuyển của từng vật
                {
                    Vector2Int nextPos = curr.Position + dir.ToVector2Int(); // Vị trí tiếp theo
                    bool canMove = true; // Cờ cho phép di chuyển
                    bool blockedByMagnet = false; // Cờ kiểm tra kẹt nam châm
                    if (IsBlockedByWall(curr.Position, nextPos)) // Nếu bị tường chặn
                    {
                        canMove = false; // Không thể di chuyển
                    }
                    else
                    {
                        GridEntity inWay = gridManager.GetEntityAt(nextPos); // Lấy vật cản phía trước
                        if (inWay != null && !tentativeMoving.Contains(inWay)) // Nếu có vật cản và nó không nằm trong nhóm di chuyển
                        {
                            canMove = false; // Không đi được
                            if (inWay.Type == EntityType.Magnet) blockedByMagnet = true; // Bị nam châm chặn
                        }
                        
                        if (canMove && curr is MagnetEntity currMag) // Nếu tạm thời đi được và vật là nam châm
                        {
                            Vector2Int[] adj = AdjacentDirections; // 4 hướng xung quanh
                            foreach (var d in adj) // Quét xung quanh vị trí mới
                            {
                                GridEntity adjEntity = gridManager.GetEntityAt(nextPos + d); // Lấy thực thể ở ô kế cận
                                // Nếu kề cận là nam châm và không cùng nhóm di chuyển
                                if (adjEntity != null && adjEntity is MagnetEntity adjMag && !tentativeMoving.Contains(adjMag))
                                {
                                    if (currMag.Polarity == adjMag.Polarity) // Nếu cùng cực (đẩy nhau)
                                    {
                                        Vector2Int repelDir = adjMag.Position - nextPos; // Hướng mà nam châm bên cạnh bị đẩy
                                        visitedCache.Clear();
                                        HashSet<GridEntity> visited = visitedCache; // Lưu vết
                                        visited.Add(currMag); // Đã xét nam châm hiện tại
                                        
                                        // Kiểm tra xem nam châm bên cạnh có trượt ra được không
                                        if (!CanRepelledMagnetMove(adjMag, repelDir, 1, tentativeMoving, visited))
                                        {
                                            // Nam châm bên cạnh kẹt. Lực sẽ dội ngược lại currMag.
                                            // Kiểm tra xem currMag có CHỖ để dội ngược hay không!
                                            Vector2Int bounceDir = -repelDir;
                                            if (!CanSimulateBounce(currMag, nextPos, bounceDir, tentativeMoving, dir.ToVector2Int()))
                                            {
                                                // Cả hai phía đều kẹt TƯỜNG (hoặc cản cứng).
                                                // Logic: Lực đẩy không có đường thoát, chặn chuyển động cơ học!
                                                canMove = false; 
                                                blockedByMagnet = true;
                                                break; 
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    if (!canMove) // Nếu phát hiện vật không thể di chuyển
                    {
                        tentativeMoving.Remove(curr); // Loại nó khỏi nhóm dự kiến
                        pruned = true; // Bật cờ cắt tỉa để lặp lại việc đánh giá (vì mất đi một khối lượng đẩy)
                        // Các dòng code dưới đây (đã comment sẵn trong bản gốc) dùng để loại bỏ cả khối nếu bị kẹt
                        // if (blockedByMagnet && curr is MagnetEntity m && m.CurrentBlock != null)
                        // {
                        //     foreach (var peer in m.CurrentBlock.Magnets)
                        //     {
                        //         tentativeMoving.Remove(peer);
                        //     }
                        // }
                    }
                }
                if (pruned) // Nếu có vật vừa bị loại
                {
                    if (!tentativeMoving.Contains(startEntity)) // Nếu vật thể khởi điểm (người chơi) đã bị loại
                    {
                        tentativeMoving.Clear(); // Hủy bỏ toàn bộ cuộc di chuyển
                    }
                    else // Nếu vẫn còn vật khởi điểm, lọc lại các vật đứt liên kết
                    {
                        reachableCache.Clear();
                        queueCache.Clear();
                        HashSet<GridEntity> reachable = reachableCache; // Tập các vật còn kết nối với gốc
                        Queue<GridEntity> q = queueCache; // Hàng đợi duyệt BFS
                        q.Enqueue(startEntity); // Bắt đầu từ gốc
                        reachable.Add(startEntity); // Thêm gốc vào tập đã duyệt
                        while (q.Count > 0) // Duyệt BFS để tìm chuỗi kết nối
                        {
                            var node = q.Dequeue(); // Lấy 1 node ra xử lý
                            
                            if (node is MagnetEntity nMag && nMag.CurrentBlock != null) // Nếu là nam châm trong khối
                            {
                                foreach (var m in nMag.CurrentBlock.Magnets) // Duyệt qua các nam châm anh em
                                {
                                    // Kéo theo các nam châm kề cạnh cùng nhóm di chuyển
                                    if (IsAdjacent(node.Position, m.Position) && tentativeMoving.Contains(m) && !reachable.Contains(m))
                                    {
                                        reachable.Add(m); // Đánh dấu kết nối
                                        q.Enqueue(m); // Cho vào hàng đợi duyệt tiếp
                                    }
                                }
                            }
                            Vector2Int nextPos = node.Position + dir.ToVector2Int(); // Vị trí đẩy phía trước
                            GridEntity inWay = gridManager.GetEntityAt(nextPos); // Lấy vật bị đẩy
                            // Nếu vật bị đẩy nằm trong nhóm di chuyển và chưa được đánh dấu
                            if (inWay != null && tentativeMoving.Contains(inWay) && !reachable.Contains(inWay))
                            {
                                reachable.Add(inWay); // Đánh dấu kết nối
                                q.Enqueue(inWay); // Đưa vào hàng đợi
                            }
                            if (node is MagnetEntity nMag2) // Kiểm tra liên kết từ trường (đẩy từ xa)
                            {
                                Vector2Int magFieldPos = nextPos + dir.ToVector2Int(); // Ô từ trường phía trước
                                GridEntity inMagField = gridManager.GetEntityAt(magFieldPos); // Lấy nam châm bị đẩy từ xa
                                // Nếu có nam châm trong từ trường và nó thuộc nhóm di chuyển
                                if (inMagField != null && inMagField is MagnetEntity fieldMag && tentativeMoving.Contains(fieldMag) && !reachable.Contains(fieldMag))
                                {
                                    if (nMag2.Polarity == fieldMag.Polarity) // Nếu cùng cực
                                    {
                                        reachable.Add(fieldMag); // Kết nối với nhau qua từ trường
                                        q.Enqueue(fieldMag); // Đưa vào hàng đợi
                                    }
                                }
                            }
                        }
                        toRemoveCache.Clear();
                        List<GridEntity> toRemove = toRemoveCache; // Danh sách các vật bị đứt kết nối
                        foreach (var e in tentativeMoving) // Tìm các vật trong nhóm
                        {
                            if (!reachable.Contains(e)) toRemove.Add(e); // Vật không kết nối với gốc thì phải bỏ đi
                        }
                        
                        if (toRemove.Count > 0) // Nếu có vật cần bỏ
                        {
                            foreach (var e in toRemove) tentativeMoving.Remove(e); // Xóa khỏi nhóm di chuyển
                            pruned = true; // Tiếp tục lặp quá trình cắt tỉa
                        }
                    }
                }
            }
            finalMoving = tentativeMoving; // Trả về nhóm vật thể cuối cùng được phép đi
            return finalMoving.Contains(startEntity); // Kiểm tra lại xem gốc (người chơi) có đi được không
        }

        // Hàm hỗ trợ tính toán tổng "khối lượng hiệu dụng" của một khối nam châm, bao gồm cả sức đẩy của người chơi
        private int GetEffectiveMass(MagnetEntity mag)
        {
            // Lấy danh sách nam châm trong khối, hoặc tạo mảng 1 phần tử nếu đứng lẻ
            List<MagnetEntity> magnets = mag.CurrentBlock != null ? mag.CurrentBlock.Magnets : new List<MagnetEntity> { mag };
            int mass = magnets.Count; // Khối lượng cơ bản là tổng số cục nam châm
            
            foreach (var m in magnets) // Duyệt qua từng nam châm
            {
                Vector2Int[] adj = AdjacentDirections; // 4 hướng kề cạnh
                foreach (var d in adj) // Quét xung quanh
                {
                    GridEntity entity = gridManager.GetEntityAt(m.Position + d); // Lấy thực thể kế bên
                    if (entity != null && entity.Type == EntityType.Player) // Nếu kế bên có người chơi
                    {
                        return mass + 3; // Người chơi chống lưng thì được cộng thêm 3 vào sức kháng lực/đẩy
                    }
                }
            }
            return mass; // Trả về khối lượng bình thường
        }

        // Tính toán và áp dụng các lực đẩy (Repulsion) thụ động giữa các nam châm trên lưới
        public bool ApplyRepulsion(List<MergedBlock> allBlocks)
        {
            netForcesCache.Clear();
            Dictionary<MagnetEntity, Vector2Int> netForces = netForcesCache; // Từ điển lưu tổng lực trên mỗi nam châm
            
            entitiesCache.Clear();
            entitiesCache.AddRange(gridManager.GetAllEntities());
            var entities = entitiesCache; // Lấy toàn bộ thực thể
            
            for (int i = 0; i < entities.Count; i++) // Duyệt thực thể thứ 1
            {
                if (entities[i] is MagnetEntity m1) // Nếu là nam châm
                {
                    for (int j = i + 1; j < entities.Count; j++) // Duyệt thực thể thứ 2 (tránh so sánh 2 lần)
                    {
                        if (entities[j] is MagnetEntity m2) // Nếu thực thể 2 cũng là nam châm
                        {
                            // Nếu hai nam châm cùng cực (cùng loại âm/dương) và nằm kề nhau
                            if (m1.Polarity == m2.Polarity && IsAdjacent(m1.Position, m2.Position))
                            {
                                int size1 = GetEffectiveMass(m1); // Lấy độ nặng của khối 1
                                int size2 = GetEffectiveMass(m2); // Lấy độ nặng của khối 2
                                
                                Vector2Int diff = m2.Position - m1.Position; // Tính hướng từ m1 sang m2
                                Vector2Int forceOnM2 = diff; // Lực tác động lên m2 đẩy m2 ra xa
                                Vector2Int forceOnM1 = -diff; // Lực tác động lên m1 đẩy m1 ra xa
                                
                                if (size1 < size2) // Nếu khối 1 nhẹ hơn khối 2
                                {
                                    // m1 nhẹ hơn. Kiểm tra xem m1 có bị kẹt không (với sức đẩy của m2)
                                    visitedCache.Clear();
                                    emptyTentativeCache.Clear();
                                    if (CanRepelledMagnetMove(m1, forceOnM1, size2, emptyTentativeCache, visitedCache))
                                        AddForce(netForces, m1, forceOnM1); // m1 trượt ra xa
                                    else
                                        AddForce(netForces, m2, forceOnM2); // m1 kẹt, dội phản lực sang m2
                                }
                                else if (size2 < size1) // Nếu khối 2 nhẹ hơn khối 1
                                {
                                    // m2 nhẹ hơn. Kiểm tra xem m2 có bị kẹt không (với sức đẩy của m1)
                                    visitedCache.Clear();
                                    emptyTentativeCache.Clear();
                                    if (CanRepelledMagnetMove(m2, forceOnM2, size1, emptyTentativeCache, visitedCache))
                                        AddForce(netForces, m2, forceOnM2); // m2 trượt ra xa
                                    else
                                        AddForce(netForces, m1, forceOnM1); // m2 kẹt, dội phản lực sang m1
                                }
                                else // Nếu hai khối nặng bằng nhau
                                {
                                    // Kiểm tra độc lập xem mỗi khối có đi được không
                                    visitedCache.Clear();
                                    emptyTentativeCache.Clear();
                                    bool m1CanMove = CanRepelledMagnetMove(m1, forceOnM1, size2, emptyTentativeCache, visitedCache);
                                    
                                    visitedCache.Clear();
                                    bool m2CanMove = CanRepelledMagnetMove(m2, forceOnM2, size1, emptyTentativeCache, visitedCache);
                                    
                                    if (m1CanMove) AddForce(netForces, m1, forceOnM1);
                                    if (m2CanMove) AddForce(netForces, m2, forceOnM2);
                                }
                            }
                        }
                    }
                }
            }
            
            repulsionsCache.Clear();
            List<(MagnetEntity mag, Direction dir)> repulsions = repulsionsCache; // Danh sách các nam châm sẽ bị văng ra
            foreach (var kvp in netForces) // Duyệt qua từ điển các lực tổng hợp
            {
                if (kvp.Value == Vector2Int.zero) continue; // Nếu lực triệt tiêu bằng 0 thì bỏ qua
                
                if (kvp.Value.x != 0 && kvp.Value.y == 0) // Lực chỉ theo trục X ngang
                {
                    repulsions.Add((kvp.Key, kvp.Value.x > 0 ? Direction.Right : Direction.Left)); // Chuyển thành hướng Right/Left
                }
                else if (kvp.Value.y != 0 && kvp.Value.x == 0) // Lực chỉ theo trục Y dọc
                {
                    repulsions.Add((kvp.Key, kvp.Value.y > 0 ? Direction.Up : Direction.Down)); // Chuyển thành hướng Up/Down
                }
            }
            
            bool anyMoved = false; // Cờ kiểm tra xem có ai di chuyển không
            alreadyMovedCache.Clear();
            HashSet<GridEntity> alreadyMoved = alreadyMovedCache; // Tập các nam châm đã di chuyển để tránh đẩy trùng
            foreach (var rep in repulsions) // Duyệt qua danh sách các nam châm bị văng ra
            {
                if (alreadyMoved.Contains(rep.mag)) continue; // Nếu đã bay đi rồi thì bỏ qua
                // Thử di chuyển nam châm bị văng theo hướng đẩy lùi
                if (TryGetMovingEntities(rep.mag, rep.dir, out HashSet<GridEntity> movingEntities))
                {
                    anyMoved = true; // Bật cờ có chuyển động
                    Vector2Int offset = rep.dir.ToVector2Int(); // Tính độ dời
                    foreach (var e in movingEntities) gridManager.RemoveEntity(e.Position); // Xóa khỏi vị trí cũ
                    foreach (var e in movingEntities) 
                    { 
                        e.Position += offset; // Tăng vị trí
                        gridManager.AddEntity(e); // Thêm lại
                        alreadyMoved.Add(e); // Đánh dấu đã di chuyển
                    }
                }
            }

            return anyMoved; // Trả về kết quả xem có xảy ra lực đẩy làm di chuyển vật không
        }

        // Hàm tiện ích để cộng dồn lực vào từ điển netForces
        private void AddForce(Dictionary<MagnetEntity, Vector2Int> forces, MagnetEntity mag, Vector2Int force)
        {
            if (forces.ContainsKey(mag)) // Nếu nam châm đã chịu lực
                forces[mag] += force; // Cộng dồn lực mới vào
            else
                forces[mag] = force; // Gán lực đầu tiên
        }

        // Bước 4: Xử lý lực hút (Attraction) và hợp khối
        public void ProcessAttraction(List<MergedBlock> allBlocks)
        {
            // Tái tạo lại danh sách khối từ các nam châm đơn lẻ. Giúp việc tách khối dễ dàng hơn ở mỗi lượt
            foreach (var block in allBlocks)
            {
                MergedBlock.ReturnToPool(block);
            }
            allBlocks.Clear(); // Xóa sạch danh sách khối cũ
            foreach (var entity in gridManager.GetAllEntities()) // Duyệt các vật thể
            {
                if (entity is MagnetEntity mag) // Nếu là nam châm
                {
                    allBlocks.Add(MergedBlock.Get(mag)); // Tạo một khối mới chỉ chứa nam châm này bằng Pool
                }
            }
            
            bool mergedSomething = true; // Cờ theo dõi vòng lặp hút khối
            while (mergedSomething) // Lặp cho tới khi không còn khối nào dính thêm
            {
                mergedSomething = false; // Khởi tạo lại cờ
                for (int i = 0; i < allBlocks.Count; i++) // Lấy khối thứ 1
                {
                    for (int j = i + 1; j < allBlocks.Count; j++) // Lấy khối thứ 2
                    {
                        // Kiểm tra xem 2 khối này có hút nhau không
                        if (CheckAndMerge(allBlocks[i], allBlocks[j]))
                        {
                            MergedBlock.ReturnToPool(allBlocks[j]); // Trả khối cũ về pool
                            allBlocks.RemoveAt(j); // Nếu hút, khối j gộp vào khối i, nên xóa khối j đi
                            mergedSomething = true; // Đánh dấu là có hợp khối
                            break; // Thoát vòng lặp con để bắt đầu quy trình quét lại từ đầu
                        }
                    }
                    if (mergedSomething) break; // Thoát vòng lặp ngoài để quét lại
                }
            }
        }

        // Hàm kiểm tra xem 2 khối có nằm kề nhau và trái cực không. Có thì gộp chúng
        private bool CheckAndMerge(MergedBlock b1, MergedBlock b2)
        {
            foreach (var m1 in b1.Magnets) // Lấy từng nam châm của khối 1
            {
                foreach (var m2 in b2.Magnets) // Lấy từng nam châm của khối 2
                {
                    // Nếu 2 nam châm kề cạnh (Adjacent) và ngược cực tính (khác Polarity)
                    if (IsAdjacent(m1.Position, m2.Position) && m1.Polarity != m2.Polarity)
                    {
                        b1.MergeWith(b2); // Tiến hành hợp khối 2 vào khối 1
                        return true; // Báo hiệu hợp khối thành công
                    }
                }
            }
            return false; // Không có nam châm nào trái cực kề nhau
        }

        // Hàm kiểm tra 2 tọa độ p1, p2 có kề cạnh nhau (trên, dưới, trái, phải) hay không
        private bool IsAdjacent(Vector2Int p1, Vector2Int p2)
        {
            int dx = Mathf.Abs(p1.x - p2.x); // Khoảng cách trục X
            int dy = Mathf.Abs(p1.y - p2.y); // Khoảng cách trục Y
            // Trả về true nếu cách 1 ô trên 1 trục và 0 ô trên trục kia
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }
    }
}
