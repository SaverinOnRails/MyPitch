namespace MyPitch.Tests;

using MyPitch.Models;
public class MusicTheoryTests
{
    //Hard circle of fifths for all notes
    private static readonly Dictionary<Key, string[]> ExpectedByTonic = new()
    {
        [Key.C] = new[] { "C", "G", "D", "A", "E", "B", "Gflat", "Dflat", "Aflat", "Eflat", "Bflat", "F" },
        [Key.G] = new[] { "G", "D", "A", "E", "B", "Gflat", "Dflat", "Aflat", "Eflat", "Bflat", "F", "C" },
        [Key.D] = new[] { "D", "A", "E", "B", "Gflat", "Dflat", "Aflat", "Eflat", "Bflat", "F", "C", "G" },
        [Key.A] = new[] { "A", "E", "B", "Gflat", "Dflat", "Aflat", "Eflat", "Bflat", "F", "C", "G", "D" },
        [Key.E] = new[] { "E", "B", "Gflat", "Dflat", "Aflat", "Eflat", "Bflat", "F", "C", "G", "D", "A" },
        [Key.B] = new[] { "B", "Gflat", "Dflat", "Aflat", "Eflat", "Bflat", "F", "C", "G", "D", "A", "E" },
        [Key.Gflat] = new[] { "Gflat", "Dflat", "Aflat", "Eflat", "Bflat", "F", "C", "G", "D", "A", "E", "B" },
        [Key.Dflat] = new[] { "Dflat", "Aflat", "Eflat", "Bflat", "F", "C", "G", "D", "A", "E", "B", "Gflat" },
        [Key.Aflat] = new[] { "Aflat", "Eflat", "Bflat", "F", "C", "G", "D", "A", "E", "B", "Gflat", "Dflat" },
        [Key.Eflat] = new[] { "Eflat", "Bflat", "F", "C", "G", "D", "A", "E", "B", "Gflat", "Dflat", "Aflat" },
        [Key.Bflat] = new[] { "Bflat", "F", "C", "G", "D", "A", "E", "B", "Gflat", "Dflat", "Aflat", "Eflat" },
        [Key.F] = new[] { "F", "C", "G", "D", "A", "E", "B", "Gflat", "Dflat", "Aflat", "Eflat", "Bflat" },
    };

    [Fact]
    public void NoteAtDegree_CorrectFifths_MatchesExpectedForAllTonics()
    {
        foreach (var kvp in ExpectedByTonic)
        {
            var tonic = kvp.Key;
            var expectedSequence = kvp.Value;

            for (int degree = 1; degree <= 12; degree++)
            {
                var actual = MusicTheory.NoteAtDegree(
                    tonic,
                    degree,
                    correctForFifths: true
                );
                Assert.Equal(
                    expectedSequence[degree - 1],
                    actual
                );
            }
        }
    }
}
