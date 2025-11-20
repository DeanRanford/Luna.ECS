namespace Luna.ECS;

public abstract class System(IFilter filter)
{
    public bool Destroyed { get; private set; }
    public bool Enabled { get; set; } = true;
    public IFilter Filter { get; set; } = filter;
    public int Priority { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    //Supressed because it gets assigned to at object add into world
    public World World { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public List<int> Entities { get; set; } = [];
    public void Destroy() => this.Destroyed = true;

    public void Update(float deltaTime)
    {
        this.PreProcess(deltaTime);
        foreach (var entity in this.Entities.ToList())
        {
            if (this.Destroyed || !this.Enabled)
            {
                break;
            }

            if (this.World.IsEntityDestroyed(entity))
            {
                continue;
            }

            this.Process(entity, deltaTime);
        }
        if (this.Destroyed || !this.Enabled)
        {
            return;
        }

        this.PostProcess(deltaTime);
    }
    public abstract void Init();
    public abstract void OnAdd(int entity);
    public abstract void OnRemove(int entity);
    public abstract void PreProcess(float deltaTime);
    public abstract void Process(int entity, float deltaTime);
    public abstract void PostProcess(float deltaTime);
}
