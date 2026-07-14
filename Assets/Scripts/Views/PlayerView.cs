using UnityEngine;

namespace PolarBond.Views
{
    public class PlayerView : EntityView
    {
        [Header("Rotation Settings")]
        public float rotationSpeed = 1000f; // Tốc độ xoay (nếu không dùng xoay ngay lập tức)
        public float angleOffset = -90f; // Bù trừ góc
        public bool instantRotation = true; // Xoay ngay lập tức để tránh hiện tượng xoay chéo (diagonal)

        private Vector2Int lastLogicPosition;
        private Quaternion targetRotation;

        public override void Initialize(PolarBond.Entities.GridEntity entity)
        {
            base.Initialize(entity);
            if (entity != null)
            {
                lastLogicPosition = entity.Position;
            }
            targetRotation = transform.rotation;
        }

        protected override void Update()
        {
            base.Update();

            if (LogicEntity != null)
            {
                // Chỉ tính toán hướng mới khi toạ độ lưới (Grid) thay đổi
                if (LogicEntity.Position != lastLogicPosition)
                {
                    Vector2Int dirInt = LogicEntity.Position - lastLogicPosition;
                    lastLogicPosition = LogicEntity.Position;

                    if (dirInt.sqrMagnitude > 0)
                    {
                        float targetAngle = Mathf.Atan2(dirInt.y, dirInt.x) * Mathf.Rad2Deg + angleOffset;
                        targetRotation = Quaternion.Euler(0, 0, targetAngle);
                    }
                }

                if (instantRotation)
                {
                    // Đặt thẳng góc quay, bỏ qua việc xoay chéo qua các góc trung gian
                    transform.rotation = targetRotation;
                }
                else
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }
    }
}
