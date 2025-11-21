using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Chat;

public sealed class ChatDisabledError() : CartesianError("chat_disabled", "Chat is disabled for this context");
