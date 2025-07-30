using Unity.Entities;
using UnityEngine;

public class MeleeAttackAutoring : MonoBehaviour
{
    public float timerMax;
    public int damageAmount;
    public float colliderSize;
    public class Baker : Baker<MeleeAttackAutoring>
    {
        public override void Bake(MeleeAttackAutoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MeleeAttack
            {
                timerMax = authoring.timerMax,
                damageAmount = authoring.damageAmount,
                colliderSize = authoring.colliderSize
            });
        }
    }
}

public struct MeleeAttack : IComponentData
{
    public float timer;
    public float timerMax;
    public int damageAmount;
    public float colliderSize;
}
