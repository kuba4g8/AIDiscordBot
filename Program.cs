using Discord;
using Discord.Net;
using Discord.WebSocket;
using DotNetEnv;
using Newtonsoft.Json.Bson;
using nwmCoTo;
using System.Threading.Channels;

class Program
{
    private static DiscordSocketClient _client;
    private static string token;
    private static AIHandler ollama;
    private static GatewayIntents intents =  GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.DirectMessages;

    private static bool isGenerating = false;

    private static DiscordSocketConfig config = new DiscordSocketConfig
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.MessageContent
    };


    static string loadToken()
    {
        var token = File.ReadAllText("token.txt");

        if (token==null)
        {
            Environment.Exit(0);
        }
        return token;
    }


    public static async Task Main()
    {
        ollama = new AIHandler();
        //await ollama.teachModel();


        _client = new DiscordSocketClient(config: config);

        _client.Log += Log;

        token = loadToken();

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        Console.WriteLine("GOTOWE!");
        _client.MessageReceived += HandleRecivedMessage;
        while (true)
        {
            
        }
    }


    public static async Task HandleRecivedMessage(SocketMessage message)
    {
        if (message.Author.IsBot || isGenerating)
            return;

        ISocketMessageChannel Channel = message.Channel;

        //DEBUG
        Console.WriteLine($"{Channel.CachedMessages.ToString()}");
        Console.WriteLine($"Kanał: {Channel.Name}");
        //DEBUG

        isGenerating = true;
        Task<string> response = ollama.responseToMsg(message.Content);

        response.Wait();
        isGenerating = false;

        await Channel.SendMessageAsync(text: response.Result);
        Console.WriteLine(response.Result);
    }

    private static Task Log(LogMessage log)
    {
        Console.WriteLine(log.ToString());

        return Task.CompletedTask;
    }
}


