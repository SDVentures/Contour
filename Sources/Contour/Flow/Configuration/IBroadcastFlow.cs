﻿namespace Contour.Flow.Configuration
{
    public interface IBroadcastFlow
    {
        IActingFlowConcatenation<TOutput> Broadcast<TOutput>();
    }
}