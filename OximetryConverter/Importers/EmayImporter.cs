using System.Text;

namespace cpap_app.Importers;

public class EmayImporterCSV : IOximetryImporter
{
	// NOTES:
	// Sample filename: "EMAY SpO2-20230713-045916.csv"
	// Header: "Date,Time,SpO2(%),PR(bpm)"
	
	#region Public properties 
	
	public string FriendlyName  { get => "EMAY Pulse Oximeter"; }

	public string Source { get => "EMAY"; }

	public string FileExtension { get => "csv"; }

	public string FileTypeFilters { get; } = "EMAY Pulse Oximeter File|EMAY*.csv";

	public string FilenameMatchPattern { get => @"EMAY SpO2-\d{8}-\d{6}\.csv"; }
	
	#endregion 
	
	#region Private fields 

	private static string[] _expectedHeaders = { "Date", "Time", "SpO2(%)", "PR(bpm)" };
	
	#endregion 
	
	#region Public functions 

	public Session? Load( string filename, Stream stream )
	{
		using( var reader = new StreamReader( stream, Encoding.Default, leaveOpen: true ) )
		{
			var firstLine = reader.ReadLine();
			if( string.IsNullOrEmpty( firstLine ) )
			{
				return null;
			}

			var headers = firstLine.Split( ',' );
			if( !_expectedHeaders.SequenceEqual( headers ) )
			{
				return null;
			}

			Signal oxygen = new Signal
			{
				FrequencyInHz     = 1,
				MinValue          = 80,
				MaxValue          = 100,
			};
		
			Signal pulse = new Signal
			{
				FrequencyInHz     = 1,
				MinValue          = 60,
				MaxValue          = 120,
			};

			Session session = new()
			{
				Signals = { oxygen, pulse },
			};

			bool isStartRecord = true;
			byte lastGoodOxy   = 0;
			byte lastGoodHR = 0;

			while( !reader.EndOfStream )
			{
				var line = reader.ReadLine();

				if( string.IsNullOrEmpty( line ) )
				{
					return null;
				}

				var lineData = line.Split( ',' );
				
				var dateTimeText = $"{lineData[ 0 ]} {lineData[ 1 ]}";
				if( !DateTime.TryParse( dateTimeText, out DateTime dateTimeValue ) )
				{
					return null;
				}
				
				if( isStartRecord )
				{
					session.StartTime = dateTimeValue;
					isStartRecord    = false;
				}

				session.EndTime = dateTimeValue;

				if( byte.TryParse( lineData[ 2 ], out var oxy ) )
				{
					oxygen.Samples.Add( oxy );
					lastGoodOxy = oxy;
				}
				else
				{
					// EMAY pulse oximeters may leave the SpO2 and PR fields blank to indicate an invalid reading
					// TODO: How to handle invalid records in imported files. Split the file, duplicate last good reading, etc?
					oxygen.Samples.Add( lastGoodOxy );
				}

				if( byte.TryParse( lineData[ 3 ], out var hr ) )
				{
					pulse.Samples.Add( hr );
					lastGoodHR = hr;
				}
				else
				{
					// EMAY pulse oximeters may leave the SpO2 and PR fields blank to indicate an invalid reading
					// TODO: How to handle invalid records in imported files. Split the file, duplicate last good reading, etc?
					pulse.Samples.Add( lastGoodHR );
				}
			}
			
			oxygen.StartTime = pulse.StartTime = session.StartTime;
			oxygen.EndTime   = pulse.EndTime   = session.EndTime;

			return session;
		}
	}
	
	#endregion 
}
