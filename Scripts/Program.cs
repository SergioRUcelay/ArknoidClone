using Command_Interpreter;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;

HttpListener listener = new HttpListener();
Commands com = new Commands();

Action<HttpListener> Aclistener = Listener;

using var game = new Arkanoid_02.ArkaGame(Aclistener, listener);

// Adding functions to the CommandInterpreter.
try
{
	com.AddFunc("il", game.InfiniteLives, "Activate infinite lives");
	com.AddFunc("lv", game.SwichLevel, "Swich the current level");
	com.AddFunc("lv", game.GetLevel, "Show what the current level is");
	com.AddFunc("Seg", game.DrawShape, "Swich on-off the visivility of the segments");
	com.AddFunc("Brick", game.DrawBrick, "Swich on-off the visivility of the bricks");
	com.AddFunc("Cap", game.CaptureWindow, "Capture a screen of the game");

}
catch (Exception e)
{

	Console.WriteLine(e.ToString());
}


//--------- Configuration the console. ------------------

// Configure the port to be used.
listener.Prefixes.Add("http://localhost:7000/");
// Initialize the port.
listener.Start();// I initialize the listening
Console.WriteLine($"Websocket server started at ws://localhost:7000/");

async void Listener(HttpListener listener)
{
	HttpListenerContext context = await listener.GetContextAsync(); // I store in "context" what arrives through the listener.
	if (context.Request.IsWebSocketRequest)// If there is a request.
	{
		HttpListenerWebSocketContext wsConstext = await context.AcceptWebSocketAsync(null); // We access the Listener information
																							// We put it in a separate Task. This way, we don't lose responses (if we put an await, we might lose it).
		Task.Run(() => ReadWebSocket(wsConstext.WebSocket, com));
	}
	else // We only expect websocket requests. We return an error for non-websocket requests.
	{
		context.Response.StatusCode = 400;
		context.Response.Close();
	}
}

static async Task ReadWebSocket(WebSocket socket, Commands com)
{
	byte[] buffer = new byte[10234]; // Creating the byte array where we will save the request.
	while (socket.State == WebSocketState.Open) // As long as the websocket is open.
	{
		// We save in 'result' what we receive from the WebSocket, as an Array segment of what is in the 'buffer'
		WebSocketReceiveResult webCommand = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
		string command = Encoding.UTF8.GetString(buffer, 0, webCommand.Count);// We encode a String in UTF8

		CommandReply result = com.Command(command);
		if (result.Return is Texture2D buffa)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				buffa.SaveAsPng(ms, buffa.Width, buffa.Height);
				result.Return = Convert.ToBase64String(ms.ToArray());
			}
		}
		Console.WriteLine($"Message received: {command}");

		string xmlOutput = WriterOfNewXmlString(result);
		string response = XmlToText(xmlOutput);// Response string
		byte[] responseBytes = Encoding.UTF8.GetBytes(response); // Conversion of String to Array of Bytes
		await socket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None); // Enviamos la respuesta.
	}
}
static string XmlToText(string xml)
{
	XslCompiledTransform xslTranslater = new();
	try
	{
		var xslFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content/ConsoleFiles", "XSL_HTMLTFile.xslt");
		xslTranslater.Load(xslFile);
		using (StringWriter texOutput = new())
		{
			using XmlReader xmlMemory = XmlReader.Create(new StringReader(xml));
			xslTranslater.Transform(xmlMemory, null, texOutput);
			xml = texOutput.ToString();
		}
		xml = xml.Replace("\\x1b", "\x1b"); // Replace the \x1b with the escape character for color as .NET can not generate escape characters from Xslt.
	}
	catch (FileNotFoundException ex)
	{
		Console.WriteLine($"The {ex.FileName} not found at said location");
	}
	return xml;
}

static string WriterOfNewXmlString<T>(T newxmlmessage)
{
	// Declare the needed variables
	string consoleOutput;

	StringWriter logEntryWriter = new();
	XmlSerializer _serializerFor_LogEntry = new(typeof(T));

	_serializerFor_LogEntry.Serialize(logEntryWriter, newxmlmessage);

	consoleOutput = logEntryWriter.ToString();
	return consoleOutput;
}


game.Run();