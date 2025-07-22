using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ShootVictimAuthoting : MonoBehaviour
{
    public Transform hitPositionTransform;

    public class Baker : Baker<ShootVictimAuthoting>
    {
        public override void Bake(ShootVictimAuthoting authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ShootVictim
            {
                hitLocalPosition = authoring.hitPositionTransform.localPosition,
            });
        }
    }
}

public struct ShootVictim : IComponentData
{
    public float3 hitLocalPosition;
}
