namespace cpap_app.Importers;

public class Session : IComparable<Session>
{
	#region Public properties 
	
	public DateTime StartTime { get; set; }
	public DateTime EndTime   { get; set; }

	public TimeSpan Duration { get => EndTime - StartTime; }

	public List<Signal> Signals { get; set; } = new List<Signal>();
	
	#endregion 

	#region IComparable<Session> interface implementation 
		
	public int CompareTo( Session? other )
	{
		return StartTime.CompareTo( other?.StartTime );
	}
		
	#endregion 

	#region Base class overrides

	public override string ToString()
	{
		return $"{StartTime.ToShortDateString()}    {StartTime.ToLongTimeString()} - {EndTime.ToLongTimeString()}";
	}

	#endregion
}