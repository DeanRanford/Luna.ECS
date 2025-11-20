namespace Luna.ECS;

public class World
{
    private int _entityCounter;
    private int EntityCounter => ++this._entityCounter;
    private List<int> DestroyedEntities { get; set; } = [];
    public Dictionary<int, List<IComponent>> Entities { get; private set; } = [];
    public List<System> Systems { get; private set; } = [];

    private void RemoveDestroyedEntities()
    {
        foreach (var entity in this.DestroyedEntities)
        {
            this.Entities.Remove(entity);
            foreach (var system in this.Systems)
            {
                system.Entities.Remove(entity);
            }
        }
        this.DestroyedEntities = [];
    }

    private void UpdateEntity(int entity)
    {
        foreach (var system in this.Systems)
        {
            this.UpdateSystemEntity(system, entity);
        }
    }

    private void UpdateSystem(System system)
    {
        foreach (var entity in this.Entities)
        {
            this.UpdateSystemEntity(system, entity.Key);
        }
    }

    private void UpdateSystemEntity(System system, int entity)
    {
        var matches = system.Filter.Matches(this.Entities[entity]);
        var contains = system.Entities.Contains(entity);
        if (!contains && matches) // Entity needs to be registered in system
        {
            system.Entities.Add(entity);
            system.OnAdd(entity);
        }
        else if (contains && !matches) // Entity is no longer needed in system
        {
            system.Entities.Remove(entity);
            system.OnRemove(entity);
        }
    }

    public bool AddComponent(int entity, IComponent component)
    {
        if (!this.Entities.TryGetValue(entity, out var value))
        {
            return false;
        }

        value.Add(component);
        this.UpdateEntity(entity);
        return true;
    }

    public List<System> GetEntitySystems(int entity)
    {
        List<System> entitySystems = [];
        foreach (var system in this.Systems)
        {
            if (system.Entities.Contains(entity))
            {
                entitySystems.Add(system);
            }
        }
        return entitySystems;
    }

    public void RemoveSystem(System system) => this.Systems.Remove(system);

    public void AddSystem(System system)
    {
        if (this.Systems.Contains(system))
        {
            return;
        }

        this.Systems.Add(system);
        system.World = this;
        this.UpdateSystem(system);
        system.Init();
    }

    public int CreateEntity()
    {
        var id = this.EntityCounter;
        this.Entities.Add(id, []);
        return id;
    }

    public List<IComponent> GetComponent(int entity)
    {
        if (!this.Entities.TryGetValue(entity, out var components))
        {
            return [];
        }

        return [.. components];
    }

    public List<T> GetComponent<T>(int entity) where T : IComponent
    {
        if (!this.Entities.TryGetValue(entity, out var value))
        {
            return [];
        }

        return [.. value.OfType<T>()];
    }

    public bool IsEntityDestroyed(int entity) => this.DestroyedEntities.Contains(entity);

    public void RemoveComponent(int entity, IComponent component)
    {
        if (!this.Entities.TryGetValue(entity, out var value))
        {
            return;
        }

        value.Remove(component);
        this.UpdateEntity(entity);
    }

    public void RemoveEntity(int entity)
    {
        if (this.Entities.Remove(entity))
        {
            this.DestroyedEntities.Add(entity);
        }
    }

    public void Update(float deltaTime)
    {
        this.RemoveDestroyedEntities();
        foreach (var system in this.Systems.ToList())
        {
            if (system.Destroyed)
            {
                this.Systems.Remove(system);
                continue;
            }
            if (!system.Enabled)
            {
                continue;
            }

            system.Update(deltaTime);
        }
    }
}
