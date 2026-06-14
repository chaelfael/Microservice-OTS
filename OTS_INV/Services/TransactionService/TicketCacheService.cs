using OTS_INV.Interface;
using StackExchange.Redis;

namespace OTS_INV.Services
{
   

    public class TicketCacheService : ITicketCacheService
    {
        private readonly IDatabase _db;

        public TicketCacheService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase(); 
        }

        public async Task InitializeTicketsAsync(string ticketCode, int totalStock)
        {
            var key = $"TicketStock:{ticketCode}";
            await _db.StringSetAsync(key, totalStock);
        }

        public async Task<bool> TryReserveTicketAsync(string ticketCode, int quantity)
        {
            var key = $"TicketStock:{ticketCode}";

            // -----------------------------------------------------------------
            // THE ULTIMATE BOUNCER: Atomic Check-and-Deduct via Lua Script
            // 1. Get the current stock.
            // 2. If stock >= requested quantity, deduct the quantity and return 1 (true).
            // 3. Else, do nothing and return 0 (false).
            // -----------------------------------------------------------------
            var script = @"
                local stock = tonumber(redis.call('get', KEYS[1]))
                if stock == nil then return 0 end
                local req = tonumber(ARGV[1])
                if stock >= req then
                    redis.call('decrby', KEYS[1], req)
                    return 1
                else
                    return 0
                end";

            // Run the script securely
            var result = await _db.ScriptEvaluateAsync(script, 
                new RedisKey[] { key }, 
                new RedisValue[] { quantity });

            return (int)result == 1;
        }

        public async Task ReturnTicketAsync(string ticketCode, int quantity)
        {
            var key = $"TicketStock:{ticketCode}";
            // Use StringIncrementAsync with the specific quantity to bulk-return
            await _db.StringIncrementAsync(key, quantity);
        }
    }
}