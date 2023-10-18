namespace cpap_app.Importers;

public class Signal
{
	/// <summary>
	/// The name of the channel, such as SaO2, Flow, Mask Pressure, etc.
	/// </summary>
	public string Name { get; set; } = "";

	/// <summary>
	/// Returns the number of samples per second contained in this Signal.
	/// </summary>
	public double FrequencyInHz { get; set; }

	/// <summary>
	/// The minimum value that any sample can potentially have for this type of Signal
	/// </summary>
	public double MinValue { get; set; }

	/// <summary>
	/// The maximum value that any sample can potentially have for this type of Signal
	/// </summary>
	public double MaxValue { get; set; }

	/// <summary>
	/// The signal data for this session 
	/// </summary>
	public List<double> Samples { get; internal set; } = new List<double>();

	/// <summary>
	/// The time when recording of this signal was started 
	/// </summary>
	public DateTime StartTime { get; set; }

	/// <summary>
	/// The time when recording of this signal was stopped 
	/// </summary>
	public DateTime EndTime { get; set; }

	/// <summary>
	/// The duration of this recording session
	/// </summary>
	public TimeSpan Duration { get => EndTime - StartTime; }

		#region Base class overrides

	public override string ToString()
	{
		return Name;
	}

		#endregion
}
