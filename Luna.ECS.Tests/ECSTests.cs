namespace Luna.ECS.Tests;

using Luna.ECS;

[TestClass]
public sealed class ECSTests
{
    [TestMethod]
    public void ECSTest1()
    {
        var ecs = new World();
        var system = new System1(new HasFilter([typeof(Component1)]));
        ecs.AddSystem(system);
        var entity = ecs.CreateEntity();
        var component = new Component1();
        ecs.AddComponent(entity, component);
        var findComponent = ecs.GetComponent<Component1>(entity).First();
        Assert.AreEqual(1234, findComponent.Value);
        ecs.Update(0.1f);
        findComponent = ecs.GetComponent<Component1>(entity).First();
        Assert.AreEqual(4321, findComponent.Value);
        Assert.IsTrue(system.DidAdd);
        Assert.IsTrue(system.DidInit);
        Assert.IsTrue(system.DidPreProcess);
        Assert.IsTrue(system.DidProcess);
        Assert.IsTrue(system.DidPostProcess);
        ecs.RemoveEntity(entity);
        Assert.IsFalse(system.DidRemove);

    }

    public class Component1 : IComponent
    {
        public int Value { get; set; } = 1234;
    }

    public class System1(IFilter filter) : System(filter)
    {
        public bool DidAdd;
        public bool DidInit;
        public bool DidRemove;
        public bool DidPostProcess;
        public bool DidPreProcess;
        public bool DidProcess;

        public override void Init() => this.DidInit = true;

        public override void OnAdd(int entity) => this.DidAdd = true;

        public override void OnRemove(int entity) => this.DidRemove = true;

        public override void PostProcess(float deltaTime) => this.DidPostProcess = true;

        public override void PreProcess(float deltaTime) => this.DidPreProcess = true;

        public override void Process(int entity, float deltaTime)
        {
            this.DidProcess = true;
            var component = this.World.GetComponent<Component1>(entity).First();
            component.Value = 4321;
        }
    }
}
