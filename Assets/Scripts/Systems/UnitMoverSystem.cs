using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

partial struct UnitMoverSystem : ISystem
{
    public const float REACHED_TARGET_POSITION_SQ = 2f;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        UnitMoverJob unitMoverJob = new UnitMoverJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
        };
        unitMoverJob.ScheduleParallel();
        /*
        foreach ((
            RefRW<LocalTransform>  localTransform,
            RefRO<UnitMover> unitMover,
            RefRW < PhysicsVelocity> physicsVelocity)
            in SystemAPI.Query<
                RefRW<LocalTransform>, 
                RefRO<UnitMover>,
                RefRW<PhysicsVelocity>>())
        {
            float3 moveDirection = unitMover.ValueRO.targetPosition - localTransform.ValueRO.Position;
            moveDirection = math.normalize(moveDirection);

            localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation, 
                quaternion.LookRotation(moveDirection, math.up()), 
                SystemAPI.Time.DeltaTime * unitMover.ValueRO.rotationSpeed);

            physicsVelocity.ValueRW.Linear = moveDirection * unitMover.ValueRO.moveSpeed;
            //localTransforn.ValueRW.Position += moveDirection * moveSpeed.ValueRO.value * SystemAPI.Time.DeltaTime;
        
        }
        */
    }
}

[BurstCompile]
public partial struct UnitMoverJob : IJobEntity
{
    public float deltaTime;
    public void Execute(ref LocalTransform localTransform, in UnitMover unitMover, ref PhysicsVelocity physicsVelocity)
    {
        float3 moveDirection = unitMover.targetPosition - localTransform.Position;
        float reachedTargetDistances = UnitMoverSystem.REACHED_TARGET_POSITION_SQ;
        if(math.lengthsq(moveDirection) < reachedTargetDistances)
        {
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            return;
        }
        moveDirection = math.normalize(moveDirection);

        localTransform.Rotation = math.slerp(localTransform.Rotation,
            quaternion.LookRotation(moveDirection, math.up()),
            deltaTime  * unitMover.rotationSpeed);

        physicsVelocity.Linear = moveDirection * unitMover.moveSpeed;
        physicsVelocity.Angular = float3.zero;
    }
}
