using SupportPlatform.Application.NlQuery.RuleBased;

namespace SupportPlatform.Application.Tests.NlQuery;

/// <summary>
/// The stems only have to be consistent, not linguistically right — these pin the forms the
/// parser actually depends on, and the separations it must not blur.
/// </summary>
public class HebrewTextTests
{
    [Theory]
    [InlineData("עמותה", "עמותות")]  // singular / plural
    [InlineData("מאושר", "אושרו")]   // attached particle / verb ending
    [InlineData("מחוז", "במחוז")]    // two stacked particles
    [InlineData("שנת", "שנה")]       // ending stripped before the particle
    [InlineData("תחום", "בתחום")]
    public void Reduces_forms_of_the_same_word_to_one_stem(string a, string b) =>
        Assert.Equal(HebrewText.Normalize(a), HebrewText.Normalize(b));

    [Theory]
    [InlineData("תרבות", "ספורט")]
    [InlineData("צפון", "דרום")]
    [InlineData("תחום", "שנה")]
    public void Keeps_different_words_apart(string a, string b) =>
        Assert.NotEqual(HebrewText.Normalize(a), HebrewText.Normalize(b));

    [Fact]
    public void Splits_letters_from_digits_so_years_stand_alone() =>
        Assert.Equal(["בין", "2023", "ל", "2025"], HebrewText.Tokenize("בין 2023 ל-2025"));

    [Fact]
    public void Leaves_latin_reference_codes_intact_apart_from_case() =>
        Assert.Equal("culture", HebrewText.Normalize("Culture"));
}
