using cpap_app.Importers;

namespace OximetryConverter.Exporters;

public class MedViewExporter : IOximetryExporter
{
	public string FriendlyName { get => "OSCAR-Compatible .dat file"; }

	public string FileExtension { get => "dat"; }

	public string FilenameFilter { get => "MedView Data File|*.dat"; }

	public string GetSuggestedFilename( Session session )
	{
		return $"{session.StartTime:u}.dat".Replace( ':', '-' ).Replace( "Z", "" );
	}

	public bool Export( Session data, string exportFullPath )
	{
		// Note: Even though we're exporting this for OSCAR, it appears that there's somehow a mismatch in 
		// the timing of the readings as far as OSCAR is concerned, so you may still need to match the 
		// imported oximetrySession to a CPAP oximetrySession during import. 
		var currentTime = data.StartTime;

		// NOTE: Because this code was borrowed from a different project and most of the functionality of the
		// borrowed classes was deleted until only absolute what was needed for this project remained with no
		// consideration of future upkeep or maintenance, it uses hard-coded signal index values.
		// It's dumb, but it'll have to do.
		var oxygen = data.Signals[ 0 ];
		var pulse  = data.Signals[ 1 ];

		var recordCount    = data.Signals[ 0 ].Samples.Count;
		var sampleInterval = 1.0 / oxygen.FrequencyInHz;

		try
		{
			using( var file = File.Create( exportFullPath ) )
			{
				var writer = new BinaryWriter( file );

				// MedView .dat file (ChoiceMMed MD300B, MD300KI, MD300I, MD300W1, MD300C318, MD2000A)
				// Format:
				// Bytes  0	   (1  2)
				//        id   count
				// n*11   0  1  2  3  4  5  6  7  8  9  10
				//        0  0  id yr mm dd hh mm ss o2 pulse
				writer.Write( (byte)0x00 );
				writer.Write( (ushort)oxygen.Samples.Count );

				int recordsWritten = 0;

				for( int index = 0; index < recordCount; index++ )
				{
					// The output file format requires that all samples are in 1-second intervals, so we'll duplicate
					// samples if the session's sample rate is larger than one per second. 
					for( int i = 0; i < sampleInterval; i++ )
					{
						// 0  0  id yr mm dd hh mm ss o2 pulse
						writer.Write( (byte)0x00 );
						writer.Write( (byte)0x00 );
						writer.Write( (byte)0xCC );

						writer.Write( (byte)(currentTime.Year - 2000) );
						writer.Write( (byte)currentTime.Month );
						writer.Write( (byte)currentTime.Day );
						writer.Write( (byte)currentTime.Hour );
						writer.Write( (byte)currentTime.Minute );
						writer.Write( (byte)currentTime.Second );

						writer.Write( (byte)oxygen.Samples[ index ] );
						writer.Write( (byte)pulse.Samples[ index ] );

						currentTime    =  currentTime.AddSeconds( 1 );
						recordsWritten += 1;
					}
				}

				// Store the *actual* number of records written
				writer.BaseStream.Position = 1;
				writer.Write( (ushort)recordsWritten );
			}
		}
		catch( IOException e )
		{
			Console.WriteLine( e.Message );
			return false;
		}

		return true;
	}
}
