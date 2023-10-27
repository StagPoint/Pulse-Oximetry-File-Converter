using cpap_app.Importers;

using OximetryConverter.Exporters;

const string LAST_IMPORT_FILENAME = "last-import.txt";

if( args.Length > 0 )
{
	var workingFolder = args[0].Trim( '"' );
	if( !Directory.Exists( workingFolder ) )
	{
		Console.WriteLine( "Argument is not a valid existing directory path" );
		return;
	}

	Console.WriteLine( $"Setting working path to {workingFolder}" );
	
	Environment.CurrentDirectory = workingFolder;
}

var exporter = new MedViewExporter();

var files = Directory.GetFiles( Directory.GetCurrentDirectory(), "*.csv" );
if( files.Length == 0 )
{
	Console.WriteLine( "No CSV files found in the current directory" );

	return;
}

// Only files newer than this timestamp will be converted 
var lastImportTime = DateTime.Today.AddDays( -180 );

// Check to see if we've left a "last import time" file in the source folder.
// If so, use the time from that file instead
if( File.Exists( LAST_IMPORT_FILENAME ) )
{
	lastImportTime = File.GetLastWriteTime( LAST_IMPORT_FILENAME );
}

int numberOfFilesConverted = 0;
int numberOfFilesFound     = 0;
int numberOfFilesSkipped   = 0;
int numberOfFilesFailed    = 0;
int numberOfUnhandledFiles = 0;

foreach( var filePath in files )
{
	var fileTimestamp = File.GetLastWriteTime( filePath );
	if( fileTimestamp < lastImportTime )
	{
		numberOfFilesSkipped += 1;
		continue;
	}
	
	var baseFilename  = Path.GetFileName( filePath );

	Console.WriteLine( $"Looking for importers for '{baseFilename}'" );
	numberOfFilesFound += 1;

	var importer = OximetryImporterRegistry.GetImporterForFile( Path.GetFileName( baseFilename ) );
	if( importer == null )
	{
		Console.WriteLine( $"   No suitable importer found for '{baseFilename}'" );
		numberOfUnhandledFiles += 1;
		
		continue;
	}

	using var file = File.OpenRead( filePath );

	var session = importer.Load( filePath, file );
	if( session == null )
	{
		Console.WriteLine( $"    Failed to import '{baseFilename}'" );
		numberOfFilesFailed += 1;
		
		continue;
	}

	if( !Directory.Exists( "Export" ) )
	{
		Console.WriteLine( $"Creating export directory" );
		Directory.CreateDirectory( "Export" );
	}

	var outputFilename = Path.Combine( "Export", exporter.GetSuggestedFilename( session ) );
	if( File.Exists( outputFilename ) )
	{
		Console.WriteLine( $"    File '{baseFilename}' has already been converted" );
		numberOfFilesSkipped += 1;
		
		continue;
	}
	
	Console.WriteLine( $"    Exporting '{outputFilename}' ({importer.FriendlyName})" );
	if( exporter.Export( session, outputFilename ) )
	{
		numberOfFilesConverted += 1;
	}
	else
	{
		numberOfFilesFailed += 1;
	}
}

if( numberOfFilesConverted > 0 )
{
	using var file = File.CreateText( LAST_IMPORT_FILENAME );
	file.WriteLine( DateTime.Now );
}

Console.WriteLine();
Console.WriteLine( "-----------------------------------------------------");
Console.WriteLine( $"Total number of files found: {numberOfFilesFound}" );
Console.WriteLine( $"Total converted: {numberOfFilesConverted}" );
Console.WriteLine( $"Total failed: {numberOfFilesFailed}" );
Console.WriteLine( $"Total skipped: {numberOfFilesSkipped}" );
Console.WriteLine( $"Unknown file formats: {numberOfUnhandledFiles}" );
Console.WriteLine( "-----------------------------------------------------");
Console.WriteLine();
