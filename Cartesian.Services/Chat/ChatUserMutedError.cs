using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Chat;

public sealed class ChatUserMutedError() : CartesianError("chat_user_muted", "You are muted in this chat");
