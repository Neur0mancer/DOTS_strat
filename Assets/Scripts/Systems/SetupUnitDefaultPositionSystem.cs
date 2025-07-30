using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

partial struct SetupUnitDefaultPositionSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        foreach ((
            RefRO<LocalTransform> localTransform,
            RefRW<UnitMover> unitMover,
            RefRO<SetupUnitDefaultPosition> SetupUnitDefaultPosition,
            Entity entity)
            in SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRW<UnitMover>,
                RefRO<SetupUnitDefaultPosition>>().WithEntityAccess())
        {
            unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;
            entityCommandBuffer.RemoveComponent<SetupUnitDefaultPosition>(entity);
        }
    }
}
