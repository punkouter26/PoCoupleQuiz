using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace PoCoupleQuiz.Core.Telemetry
{
    /// <summary>
    /// Central telemetry instrumentation for Po.CoupleQuiz application.
    /// Provides ActivitySource for distributed tracing and Meter for custom metrics.
    /// Follows OpenTelemetry semantic conventions for consistent observability.
    /// </summary>
    public static class PoCoupleQuizTelemetry
    {
        /// <summary>
        /// Application name used for telemetry identification.
        /// </summary>
        public const string ServiceName = "Po.CoupleQuiz";

        /// <summary>
        /// Application version for telemetry correlation.
        /// </summary>
        public const string ServiceVersion = "1.0.0";

        /// <summary>
        /// ActivitySource for custom distributed tracing spans.
        /// Use this to create Activities for business-critical operations.
        /// </summary>
        public static readonly ActivitySource ActivitySource = new(ServiceName, ServiceVersion);

        /// <summary>
        /// Meter for custom business metrics.
        /// Use this to track counters, gauges, and histograms.
        /// </summary>
        public static readonly Meter Meter = new(ServiceName, ServiceVersion);

        // ========================================
        // CUSTOM METRICS (Instruments)
        // ========================================

        /// <summary>
        /// Counter: Total number of games created.
        /// Tags: difficulty (easy/medium/hard)
        /// </summary>
        public static readonly Counter<long> GamesCreated = Meter.CreateCounter<long>(
            name: "po.couplequiz.games.created",
            unit: "games",
            description: "Total number of games created, partitioned by difficulty level");

        /// <summary>
        /// Counter: Total number of games completed.
        /// Tags: difficulty, completion_status (completed/abandoned)
        /// </summary>
        public static readonly Counter<long> GamesCompleted = Meter.CreateCounter<long>(
            name: "po.couplequiz.games.completed",
            unit: "games",
            description: "Total number of games completed or abandoned");

        /// <summary>
        /// Histogram: Game duration in seconds.
        /// Tags: difficulty, player_count
        /// </summary>
        public static readonly Histogram<double> GameDuration = Meter.CreateHistogram<double>(
            name: "po.couplequiz.game.duration",
            unit: "seconds",
            description: "Duration of completed games in seconds");

        /// <summary>
        /// Histogram: Question generation latency in milliseconds.
        /// Tags: question_source (openai/mock), difficulty
        /// </summary>
        public static readonly Histogram<double> QuestionGenerationLatency = Meter.CreateHistogram<double>(
            name: "po.couplequiz.questions.generation_latency",
            unit: "milliseconds",
            description: "Time taken to generate questions via AI or mock service");

        /// <summary>
        /// Counter: Total number of questions generated.
        /// Tags: question_source (openai/mock), difficulty
        /// </summary>
        public static readonly Counter<long> QuestionsGenerated = Meter.CreateCounter<long>(
            name: "po.couplequiz.questions.generated",
            unit: "questions",
            description: "Total number of questions generated");

        /// <summary>
        /// Gauge: Current number of active players.
        /// Observable gauge that reports current active player count.
        /// </summary>
        public static ObservableGauge<int> ActivePlayers { get; private set; } = null!;

        /// <summary>
        /// Counter: Total player answers submitted.
        /// Tags: is_correct (true/false), difficulty
        /// </summary>
        public static readonly Counter<long> PlayerAnswers = Meter.CreateCounter<long>(
            name: "po.couplequiz.answers.submitted",
            unit: "answers",
            description: "Total number of player answers submitted");

        /// <summary>
        /// Histogram: Player answer time in seconds.
        /// Tags: difficulty, player_type (king/regular)
        /// </summary>
        public static readonly Histogram<double> AnswerTime = Meter.CreateHistogram<double>(
            name: "po.couplequiz.answer.time",
            unit: "seconds",
            description: "Time taken by players to submit answers");

        /// <summary>
        /// Counter: Storage operations executed.
        /// Tags: operation (create/read/update/delete), table_name, status (success/failure)
        /// </summary>
        public static readonly Counter<long> StorageOperations = Meter.CreateCounter<long>(
            name: "po.couplequiz.storage.operations",
            unit: "operations",
            description: "Total number of Azure Table Storage operations");

        /// <summary>
        /// Histogram: Storage operation latency in milliseconds.
        /// Tags: operation, table_name
        /// </summary>
        public static readonly Histogram<double> StorageLatency = Meter.CreateHistogram<double>(
            name: "po.couplequiz.storage.latency",
            unit: "milliseconds",
            description: "Latency of Azure Table Storage operations");

        // ========================================
        // INITIALIZATION
        // ========================================

        /// <summary>
        /// Initializes observable metrics (gauges).
        /// Call this during application startup to register observable callbacks.
        /// </summary>
        /// <param name="getActivePlayerCount">Callback to retrieve current active player count</param>
        public static void Initialize(Func<int> getActivePlayerCount)
        {
            ActivePlayers = Meter.CreateObservableGauge<int>(
                name: "po.couplequiz.players.active",
                observeValue: getActivePlayerCount,
                unit: "players",
                description: "Current number of active players in games");
        }
    }
}
