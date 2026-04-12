using System.Security.Cryptography;
using PasswordGenerator.Application.DTOs;
using PasswordGenerator.Application.Interfaces;

namespace PasswordGenerator.Application.Services;

public class PasswordGeneratorService : IPasswordGeneratorService
{
    private const string Uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string UppercaseAll = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Lowercase = "abcdefghjkmnpqrstuvwxyz";
    private const string LowercaseAll = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "23456789";
    private const string DigitsAll = "0123456789";
    private const string Special = "!@#$%^&*()-_=+[]{}|;:,.<>?";

    private static readonly string[] WordList =
    [
        "apple", "brave", "crane", "delta", "eagle", "flame", "grape", "honey",
        "ivory", "joker", "karma", "lemon", "maple", "noble", "ocean", "pearl",
        "quest", "river", "solar", "tiger", "ultra", "vivid", "whale", "xenon",
        "yield", "zephyr", "anchor", "blaze", "cloud", "dream", "ember", "frost",
        "ghost", "haven", "indie", "jewel", "knack", "lunar", "mango", "nexus",
        "orbit", "prism", "quilt", "royal", "storm", "trail", "unity", "vault",
        "wired", "pixel", "amber", "berry", "coral", "daisy", "elfin", "fairy",
        "globe", "haste", "irony", "jumbo", "kayak", "logic", "mercy", "never",
        "oasis", "piano", "quiet", "raven", "shade", "torch", "urban", "vigor",
        "witty", "youth", "zebra", "blend", "charm", "drift", "epoch", "flora",
        "grace", "humor", "image", "jelly", "kiosk", "lyric", "mocha", "nerve",
        "olive", "plume", "quota", "ridge", "spice", "token", "usher", "venom",
        "whirl", "oxide", "aroma", "cabin", "dwarf", "envoy", "fable", "gizmo",
        "heron", "ivory", "jolly", "koala", "llama", "metro", "night", "opera",
        "panda", "radar", "sable", "talon", "umbra", "viola", "whelp", "axiom",
        "azure", "basil", "cedar", "disco", "easel", "flock", "golem", "helix",
        "inlet", "jazzy", "kebab", "latch", "mirth", "natal", "outdo", "pasty",
        "rebel", "savor", "tidal", "unzip", "vowel", "waltz", "xerox", "yacht"
    ];

    public GenerateResult GeneratePassword(GenerateRequest request)
    {
        var pool = BuildCharacterPool(request);
        if (pool.Length == 0)
            throw new ArgumentException("At least one character type must be selected.");

        var password = new char[request.Length];
        for (int i = 0; i < request.Length; i++)
        {
            password[i] = pool[RandomNumberGenerator.GetInt32(pool.Length)];
        }

        var result = new string(password);
        var entropy = PasswordStrengthCalculator.CalculateEntropy(result);

        return new GenerateResult
        {
            Password = result,
            Entropy = entropy,
            Strength = PasswordStrengthCalculator.GetStrength(entropy)
        };
    }

    public GenerateResult GeneratePassphrase(GenerateRequest request)
    {
        var wordCount = request.WordCount > 0 ? request.WordCount : 4;
        var words = new string[wordCount];

        for (int i = 0; i < wordCount; i++)
        {
            words[i] = WordList[RandomNumberGenerator.GetInt32(WordList.Length)];
        }

        var passphrase = string.Join(request.Separator ?? "-", words);
        var entropy = Math.Log2(Math.Pow(WordList.Length, wordCount));

        return new GenerateResult
        {
            Password = passphrase,
            Entropy = entropy,
            Strength = PasswordStrengthCalculator.GetStrength(entropy)
        };
    }

    private static string BuildCharacterPool(GenerateRequest request)
    {
        var pool = "";

        if (request.IncludeUppercase)
            pool += request.ExcludeAmbiguous ? Uppercase : UppercaseAll;

        if (request.IncludeLowercase)
            pool += request.ExcludeAmbiguous ? Lowercase : LowercaseAll;

        if (request.IncludeDigits)
            pool += request.ExcludeAmbiguous ? Digits : DigitsAll;

        if (request.IncludeSpecial)
            pool += Special;

        return pool;
    }
}
