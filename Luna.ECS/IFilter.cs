namespace Luna.ECS;

public interface IFilter
{
    public bool Matches(List<IComponent> components);
}
