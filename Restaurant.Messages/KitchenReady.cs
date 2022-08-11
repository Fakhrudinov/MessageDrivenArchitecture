﻿using Restaurant.Messages.Interfaces;
using System;

namespace Restaurant.Messages
{
    public class KitchenReady : IKitchenReady
    {
        public Guid OrderId { get; }
        public bool Ready { get; }

        public KitchenReady(Guid orderId, bool ready)
        {
            OrderId = orderId;
            Ready = ready;
        }
    }
}