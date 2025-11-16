
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PROG6212_POE_P3.Models;
using System;

namespace PROG6212_POE_P3.Services
{
    public class ClaimStore
    {
        private readonly ConcurrentDictionary<int, Claim> _claims = new();
        private int _nextId = 100;

        public IEnumerable<Claim> GetAll() => _claims.Values.OrderByDescending(c => c.DateSubmitted);
        public Claim Get(int id) => _claims.TryGetValue(id, out var c) ? c : null;

        public Claim Add(Claim claim)
        {
            claim.Id = System.Threading.Interlocked.Increment(ref _nextId);
            claim.DateSubmitted = DateTime.UtcNow;
            _claims[claim.Id] = claim;
            return claim;
        }

        public bool Update(Claim claim)
        {
            if (!_claims.ContainsKey(claim.Id)) return false;
            _claims[claim.Id] = claim;
            return true;
        }
    }
}

