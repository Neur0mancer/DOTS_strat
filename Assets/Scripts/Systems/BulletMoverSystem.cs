using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
partial struct BulletMoverSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer =
                        SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        foreach ((
            RefRW<LocalTransform> localTransform, 
            RefRO<Bullet> bullet,
            RefRO<Target> target,
            Entity entity) in
            SystemAPI.Query
                <RefRW<LocalTransform>,
                RefRO<Bullet>,
                RefRO<Target>>().WithEntityAccess())
        {
            if(target.ValueRO.targetEntity == Entity.Null)
            {
                entityCommandBuffer.DestroyEntity(entity);
                continue;
            }
            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
            ShootVictim targetShootVictim = SystemAPI.GetComponent<ShootVictim>(target.ValueRO.targetEntity);
            float3 targetPosition = targetLocalTransform.TransformPoint(targetShootVictim.hitLocalPosition);

            float distanceBeforSq = math.distancesq(localTransform.ValueRO.Position, targetPosition);

            float3 moveDirection = targetPosition - localTransform.ValueRO.Position;
            moveDirection = math.normalize(moveDirection);

            localTransform.ValueRW.Position += moveDirection * bullet.ValueRO.speed * SystemAPI.Time.DeltaTime;

            float distanceAfterSq = math.distancesq(localTransform.ValueRO.Position, targetPosition);

            if(distanceAfterSq > distanceBeforSq)
            {
                localTransform.ValueRW.Position = targetPosition;
            }

            float destroyDistanceSQ = .2f;
            if (math.distancesq(localTransform.ValueRO.Position, targetPosition) < destroyDistanceSQ)
            {
                RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
                targetHealth.ValueRW.healthAmount -= bullet.ValueRO.damageAmount;
                targetHealth.ValueRW.onHealthChanged = true;
                entityCommandBuffer.DestroyEntity(entity);
            }
        }
    }
}
