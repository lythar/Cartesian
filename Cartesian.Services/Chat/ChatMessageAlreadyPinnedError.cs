using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Chat;

public class ChatMessageAlreadyPinnedError() 
    : CartesianError("Message is already pinned in this channel");
