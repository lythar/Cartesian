using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Chat;

public sealed class ChatAccessDeniedError() : CartesianError("chat_access_denied", "You don't have access to this chat");
