using Kit.Sql.Attributes;

namespace Kit.Sql.Enums
{

	[StoreAsText]
	public enum NotifyTableChangedAction
	{
		Insert,
		Update,
		Delete,
	}
}
