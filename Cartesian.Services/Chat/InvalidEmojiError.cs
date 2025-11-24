using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Chat;

public class InvalidEmojiError(string emoji) 
    : CartesianError($"Invalid emoji: {emoji}")
{
    public string Emoji { get; } = emoji;
}
