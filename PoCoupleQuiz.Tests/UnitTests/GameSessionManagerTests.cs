using Microsoft.Extensions.Logging.Abstractions;
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;
using Xunit;

namespace PoCoupleQuiz.Tests.UnitTests;

/// <summary>
/// Unit tests for <see cref="GameSessionManager"/>.
/// Tests are pure in-memory — no networking or external dependencies.
/// </summary>
public class GameSessionManagerTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static GameSessionManager CreateManager() =>
        new(NullLogger<GameSessionManager>.Instance);

    private static GameSession CreateLobbyHelper(
        IGameSessionManager mgr,
        string hostConnId = "conn-host",
        string hostName = "Alice",
        DifficultyLevel difficulty = DifficultyLevel.Medium) =>
        mgr.CreateLobby(hostConnId, hostName, difficulty);

    // ── CreateLobby ──────────────────────────────────────────────────────────

    [Fact]
    public void CreateLobby_Returns_Session_With_Valid_GameCode()
    {
        var mgr = CreateManager();

        var session = CreateLobbyHelper(mgr);

        Assert.NotNull(session);
        Assert.False(string.IsNullOrWhiteSpace(session.GameCode));
        Assert.Equal(7, session.GameCode.Length); // e.g. "XK9-Z4T"
        Assert.Contains('-', session.GameCode);
    }

    [Fact]
    public void CreateLobby_Sets_Host_And_State()
    {
        var mgr = CreateManager();

        var session = CreateLobbyHelper(mgr, "conn-1", "Bob");

        Assert.Equal(SessionState.Waiting, session.State);
        Assert.Equal("conn-1", session.HostConnectionId);
        Assert.Equal("Bob", session.HostName);
        Assert.Single(session.Players);
        Assert.Equal("Bob", session.Players[0].Name);
    }

    [Fact]
    public void CreateLobby_Different_Codes_Each_Call()
    {
        var mgr = CreateManager();
        var codes = new HashSet<string>();

        for (int i = 0; i < 20; i++)
            codes.Add(mgr.CreateLobby($"conn-{i}", $"Player{i}", DifficultyLevel.Easy).GameCode);

        // Statistically safe: 20 random 6-char codes should all be unique
        Assert.Equal(20, codes.Count);
    }

    // ── JoinLobby ────────────────────────────────────────────────────────────

    [Fact]
    public void JoinLobby_Adds_Player_To_Existing_Lobby()
    {
        var mgr = CreateManager();
        var host = CreateLobbyHelper(mgr, hostName: "Host");

        var joined = mgr.JoinLobby(host.GameCode, "conn-2", "Guest");

        Assert.NotNull(joined);
        Assert.Equal(2, joined!.Players.Count);
        Assert.Contains(joined.Players, p => p.Name == "Guest");
    }

    [Fact]
    public void JoinLobby_Returns_Null_For_Unknown_GameCode()
    {
        var mgr = CreateManager();

        var result = mgr.JoinLobby("ZZZZZZ", "conn-x", "Nobody");

        Assert.Null(result);
    }

    [Fact]
    public void JoinLobby_Returns_Null_When_Game_Already_Started()
    {
        var mgr = CreateManager();
        var host = CreateLobbyHelper(mgr, "conn-h", "Host");
        mgr.JoinLobby(host.GameCode, "conn-g", "Guest");
        mgr.StartGame(host.GameCode, "Test question?", "Relationships");

        var late = mgr.JoinLobby(host.GameCode, "conn-late", "Latecomer");

        Assert.Null(late);
    }

    // ── GetSession / GetSessionByConnection ───────────────────────────────────

    [Fact]
    public void GetSession_Returns_Session_For_Valid_Code()
    {
        var mgr = CreateManager();
        var host = CreateLobbyHelper(mgr);

        var found = mgr.GetSession(host.GameCode);

        Assert.NotNull(found);
        Assert.Equal(host.GameCode, found!.GameCode);
    }

    [Fact]
    public void GetSession_Returns_Null_For_Unknown_Code()
    {
        var mgr = CreateManager();
        Assert.Null(mgr.GetSession("XXXXXX"));
    }

    [Fact]
    public void GetSessionByConnection_Returns_Session_For_Host()
    {
        var mgr = CreateManager();
        CreateLobbyHelper(mgr, "conn-host");

        var found = mgr.GetSessionByConnection("conn-host");

        Assert.NotNull(found);
    }

    [Fact]
    public void LobbyExists_Returns_True_For_Waiting_Lobby()
    {
        var mgr = CreateManager();
        var host = CreateLobbyHelper(mgr);
        Assert.True(mgr.LobbyExists(host.GameCode));
    }

    [Fact]
    public void LobbyExists_Returns_False_For_Started_Game()
    {
        var mgr = CreateManager();
        var host = CreateLobbyHelper(mgr, "conn-h", "Host");
        mgr.JoinLobby(host.GameCode, "conn-g", "Guest");
        mgr.StartGame(host.GameCode, "Q?", "Relationships");

        Assert.False(mgr.LobbyExists(host.GameCode));
    }

    // ── RemovePlayer ─────────────────────────────────────────────────────────

    [Fact]
    public void RemovePlayer_Returns_Session_And_Decrements_Count()
    {
        var mgr = CreateManager();
        var host = CreateLobbyHelper(mgr, "conn-h", "Host");
        mgr.JoinLobby(host.GameCode, "conn-g", "Guest");

        var session = mgr.RemovePlayer("conn-g", out bool empty);

        Assert.NotNull(session);
        Assert.False(empty);
        Assert.Single(session!.Players);
    }

    [Fact]
    public void RemovePlayer_Sets_SessionEmpty_When_Last_Player_Leaves()
    {
        var mgr = CreateManager();
        CreateLobbyHelper(mgr, "conn-only", "Solo");

        mgr.RemovePlayer("conn-only", out bool empty);

        Assert.True(empty);
    }

    [Fact]
    public void RemovePlayer_Returns_Null_For_Unknown_Connection()
    {
        var mgr = CreateManager();

        var session = mgr.RemovePlayer("no-such-conn", out _);

        Assert.Null(session);
    }

    // ── StartGame ────────────────────────────────────────────────────────────

    [Fact]
    public void StartGame_Transitions_State_To_InProgress()
    {
        var mgr = CreateManager();
        var host = CreateLobbyHelper(mgr, "conn-h", "Host");
        mgr.JoinLobby(host.GameCode, "conn-g", "Guest");

        var game = mgr.StartGame(host.GameCode, "First question?", "Relationships");

        var session = mgr.GetSession(host.GameCode)!;
        Assert.Equal(SessionState.InProgress, session.State);
        Assert.NotNull(game);
        Assert.NotNull(session.ActiveGame);
    }

    [Fact]
    public void StartGame_Sets_First_Question()
    {
        var mgr = CreateManager();
        var host = CreateLobbyHelper(mgr, "conn-h", "Host");
        mgr.JoinLobby(host.GameCode, "conn-g", "Guest");

        mgr.StartGame(host.GameCode, "Who does the dishes?", "Daily");

        var session = mgr.GetSession(host.GameCode)!;
        Assert.Equal("Who does the dishes?", session.CurrentQuestion?.Question);
    }

    [Fact]
    public void StartGame_Creates_RoundAnswers_Slot_For_Each_Player()
    {
        var mgr = CreateManager();
        var host = CreateLobbyHelper(mgr, "conn-h", "Host");
        mgr.JoinLobby(host.GameCode, "conn-g", "Guest");

        mgr.StartGame(host.GameCode, "Q?", "Relationships");

        var session = mgr.GetSession(host.GameCode)!;
        Assert.Contains("Host", session.RoundAnswers.Keys);
        Assert.Contains("Guest", session.RoundAnswers.Keys);
    }

    // ── RecordAnswer ─────────────────────────────────────────────────────────

    [Fact]
    public void RecordAnswer_Returns_False_Until_All_Players_Answer()
    {
        var mgr = CreateManager();
        var host = CreateLobbyHelper(mgr, "conn-h", "Host");
        mgr.JoinLobby(host.GameCode, "conn-g", "Guest");
        mgr.StartGame(host.GameCode, "Q?", "Relationships");

        bool done = mgr.RecordAnswer(host.GameCode, "Host", "My answer");

        Assert.False(done);
    }

    [Fact]
    public void RecordAnswer_Returns_True_When_All_Players_Answer()
    {
        var mgr = CreateManager();
        var host = CreateLobbyHelper(mgr, "conn-h", "Host");
        mgr.JoinLobby(host.GameCode, "conn-g", "Guest");
        mgr.StartGame(host.GameCode, "Q?", "Relationships");

        mgr.RecordAnswer(host.GameCode, "Host", "Answer A");
        bool allDone = mgr.RecordAnswer(host.GameCode, "Guest", "Answer B");

        Assert.True(allDone);
    }

    // ── AdvanceRound ─────────────────────────────────────────────────────────

    [Fact]
    public void AdvanceRound_Increments_CurrentRound()
    {
        var mgr = CreateManager();
        var host = CreateLobbyHelper(mgr, "conn-h", "Host");
        mgr.JoinLobby(host.GameCode, "conn-g", "Guest");
        mgr.StartGame(host.GameCode, "Q?", "Relationships");

        var game = mgr.AdvanceRound(host.GameCode);

        Assert.Equal(1, game.CurrentRound);
    }

    [Fact]
    public void AdvanceRound_Resets_RoundAnswers()
    {
        var mgr = CreateManager();
        var host = CreateLobbyHelper(mgr, "conn-h", "Host");
        mgr.JoinLobby(host.GameCode, "conn-g", "Guest");
        mgr.StartGame(host.GameCode, "Q?", "Relationships");
        mgr.RecordAnswer(host.GameCode, "Host", "A");
        mgr.RecordAnswer(host.GameCode, "Guest", "B");

        mgr.AdvanceRound(host.GameCode);

        var session = mgr.GetSession(host.GameCode)!;
        Assert.All(session.RoundAnswers.Values, v => Assert.Null(v));
    }

    // ── ApplyRoundScores ─────────────────────────────────────────────────────

    [Fact]
    public void ApplyRoundScores_Updates_Player_Scores()
    {
        var mgr = CreateManager();
        var host = CreateLobbyHelper(mgr, "conn-h", "Host");
        mgr.JoinLobby(host.GameCode, "conn-g", "Guest");
        mgr.StartGame(host.GameCode, "Q?", "Relationships");

        mgr.ApplyRoundScores(host.GameCode, new Dictionary<string, int>
        {
            ["Host"] = 10,
            ["Guest"] = 5
        });

        var game = mgr.GetSession(host.GameCode)!.ActiveGame!;
        Assert.Equal(10, game.Players.First(p => p.Name == "Host").Score);
        Assert.Equal(5,  game.Players.First(p => p.Name == "Guest").Score);
    }

    // ── PromoteNextHost ───────────────────────────────────────────────────────

    [Fact]
    public void PromoteNextHost_Assigns_New_Host_When_Current_Host_Left()
    {
        var mgr = CreateManager();
        var host = CreateLobbyHelper(mgr, "conn-h", "Host");
        mgr.JoinLobby(host.GameCode, "conn-g", "Guest");
        mgr.RemovePlayer("conn-h", out _);

        var newHost = mgr.PromoteNextHost(host.GameCode);

        Assert.Equal("Guest", newHost);
        var session = mgr.GetSession(host.GameCode)!;
        Assert.Equal("conn-g", session.HostConnectionId);
    }

    [Fact]
    public void PromoteNextHost_Returns_Null_When_No_Players_Remain()
    {
        var mgr = CreateManager();
        var host = CreateLobbyHelper(mgr, "conn-h", "Host");
        mgr.RemovePlayer("conn-h", out _);

        var newHost = mgr.PromoteNextHost(host.GameCode);

        Assert.Null(newHost);
    }

    // ── FinishGame ────────────────────────────────────────────────────────────

    [Fact]
    public void FinishGame_Sets_State_To_Finished()
    {
        var mgr = CreateManager();
        var host = CreateLobbyHelper(mgr, "conn-h", "Host");
        mgr.JoinLobby(host.GameCode, "conn-g", "Guest");
        mgr.StartGame(host.GameCode, "Q?", "Relationships");

        mgr.FinishGame(host.GameCode);

        var session = mgr.GetSession(host.GameCode);
        // Session may still exist briefly before auto-cleanup; just verify state if present
        if (session is not null)
            Assert.Equal(SessionState.Finished, session.State);
    }
}
