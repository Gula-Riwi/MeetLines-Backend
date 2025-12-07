using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;
using MeetLines.Infrastructure.Data;

namespace MeetLines.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public PaymentRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken ct = default)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId, ct);
        }

        public async Task<Payment?> GetByMercadoPagoPaymentIdAsync(long mercadoPagoPaymentId, CancellationToken ct = default)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.MercadoPagoPaymentId == mercadoPagoPaymentId, ct);
        }

        public async Task<Payment?> GetByPreferenceIdAsync(string preferenceId, CancellationToken ct = default)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.MercadoPagoPreferenceId == preferenceId, ct);
        }

        public async Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task AddAsync(Payment payment, CancellationToken ct = default)
        {
            await _context.Payments.AddAsync(payment, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Payment payment, CancellationToken ct = default)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync(ct);
        }
    }
}