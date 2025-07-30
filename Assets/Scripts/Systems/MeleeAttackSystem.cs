using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct MeleeAttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingletone = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingletone.CollisionWorld;
        NativeList<RaycastHit> raycastHitList = new NativeList<RaycastHit>(Allocator.Temp);

        foreach ((
            RefRO<LocalTransform> localTransfrom,
            RefRW<MeleeAttack> meleeAttack,
            RefRO<Target> target,
            RefRW<UnitMover> unitMover)
            in SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRW<MeleeAttack>,
                RefRO<Target>,
                RefRW<UnitMover>>().WithDisabled<MoveOverride>())
        {
            if (target.ValueRO.targetEntity == null)
            {
                continue;
            }
            LocalTransform targetLocalTranform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
            float meleeAttackDistanceSq = 2f;
            bool isCloseEnoughtToAttack = math.distancesq(localTransfrom.ValueRO.Position, targetLocalTranform.Position) > meleeAttackDistanceSq;

            bool isTouchingTarget = false;
            if (!isCloseEnoughtToAttack)
            {
                float3 dirToTarget = targetLocalTranform.Position - localTransfrom.ValueRO.Position;
                dirToTarget = math.normalize(dirToTarget);
                RaycastInput raycastInput = new RaycastInput
                {
                    Start = localTransfrom.ValueRO.Position,
                    End = localTransfrom.ValueRO.Position + dirToTarget * meleeAttack.ValueRO.colliderSize,
                    Filter = CollisionFilter.Default,
                };
                raycastHitList.Clear();
                if (collisionWorld.CastRay(raycastInput, ref raycastHitList))
                {
                    foreach (RaycastHit raycastHit in raycastHitList)
                    {
                        if (raycastHit.Entity == target.ValueRO.targetEntity)
                        {
                            isTouchingTarget = true;
                            break;
                        }
                    }
                }
            }

            if (!isCloseEnoughtToAttack && !isTouchingTarget)
            {
                unitMover.ValueRW.targetPosition = targetLocalTranform.Position;
            }
            else
            {
                unitMover.ValueRW.targetPosition = localTransfrom.ValueRO.Position;

                meleeAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if (meleeAttack.ValueRO.timer > 0)
                {
                    continue;
                }
                meleeAttack.ValueRW.timer = meleeAttack.ValueRO.timerMax;

                RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
                targetHealth.ValueRW.healthAmount -= meleeAttack.ValueRO.damageAmount;
                targetHealth.ValueRW.onHealthChanged = true;
            }
        }
    }
}
