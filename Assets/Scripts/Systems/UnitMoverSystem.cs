using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

partial struct UnitMoverSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((
            RefRW<LocalTransform>  localTransforn,
            RefRO<UnitMover> unitMover,
            RefRW < PhysicsVelocity> physicsVelocity)
            in SystemAPI.Query<
            RefRW<LocalTransform>, 
            RefRO<UnitMover>,
            RefRW<PhysicsVelocity>>())
        {
            float3 moveDirection = unitMover.ValueRO.targetPosition - localTransforn.ValueRO.Position;
            moveDirection = math.normalize(moveDirection);

            localTransforn.ValueRW.Rotation = math.slerp(localTransforn.ValueRO.Rotation, 
                quaternion.LookRotation(moveDirection, math.up()), 
                SystemAPI.Time.DeltaTime * unitMover.ValueRO.rotationSpeed);

            physicsVelocity.ValueRW.Linear = moveDirection * unitMover.ValueRO.moveSpeed;
            //localTransforn.ValueRW.Position += moveDirection * moveSpeed.ValueRO.value * SystemAPI.Time.DeltaTime;
        }
    }
}
