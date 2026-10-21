namespace Workoutly.Mobile.Services;

public sealed record ExerciseVisual(string ImageSource, string? QuickInstruction);

/// <summary>
/// Associa as ilustrações locais licenciadas do catálogo inicial aos IDs estáveis do banco.
/// Exercícios privados não recebem uma imagem ou instrução presumida.
/// </summary>
public static class ExerciseVisuals
{
    private const string SquatId = "e1000000-0000-0000-0000-000000000001";
    private static readonly IReadOnlyDictionary<string, ExerciseVisual> Catalogue =
        new Dictionary<string, ExerciseVisual>(StringComparer.Ordinal)
        {
            [SquatId] = new("barbell_squat.svg", "Apoie a barra com estabilidade, flexione quadris e joelhos com os pés firmes e retorne de forma controlada."),
            ["e1000000-0000-0000-0000-000000000002"] = new("leg_press.svg", "Mantenha a lombar apoiada, desça a plataforma com controle e empurre sem travar o movimento."),
            ["e1000000-0000-0000-0000-000000000003"] = new("leg_extension.svg", "Ajuste o equipamento, estenda os joelhos com controle e retorne devagar à posição inicial."),
            ["e1000000-0000-0000-0000-000000000004"] = new("bench_press.svg", "Com costas e pés apoiados, desça a barra com controle em direção ao peito e empurre mantendo a estabilidade."),
            ["e1000000-0000-0000-0000-000000000005"] = new("bent_over_row.svg", "Incline o tronco com a coluna neutra, puxe a barra em direção ao abdômen e controle a descida."),
            ["e1000000-0000-0000-0000-000000000006"] = new("lat_pulldown.svg", "Sente-se firme, traga a barra em direção à parte alta do peito e retorne sem balançar o tronco."),
            ["e1000000-0000-0000-0000-000000000007"] = new("dumbbell_shoulder_press.svg", "Comece com os halteres na altura dos ombros e eleve-os de forma controlada, mantendo o tronco estável."),
            ["e1000000-0000-0000-0000-000000000008"] = new("barbell_curl.svg", "Mantenha os cotovelos próximos ao corpo, flexione os braços e abaixe a barra de forma controlada."),
            ["e1000000-0000-0000-0000-000000000009"] = new("triceps_pushdown.svg", "Mantenha os cotovelos junto ao corpo, estenda os braços para baixo e retorne devagar."),
            ["e1000000-0000-0000-0000-000000000010"] = new("calf_raise.svg", "Apoie-se com estabilidade, eleve os calcanhares e desça de forma controlada."),
        };

    public static ExerciseVisual? Find(string exerciseId) =>
        Catalogue.TryGetValue(exerciseId, out var visual) ? visual : null;
}
