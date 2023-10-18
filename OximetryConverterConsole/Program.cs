﻿using System.Diagnostics;

using cpap_app.Importers;

using OximetryConverter.Exporters;

var exporter = new MedViewExporter();

var files = Directory.GetFiles( Directory.GetCurrentDirectory(), "*.csv" );
if( files.Length == 0 )
{
	Console.WriteLine( "No CSV files found in the current directory" );
	Console.ReadKey();

	return;
}

int numberOfFilesConverted = 0;
int numberOfFilesFound     = 0;
int numberOfFilesSkipped   = 0;
int numberOfFilesFailed    = 0;
int numberOfUnhandledFiles = 0;

foreach( var filePath in files )
{
	var baseFilename = Path.GetFileName( filePath );

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
	exporter.Export( session, outputFilename );
	
	numberOfFilesConverted += 1;
}

Console.WriteLine();
Console.WriteLine( $"Total number of files found: {numberOfFilesFound}" );
Console.WriteLine( $"Total failed: {numberOfFilesFailed}" );
Console.WriteLine( $"Total skipped: {numberOfFilesSkipped}" );
Console.WriteLine( $"Total converted: {numberOfFilesConverted}" );
Console.WriteLine( $"Unknown file formats: {numberOfUnhandledFiles}" );

Console.WriteLine();
Console.Write( "Press ENTER to exit...");
Console.ReadKey();