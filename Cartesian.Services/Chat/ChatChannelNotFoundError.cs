using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Chat;

public sealed class ChatChannelNotFoundError() : CartesianError("chat_channel_not_found", "Chat channel not found");
