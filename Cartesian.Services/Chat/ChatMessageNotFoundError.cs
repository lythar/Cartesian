using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Chat;

public sealed class ChatMessageNotFoundError() : CartesianError("chat_message_not_found", "Chat message not found");
