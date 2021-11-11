namespace essentialMix.Data.Model
{
	public interface IEntity
	{
	}

	public interface IEntity<T> : IEntity
	{
		T Id { get; set; }
	}
}